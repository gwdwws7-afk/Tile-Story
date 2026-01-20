using System;
using System.Collections;
using System.Collections.Generic;
using YandexMobileAds;
using YandexMobileAds.Base;
using Mycom.Target.Unity.Ads;

public sealed class RewardAds_YandexMyTargetMix : AdUnitBase
{
    private YandexMobileAds.RewardedAd rewardedAd_Yandex;
    private RewardedAdLoader rewardedAdLoader;

    private volatile Mycom.Target.Unity.Ads.RewardedAd rewardedAd_MyTarget;

    private object userData;
    private bool isUserEarnedReward;

    public override void LoadAd(string[] ids = null)
    {
        ids = ids == null ? base.AdsIds : ids;
        if (ids == null || ids.Length == 0)
        {
            Log.Error("YandexMyTargetMix RewardedAd adUnitId is invalid");
            return;
        }

        if (IsLoaded())
        {
            Log.Error("YandexMyTargetMix RewardedAd adUnitId already load:" + ids);
            return;
        }

        base.SetAdsIds(ids);
        base.LoadAd(ids);

        if (base.CurAdsId == null)
        {
            UnityEngine.Debug.Log($"YandexMyTargetMix RewardedAd Can Load Id is Null! Id :{base.CurAdsId}");
            return;
        }

        UnityEngine.Debug.Log($"YandexMyTargetMix RewardedAd Start Load...{base.CurAdsId}");
        GameManager.Ads.IsRequestingRewardAd = true;

        if (rewardedAd_Yandex != null)
        {
            rewardedAd_Yandex.Destroy();
            rewardedAd_Yandex = null;
        }

        if (rewardedAd_MyTarget != null)
        {
            rewardedAd_MyTarget.AdLoadCompleted -= HandleRewardAdLoadCompleted;
            rewardedAd_MyTarget.AdLoadFailed -= HandleRewardAdLoadFailed;
            rewardedAd_MyTarget.AdDisplayed -= HandleRewardAdDisplayed;
            rewardedAd_MyTarget.AdDismissed -= HandleRewardAdDismissed;

            rewardedAd_MyTarget.Dispose();
            rewardedAd_MyTarget = null;
        }

        bool isYandex = base.CurAdsId.Contains("R-M-");
        if (isYandex)
        {
            if (rewardedAdLoader == null)
            {
                this.rewardedAdLoader = new RewardedAdLoader();
                this.rewardedAdLoader.OnAdLoaded += this.HandleRewardedAdLoaded;
                this.rewardedAdLoader.OnAdFailedToLoad += this.HandleRewardedAdFailedToLoad;
            }

            AdRequestConfiguration request = new AdRequestConfiguration.Builder(base.CurAdsId).Build();
            this.rewardedAdLoader.LoadAd(request);
        }
        else
        {
            rewardedAd_MyTarget = new Mycom.Target.Unity.Ads.RewardedAd(uint.Parse(base.CurAdsId));

            rewardedAd_MyTarget.AdLoadCompleted += HandleRewardAdLoadCompleted;
            rewardedAd_MyTarget.AdLoadFailed += HandleRewardAdLoadFailed;
            rewardedAd_MyTarget.AdDisplayed += HandleRewardAdDisplayed;
            rewardedAd_MyTarget.AdDismissed += HandleRewardAdDismissed;

            rewardedAd_MyTarget.Load();
        }
    }

    public override bool IsLoaded()
    {
        return (rewardedAd_Yandex != null || rewardedAd_MyTarget != null) && IsLoad;
    }

