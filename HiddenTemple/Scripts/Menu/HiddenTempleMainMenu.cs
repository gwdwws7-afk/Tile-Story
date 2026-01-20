using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝主界面
    /// </summary>
    public sealed class HiddenTempleMainMenu : HiddenTempleBaseMenu, IItemFlyReceiver, ICustomOnEscapeBtnClicked
    {
        [SerializeField]
        private TempleArea m_TempleArea;
        [SerializeField]
        private ChestArea m_ChestArea;
        [SerializeField]
        private DigArea m_DigArea;
        [SerializeField]
        private Button m_TipButton, m_CloseButton, m_SaleButton;
        [SerializeField]
        private ClockBar m_ClockBar;
        [SerializeField]
        private Image m_GuideRoot;
        [SerializeField]
        private TextMeshProAdapterBox m_GuideTip;
        [SerializeField]
        private SkeletonAnimation m_Finger;

        private bool m_IsFinished;

        /// <summary>
        /// 神庙区域
        /// </summary>
        public TempleArea TempleArea => m_TempleArea;

        /// <summary>
        /// 宝箱区域
        /// </summary>
        public ChestArea ChestArea => m_ChestArea;

        /// <summary>
        /// 挖掘区域
        /// </summary>
        public DigArea DigArea => m_DigArea;

        /// <summary>
        /// 教程面板
        /// </summary>
        public GameObject GuidePanel => m_GuideRoot.gameObject;

        /// <summary>
        /// 活动是否结束
        /// </summary>
        public bool IsFinished => m_IsFinished;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            RewardManager.Instance.RegisterItemFlyReceiver(this);

            m_TipButton.SetBtnEvent(OnTipButtonClick);
            m_CloseButton.SetBtnEvent(OnCloseButtonClick);
            m_SaleButton.SetBtnEvent(OnSaleButtonClick);
            m_SaleButton.gameObject.SetActive(HiddenTempleManager.PlayerData.GetCurrentStage() <= HiddenTempleManager.PlayerData.GetMaxStage());

            int stage = HiddenTempleManager.PlayerData.GetCurrentStage();
            m_TempleArea.Initialize(stage, this);
            m_ChestArea.Initialize(stage, this);
            m_DigArea.Initialize(stage, this);

            m_GuideRoot.gameObject.SetActive(false);
            m_IsFinished = false;

            bool isFinishedAll = HiddenTempleManager.PlayerData.GetCurrentStage() > HiddenTempleManager.PlayerData.GetMaxStage() && HiddenTempleManager.Instance.GetUnclaimedChestNum() == 0;

            m_ClockBar.OnReset();
            if (!isFinishedAll && HiddenTempleManager.Instance.CheckActivityHasStarted() && DateTime.Now < GameManager.Activity.GetCurActivityEndTime()) 
            {
                m_ClockBar.CountdownOver += OnCountdownOver;
                m_ClockBar.StartCountdown(GameManager.Activity.GetCurActivityEndTime());
            }
            else
            {
                m_IsFinished = true;
                m_ClockBar.SetFinishState();
            }

            GameManager.Ads.HideBanner("HiddenTemple");
        }

        public override void OnReset()
        {
            m_ClockBar.OnReset();

            m_TempleArea.Release();
            m_ChestArea.Release();
            m_DigArea.Release();

            base.OnReset();
        }

        public override void OnRelease()
        {
            RewardManager.Instance.UnregisterItemFlyReceiver(this);

            OnReset();

            GameManager.Ads.ShowBanner("HiddenTemple");

            base.OnRelease();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_ClockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        public override bool CheckInitComplete()
        {
            return m_TempleArea.CheckInitComplete();
        }

        public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
        {
            base.OnShow(showSuccessAction, userData);

            if (m_IsFinished)
                return;

            if(GameManager.DataNode.GetData<bool>("HiddenTempleFirstTimeOpen", false))
            {
                GameManager.DataNode.SetData<bool>("HiddenTempleFirstTimeOpen", false);
                m_GuideRoot.gameObject.SetActive(true);

                GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                {
                    DigGrid targetGrid = m_DigArea.HighlightTargetGrid(1, 2);
                    if (targetGrid != null)
                    {
                        m_GuideRoot.DOFade(0.75f, 0.2f).onComplete = () =>
                        {
                            m_GuideTip.Refresh();
                            m_GuideTip.gameObject.SetActive(true);

                            m_Finger.transform.position = targetGrid.transform.position;
                            m_Finger.Initialize(false);
                            m_Finger.AnimationState.SetAnimation(0, "02", true);
                            m_Finger.gameObject.SetActive(true);
                        };
                    }
                    else
                    {
                        m_GuideRoot.gameObject.SetActive(false);
                    }
                });
            }
            else
            {
                if (!m_IsFinished) 
                {
                    if (!HiddenTempleManager.PlayerData.GetHasAutoShowedGiftPackMenu())
                    {
                        HiddenTempleManager.PlayerData.SetHasAutoShowedGiftPackMenu(true);
                        GameManager.UI.ShowUIForm("HiddenTempleGiftPackMenu", form =>
                        {
                            form.m_OnHideCompleteAction = () =>
                            {
                                m_ChestArea.ShowCurChestItemPromptBox();
                            };
                        });
                    }
                    else
                    {
                        m_ChestArea.ShowCurChestItemPromptBox();
                    }
                }
            }

            GameManager.Sound.PlayMusic("BGM_LostTemple");
        }

        public override void OnHide(Action hideSuccessAction = null, object userData = null)
        {
            base.OnHide(hideSuccessAction, userData);

            GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
        }

        public void HideSaleButton()
        {
            m_SaleButton.gameObject.SetActive(false);
        }

        private void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm("HiddenTempleHowToPlayMenu");

            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
        }

        private void OnCloseButtonClick()
        {
            //活动结束时，玩家有未使用的稿子同时并不是所有大门都完成了，退出时弹出二次确认面板
            if (m_IsFinished)
            {
                if (HiddenTempleManager.PlayerData.GetPickaxeNum() > 0 && HiddenTempleManager.PlayerData.GetCurrentStage() <= HiddenTempleManager.PlayerData.GetMaxStage()) 
                {
                    GameManager.UI.ShowUIForm("HiddenTempleReconfirmMenu");
                }
                else
                {
                    GameManager.UI.HideUIForm("HiddenTempleMainMenu");
                    HiddenTempleManager.Instance.EndActivity();
                }
            }
            else
            {
                GameManager.UI.HideUIForm(this);
            }

            GameManager.Process.EndProcess(ProcessType.ShowHiddenTempleStartProcess);
            GameManager.Process.EndProcess(ProcessType.ShowHiddenTempleLastChance);

            if (HiddenTempleManager.PlayerData.GetPickaxeNum() < HiddenTempleManager.PlayerData.CanAutoShowMenuPickaxeNum())
            {
                GameManager.DataNode.SetData<int>("HiddenTempleNextAutoShowBackTime", 0);
            }

            GameManager.Sound.PlayAudio(SoundType.SFX_UI_close.ToString());
        }

        private void OnSaleButtonClick()
        {
            GameManager.UI.ShowUIForm("HiddenTempleGiftPackMenu");

            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            m_IsFinished = true;
            m_ClockBar.SetFinishState();
        }

        public void OnEscapeBtnClicked()
        {
            OnCloseButtonClick();
        }

        #region IItemFlyReceiver

        public ReceiverType ReceiverType => ReceiverType.Pickaxe;

        public GameObject GetReceiverGameObject() => gameObject;

        public Vector3 GetItemTargetPos(TotalItemData type)
        {
            try
            {
                return m_DigArea.GetItemTargetPos();
            }
            catch
            {
                return Vector3.zero;
            }
        }

        public void OnFlyHit(TotalItemData type)
        {
        }

        public void OnFlyEnd(TotalItemData type)
        {
            m_DigArea.OnFlyHit();
        }

#endregion
    }
}
