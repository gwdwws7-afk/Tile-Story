using System;
using YandexMobileAds;
using YandexMobileAds.Base;

/// <summary>
/// Yandex������浥Ԫ
/// </summary>
public sealed class InterstitialAds_Yandex : AdUnitBase
{
    // ~InterstitialAds_Yandex()
    // {
    //     if (interstitialAd != null)
    //     {
    //         interstitialAd.Destroy();
    //         interstitialAd = null;
    //     }
    //     interstitialAdLoader = null;
    // }

    
    private YandexMobileAds.Interstitial interstitialAd;
    private InterstitialAdLoader interstitialAdLoader;

    public override bool IsLoaded()
    {
        return interstitialAd != null && IsLoad;
    }

    public override void LoadAd(string[] ids=null)
    {
        ids = ids == null ? base.AdsIds : ids;
        if (ids == null || ids.Length == 0)
        {
            Log.Error("Yandex InterstitialAd adUnitId is invalid!");
            return;
        }

        if (IsLoaded())
        {
            Log.Error("Yandex InterstitialAd adUnitId already load:" + ids);
            return;
        }

        base.SetAdsIds(ids);
        base.LoadAd(ids);

        if (base.CurAdsId == null)
        {
            Log.Debug($"Yandex InterstitialAd Can Load Id is Null! Id :{base.CurAdsId}");
            return;
        }

        Log.Debug($"Yandex InterstitialAd Start Load...{base.CurAdsId}");
        GameManager.Ads.IsRequestingInterstitialAd = true;

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        if (interstitialAdLoader == null)
        {
            this.interstitialAdLoader = new InterstitialAdLoader();
            this.interstitialAdLoader.OnAdLoaded += this.HandleInterstitialAdLoaded;
            this.interstitialAdLoader.OnAdFailedToLoad += this.HandleInterstitialAdFailedToLoad;
        }

        AdRequestConfiguration request = new AdRequestConfiguration.Builder(base.CurAdsId).Build();
        this.interstitialAdLoader.LoadAd(request);
    }

    public override bool Show(object userData)
    {
        if (IsLoaded())
        {
            GameManager.CurState = "ShowInterstitialAds_Yandex";
            
            this.interstitialAd.OnAdFailedToShow -= this.HandleInterstitialAdFailedToShow;
            this.interstitialAd.OnAdDismissed -= this.HandleInterstitialAdClosed;
            this.interstitialAd.OnAdFailedToShow += this.HandleInterstitialAdFailedToShow;
            this.interstitialAd.OnAdDismissed += this.HandleInterstitialAdClosed;
            interstitialAd.Show();
            
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        IsLoad = false;
        if(interstitialAd!=null)interstitialAd.Destroy();
        interstitialAd = null;
        base.Dispose();
    }

    private void HandleInterstitialAdLoaded(object sender, InterstitialAdLoadedEventArgs   args)
    {
        eventQueue.Enqueue(AdsEvent.Create(() =>
        {
            IsLoad = true;
            GameManager.Ads.IsRequestingInterstitialAd = false;
            this.interstitialAd = args.Interstitial;
            AdsIdModel.LoadSuccess();
            Log.Info("Yandex InterstitialAd Loaded...");
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
}
