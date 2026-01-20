using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;

public class RewardAdsGroup_Admob : AdUnitBase
{
    private AdUnitBase adUnit;//保底组广告位
    private RewardAds_CustomKV_Admob_Test adUnit_KV;//KV组广告位
    
    public RewardAdsGroup_Admob() : base()
    {
        adUnit = new RewardAds_Admob();
        adUnit_KV = new RewardAds_CustomKV_Admob_Test();
    }

    public override long GetPrice()
    {
        //优先KV广告
        if(adUnit_KV.IsLoaded())
            return adUnit_KV.GetPrice();

        if (adUnit.IsLoaded())
            return adUnit.GetPrice();

        return base.GetPrice();
    }
    
    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        base.Update(elapseSeconds, realElapseSeconds);

        adUnit.Update(elapseSeconds, realElapseSeconds);
        adUnit_KV.Update(elapseSeconds, realElapseSeconds);
    }
    
    public override bool IsLoaded()
    {
        return adUnit.IsLoaded() || adUnit_KV.IsLoaded();
    }
    
    public override void LoadAd(string[] ids)
    {
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnAdsLoaded);
        GameManager.Event.Subscribe(AdsEcpmRefreshEventArgs.EventId, OnAdsEcpmRefresh);
        
        //优先加载保底组的
        if (!adUnit.IsLoaded())
        {
            adUnit.LoadAd(ids);
        }
    }

    private void LoadKVAd()
    {
        if (!adUnit_KV.IsValid && CustomKVManager.RewardedAdsEcpm != -1 && adUnit.IsLoaded())  
        {
            adUnit_KV.IsValid = true;
            adUnit_KV.LoadAd();
        }
    }

    public override bool Show(object userData)
    {
        //优先展示KV组的
        if (adUnit_KV.IsLoaded())
        {
            adUnit_KV.Show(userData);
            return true;
        }
        
        if (adUnit.IsLoaded())
        {
            adUnit.Show(userData);
            return true;
        }

        return false;
    }

    public override void Dispose()
    {
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnAdsLoaded);
        GameManager.Event.Unsubscribe(AdsEcpmRefreshEventArgs.EventId, OnAdsEcpmRefresh);
    }

    private void OnAdsLoaded(object sender, GameEventArgs e)
    {
        LoadKVAd();
    }

    private void OnAdsEcpmRefresh(object sender, GameEventArgs e)
    {
        AdsEcpmRefreshEventArgs ne = e as AdsEcpmRefreshEventArgs;

        if (ne != null && ne.IsRV) 
        {
            LoadKVAd();   
        }
    }
}
