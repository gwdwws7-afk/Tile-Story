
/// <summary>
/// 广告管理器接口
/// </summary>
public interface IAdsManager
{
    bool IsRemovePopupAds { get; set; }
    
    bool IsInitComplete { get; set; }

    void DestroyBannerAndReLoad();

    void Init();

    void Update(float elapseSeconds, float realElapseSeconds);

    void RequestAds();

    bool CheckInterstitialAdIsLoaded();

    bool CheckRewardedAdIsLoaded();

    void ShowBanner();

    void HideBanner();

    bool ShowInterstitial();

    bool ShowRewardedAd(string rewardType);

    void SetChildDirectedTreatment(bool open);

    int GetBannerAdsHeight();

    void ReInitAds();
}
