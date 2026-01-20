using DG.Tweening;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class MergeMainMenu_LoveGiftBattle : MergeMainMenuBase, IItemFlyReceiver
    {
        public DelayButton m_SupplyButton;
        public DelayButton m_StorageButton;
        public DelayButton m_SaleButton;
        public DelayButton m_GetMoreButton;
        public TextMeshProUGUI m_SupplyNumText;
        public TextMeshProUGUI m_StorageNumText;
        public ParticleSystem m_BtnGetRewardEffect;
        public GameObject m_SupplyButtonDisplayEffect;
        public MergeThiefBoard m_ThiefBoard;
        public NewGiftUnlockBoard m_NewGiftUnlockBoard;
        public Image m_SupplyImage;

        private bool isUseSupply;
        private AsyncOperationHandle spriteHandle;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            GameManager.Event.Subscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);
            GameManager.Event.Subscribe(MergeOfferCompleteEventArgs.EventId, MergeOfferComplete);

            RewardManager.Instance.RegisterItemFlyReceiver(this);

            RefreshSupply(false);
            isUseSupply = false;
            m_SupplyButtonDisplayEffect.SetActive(false);

            RefreshSale();
            m_ThiefBoard.Init();
        }

        public override void OnReset()
        {
            GameManager.Event.Unsubscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);
            GameManager.Event.Unsubscribe(MergeOfferCompleteEventArgs.EventId, MergeOfferComplete);

            RewardManager.Instance.UnregisterItemFlyReceiver(this);

            m_ChestPromptBox.Release();
            m_ThiefBoard.Recycle();
            m_NewGiftUnlockBoard.Release();

            base.OnReset();
        }

        public override void OnRelease()
        {
            base.OnRelease();

            UnityUtility.UnloadAssetAsync(spriteHandle);
            spriteHandle = default;

            m_ThiefBoard.Release();
        }

        private void OnEnable()
        {
            UIGroup group = GameManager.UI.GetUIGroup("PopupUI");
            group.transform.parent.GetComponent<Canvas>().sortingOrder = 11;

            GameManager.Ads.HideBanner("Merge");
        }

        private void OnDisable()
        {
            UIGroup group = GameManager.UI.GetUIGroup("PopupUI");
            group.transform.parent.GetComponent<Canvas>().sortingOrder = 8;

            GameManager.Ads.ShowBanner("Merge");
        }

        public override void OnPause()
        {
            m_SupplyButton.interactable = false;
            m_StorageButton.interactable = false;
            m_SaleButton.interactable = false;
            m_GetMoreButton.interactable = false;

            base.OnPause();
        }

        public override void OnResume()
        {
            m_SupplyButton.interactable = true;
            m_StorageButton.interactable = true;
            m_SaleButton.interactable = true;
            m_GetMoreButton.interactable = true;

            base.OnResume();
        }

        protected override void InitializeButton()
        {
            base.InitializeButton();

            m_SupplyButton.OnInit(OnSupplyButtonClick);
            m_StorageButton.OnInit(OnStorageButtonClick);
            m_SaleButton.OnInit(OnSaleButtonClick);
            m_GetMoreButton.OnInit(OnGetMoreButtonClick);

            m_SupplyButton.interactable = true;
            m_StorageButton.interactable = true;
            m_SaleButton.interactable = true;
            m_GetMoreButton.interactable = true;
        }

        public void RefreshSupply(bool showAnim, bool delayShow = false)
        {
            string supplySpriteName = null;
            if (StorePropIds.Count > 0)
            {
                m_SupplyNumText.text = (MergeManager.PlayerData.GetMergeEnergyBoxNum() + StorePropIds.Count).ToString();
                m_SupplyButton.gameObject.SetActive(true);
                m_GetMoreButton.gameObject.SetActive(false);

                int index = StorePropIds.Count - 1;
                int propId = StorePropIds[index].PropId;
                IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
                var propData = propDataTable.GetDataRow(propId);
                supplySpriteName = propData.AssetName;
            }
            else
            {
                int boxNum = MergeManager.PlayerData.GetMergeEnergyBoxNum();
                if (boxNum > 0)
                {
                    m_SupplyNumText.text = boxNum.ToString();
                    m_SupplyButton.gameObject.SetActive(true);
                    m_GetMoreButton.gameObject.SetActive(false);
                }
                else
                {
                    m_SupplyButton.gameObject.SetActive(false);
                    m_SupplyButtonDisplayEffect.SetActive(true);
                    if (delayShow)
                    {
                        GameManager.Task.AddDelayTriggerTask(0.7f, () =>
                        {
                            RefreshSupply(true);
                        });
                    }
                    else
                    {
                        m_GetMoreButton.DOKill();

                        if (DateTime.Now <= MergeManager.Instance.GetActivityEndTime() && !MergeManager.Instance.CheckActivityIsComplete())
                        {
                            if (showAnim)
                            {
                                m_GetMoreButton.transform.localScale = Vector3.zero;
                                m_GetMoreButton.gameObject.SetActive(true);
                                m_GetMoreButton.transform.DOScale(0.66f, 0.2f).onComplete = () =>
                                {
                                    m_GetMoreButton.transform.DOScale(0.6f, 0.2f);
                                };
                            }
                            else
                            {
                                m_GetMoreButton.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                                m_GetMoreButton.gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            m_GetMoreButton.gameObject.SetActive(false);
                        }
                    }
                }

                supplySpriteName = "Flower_1_1";
            }

            m_SupplyImage.DOKill();
            m_SupplyImage.color = new Color(1, 1, 1, 0);
            UnityUtility.UnloadAssetAsync(spriteHandle);
            spriteHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(supplySpriteName, MergeManager.Instance.GetMergeAtlasName("MergePropAtlas")), sp =>
            {
                m_SupplyImage.sprite = sp;
                m_SupplyImage.DOFade(1, 0.1f);
            });
        }

        public override void RefreshSale()
        {
            int curLevel = MergeManager.PlayerData.GetCurMergeOfferLevel();
            IDataTable<DRMergeOffer> dataTable = MergeManager.DataTable.GetDataTable<DRMergeOffer>(MergeManager.Instance.GetMergeDataTableName());
            if (curLevel > dataTable.Count)
            {
                m_SaleButton.gameObject.SetActive(false);
            }
            else
            {
                m_SaleButton.gameObject.SetActive(true);
            }
        }

        public override void RefreshStorage()
        {
            RefreshSupply(true);
        }

        protected virtual void GenerateProp()
        {
            if (MergeManager.PlayerData.GetMergeEnergyBoxNum() <= 0)
            {
                return;
            }

            m_SupplyButton.interactable = false;
            bool triggerDragMergeGuide = false;

            Square randomSquare = MergeManager.Instance.GetTapBoxGuideTargetSquare();
            if (randomSquare != null && randomSquare.FilledProp == null && !MergeGuideMenu.CheckGuideIsComplete(GuideTriggerType.Guide_DragMerge))
            {
                triggerDragMergeGuide = true;
            }
            else
            {
                randomSquare = MergeManager.Merge.GetRandomEmptySquare();
            }

            if (randomSquare != null)
            {
                int propId = 10101;

                PropLogic prop = MergeManager.Merge.GenerateProp(propId, 0, m_SupplyButton.transform.position, randomSquare, PropMovementState.Bouncing);

                prop.SpawnPropComplete += p =>
                {
                    m_SupplyButton.interactable = true;

                    if (triggerDragMergeGuide)
                    {
                        GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                        {
                            m_GuideMenu?.TriggerGuide(GuideTriggerType.Guide_DragMerge);
                        });
                    }
                };

                isUseSupply = true;
                MergeManager.PlayerData.SubtractMergeEnergyBoxNum(1);
                GameManager.Event.Fire(this, RefreshMergeEnergyBoxEventArgs.Create());
                GameManager.Sound.PlayAudio(SoundType.SFX_ClickGenerator.ToString());
            }
            else
            {
                m_SupplyButton.interactable = true;
                ShowWeakHint("Merge.Board is full!");
            }
        }

        public void OnSupplyButtonClick()
        {
            if (StorePropIds.Count > 0)
            {
                WithdrawProp();
            }
            else
            {
                GenerateProp();
            }

            HideChestRewardTipBox();
            HideBubbleOperationBox();
            HideFingerAnim();
            MergeManager.Merge.ResetHintTimer();
        }

        public override void ShowChestRewardTipBox(List<ItemData> datas, PropLogic propLogic)
        {
            GameManager.Task.AddDelayTriggerTask(0, () =>
            {
                if (m_ChestPromptBox == null)
                    return;

                m_SelectedChestProp = propLogic;

                Square square = propLogic.Square;
                if (square != null)
                {
                    float offsetX = 0f;
                    if (datas.Count == 3 || datas.Count > 4)
                    {
                        if (square.m_Col <= 0)
                            offsetX = 130;
                        else if (square.m_Col >= 6)
                            offsetX = -130;
                    }
                    else
                    {
                        if (square.m_Col <= 0)
                            offsetX = 85;
                        else if (square.m_Col >= 6)
                            offsetX = -85;
                    }

                    m_ChestPromptBox.Init(datas);
                    m_ChestPromptBox.Show(square.transform.position, offsetX);
                }
            });
        }

        public override void ShowBubbleOperationBox(int level, PropLogic propLogic)
        {
            bool isShowAdsButton = CanShowBubbleAdsButton(level);
            Square square = propLogic.Square;
            float offsetX = 0f;
            if (isShowAdsButton)
            {
                if (square.m_Col <= 0)
                    offsetX = 150;
                else if (square.m_Col >= 6)
                    offsetX = -150;
            }
            else
            {
                if (square.m_Col <= 0)
                    offsetX = 70;
                else if (square.m_Col >= 6)
                    offsetX = -70;
            }

            m_BubbleOperationBox.Initialize(level, propLogic, isShowAdsButton);
            m_BubbleOperationBox.Show(square.transform.position, offsetX);
        }

        public override Vector3 GetStoragePropGeneratePos()
        {
            return m_SupplyButton.transform.position;
        }

        public override void ShowClickSupplyFingerAnim()
        {
            ShowFingerAnim(m_SupplyButton.transform.position);
        }

        public override void ShowClickStorageFingerAnim()
        {
            ShowFingerAnim(m_StorageButton.transform.position);
        }

        public void OnStorageButtonClick()
        {
            OnSupplyButtonClick();
        }

        private void OnSaleButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeOfferMenu"));
        }

        private void OnGetMoreButtonClick()
        {
            if (DateTime.Now < MergeManager.Instance.GetActivityEndTime())
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeGetBoxMenu"));
        }

        protected override void OnCloseButtonClick()
        {
            if (MergeManager.Instance.CheckActivityHasStarted() && (DateTime.Now > MergeManager.Instance.GetActivityEndTime() || MergeManager.Instance.CheckActivityIsComplete())) 
            {
                if (MergeManager.Instance.CheckActivityIsComplete())
                {
                    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeGiveUpMenu"));
                }
                else
                {
                    if (GameManager.DataNode.GetData<bool>("ShowedMergeLastChanceMenu", false))
                    {
                        if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
                        {
                            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeGiveUpMenu"));
                        }
                        else
                        {
                            GameManager.UI.HideUIForm(this);
                            MergeManager.Instance.EndActivity();
                        }
                    }
                    else
                    {
                        if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
                        {
                            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeLastChanceMenu"));
                        }
                        else
                        {
                            GameManager.UI.HideUIForm(this);
                            MergeManager.Instance.EndActivity();
                        }
                    }
                }
            }
            else
            {
                if (!isUseSupply && GameManager.Process.CurrentProcessName == ProcessType.AutoShowMergeProcess.ToString())
                {
                    GameManager.DataNode.SetData<int>("MergeGameToMapTime", 2);
                }

                GameManager.UI.HideUIForm(this);
            }

            GameManager.Process.EndProcess(ProcessType.AutoShowMergeProcess);
        }

        public void ShowNewGiftUnlockBoard(int propId, Action callback)
        {
            IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
            var propData = propDataTable.GetDataRow(propId);
            m_NewGiftUnlockBoard.Init(propData.AssetName, m_ThiefBoard.m_ThiefHitPos.position, m_ThiefBoard.m_ThiefLeftGiftPos.position, () =>
            {
                m_NewGiftUnlockBoard.m_HideAction = square =>
                {
                    MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                    if (square == null)
                    {
                        if (mainBoard != null) 
                            mainBoard.StoreProp(propId);
                    }
                    else
                    {
                        var propLogic = MergeManager.Merge.GenerateProp(propId, 0, square.transform.position, square, PropMovementState.Static);

                        if (mainBoard != null && !MergeGuideMenu.CheckGuideIsComplete(GuideTriggerType.Guide_TapMaxReward) && m_ThiefBoard.CurMerchantInventory != null)
                        {
                            GameManager.Task.AddDelayTriggerTask(2.5f, () =>
                            {
                                if (mainBoard != null)
                                    mainBoard.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_TapMaxReward, propLogic);
                            });
                        }
                    }

                    callback?.Invoke();
                };

                GameManager.Task.AddDelayTriggerTask(1.3f, () =>
                {
                    m_NewGiftUnlockBoard.Show();
                });
            });

            GameManager.Firebase.RecordMessageByEvent("Merge_Task_Unlock", new Firebase.Analytics.Parameter("stage", propId % 90100 - 1));
        }

        public void ReleaseAllFlowerProp()
        {
            List<int> targetList = new List<int>() { 10101, 10102, 10103, 10104 };
            var list = MergeManager.Merge.GetPropListOnChessboard(targetList);
            foreach (var prop in list)
            {
                MergeManager.Merge.ReleaseProp(prop);
            }

            MergeManager.PlayerData.SubtractMergeEnergyBoxNum(MergeManager.PlayerData.GetMergeEnergyBoxNum());

            //if (StorePropIds != null && StorePropIds.Count > 0)
            //{
            //    StorePropIds.RemoveAll(t =>
            //    {
            //        return targetList.Contains(t);
            //    });

            //    MySelf.Model.MergeModel.Instance.StorePropIds = null;
            //    for (int i = 0; i < StorePropIds.Count; i++)
            //    {
            //        MergeManager.PlayerData.AddStorePropId(StorePropIds[i], 1, false);
            //    }
            //}

            RefreshStorage();
        }

        private void OnRefreshMergeEnergyBox(object sender, GameEventArgs e)
        {
            RefreshSupply(true, true);
        }

        public void MergeOfferComplete(object sender, GameEventArgs e)
        {
            RefreshSale();
        }

        #region reciver
        public ReceiverType ReceiverType => ReceiverType.Common;

        public GameObject GetReceiverGameObject() => m_SupplyButton.gameObject;

        public void OnFlyHit(TotalItemData type)
        {

        }

        public void OnFlyEnd(TotalItemData type)
        {
            if (m_SupplyButton.gameObject != null)
            {
                GameManager.Sound.PlayAudio("SFX_itemget");
                m_SupplyButton.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
                {
                    m_SupplyButton.transform.localScale = Vector3.one;
                };
                m_BtnGetRewardEffect.Play();
            }
        }

        public Vector3 GetItemTargetPos(TotalItemData type)
        {
            return m_SupplyButton.transform.position;
        }
        #endregion
    }
}
