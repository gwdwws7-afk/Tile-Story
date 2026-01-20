using DG.Tweening;
using GameFramework.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// ÒÅ¼£Ñ°±¦Èë¿Ú
    /// </summary>
    public sealed class HiddenTempleEntrance : EntranceUIForm, IItemFlyReceiver
    {
        [SerializeField]
        private Transform mainBody;
        [SerializeField]
        private CountdownTimer m_CountdownTimer;
        [SerializeField]
        private ParticleSystem m_ReachEffect;
        [SerializeField]
        private GameObject m_WarningSign, m_FinishedText, m_RewardWarning;
        [SerializeField]
        private TextMeshProUGUI m_PickaxeNumText, m_RewardNumText;
        [SerializeField]
        private TextMeshProUGUILocalize m_UnlockText;
        [SerializeField]
        private GameObject m_Banner, m_PreviewBanner;
        [SerializeField]
        private Image m_DoorImg;

        private int m_Period = 0;

        public Button ClickButton => entranceBtn;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            RewardManager.Instance.RegisterItemFlyReceiver(this);

            GameManager.Event.Subscribe(HiddenTempleStartEventArgs.EventId, OnActivityStart);
            GameManager.Event.Subscribe(HiddenTempleEndEventArgs.EventId, OnActivityEnd);
            GameManager.Event.Subscribe(PickaxeNumChangeEventArgs.EventId, OnPickaxeNumChange);
            GameManager.Event.Subscribe(ChestClaimEventArgs.EventId, OnChestClaim);

            m_Period = 0;
            UpdateBtnActiveAndTimer();

            if (GameManager.PlayerData.NowLevel < HiddenTempleManager.PlayerData.GetActivityUnlockLevel())
            {
                OnLocked();
            }
            else
            {
                OnUnlocked();
            }
        }

        public override void OnRelease()
        {
            RewardManager.Instance.UnregisterItemFlyReceiver(this);

            GameManager.Event.Unsubscribe(HiddenTempleStartEventArgs.EventId, OnActivityStart);
            GameManager.Event.Unsubscribe(HiddenTempleEndEventArgs.EventId, OnActivityEnd);
            GameManager.Event.Unsubscribe(PickaxeNumChangeEventArgs.EventId, OnPickaxeNumChange);
            GameManager.Event.Unsubscribe(ChestClaimEventArgs.EventId, OnChestClaim);

            base.OnRelease();
        }

        private void Update()
        {
            m_CountdownTimer.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
        }

        private void UpdateBtnActiveAndTimer()
        {
            if (HiddenTempleManager.Instance.CheckEntranceCanShow())
            {
                gameObject.SetActive(true);

                m_CountdownTimer.OnReset();
                if (HiddenTempleManager.Instance.CheckActivityHasStarted()) 
                {
                    m_Period = GameManager.Activity.GetCurPeriodID();
                    m_CountdownTimer.timeText.gameObject.SetActive(true);
                    m_FinishedText.SetActive(false);
                    m_CountdownTimer.CountdownOver += OnCountdownOver;
                    m_CountdownTimer.StartCountdown(GameManager.Activity.GetCurActivityEndTime());
                }
                else
                {
                    var scheduleData = GameManager.Activity.CheckIsInSchedule(HiddenTempleManager.Instance.ActivityID, 0);
                    if (scheduleData != null)
                    {
                        m_CountdownTimer.timeText.gameObject.SetActive(true);
                        m_FinishedText.SetActive(false);
                        m_CountdownTimer.CountdownOver += OnCountdownOver;
                        m_CountdownTimer.StartCountdown(scheduleData.EndTimeDT);
                    }
                    else
                    {
                        m_CountdownTimer.timeText.gameObject.SetActive(false);
                        m_FinishedText.SetActive(false);
                    }
                }

                RefreshWarning();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void RefreshWarning()
        {
            if (m_Period != 0)
            {
                if (HiddenTempleManager.Instance.GetUnclaimedChestNum() > 0)
                {
                    int unclaimedChestNum = HiddenTempleManager.Instance.GetUnclaimedChestNum();
                    if (unclaimedChestNum > 0)
                    {
                        m_RewardNumText.text = unclaimedChestNum.ToString();
                    }
                    m_WarningSign.SetActive(false);
                    m_RewardWarning.SetActive(unclaimedChestNum > 0);
                }
                else if (HiddenTempleManager.PlayerData.GetCurrentStage() <= HiddenTempleManager.PlayerData.GetMaxStage() && HiddenTempleManager.PlayerData.GetPickaxeNum() > 0)
                {
                    int num = HiddenTempleManager.PlayerData.GetPickaxeNum();
                    m_PickaxeNumText.text = num.ToString();
                    m_WarningSign.SetActive(true);
                    m_RewardWarning.SetActive(false);
                }
                else
                {
                    m_WarningSign.SetActive(false);
                    m_RewardWarning.SetActive(false);
                }
            }
            else
            {
                m_WarningSign.SetActive(false);
                m_RewardWarning.SetActive(false);
            }
        }

        public void ShowGetFlyReward()
        {
            int num = HiddenTempleManager.PlayerData.GetPickaxeLevelCollectNum();
            EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent("TotalItemAtlas[Pickaxe]", num, 21, new Vector3(250f, 0), Vector3.zero, gameObject,
                () =>
                {
                    HiddenTempleManager.PlayerData.AddPickaxeNum(num);
                    HiddenTempleManager.PlayerData.ClearPickaxeLevelCollectNum();
                }, () =>
                {
                    PunchMainBody(() =>
                    {
                        EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
                        RefreshWarning();
                    });
                }, false);
        }

        private void PunchMainBody(Action callback = null)
        {
            mainBody.DOScale(Vector3.one * 0.8f, 0.15f).onComplete = () =>
              {
                  mainBody.DOScale(Vector3.one, 0.15f);

                  callback?.Invoke();
              };

            if (m_ReachEffect != null)
            {
                m_ReachEffect.Play();
            }
        }

        public override void OnButtonClick()
        {
            if(GameManager.PlayerData.NowLevel < HiddenTempleManager.PlayerData.GetActivityUnlockLevel())
            {
                GameManager.UI.ShowUIForm("HiddenTempleStartMenu", userData: true);
                return;
            }

            //if (GameManager.PlayerData.NowLevel >= HiddenTempleManager.PlayerData.GetActivityPreviewLevel()) 
            //{
            //    if (GameManager.PlayerData.NowLevel < HiddenTempleManager.PlayerData.GetActivityUnlockLevel())
            //        GameManager.UI.ShowUIForm<HiddenTempleStartMenu>(userData: true);
            //    else
            //        GameManager.UI.ShowUIForm<HiddenTempleMainMenu>();
            //}

            if (GameManager.PlayerData.NowLevel >= HiddenTempleManager.PlayerData.GetActivityUnlockLevel())
            {
                entranceBtn.interactable = false;
                GameManager.UI.ShowUIForm("HiddenTempleMainMenu",f =>
                {
                    entranceBtn.interactable = true;
                }, () =>
                {
                    entranceBtn.interactable = true;
                });
            }

            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
        }

        public override void OnLocked()
        {
            m_UnlockText.SetParameterValue("level", HiddenTempleManager.PlayerData.GetActivityUnlockLevel().ToString());
            m_Banner.SetActive(false);
            m_PreviewBanner.SetActive(true);

            m_DoorImg.color = Color.gray;

            base.OnLocked();
        }

        public override void OnUnlocked()
        {
            m_Banner.SetActive(true);
            m_PreviewBanner.SetActive(false);

            m_DoorImg.color = Color.white;

            base.OnUnlocked();
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            gameObject.SetActive(false);

            HiddenTempleManager.Instance.ActivityEndProcess();
        }

        private void OnActivityStart(object sender, GameEventArgs e)
        {
            UpdateBtnActiveAndTimer();
        }

        private void OnActivityEnd(object sender, GameEventArgs e)
        {
            HiddenTempleEndEventArgs ne = e as HiddenTempleEndEventArgs;

            if (ne != null)
            {
                if (ne.PeriodId != m_Period)
                    UpdateBtnActiveAndTimer();
                else
                    gameObject.SetActive(false);
            }
        }

        private void OnPickaxeNumChange(object sender, GameEventArgs e)
        {
            RefreshWarning();
        }

        private void OnChestClaim(object sender, GameEventArgs e)
        {
            RefreshWarning();
        }

#region IItemFlyReceiver

        public ReceiverType ReceiverType => ReceiverType.Pickaxe;

        public GameObject GetReceiverGameObject() => gameObject;

        public Vector3 GetItemTargetPos(TotalItemData type)
        {
            try
            {
                return transform.position;
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
            PunchMainBody(() =>
            {
                RefreshWarning();
            });
        }

#endregion
    }
}