    public override bool Show(object userData)
    {
        if (IsLoaded())
        {
            this.userData=userData;
            GameManager.CurState = "ShowRewardAds_Yandex";

            if (rewardedAd_Yandex != null)
            {
                rewardedAd_Yandex.OnAdFailedToShow -= this.HandleRewardedAdFailedToShow;
                rewardedAd_Yandex.OnAdDismissed -= this.HandleRewardedAdClosed;
                rewardedAd_Yandex.OnRewarded -= this.HandleUserEarnedReward;
                rewardedAd_Yandex.OnAdFailedToShow += this.HandleRewardedAdFailedToShow;
                rewardedAd_Yandex.OnAdDismissed += this.HandleRewardedAdClosed;
                rewardedAd_Yandex.OnRewarded += this.HandleUserEarnedReward;
                rewardedAd_Yandex.Show();
            }
            else if (rewardedAd_MyTarget != null) 
            {
                rewardedAd_MyTarget.Show();
            }
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        IsLoad = false;
        if(rewardedAd_Yandex!=null)rewardedAd_Yandex.Destroy();
        rewardedAd_Yandex = null;

        if (rewardedAd_MyTarget != null)
        {
            rewardedAd_MyTarget.AdLoadCompleted -= HandleRewardAdLoadCompleted;
            rewardedAd_MyTarget.AdLoadFailed -= HandleRewardAdLoadFailed;
            rewardedAd_MyTarget.AdDisplayed -= HandleRewardAdDisplayed;
            rewardedAd_MyTarget.AdDismissed -= HandleRewardAdDismissed;

            rewardedAd_MyTarget.Dispose();
            rewardedAd_MyTarget = null;
        }

        isUserEarnedReward = false;
        //userData = null;
        base.Dispose();
    }

    private void HandleRewardedAdLoaded(object sender, RewardedAdLoadedEventArgs  arg)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            this.rewardedAd_Yandex = arg.RewardedAd;
            IsLoad = true;
            Log.Info("Yandex RewardedAd Loaded...");
            GameManager.Ads.IsRequestingRewardAd = false;
            AdsIdModel.LoadSuccess();
            GameManager.Event.Fire(this, RewardAdLoadCompleteEventArgs.Create());
        }));
    }

    private void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs  e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            UnityEngine.Debug.Log($"Yandex RewardedAd Failed To Load,Reason {base.CurAdsId}...Error:{e.Message}");
            GameManager.Ads.IsRequestingRewardAd = false;
            Dispose();

            AdsIdModel.LoadFail();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleRewardedAdFailedToShow(object sender, AdFailureEventArgs  e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Error("YandexRewardedAd Failed To Show,Reason ..." + e.Message);

            if (userData != null)
                GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, userData));
            isUserEarnedReward = false;
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Reward_Ads_Complete);

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleRewardedAdClosed(object sender, EventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            if (userData != null)
                GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, userData));

            if (isUserEarnedReward)
            {
                isUserEarnedReward = false;
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Reward_Ads_Complete);
            }
            else
            {
                Log.Warning("User not earned reward!");
            }

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleUserEarnedReward(object sender, YandexMobileAds.Base.Reward e)
    {
        isUserEarnedReward = true;
    }

    #region MyTarget

    private void HandleRewardAdDismissed(object sender, EventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            if (userData != null)
                GameManager.Event.Fire(this, RewardAdEarnedRewardEventArgs.Create(true, userData));

            if (isUserEarnedReward)
            {
                isUserEarnedReward = false;
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Reward_Ads_Complete);
            }
            else
            {
                Log.Warning("User not earned reward!");
            }

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleRewardAdDisplayed(object sender, EventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Info("MyTarget RewardAd Displayed...");
        }, 0));
    }

    private void HandleRewardAdLoadFailed(object sender, ErrorEventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            UnityEngine.Debug.Log($"MyTarget RewardedAd Failed To Load,Reason {base.CurAdsId}...Error:{e.Message}");
            GameManager.Ads.IsRequestingRewardAd = false;
            Dispose();

            AdsIdModel.LoadFail();
            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleRewardAdLoadCompleted(object sender, EventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            IsLoad = true;
            Log.Info("MyTarget RewardedAd Loaded...");
            GameManager.Ads.IsRequestingRewardAd = false;
            AdsIdModel.LoadSuccess();
            GameManager.Event.Fire(this, RewardAdLoadCompleteEventArgs.Create());
        }));
    }

    #endregion
}
