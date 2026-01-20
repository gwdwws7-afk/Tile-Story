
/// <summary>
/// 广告管理器
/// </summary>
public sealed class AdsManager_Yandex : AdsManagerBase
{
    private string[] Yandex_BannerIds = new string[]
    {
        "R-M-8612826-1","R-M-8612826-2","R-M-8612826-3","R-M-8612826-4",
        "R-M-8612826-5","R-M-8612826-6","R-M-8612826-7","R-M-8612826-8","R-M-8612826-9",
    };
    private string[] Yandex_InterstitialIds = new string[]
    {
        "R-M-8612826-11","R-M-8612826-12","R-M-8612826-13","R-M-8612826-14",
        "R-M-8612826-15","R-M-8612826-16","R-M-8612826-17","R-M-8612826-18",
        "R-M-8612826-19","R-M-8612826-20","R-M-8612826-21",
    };
    private string[] Yandex_RewardedIds = new string[]
    {
        "R-M-8612826-22","R-M-8612826-23","R-M-8612826-24","R-M-8612826-25",
        "R-M-8612826-26","R-M-8612826-27","R-M-8612826-28","R-M-8612826-29",
        "R-M-8612826-30","R-M-8612826-31","R-M-8612826-32",
    };

    protected override string[] GetAdsId(AdsType type)
    {
        switch (type)
        {
            case AdsType.Banner: return Yandex_BannerIds;
            case AdsType.Interstitial: return Yandex_InterstitialIds;
            case AdsType.Reward: return Yandex_RewardedIds;
        }
        return null;
    }

    protected override AdUnitBase GetBannerAdUnit() => new BannerAds_Yandex();
    protected override AdUnitBase GetInterstitialAdUnit() => new InterstitialAds_Yandex();
    protected override AdUnitBase GetRewardedAdUnit() => new RewardAds_Yandex();

    public override void SetChildDirectedTreatment(bool open)=> YandexMobileAds.MobileAds.SetUserConsent(open);

    public override void Init()
    {
        if (GameManager.PlayerData.NowLevel < 8) return;
        if(IsInit)return;
        IsInit = true;
        
        if(IsInitComplete)return;
        IsInitComplete = true;

        SetChildDirectedTreatment(GameManager.PlayerData.KidMode);
        InitAllAdUnits();
    }
}

