using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

/// <summary>
/// 广告管理器
/// </summary>
public abstract class AdsManagerBase : IAdsManager
{
    public Queue<AdsEvent> adsEventQueue = new Queue<AdsEvent>();
    private AdUnitBase interstitialAdUnit;
    private AdUnitBase rewardAdUnit;
    private AdUnitBase bannerAdUnit;
    private AdUnitBase nativeInterstitialAdUnit;
    private AdUnitBase nativeRewardAdUnit;

    private bool isInit = false;

    public bool IsInit
    {
        get => isInit;
        set => isInit = value;
    }

    private bool isInitComplete = false;
    public bool IsInitComplete
    {
        get => isInitComplete;
        set
        {
            isInitComplete = value;
        }
    }
    
    private bool isRequestInterstitialAd;
    private bool isRequestNativeInterstitialAd;
    private bool isRequestNativeRewardAd;
    private bool isRequestRewardAd;
    private bool isRequestBannerAd;

    public bool IsRemovePopupAds
    {
        get=>GameManager.PlayerData.IsRemoveAds;
        set
        {
            if (value)
            {
                Log.Info($"IsRemovePopupAds:{value}");
                ClearInterstitialAd();
                ClearBannerAd();
            }
        }
    }
    
    protected abstract string[] GetAdsId(AdsType type);
    protected abstract AdUnitBase GetBannerAdUnit();
    protected abstract AdUnitBase GetInterstitialAdUnit();
    protected abstract AdUnitBase GetRewardedAdUnit();
    protected virtual AdUnitBase GetNativeInterstitialAdUnit() => null;
    protected virtual AdUnitBase GetNativeRewardAdUnit() => null;
    public abstract void Init();
    public abstract void SetChildDirectedTreatment(bool open);

    public void ShowBanner()
    {
        if (bannerAdUnit != null) bannerAdUnit.ShowBanner();
    }

    public void HideBanner()
    {
        if (bannerAdUnit != null) bannerAdUnit.HideBanner();
    }

    protected void InitAllAdUnits()
    {
        bannerAdUnit = GetBannerAdUnit();
        interstitialAdUnit = GetInterstitialAdUnit();
        rewardAdUnit = GetRewardedAdUnit();

#if UNITY_ANDROID
        nativeInterstitialAdUnit = GetNativeInterstitialAdUnit();
        nativeRewardAdUnit = GetNativeRewardAdUnit();
#endif
    }

    public void DestroyBannerAndReLoad()
    {
        ClearBannerAd();

        bannerAdUnit = GetBannerAdUnit();
        
        if (GameManager.PlayerData.NowLevel >= AdsCommon.BannerNeedLevel)
        {
            adsEventQueue.Enqueue(AdsEvent.Create(() =>
            {
                CreateAndLoadBannerAd(GetAdsId(AdsType.Banner));
            }, 2));
        }
    }

    public void ReInitAds()
    {
        if(interstitialAdUnit!=null)interstitialAdUnit.Dispose();
        if(nativeInterstitialAdUnit!=null)nativeInterstitialAdUnit.Dispose();
        if(bannerAdUnit!=null)bannerAdUnit.Dispose();

        interstitialAdUnit = GetInterstitialAdUnit();
        nativeInterstitialAdUnit = GetNativeInterstitialAdUnit();
        bannerAdUnit = GetBannerAdUnit();
    }

    public virtual void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (isInitComplete)
        {
            if (interstitialAdUnit!=null) interstitialAdUnit.Update(elapseSeconds, realElapseSeconds);
            if (rewardAdUnit != null) rewardAdUnit.Update(elapseSeconds, realElapseSeconds);
            if (bannerAdUnit != null) bannerAdUnit.Update(elapseSeconds,realElapseSeconds);
            if (nativeInterstitialAdUnit != null) nativeInterstitialAdUnit.Update(elapseSeconds,realElapseSeconds);
            if (nativeRewardAdUnit != null) nativeRewardAdUnit.Update(elapseSeconds,realElapseSeconds);
        }
        
