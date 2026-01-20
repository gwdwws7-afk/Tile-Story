using DG.Tweening;
using GameFramework.Event;
using Spine.Unity;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeEntrance : EntranceUIForm, IItemFlyReceiver
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
        private TextMeshProUGUI m_TipNumText, m_RewardNumText;
        [SerializeField]
        private TextMeshProUGUILocalize m_UnlockText;
        [SerializeField]
        private GameObject m_Banner, m_PreviewBanner, m_LockIcon;

        [SerializeField] private GameObject m_StPatricks, m_LoveGiftBattle, m_DigTreasure, m_Halloween, m_Christmas;

        private int period = 0;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            RewardManager.Instance.RegisterItemFlyReceiver(this);

            GameManager.Event.Subscribe(MergeStartEventArgs.EventId, OnActivityStart);
            GameManager.Event.Subscribe(MergeEndEventArgs.EventId, OnActivityEnd);
            GameManager.Event.Subscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);

            period = 0;
            UpdateBtnActiveAndTimer();

            if (GameManager.PlayerData.NowLevel < MergeManager.PlayerData.GetActivityUnlockLevel())
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

            GameManager.Event.Unsubscribe(MergeStartEventArgs.EventId, OnActivityStart);
            GameManager.Event.Unsubscribe(MergeEndEventArgs.EventId, OnActivityEnd);
            GameManager.Event.Unsubscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);

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
            if (MergeManager.Instance.CheckEntranceCanShow())
            {
                gameObject.SetActive(true);

                m_CountdownTimer.OnReset();
                if (MergeManager.Instance.CheckActivityHasStarted())
                {
                    period = MergeManager.PlayerData.GetSavedPeriod()/* GameManager.Activity.GetCurPeriodID()*/;
                    m_CountdownTimer.timeText.gameObject.SetActive(true);
                    m_FinishedText.SetActive(false);
                    m_CountdownTimer.CountdownOver += OnCountdownOver;
                    m_CountdownTimer.StartCountdown(MergeManager.Instance.EndTime/*GameManager.Activity.GetCurActivityEndTime()*/);
                }
                else
                {
                    //var scheduleData = GameManager.Activity.CheckIsInSchedule(MergeManager.Instance.ActivityID, 0);
                    //if (scheduleData != null)
                    //{
                    //    m_CountdownTimer.timeText.gameObject.SetActive(true);
                    //    m_FinishedText.SetActive(false);
                    //    m_CountdownTimer.CountdownOver += OnCountdownOver;
                    //    m_CountdownTimer.StartCountdown(scheduleData.EndTimeDT);
                    //}
                    //else
                    //{
                    //    m_CountdownTimer.timeText.gameObject.SetActive(false);
                    //    m_FinishedText.SetActive(false);
                    //}
                    period = MergeManager.PlayerData.GetSavedPeriod()/* GameManager.Activity.GetCurPeriodID()*/;
                    m_CountdownTimer.timeText.gameObject.SetActive(true);
                    m_FinishedText.SetActive(false);
                    m_CountdownTimer.CountdownOver += OnCountdownOver;
                    m_CountdownTimer.StartCountdown(MergeManager.Instance.EndTime/*GameManager.Activity.GetCurActivityEndTime()*/);
                }

                RefreshWarning();

                m_StPatricks.SetActive(MergeManager.Instance.Theme == MergeTheme.StPatricks);
                m_LoveGiftBattle.SetActive(MergeManager.Instance.Theme == MergeTheme.LoveGiftBattle);
                m_DigTreasure.SetActive(MergeManager.Instance.Theme == MergeTheme.DigTreasure);
                m_Halloween.SetActive(MergeManager.Instance.Theme == MergeTheme.Halloween);
                m_Christmas.SetActive(MergeManager.Instance.Theme == MergeTheme.Christmas);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void RefreshWarning()
        {
            if (period != 0)
            {
                int num = MergeManager.PlayerData.GetMergeEnergyBoxNum();
                if (num > 0)
                {
                    m_TipNumText.text = num.ToString();
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
            int num = MergeManager.PlayerData.GetMergeEnergyBoxLevelCollectNum();
            string spriteName = $"TotalItemAtlas[{MergeManager.Instance.GetMergeEnergyBoxName()}]";
            EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent(spriteName, num, 21, new Vector3(250f, 0), Vector3.zero, gameObject,
                () =>
                {
                    MergeManager.PlayerData.AddMergeEnergyBoxNum(num);
                    MergeManager.PlayerData.ClearMergeEnergyBoxLevelCollectNum();
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
            if (GameManager.PlayerData.NowLevel >= MergeManager.PlayerData.GetActivityUnlockLevel())
            {
                entranceBtn.interactable = false;
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"), f =>
                {
                    entranceBtn.interactable = true;
                }, () =>
                {
                    entranceBtn.interactable = true;
                });
            }
            else
            {
                entranceBtn.interactable = false;
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeStartMenu"), f =>
                {
                    entranceBtn.interactable = true;
                }, () =>
                {
                    m_LockIcon.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
                    ShowUnlockPromptBox(MergeManager.PlayerData.GetActivityUnlockLevel());
                    entranceBtn.interactable = true;
                });
            }

            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
        }

        public override void OnLocked()
        {
            if (IsLocked)
                return;

            m_UnlockText.SetParameterValue("level", MergeManager.PlayerData.GetActivityUnlockLevel().ToString());
            m_Banner.SetActive(false);
            m_PreviewBanner.SetActive(true);

            var imgs = mainBody.GetComponentsInChildren<Image>(true);
            foreach (var img in imgs)
            {
                img.color = Color.gray;
            }

            var spines = mainBody.GetComponentsInChildren<SkeletonGraphic>(true);
            foreach (var spine in spines)
            {
                spine.color = Color.gray;
                spine.freeze = true;
            }

            base.OnLocked();
        }

        public override void OnUnlocked()
        {
            if (!IsLocked)
                return;

            m_Banner.SetActive(true);
            m_PreviewBanner.SetActive(false);

            var imgs = mainBody.GetComponentsInChildren<Image>(true);
            foreach (var img in imgs)
            {
                img.color = Color.white;
            }

            var spines = mainBody.GetComponentsInChildren<SkeletonGraphic>(true);
            foreach (var spine in spines)
            {
                spine.color = Color.white;
                spine.freeze = false;
            }

            base.OnUnlocked();
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            gameObject.SetActive(false);

            MergeManager.Instance.ActivityEndProcess();
        }

        private void OnActivityStart(object sender, GameEventArgs e)
        {
            UpdateBtnActiveAndTimer();
        }

        private void OnActivityEnd(object sender, GameEventArgs e)
        {
            MergeEndEventArgs ne = e as MergeEndEventArgs;

            if (ne != null)
            {
                if (ne.PeriodId != period)
                    UpdateBtnActiveAndTimer();
                else
                    gameObject.SetActive(false);
            }
        }

        private void OnRefreshMergeEnergyBox(object sender, GameEventArgs e)
        {
            RefreshWarning();
        }

        #region IItemFlyReceiver

        public ReceiverType ReceiverType => ReceiverType.MergeEnergyBox;

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
