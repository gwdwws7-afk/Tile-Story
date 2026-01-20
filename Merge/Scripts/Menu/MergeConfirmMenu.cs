using DG.Tweening;
using Firebase.Analytics;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class MergeConfirmMenu : PopupMenuForm
    {
        public Image m_TargetImage;
        public DelayButton m_PopButton, m_CancelButton, m_CoinBuyButton, m_CloseButton, m_TipButton;
        public GameObject m_PopText, m_UseCoinText;
        public TextMeshProUGUI m_CoinCostText;

        private PropLogic selectedProp;
        private int popCostCoinNum = 0;
        private AsyncOperationHandle spriteHandle;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            selectedProp = MergeManager.Merge.SelectedProp;
            if (selectedProp == null || selectedProp.AttachmentId != 1 || selectedProp.Square == null)
            {
                GameManager.UI.HideUIForm(this);
                return;
            }

            m_CloseButton.OnInit(OnCloseButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);
            m_PopButton.OnInit(OnPopButtonClick);
            m_CancelButton.OnInit(OnCancelButtonClick);
            m_CoinBuyButton.OnInit(OnCoinBuyButtonClick);
            m_CoinBuyButton.interactable = true;

            bool isPopMenu = (bool)userData;
            m_PopText.SetActive(isPopMenu);
            m_UseCoinText.SetActive(!isPopMenu);
            m_PopButton.gameObject.SetActive(isPopMenu);
            m_CancelButton.gameObject.SetActive(isPopMenu);
            m_CoinBuyButton.gameObject.SetActive(!isPopMenu);

            if (!isPopMenu)
            {
                IDataTable<DRMergeGenerateBubble> dataTable = MergeManager.DataTable.GetDataTable<DRMergeGenerateBubble>(MergeManager.Instance.GetMergeDataTableName());
                var allData = dataTable.GetAllDataRows();
                foreach (var targetData in allData)
                {
                    if (targetData.GenerateBubble == selectedProp.PropId)
                    {
                        popCostCoinNum = targetData.BubbleCostCoinNum;
                        m_CoinCostText.text = popCostCoinNum.ToString();
                        break;
                    }
                }

                //IDataTable<DRMergeAdditionalOutput> additionDataTable = MergeManager.DataTable.GetDataTable<DRMergeAdditionalOutput>(MergeManager.Instance.GetMergeDataTableName());
                //var data = additionDataTable.GetDataRow(selectedProp.Rank);
                //if (data != null)
                //{
                //    popCostCoinNum = data.BubbleCostCoinNum;
                //    m_CoinCostText.text = popCostCoinNum.ToString();
                //}
            }

            m_TargetImage.color = new Color(1, 1, 1, 0);

            IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
            var propData = propDataTable.GetDataRow(selectedProp.PropId);
            UnityUtility.UnloadAssetAsync(spriteHandle);
            spriteHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(propData.AssetName, MergeManager.Instance.GetMergeAtlasName("MergePropAtlas")), sp =>
            {
                m_TargetImage.sprite = sp as Sprite;
                m_TargetImage.SetNativeSize();

                m_TargetImage.DOFade(1, 0.1f);
            });
        }

        public override void OnReset()
        {
            m_PopButton.onClick.RemoveAllListeners();
            m_CancelButton.onClick.RemoveAllListeners();
            m_CoinBuyButton.onClick.RemoveAllListeners();

            UnityUtility.UnloadAssetAsync(spriteHandle);
            spriteHandle = default;

            base.OnReset();
        }

        public override void OnRelease()
        {
            base.OnRelease();
        }

        private void OnPopButtonClick()
        {
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
                mainBoard.HideBubbleOperationBox();
            OnCloseButtonClick();

            if (selectedProp != null && selectedProp.AttachmentId == 1 && selectedProp.Square != null)
            {
                MergeManager.Merge.ReplaceProp(20101, selectedProp, selectedProp.Square, p =>
                {
                    p?.ShowPropGenerateEffect();
                });

                GameManager.Sound.PlayAudio(SoundType.SFX_Pop_Element_Break.ToString());
            }
        }

        private void OnCancelButtonClick()
        {
            GameManager.UI.HideUIForm(this);
        }

        private void OnCoinBuyButtonClick()
        {
            if (popCostCoinNum <= 0)
            {
                OnCloseButtonClick();
                return;
            }

            if (selectedProp != null && selectedProp.AttachmentId == 1)
            {
                if (GameManager.PlayerData.CoinNum >= popCostCoinNum)
                {
                    GameManager.PlayerData.UseItem(TotalItemData.Coin, popCostCoinNum);

                    OnCloseButtonClick();
                    MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                    if (mainBoard != null)
                        mainBoard.HideBubbleOperationBox();
                    if (selectedProp.Prop != null)
                    {
                        selectedProp.Prop.BodyScale = 1f;
                        selectedProp.Prop.ClearAnim();
                        selectedProp.Prop.ShowPunchAnim();
                        selectedProp.Prop.ShowPropGenerateEffect();
                    }
                    selectedProp.ReleaseAttachment();
                    MergeManager.Merge.SavePropDistributedMap();

                    GameManager.Sound.PlayAudio(SoundType.SFX_Pop_Element_Break.ToString());

                    GameManager.Firebase.RecordMessageByEvent("Merge_Spend_Coin_For_Pop", new Firebase.Analytics.Parameter("num", popCostCoinNum));
                }
                else
                {
                    m_CoinBuyButton.interactable = false;
                    GameManager.UI.ShowUIForm("ShopMenuManager",(obj) => { m_CoinBuyButton.interactable = true; });

                    GameManager.Firebase.RecordMessageByEvent(
                        Constant.AnalyticsEvent.Coin_Not_Enough,
                        new Parameter("EntranceID", 8),
                        new Parameter("Source", "BuyMergePops"));
                }
            }
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);
        }

        private void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }
    }
}