        if (adsEventQueue.Count > 0) 
        {
            AdsEvent adsEvent = adsEventQueue.Peek();
            adsEvent.delayTime -= elapseSeconds;

            if (adsEvent.delayTime <= 0)
            {
                adsEventQueue.Dequeue();
                adsEvent.action?.Invoke();
                ReferencePool.Release(adsEvent);
            }
        }
    }

    public void RequestAds()
    {
        if(!IsInitComplete)return;//等待广告sdk初始化完毕
        
        Log.Info($"AdsManager.RequestAds");

        int nowLevel = GameManager.PlayerData.NowLevel;

        if (!isRequestNativeInterstitialAd && nativeInterstitialAdUnit != null && !IsRemovePopupAds && nowLevel > AdsCommon.InterstitialLoadLevel) 
        {
            isRequestNativeInterstitialAd = true;

            adsEventQueue.Enqueue(AdsEvent.Create(() =>
            {
                CreateAndLoadNativeInterstitialAd(GetAdsId(AdsType.NativeInterstitial));
            }, 2));
        }

        if (!isRequestNativeRewardAd && nativeRewardAdUnit != null) 
        {
            isRequestNativeRewardAd = true;

            adsEventQueue.Enqueue(AdsEvent.Create(() =>
            {
                CreateAndLoadNativeRewardAd(GetAdsId(AdsType.NativeReward));
            }, 2));
        }

        if (!isRequestBannerAd && bannerAdUnit != null && !IsRemovePopupAds && nowLevel >= AdsCommon.BannerNeedLevel) 
        {
            isRequestBannerAd = true;

            adsEventQueue.Enqueue(AdsEvent.Create(() =>
            {
                CreateAndLoadBannerAd(GetAdsId(AdsType.Banner));
            }, 2));
        }

        if (!SystemInfoManager.CheckIsSpecialDeviceUseNativeAds())
        {
            if (!isRequestInterstitialAd && interstitialAdUnit != null && !IsRemovePopupAds && nowLevel > AdsCommon.InterstitialLoadLevel) 
            {
                isRequestInterstitialAd = true;

                adsEventQueue.Enqueue(AdsEvent.Create(() =>
                {
                    CreateAndLoadInterstitialAd(GetAdsId(AdsType.Interstitial));
                }, 2));
            }

            if (!isRequestRewardAd && rewardAdUnit!=null)
            {
                isRequestRewardAd = true;

                adsEventQueue.Enqueue(AdsEvent.Create(() =>
                {
                    CreateAndLoadRewardedAd(GetAdsId(AdsType.Reward));
                }, 2));
            }   
        }
    }

    /// <summary>
    /// 检测是否有插屏广告加载完毕
    /// </summary>
    /// <returns>是否加载完毕</returns>
    public bool CheckInterstitialAdIsLoaded()
    {
        return (interstitialAdUnit != null && interstitialAdUnit.IsLoaded())
               || (nativeInterstitialAdUnit != null && nativeInterstitialAdUnit.IsLoaded());
    }

    /// <summary>
    /// 检测是否有奖励广告加载完毕
    /// </summary>
    /// <returns>是否加载完毕</returns>
    public bool CheckRewardedAdIsLoaded()
    {
        return (rewardAdUnit != null && rewardAdUnit.IsLoaded())
               || (nativeRewardAdUnit != null && nativeRewardAdUnit.IsLoaded());
    }

    private bool IsShowBanner()
    {
        if (IsRemovePopupAds)return false;
        return  bannerAdUnit!=null&&bannerAdUnit.IsLoaded();
    }
    
    /// <summary>
    /// 展示插屏广告
    /// </summary>
    /// <returns>是否展示成功</returns>
    public bool ShowInterstitial()
    {
        if (IsRemovePopupAds)return false;

        long adPrice = -1;
        AdUnitBase maxPriceAdUnit = null;

        //如果ecpm为0 则展示一下并记录
        if (nativeInterstitialAdUnit != null && nativeInterstitialAdUnit.IsLoaded())
        {
            long price = nativeInterstitialAdUnit.GetPrice();
            if (price < 0)
            {
                return nativeInterstitialAdUnit.Show(null);
            }
            else if (price > adPrice)
            {
                adPrice = price;
                maxPriceAdUnit = nativeInterstitialAdUnit;
            }
        }

        if (interstitialAdUnit != null && interstitialAdUnit.IsLoaded())
        {
            long price = interstitialAdUnit.GetPrice();
            if (price < 0)
            {
                return interstitialAdUnit.Show(null);
            }
            else if (price > adPrice)
            {
                adPrice = price;
                maxPriceAdUnit = interstitialAdUnit;
            }
        }

        if (nativeInterstitialAdUnit != null && interstitialAdUnit != null)
        {
            Debug.Log($"ShowInterstitial Ecpm Compete...Native:{nativeInterstitialAdUnit.GetPrice()} vs Normal:{interstitialAdUnit.GetPrice()}");   
        }
        
        return maxPriceAdUnit != null && maxPriceAdUnit.Show(null);
    }

    /// <summary>
    /// 展示奖励广告
    /// </summary>
    /// <returns>是否展示成功</returns>
    public bool ShowRewardedAd(string rewardType)
    {
        long adPrice = -1;
        AdUnitBase maxPriceAdUnit = null;

        if (nativeRewardAdUnit != null && nativeRewardAdUnit.IsLoaded())
        {
            long price = nativeRewardAdUnit.GetPrice();
            if (price < 0)
            {
                return nativeRewardAdUnit.Show(rewardType);
            }
            else if (price > adPrice)
            {
                adPrice = price;
                maxPriceAdUnit = nativeRewardAdUnit;
            }
        }

        if (rewardAdUnit != null && rewardAdUnit.IsLoaded())
        {
            long price = rewardAdUnit.GetPrice();
            if (price < 0)
            {
                return rewardAdUnit.Show(rewardType);
            }
            else if (price > adPrice)
            {
                adPrice = price;
                maxPriceAdUnit = rewardAdUnit;
            }
        }

        if (nativeRewardAdUnit != null && rewardAdUnit != null)
        {
            Debug.Log($"ShowRewardedAd Ecpm Compete...Native:{nativeRewardAdUnit.GetPrice()} vs Normal:{rewardAdUnit.GetPrice()}");   
        }
        
        return maxPriceAdUnit != null && maxPriceAdUnit.Show(rewardType);
    }

    /// <summary>
    /// 清空插屏广告
    /// </summary>
    public void ClearInterstitialAd()
    {
        if (interstitialAdUnit != null)
        {
            interstitialAdUnit.Dispose();
            interstitialAdUnit = null;
        }

        if (nativeInterstitialAdUnit != null)
        {
            nativeInterstitialAdUnit.Dispose();
            nativeInterstitialAdUnit = null;
        }
    }
    public void ClearBannerAd()
    {
        if (bannerAdUnit != null)
		{
            bannerAdUnit.Dispose();
            bannerAdUnit = null;
        }
    }
    
    private void CreateAndLoadInterstitialAd(string[] adUnitIds)
    {
        Log.Info($"CreateAndLoadInterstitialAd:{!IsRemovePopupAds}:{adUnitIds}");
        
        if (IsRemovePopupAds||adUnitIds==null)return;
     
        if (interstitialAdUnit != null && !interstitialAdUnit.IsLoaded())
        {
            interstitialAdUnit.LoadAd(adUnitIds);
        }
    }
    
    private void CreateAndLoadNativeInterstitialAd(string[] adUnitIds)
    {
        Log.Info($"CreateAndLoadNativeInterstitialAd:{!IsRemovePopupAds}:{adUnitIds}");
        
        if (IsRemovePopupAds||adUnitIds==null)return;

        if (nativeInterstitialAdUnit != null && !nativeInterstitialAdUnit.IsLoaded())
        {
            nativeInterstitialAdUnit.LoadAd(adUnitIds);
        }
    }

    private void CreateAndLoadRewardedAd(string[] adUnitIds)
    {
        Log.Info($"CreateAndLoadRewardedAd:{adUnitIds}");
        
        if (adUnitIds==null) return;

        if (rewardAdUnit != null && !rewardAdUnit.IsLoaded())
        {
            rewardAdUnit.LoadAd(adUnitIds);
        }
    }
    
    private void CreateAndLoadNativeRewardAd(string[] adUnitIds)
    {
        Log.Info($"CreateAndLoadNativeRewardAd:{adUnitIds}");
        
        if (nativeRewardAdUnit != null && !nativeRewardAdUnit.IsLoaded())
        {
            nativeRewardAdUnit.LoadAd(adUnitIds);
        }
    }

    private void CreateAndLoadBannerAd(string[] adUnitIds)
    {
        //if(!AdsCommon.IsLowDeviceLoadBanner&&SystemInfoManager.IsSurpLowMenorySize)return;
        Log.Info($"CreateAndLoadBannerAd:{!IsRemovePopupAds}:{adUnitIds}");
        
        if (IsRemovePopupAds||adUnitIds==null)return;
  
        if (bannerAdUnit != null && !bannerAdUnit.IsLoaded())
        {
            bannerAdUnit.LoadAd(adUnitIds);
        }
    }

    public int GetBannerAdsHeight()
    {
        return bannerAdUnit != null && IsShowBanner() ? bannerAdUnit.GetHeight():0;
    }
}

