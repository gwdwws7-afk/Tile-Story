using DG.Tweening;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Merge
{
    public class MergeMainMenu : MergeMainMenuBase, IItemFlyReceiver
    {
        public ProgressBoard m_ProgressBoard;
        public NewPropMergedBoard m_NewPropMergedBoard;
        public DelayButton m_SupplyButton;
        public DelayButton m_StorageButton;
        public DelayButton m_SaleButton;
        public DelayButton m_FinalRewardButton;
        public DelayButton m_GetMoreButton;
        public TextMeshProUGUI m_SupplyNumText;
        public TextMeshProUGUI m_StorageNumText;
        public ChestPromptBox m_FinalChestPromptBox;
        public ParticleSystem m_BtnGetRewardEffect;
        public GameObject m_SupplyButtonDisplayEffect;
        public TextMeshProUGUI m_SaleNumText;
        public GameObject m_BottomText;

        private bool isUseSupply;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            GameManager.Event.Subscribe(MaxStageUpgradeEventArgs.EventId, OnMaxStageUpgrade);
            GameManager.Event.Subscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);
            GameManager.Event.Subscribe(MergeOfferCompleteEventArgs.EventId, MergeOfferComplete);

            RewardManager.Instance.RegisterItemFlyReceiver(this);

            m_ProgressBoard.RefreshProgress(true);

            RefreshSupply(false);
            isUseSupply = false;
            m_SupplyButtonDisplayEffect.SetActive(false);

            RefreshSale();
            
            m_BottomText.SetActive(DateTime.Now < MergeManager.Instance.GetActivityEndTime());
        }

        public override void OnReset()
        {
            GameManager.Event.Unsubscribe(MaxStageUpgradeEventArgs.EventId, OnMaxStageUpgrade);
            GameManager.Event.Unsubscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);
            GameManager.Event.Unsubscribe(MergeOfferCompleteEventArgs.EventId, MergeOfferComplete);

            RewardManager.Instance.UnregisterItemFlyReceiver(this);

            m_ChestPromptBox.Release();
            m_FinalChestPromptBox.Release();

            base.OnReset();
        }

        public override void OnRelease()
        {
            m_NewPropMergedBoard.Release();

            base.OnRelease();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
            {
                HideFinalChestRewardTipBox();
            }
        }

        private void OnEnable()
        {
            UIGroup group = GameManager.UI.GetUIGroup("PopupUI");
            group.transform.parent.GetComponent<Canvas>().sortingOrder = 11;
        }

        private void OnDisable()
        {
            UIGroup group = GameManager.UI.GetUIGroup("PopupUI");
            group.transform.parent.GetComponent<Canvas>().sortingOrder = 8;
        }

        public override void OnPause()
        {
            m_SupplyButton.interactable = false;
            m_StorageButton.interactable = false;
            m_SaleButton.interactable = false;
            m_GetMoreButton.interactable = false;
            m_FinalRewardButton.interactable = false;

            base.OnPause();
        }

        public override void OnResume()
        {
            m_SupplyButton.interactable = true;
            m_StorageButton.interactable = true;
            m_SaleButton.interactable = true;
            m_GetMoreButton.interactable = true;
            m_FinalRewardButton.interactable = true;

            base.OnResume();
        }

        protected override void InitializeButton()
        {
            base.InitializeButton();

            m_SupplyButton.OnInit(OnSupplyButtonClick);
            m_StorageButton.OnInit(OnStorageButtonClick);
            m_SaleButton.OnInit(OnSaleButtonClick);
            m_FinalRewardButton.OnInit(OnFinalRewardButtonClick);
            m_GetMoreButton.OnInit(OnGetMoreButtonClick);

            m_SupplyButton.interactable = true;
            m_StorageButton.interactable = true;
            m_SaleButton.interactable = true;
            m_FinalRewardButton.interactable = true;
            m_GetMoreButton.interactable = true;
        }

        public virtual void RefreshSupply(bool showAnim, bool delayShow = false)
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

                    if (DateTime.Now <= MergeManager.Instance.GetActivityEndTime())
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

                if (curLevel % 3 != 0)
                {
                    m_SaleNumText.text = (3 - curLevel % 3).ToString();
                    m_SaleNumText.transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    m_SaleNumText.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        public override void RefreshStorage()
        {
            if (StorePropIds.Count > 0)
            {
                m_StorageNumText.text = StorePropIds.Count.ToString();
                m_StorageButton.gameObject.SetActive(true);
            }
            else
            {
                m_StorageButton.gameObject.SetActive(false);
            }
        }

        public void ShowFinalChestRewardTipBox(List<ItemData> datas, Vector3 position)
        {
            if (m_FinalChestPromptBox != null)
            {
                GameManager.Task.AddDelayTriggerTask(0, () =>
                {
                    m_FinalChestPromptBox.Init(datas);
                    m_FinalChestPromptBox.Show(position);
                });
            }
        }

        public void HideFinalChestRewardTipBox()
        {
            if (m_FinalChestPromptBox != null)
            {
                m_FinalChestPromptBox.Hide();
            }
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
                int propId = GetGeneratePropId();

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

        protected virtual int GetGeneratePropId()
        {
            int propId = 10101;
            IDataTable<DRGeneratePropWeights> dataTable = MergeManager.DataTable.GetDataTable<DRGeneratePropWeights>(MergeManager.Instance.GetMergeDataTableName());
            DRGeneratePropWeights data = dataTable.GetDataRow(MergeManager.PlayerData.GetCurrentMaxMergeStage() + 1);
            if (data != null)
            {
                float random = UnityEngine.Random.Range(0, 1f);
                for (int i = 0; i < data.GenerateWeights.Count; i++)
                {
                    random -= data.GenerateWeights[i];
                    if (random <= 0)
                    {
                        propId = 10101 + i;
                        break;
                    }
                }
            }

            return propId;
        }

        public override Vector3 GetStoragePropGeneratePos()
        {
            return m_StorageButton.transform.position;
        }

        public override void ShowClickSupplyFingerAnim()
        {
            ShowFingerAnim(m_SupplyButton.transform.position);
        }

        public override void ShowClickStorageFingerAnim()
        {
            ShowFingerAnim(m_StorageButton.transform.position);
        }

        private void OnMaxStageUpgrade(object sender, GameEventArgs e)
        {
            MaxStageUpgradeEventArgs ne = e as MaxStageUpgradeEventArgs;
            if (ne != null)
            {
                IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
                var propData = propDataTable.GetDataRow(ne.PropId);
                m_NewPropMergedBoard.Init(propData.AssetName, ne.PropLogic, () =>
                 {
                     m_NewPropMergedBoard.m_HideAction = () =>
                     {
                         ShowShiningFlyEffect(ne.PropPos, m_ProgressBoard.GetStagePos(MergeManager.PlayerData.GetCurrentMaxMergeStage()),0.5f, () =>
                         {
                             m_ProgressBoard.RefreshProgress(false);
                             GameManager.Task.AddDelayTriggerTask(1f, () =>
                             {
                                 int maxStage = MergeManager.PlayerData.GetCurrentMaxMergeStage();
                                 if (maxStage == 1)
                                     m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_Web);

                                 if (maxStage == 2)
                                     m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_KeepMerging);
                             });
                         }, true);
                     };
                     m_NewPropMergedBoard.Show();
                 }, this);

                GameManager.Firebase.RecordMessageByEvent("Merge_Task_Unlock", new Firebase.Analytics.Parameter("stage", ne.PropId % 10100 - 1));
            }
        }

        private void OnRefreshMergeEnergyBox(object sender, GameEventArgs e)
        {
            RefreshSupply(true, true);
        }

        public void MergeOfferComplete(object sender, GameEventArgs e)
        {
            RefreshSale();
        }

        public virtual void OnSupplyButtonClick()
        {
            GenerateProp();

            HideChestRewardTipBox();
            HideBubbleOperationBox();
            HideFingerAnim();
            MergeManager.Merge.ResetHintTimer();
        }

        protected override void OnCloseButtonClick()
        {
            if (MergeManager.Instance.CheckActivityHasStarted() && DateTime.Now > MergeManager.Instance.GetActivityEndTime())
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

        public virtual void OnStorageButtonClick()
        {
            WithdrawProp();
        }

        private void OnSaleButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeOfferMenu"));
        }

        public virtual void OnFinalRewardButtonClick()
        {
            IDataTable<DRMergeFinalChestReward> dataTable = MergeManager.DataTable.GetDataTable<DRMergeFinalChestReward>(MergeManager.Instance.GetMergeDataTableName());
            int time = MergeManager.PlayerData.GetFinalRewardTime();
            if (time <= 0 && GameManager.PlayerData.IsOwnTileID(MergeManager.Instance.TileId))
                time = 1;

            DRMergeFinalChestReward data = dataTable.GetDataRow(time + 1);
            if (data == null)
                data = dataTable.MaxIdDataRow;

            List<ItemData> rewardDatas = new List<ItemData>();
            if (data != null)
            {
                for (int i = 0; i < data.RewardPropIds.Count; i++)
                {
                    rewardDatas.Add(new ItemData(TotalItemData.FromInt(data.RewardPropIds[i]), data.RewardPropNums[i]));
                }
            }

            ShowFinalChestRewardTipBox(rewardDatas, m_FinalRewardButton.transform.position);
        }

        private void OnGetMoreButtonClick()
        {
            if (DateTime.Now < MergeManager.Instance.GetActivityEndTime())
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeGetBoxMenu"));
        }

        #region reciver

        public ReceiverType ReceiverType => ReceiverType.Common | ReceiverType.MergeEnergyBox;

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
