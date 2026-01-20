using DG.Tweening;
using Spine.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class MergeOfferColumn : MonoBehaviour
    {
        public Transform m_Body;
        public Image m_SelectedBg;
        public GameObject m_Bg1;
        public GameObject m_Bg2;
        public CanvasGroup m_BgCanvasGroup;
        public Transform m_PropRoot;
        public Button m_BuyButton;
        public TextMeshProUGUI m_FreeText;
        public TextMeshProUGUI m_BuyText;
        public SkeletonGraphic m_Lock;

        private DRMergeOffer m_Data;
        private MergeOfferMenu mergeOfferMenu;
        private ScrollArea scrollArea;
        private List<AsyncOperationHandle<GameObject>> handlesList = new List<AsyncOperationHandle<GameObject>>();
        private List<MergeOfferItemSlot> itemSlotsList = new List<MergeOfferItemSlot>();
        private bool isUnlock;
        private int productId;

        public void Initialize(DRMergeOffer data, MergeOfferMenu mergeOfferMenu)
        {
            m_Data = data;
            this.mergeOfferMenu = mergeOfferMenu;
            scrollArea = mergeOfferMenu.m_ScrollArea;

            m_BuyButton.onClick.RemoveAllListeners();
            m_BuyButton.onClick.AddListener(OnBuyButtonClick);
            m_BuyButton.gameObject.SetActive(true);

            Refresh(false);

            if (data.ProductID != 0)
            {
                m_FreeText.gameObject.SetActive(false);
                m_BuyText.gameObject.SetActive(true);
                productId = data.ProductID;
                string price = GameManager.Purchase.GetPrice((ProductNameType)productId);
                if (!string.IsNullOrEmpty(price))
                {
                    m_BuyText.text = price;
                }
                else
                {
                    m_BuyText.font = GameManager.Localization.CurrentFont;
                    GameManager.Localization.GetPresetMaterialAsync("Btn_Green", mat =>
                    {
                        m_BuyText.fontMaterial = mat;
                    });
                    m_BuyText.text = GameManager.Localization.GetString("Shop.Buy");
                }
            }
            else
            {
                m_FreeText.gameObject.SetActive(true);
                m_BuyText.gameObject.SetActive(false);
                productId = 0;
            }

            Vector3[] posList = UnityUtility.GetAveragePosition(Vector3.zero, new Vector3(220, 0, 0), data.RewardPropIds.Count);
            for (int i = 0; i < data.RewardPropIds.Count; i++)
            {
                TotalItemData itemData = TotalItemData.FromInt(data.RewardPropIds[i]);
                int itemNum = data.RewardPropNums[i];
                Vector3 pos = posList[i];

                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync("MergeOfferItemSlot", m_PropRoot);
                handle.Completed += res =>
                {
                    if (res.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject itemSlot = res.Result;
                        itemSlot.transform.localPosition = pos;
                        MergeOfferItemSlot slot = itemSlot.GetComponent<MergeOfferItemSlot>();
                        slot.OnInit(itemData, itemNum);
                        itemSlotsList.Add(slot);
                    }
                };
                handlesList.Add(handle);
            }

            m_Bg1.SetActive((data.Id - 1) % 3 == 2);
            m_Bg2.SetActive((data.Id - 1) % 3 != 2);
        }

        public void Release()
        {
            foreach (MergeOfferItemSlot slot in itemSlotsList)
            {
                slot.OnRelease();
            }
            itemSlotsList.Clear();

            foreach (AsyncOperationHandle<GameObject> handle in handlesList)
            {
                Addressables.ReleaseInstance(handle);
            }
            handlesList.Clear();

            productId = 0;
            m_BuyButton.onClick.RemoveAllListeners();
        }

        public void Refresh(bool isShowAnim)
        {
            isUnlock = m_Data.Id == MergeManager.PlayerData.GetCurMergeOfferLevel();

            if (isShowAnim)
            {
                if (isUnlock)
                {
                    GameManager.Sound.PlayAudio(SoundType.SFX_goldenpass_presentlocked.ToString());
                    m_Lock.AnimationState.SetAnimation(0, "active_lock", false).Complete += (s) =>
                    {
                        m_Lock.gameObject.SetActive(false);
                    };
                    m_BgCanvasGroup.DOFade(0, 1f).SetEase(Ease.Linear);
                }
                else
                {
                    var anim = m_Lock.AnimationState.SetAnimation(0, "shake_lock", false);
                    anim.AnimationStart = 0;
                    anim.AnimationEnd = 0;
                    m_Lock.gameObject.SetActive(true);
                }
            }
            else
            {
                m_Lock.gameObject.SetActive(!isUnlock);
                m_BgCanvasGroup.DOKill();
                m_BgCanvasGroup.alpha = isUnlock ? 0 : 1;

                if (!isUnlock)
                {
                    var anim = m_Lock.AnimationState.SetAnimation(0, "shake_lock", false);
                    anim.AnimationStart = 0;
                    anim.AnimationEnd = 0;
                }
            }
        }

        public void ShowMoveInAnim(int index)
        {
            m_Body.localPosition = new Vector3(1200, 0, 0);
            m_Body.DOLocalMoveX(-50, 0.25f).SetDelay((3 - index) * 0.05f).onComplete = () =>
               {
                   m_Body.DOLocalMoveX(0, 0.12f);
               };
        }

        private void OnBuyButtonClick()
        {
            if (!isUnlock)
            {
                GameManager.UI.ShowWeakHint("Endless.Claim previous offer to unlock", Vector3.zero);
                return;
            }

            if (productId != 0)
            {
                GameManager.Purchase.BuyProduct((ProductNameType)productId, OnBuySuccess);
            }
            else
            {
                OnBuySuccess();
            }

            if (MergeManager.Instance.Theme == MergeTheme.DigTreasure && m_Data != null)
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DigTreasure_Merge_Endless_Treasure_Claim, new Firebase.Analytics.Parameter("Stage", m_Data.Id - 1));
        }

        private void OnBuySuccess()
        {
            m_BuyButton.gameObject.SetActive(false);
            int curLevel = MergeManager.PlayerData.GetCurMergeOfferLevel();
            int nextLevel = curLevel + 1;
            MergeManager.PlayerData.SetCurMergeOfferLevel(nextLevel);
            for (int i = 0; i < m_Data.RewardPropIds.Count; i++)
            {
                RewardManager.Instance.SaveRewardData(TotalItemData.FromInt(m_Data.RewardPropIds[i]), m_Data.RewardPropNums[i], true);
            }
            GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(0, null));
            GameManager.Event.Fire(this, RefreshMergeEnergyBoxEventArgs.Create());

            Vector3[] posList = UnityUtility.GetAveragePosition(m_PropRoot.position + new Vector3(0, 0.1f, 0), new Vector3(0.13f, 0, 0), m_Data.RewardPropIds.Count);
            bool showSound = false;
            for (int i = 0; i < m_Data.RewardPropIds.Count; i++)
            {
                TotalItemData itemData = TotalItemData.FromInt(m_Data.RewardPropIds[i]);
                int itemNum = m_Data.RewardPropNums[i];
                Vector3 pos = posList[i];
                float delayTime = i * 0.05f;

                UnityUtility.InstantiateAsync("MergeRewardGetItemSlot", mergeOfferMenu.m_EffectsRoot, obj =>
                {
                    var item = obj.GetComponent<MergeOfferItemSlot>();
                    item.OnInit(itemData, itemNum);
                    item.gameObject.SetActive(false);
                    item.transform.position = pos;
                    item.transform.localScale = Vector3.zero;

                    GameManager.Task.AddDelayTriggerTask(0.03f + delayTime, () =>
                    {
                        UnityUtility.InstantiateAsync("MergeOfferGetRewardEffect", item.transform, (effect) =>
                        {
                            if (effect != null)
                            {
                                GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                                {
                                    UnityUtility.UnloadInstance(effect);
                                });
                            }
                        });

                        item.gameObject.SetActive(true);
                        item.transform.DOScale(0.8f, 0.2f).SetEase(Ease.OutBack);
                        item.transform.DOLocalMoveY(item.transform.localPosition.y + 40, 0.6f).SetEase(Ease.InSine);
                        var canvasGroup = item.gameObject.GetComponent<CanvasGroup>();
                        canvasGroup.DOFade(0, 0.3f).SetDelay(0.4f).onComplete += () =>
                        {
                            item.OnRelease();
                            UnityUtility.UnloadInstance(obj);
                        };

                        if (!showSound)
                        {
                            showSound = true;
                            GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());
                        }
                    });
                });
            }

            GameManager.Task.AddDelayTriggerTask(0.1f, () =>
            {
                transform.DOScale(0, 0.3f).SetDelay(0.1f);

                GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                {
                    float moveTime = 0.3f;
                    scrollArea.RemoveColumnByIndex(0, true, moveTime);

                    IDataTable<DRMergeOffer> dataTable = MergeManager.DataTable.GetDataTable<DRMergeOffer>(MergeManager.Instance.GetMergeDataTableName());
                    DRMergeOffer data = dataTable.GetDataRow(nextLevel + 2);
                    if (data == null)
                    {
                        if (nextLevel > dataTable.Count)
                        {
                            GameManager.UI.HideUIForm(MergeManager.Instance.GetMergeMenuName("MergeOfferMenu"));
                            GameManager.Event.Fire(this, Merge.MergeOfferCompleteEventArgs.Create());
                        }
                        return;
                    }
                    MergeOfferScrollColumn newColumn = new MergeOfferScrollColumn(MergeManager.Instance.GetMergeOfferColumnName("MergeOfferColumn"), data, scrollArea.recycleWidth, mergeOfferMenu);
                    scrollArea.AddColumnLast(newColumn);
                    newColumn.Spawn(res =>
                    {
                        if (newColumn.Instance != null)
                        {
                            newColumn.Instance.transform.localScale = Vector3.zero;
                            newColumn.Instance.transform.localPosition = new Vector3(0, -(scrollArea.ContentPaddingTop + newColumn.Height * (4 - 0.5f)), 0);
                            newColumn.Instance.SetActive(true);
                            newColumn.Instance.transform.DOScale(1, moveTime);
                        }
                    });
                });

                scrollArea.Refresh();
            });

            GameManager.Firebase.RecordMessageByEvent("Merge_Endless_Treasure_Claim", new Firebase.Analytics.Parameter("stage", m_Data.Id));
        }
    }
}
