using System;
using YandexMobileAds;
using YandexMobileAds.Base;
using Mycom.Target.Unity.Ads;

public sealed class InterstitialAds_YandexMyTargetMix : AdUnitBase
{
    private YandexMobileAds.Interstitial interstitialAd_Yandex;
    private InterstitialAdLoader interstitialAdLoader;

    private volatile Mycom.Target.Unity.Ads.InterstitialAd interstitialAd_MyTarget;

    public override bool IsLoaded()
    {
        return (interstitialAd_Yandex != null || interstitialAd_MyTarget != null) && IsLoad;
    }

    public override void LoadAd(string[] ids=null)
    {
        ids = ids == null ? base.AdsIds : ids;
        if (ids == null || ids.Length == 0)
        {
            Log.Error("YandexMyTargetMix InterstitialAd adUnitId is invalid!");
            return;
        }

        if (IsLoaded())
        {
            Log.Error("YandexMyTargetMix InterstitialAd adUnitId already load:" + ids);
            return;
        }

        base.SetAdsIds(ids);
        base.LoadAd(ids);

        if (base.CurAdsId == null)
        {
            Log.Debug($"YandexMyTargetMix InterstitialAd Can Load Id is Null! Id :{base.CurAdsId}");
            return;
        }

        Log.Debug($"YandexMyTargetMix InterstitialAd Start Load...{base.CurAdsId}");
        GameManager.Ads.IsRequestingInterstitialAd = true;

        if (interstitialAd_Yandex != null)
        {
            interstitialAd_Yandex.Destroy();
            interstitialAd_Yandex = null;
        }

        if (interstitialAd_MyTarget != null)
        {
            interstitialAd_MyTarget.AdLoadCompleted -= HandleInterstitialAdLoadCompleted;
            interstitialAd_MyTarget.AdLoadFailed -= HandleInterstitialAdLoadFailed;
            interstitialAd_MyTarget.AdDisplayed -= HandleInterstitialAdDisplayed;
            interstitialAd_MyTarget.AdDismissed -= HandleInterstitialAdDismissed;

            interstitialAd_MyTarget.Dispose();
            interstitialAd_MyTarget = null;
        }

        bool isYandex = base.CurAdsId.Contains("R-M-");
        if (isYandex)
        {
            if (interstitialAdLoader == null)
            {
                this.interstitialAdLoader = new InterstitialAdLoader();
                this.interstitialAdLoader.OnAdLoaded += this.HandleInterstitialAdLoaded;
                this.interstitialAdLoader.OnAdFailedToLoad += this.HandleInterstitialAdFailedToLoad;
            }

            AdRequestConfiguration request = new AdRequestConfiguration.Builder(base.CurAdsId).Build();
            this.interstitialAdLoader.LoadAd(request);
        }
        else
        {
            interstitialAd_MyTarget = new InterstitialAd(uint.Parse(base.CurAdsId));

            interstitialAd_MyTarget.AdLoadCompleted += HandleInterstitialAdLoadCompleted;
            interstitialAd_MyTarget.AdLoadFailed += HandleInterstitialAdLoadFailed;
            interstitialAd_MyTarget.AdDisplayed += HandleInterstitialAdDisplayed;
            interstitialAd_MyTarget.AdDismissed += HandleInterstitialAdDismissed;

            interstitialAd_MyTarget.Load();
        }
    }

    public override bool Show(object userData)
    {
        if (IsLoaded())
        {
            GameManager.CurState = "ShowInterstitialAds_Yandex";

            if (interstitialAd_Yandex != null)
            {
                this.interstitialAd_Yandex.OnAdFailedToShow -= this.HandleInterstitialAdFailedToShow;
                this.interstitialAd_Yandex.OnAdDismissed -= this.HandleInterstitialAdClosed;
                this.interstitialAd_Yandex.OnAdFailedToShow += this.HandleInterstitialAdFailedToShow;
                this.interstitialAd_Yandex.OnAdDismissed += this.HandleInterstitialAdClosed;
                interstitialAd_Yandex.Show();
            }
            else if (interstitialAd_MyTarget != null) 
            {
                interstitialAd_MyTarget.Show();
            }
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        IsLoad = false;
        if(interstitialAd_Yandex!=null)interstitialAd_Yandex.Destroy();
        interstitialAd_Yandex = null;

        if (interstitialAd_MyTarget != null)
        {
            interstitialAd_MyTarget.AdLoadCompleted -= HandleInterstitialAdLoadCompleted;
            interstitialAd_MyTarget.AdLoadFailed -= HandleInterstitialAdLoadFailed;
            interstitialAd_MyTarget.AdDisplayed -= HandleInterstitialAdDisplayed;
            interstitialAd_MyTarget.AdDismissed -= HandleInterstitialAdDismissed;

            interstitialAd_MyTarget.Dispose();
            interstitialAd_MyTarget = null;
        }
        base.Dispose();
    }

    private void HandleInterstitialAdLoaded(object sender, InterstitialAdLoadedEventArgs args)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            IsLoad = true;
            this.interstitialAd_Yandex = args.Interstitial;
            AdsIdModel.LoadSuccess();
            Log.Info("Yandex InterstitialAd Loaded...");
            GameManager.Ads.IsRequestingInterstitialAd = false;
        }));
    }

    private void HandleInterstitialAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            UnityEngine.Debug.Log($"Yandex InterstitialAd FailedToLoad...{base.CurAdsId}...Error:{e.Message}");
            GameManager.Ads.IsRequestingInterstitialAd = false;

            Dispose();
            AdsIdModel.LoadFail();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleInterstitialAdFailedToShow(object sender, AdFailureEventArgs  e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Error("Yandex InterstitialAd Failed To Show..." + e.Message);

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleInterstitialAdClosed(object sender, EventArgs  e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Info("Yandex InterstitialAd Closed...");

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    #region MyTarget

    private void HandleInterstitialAdDismissed(object sender, EventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Info("MyTarget InterstitialAd Closed...");

            Dispose();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleInterstitialAdDisplayed(object sender, EventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            Log.Info("MyTarget InterstitialAd Displayed...");
        }, 0));
    }

    private void HandleInterstitialAdLoadFailed(object sender, ErrorEventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            UnityEngine.Debug.Log($"MyTarget InterstitialAd FailedToLoad...{base.CurAdsId}...Error:{e.Message}");
            GameManager.Ads.IsRequestingInterstitialAd = false;

            Dispose();
            AdsIdModel.LoadFail();

            eventQueue.Enqueue(AdsEvent.Create(() =>
            {
                LoadAd();
            }, AdsDelayLoadTime));
        }));
    }

    private void HandleInterstitialAdLoadCompleted(object sender, EventArgs e)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            IsLoad = true;
            AdsIdModel.LoadSuccess();
            Log.Info("MyTarget InterstitialAd Loaded...");
            GameManager.Ads.IsRequestingInterstitialAd = false;
        }));
    }

    #endregion
}
