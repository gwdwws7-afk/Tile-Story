using GameFramework.Event;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeGetBoxMenu : PopupMenuForm
    {
        public Transform m_Banner1, m_Banner2, m_Banner3;
        public DelayButton m_Button1, m_Button2, m_Button3, m_CloseButton, m_TipButton;
        public TextMeshProUGUILocalize m_AdsText;
        public Material m_GreyMaterial;
        public CountdownTimer m_Timer;

        public virtual int RewardAdGetBoxNum => 3;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
            GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

            m_Button1.OnInit(OnButton1Click);
            m_Button2.OnInit(OnButton2Click);
            m_Button3.OnInit(OnButton3Click);
            m_CloseButton.OnInit(OnCloseButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);

            m_Button3.interactable = true;
            RefreshAdsButton();

            int curLevel = MergeManager.PlayerData.GetCurMergeOfferLevel();
            IDataTable<DRMergeOffer> dataTable = MergeManager.DataTable.GetDataTable<DRMergeOffer>(MergeManager.Instance.GetMergeDataTableName());
            RefreshBanner(curLevel <= dataTable.Count);
        }

        public override void OnReset()
        {
            GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoaded);
            GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarned);

            base.OnReset();
        }

        public override void OnRelease()
        {
            OnReset();

            base.OnRelease();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_Timer.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        protected virtual void RefreshBanner(bool showMergeOfferBanner)
        {
            m_Banner2.gameObject.SetActive(showMergeOfferBanner);
            if (showMergeOfferBanner)
            {
                m_Banner1.localPosition = new Vector3(0, 48, 0);
                m_Banner3.localPosition = new Vector3(0, -369, 0);
            }
            else
            {
                m_Banner1.localPosition = new Vector3(0, -11, 0);
                m_Banner3.localPosition = new Vector3(0, -276, 0);
            }
        }

        private void RefreshAdsButton()
        {
            DateTime readyTime = MergeManager.PlayerData.GetGetBoxNextAdsReadyTime();
            if (readyTime > DateTime.Now)
            {
                m_Timer.OnReset();
                m_Timer.CountdownOver += OnCountdownOver;
                m_Timer.StartCountdown(readyTime);

                var images = m_Button3.GetComponentsInChildren<Image>();
                foreach (Image img in images)
                {
                    img.material = m_GreyMaterial;
                }

                m_AdsText.gameObject.SetActive(false);
                m_Timer.timeText.gameObject.SetActive(true);
            }
            else
            {
                m_Timer.OnReset();
                if (GameManager.Ads.CheckRewardedAdIsLoaded())
                {
                    var images = m_Button3.GetComponentsInChildren<Image>();
                    foreach (Image img in images)
                    {
                        img.material = null;
                    }
                    m_AdsText.SetMaterialPreset(MaterialPresetName.Btn_Green);
                }
                else
                {
                    var images = m_Button3.GetComponentsInChildren<Image>();
                    foreach (Image img in images)
                    {
                        img.material = m_GreyMaterial;
                    }
                    m_AdsText.SetMaterialPreset(MaterialPresetName.Btn_Grey);
                }

                m_AdsText.gameObject.SetActive(true);
                m_Timer.timeText.gameObject.SetActive(false);
            }
        }

        private void OnButton1Click()
        {
            GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
            GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
        }

        private void OnButton2Click()
        {
            GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeOfferMenu"));
        }

        private void OnButton3Click()
        {
            if (MergeManager.PlayerData.GetGetBoxNextAdsReadyTime() > DateTime.Now)
            {
                GameManager.UI.ShowWeakHint("Common.Ad is still loading...");
                return;
            }

            if (GameManager.Ads.CheckRewardedAdIsLoaded())
            {
                m_Button3.interactable = false;
                GameManager.Ads.ShowRewardedAd("MergeGetBox");
            }
            else
            {
                GameManager.UI.ShowWeakHint("Common.Ad is still loading...");
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

        public void OnRewardAdLoaded(object sender, GameEventArgs e)
        {
            RefreshAdsButton();
        }

        private void OnRewardAdEarned(object sender, GameEventArgs e)
        {
            RewardAdEarnedRewardEventArgs ne = (RewardAdEarnedRewardEventArgs)e;
            if (ne.UserData.ToString() != "MergeGetBox")
            {
                return;
            }

            m_Button3.interactable = true;

            bool isUserEarnedReward = true;
#if UNITY_ANDROID && !UNITY_EDITOR && !AmazonStore
            isUserEarnedReward = ne.EarnedReward;
#endif

            if (isUserEarnedReward)
            {
                MergeManager.PlayerData.SetGetBoxNextAdsReadyTime(DateTime.Now.AddMinutes(5));

                GameManager.UI.HideUIForm(this);
                RewardManager.Instance.AddNeedGetReward(TotalItemData.MergeEnergyBox, RewardAdGetBoxNum);
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
                {
                    GameManager.Event.Fire(this, RefreshMergeEnergyBoxEventArgs.Create());
                });

                GameManager.Firebase.RecordMessageByEvent("Merge_Watch_Ads_Get_Box");
            }
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            RefreshAdsButton();
        }
    }
}
