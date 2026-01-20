using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager_YandexMyTargetMix : AdsManagerBase
{
    private string[] YandexMyTargetMix_InterstitialIds = new string[]
    {
        "R-M-8612826-11","R-M-8612826-12","1555382","R-M-8612826-13",
        "1555385","R-M-8612826-14","R-M-8612826-15","1555388",
        "R-M-8612826-16","R-M-8612826-17","1555391","R-M-8612826-18",
        "R-M-8612826-19","1555394","R-M-8612826-20","R-M-8612826-21","1555397"
    };

    private string[] YandexMyTargetMix_RewardedIds = new string[]
    {
        "R-M-8612826-22","R-M-8612826-23","1555400","R-M-8612826-24",
        "1555403","R-M-8612826-25","R-M-8612826-26","1555406",
        "R-M-8612826-27","R-M-8612826-28","1555409","R-M-8612826-29",
        "R-M-8612826-30","1555412","R-M-8612826-31","R-M-8612826-32","1555415"
    };

    private string[] YandexMyTargetMix_BannerIds = new string[]
    {
        "R-M-8612826-1","R-M-8612826-2","1555367","R-M-8612826-3",
        "R-M-8612826-4","1555370","R-M-8612826-5","1555373",
        "R-M-8612826-6","R-M-8612826-7","1555376","R-M-8612826-8",
        "R-M-8612826-9","1555379"
    };

    protected override string[] GetAdsId(AdsType type)
    {
        switch (type)
        {
            case AdsType.Banner: return YandexMyTargetMix_BannerIds;
            case AdsType.Interstitial: return YandexMyTargetMix_InterstitialIds;
            case AdsType.Reward: return YandexMyTargetMix_RewardedIds;
        }
        return null;
    }

    protected override AdUnitBase GetBannerAdUnit() => new BannerAds_YandexMyTargetMix();
    protected override AdUnitBase GetInterstitialAdUnit() => new InterstitialAds_YandexMyTargetMix();
    protected override AdUnitBase GetRewardedAdUnit() => new RewardAds_YandexMyTargetMix();

    public override void SetChildDirectedTreatment(bool open)
    {
        YandexMobileAds.MobileAds.SetUserConsent(open);
    }

    public override void Init()
    {
        if (GameManager.PlayerData.NowLevel < 8) return;
        if (IsInit) return;
        IsInit = true;

        if (IsInitComplete) return;
        IsInitComplete = true;

        SetChildDirectedTreatment(GameManager.PlayerData.KidMode);
        InitAllAdUnits();
    }
}
