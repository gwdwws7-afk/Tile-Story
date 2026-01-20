#if AmazonStore||UNITY_IOS||UNITY_IPHONE
/// <summary>
/// 广告管理器
/// </summary>
public sealed class AdsManager_Max : AdsManagerBase
{
#if UNITY_IOS||UNITY_IPHONE
    private string[] AppLovin_BannerIds = { "6145190fe7972311" };
    private string[] AppLovin_InterstitialIds ={ "18868bc8dfbbc753"};
    private string[] AppLovin_RewardedIds ={ "ecc892a459b6bf69"};
#else
    private string[] AppLovin_BannerIds = { "e4b2d220cc467da6" };
    private string[] AppLovin_InterstitialIds ={ "2a4b41c84da7dbdf"};
    private string[] AppLovin_RewardedIds ={ "ed9559a9e81db8c5"};
#endif

    protected override string[] GetAdsId(AdsType type)
    {
        switch (type)
        {
            case AdsType.Banner: return AppLovin_BannerIds;
            case AdsType.Interstitial: return AppLovin_InterstitialIds;
            case AdsType.Reward: return AppLovin_RewardedIds;
        }
        return null;
    }

    protected override AdUnitBase GetBannerAdUnit() => new BannerAds_AppLovin();
    protected override AdUnitBase GetInterstitialAdUnit() => new InterstitialAds_AppLovin();
    protected override AdUnitBase GetRewardedAdUnit() => new RewardAds_AppLovin();

    public override void SetChildDirectedTreatment(bool open)
    {
        MaxSdk.SetIsAgeRestrictedUser(open);
    }

    public override void Init()
    {
        if(IsInit)return;
        IsInit = true;
        
        InitAllAdUnits();
        SetChildDirectedTreatment( GameManager.PlayerData.KidMode);
        InitAds_AppLovin.Init(flag =>
        {
            if (!flag)
            {
                IsInit = false;
                Log.Error($"InitAds_AppLovin.Init fail!");
                return;
            }
            IsInitComplete = true;
            bool isMusicMute = GameManager.PlayerData.MusicMuted;
            bool isAudioMute = GameManager.PlayerData.AudioMuted;
            if (isMusicMute && isAudioMute) MaxSdk.SetMuted(true);
            else  MaxSdk.SetMuted(false);

             RequestAds();
        });
    }
}
#endif
