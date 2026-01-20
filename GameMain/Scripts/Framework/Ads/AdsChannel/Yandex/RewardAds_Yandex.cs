using System;
using System.Collections;
using System.Collections.Generic;
using YandexMobileAds;
using YandexMobileAds.Base;

/// <summary>
/// Yandex������浥Ԫ
/// </summary>
public sealed class RewardAds_Yandex : AdUnitBase
{
    // ~RewardAds_Yandex()
    // {
    //     if (rewardedAd != null)
    //     {
    //         rewardedAd.Destroy();
    //         rewardedAd = null;
    //     }
    //     rewardedAdLoader = null;
    // }
    
    private RewardedAd rewardedAd;
    private RewardedAdLoader rewardedAdLoader;
    
    private object userData;
    private bool isUserEarnedReward;

    public override void LoadAd(string[] ids = null)
    {
        ids = ids == null ? base.AdsIds : ids;
        if (ids == null || ids.Length == 0)
        {
            Log.Error("Yandex RewardedAd adUnitId is invalid");
            return;
        }

        if (IsLoaded())
        {
            Log.Error("Yandex RewardedAd adUnitId already load:" + ids);
            return;
        }

        base.SetAdsIds(ids);
        base.LoadAd(ids);

        if (base.CurAdsId == null)
        {
            UnityEngine.Debug.Log($"Yandex RewardedAd Can Load Id is Null! Id :{base.CurAdsId}");
            return;
        }

        UnityEngine.Debug.Log($"Yandex RewardedAd Start Load...{base.CurAdsId}");
        GameManager.Ads.IsRequestingRewardAd = true;

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        if (rewardedAdLoader == null)
        {
            this.rewardedAdLoader = new RewardedAdLoader();
            this.rewardedAdLoader.OnAdLoaded += this.HandleRewardedAdLoaded;
            this.rewardedAdLoader.OnAdFailedToLoad += this.HandleRewardedAdFailedToLoad;
        }

        AdRequestConfiguration request = new AdRequestConfiguration.Builder(base.CurAdsId).Build();
        this.rewardedAdLoader.LoadAd(request);
    }

    public override bool IsLoaded()
    {
        return rewardedAd != null && IsLoad;
    }

    public override bool Show(object userData)
    {
        if (IsLoaded())
        {
            this.userData=userData;
            GameManager.CurState = "ShowRewardAds_Yandex";
            
            rewardedAd.OnAdFailedToShow -= this.HandleRewardedAdFailedToShow;
            rewardedAd.OnAdDismissed -= this.HandleRewardedAdClosed;
            rewardedAd.OnRewarded -= this.HandleUserEarnedReward;
            rewardedAd.OnAdFailedToShow += this.HandleRewardedAdFailedToShow;
            rewardedAd.OnAdDismissed += this.HandleRewardedAdClosed;
            rewardedAd.OnRewarded += this.HandleUserEarnedReward;
            rewardedAd.Show();
            
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        IsLoad = false;
        if(rewardedAd!=null)rewardedAd.Destroy();
        rewardedAd = null;
        isUserEarnedReward = false;
        base.Dispose();
    }

    private void HandleRewardedAdLoaded(object sender, RewardedAdLoadedEventArgs  arg)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            this.rewardedAd = arg.RewardedAd;
            IsLoad = true;
            GameManager.Ads.IsRequestingRewardAd = false;
            Log.Info("Yandex RewardedAd Loaded...");
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

    private void HandleUserEarnedReward(object sender,YandexMobileAds.Base.Reward e)
    {
        isUserEarnedReward = true;
    }
}
