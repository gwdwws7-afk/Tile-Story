using GameFramework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

/// <summary>
/// ��浥Ԫ����
/// </summary>
public abstract class AdUnitBase : IAdUnit
{
    protected ConcurrentQueue<AdsEvent> eventQueue=new ConcurrentQueue<AdsEvent>();

    public IAdsIdModel AdsIdModel;

    public float loadAdsTimeOut=-1;

    protected float loadedAdLifeTime = Mathf.Infinity;
    
    protected long adsPrice=-1;

    public virtual long GetPrice()
    {
        return adsPrice;
    }

    public virtual void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (eventQueue.TryDequeue(out AdsEvent admobEvent))
        {
            if (admobEvent.delayTime > 0)
            {
                admobEvent.delayTime -= 0.02f;
                eventQueue.Enqueue(admobEvent);
            }
            else
            {
                try
                {
                    admobEvent.action?.Invoke();
                }
                catch { }
                ReferencePool.Release(admobEvent);
            }
        }
        if (!IsLoaded() && loadAdsTimeOut > 0)
        {
            loadAdsTimeOut -= 0.015f;

            if (loadAdsTimeOut <= 0)
            {
                if (AdsIdModel != null)
                    Log.Debug($"Ad_Time_Out:{AdsIdModel.GetCurAdsId()}");
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Ad_Time_Out);
                Dispose();
                if (AdsIdModel != null) 
                    AdsIdModel.LoadFail();
                eventQueue.Enqueue(AdsEvent.Create(() =>
                {
                    LoadAd(AdsIds);
                }, AdsDelayLoadTime));
            }
        }
        else if (IsLoaded())
        {
            loadAdsTimeOut = -1f;
        }
    }

    public abstract bool IsLoaded();

    public virtual void LoadAd(string[] ids)
    {
        this.loadAdsTimeOut = AdsTimeout();
        loadedAdLifeTime = Mathf.Infinity;
    }

    public abstract bool Show(object userData);

    public virtual void Dispose()
    {
        loadAdsTimeOut = -1f;
        loadedAdLifeTime = Mathf.Infinity;
        if (eventQueue!=null&&eventQueue.Count > 0)
        {
            while (eventQueue.Count>0)
            {
                if (eventQueue.TryDequeue(out AdsEvent adsEvent))
                {
                    ReferencePool.Release(adsEvent);
                }else
                    break;
            }
        }
    }

    public virtual int GetHeight()
    {
        return 0;
    }

    public virtual void ShowBanner() { }

    public virtual void HideBanner() { }

    public void ShowAdsCanvas(bool isShow,Action action=null)
    {
        action?.Invoke();
    }

    #region
    public bool IsLoad { get; set; }

    public string[] AdsIds { get; private set; }

    protected virtual string CurAdsId
    {
        get
        {
            if (AdsIdModel != null)
                return AdsIdModel.GetCurAdsId();
            return null;
        }
    }

    public void SetAdsIds(string[] ids)
    {
        if (ids == null)
        {
            UnityEngine.Debug.LogError($"ids is Null!");
            return;
        }
        if (this.AdsIds == ids)
		{
			return;
		}
		else
		{
			this.AdsIds = ids;

            this.AdsIdModel = GetAdsIdModel(ids);
        }
    }

    private IAdsIdModel GetAdsIdModel(string[] ids)
    {
        return new AdsIdModel_AdsIds(ids);
    }

    #endregion


    private static float adsTimeOutNum = 0;
    private static float AdsTimeout()
    {
        if (adsTimeOutNum <= 0)
        {
            adsTimeOutNum = 120;
        }
        return adsTimeOutNum;
    }

    public float AdsDelayLoadTime
    {
        get
        {
            UnityEngine.Random.InitState(Time.frameCount);
            return  GameManager.Firebase.GetLong(Constant.RemoteConfig.LoadAds_DelayTime, 5)+ DelayTime+UnityEngine.Random.Range(0f,7f);
        }
    }

    public static int Round_Delay_Load_Ads_Time => (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Round_Delay_Load_Ads_Time,20);
    public int DelayTime
    {
        get
        {
            if (GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Use_AdmobSDK, true))
            {
                if (AdsIdModel is AdsIdModel_AdsIds)
                {
                    var index = (AdsIdModel as AdsIdModel_AdsIds).curAdsIndex;
                    if (AdsIds != null && (index % AdsIds.Length == 0))
                    {
                        return Math.Min(20,Round_Delay_Load_Ads_Time * (index / AdsIds.Length));
                    }
                }
            }
            return 0;
        }
    }
}

public interface IAdsIdModel
{
    void LoadFail();
    void LoadSuccess();
    string GetCurAdsId();
    int GetFailCount();
}

public class AdsIdModel_AdsIds : IAdsIdModel
{
    private List<string> list = new List<string>();

    public int curAdsIndex=0;
    public AdsIdModel_AdsIds(string[] adsIds)
    {
        if (adsIds != null)
        {
            list = adsIds.ToList();
        }
    }

    public void LoadFail()
    {
        curAdsIndex++;
    }

    public void LoadSuccess()
    {
        curAdsIndex = 0;
    }

    public string GetCurAdsId()
    {
        return list[curAdsIndex % list.Count];
    }

    public int GetFailCount()
    {
        return curAdsIndex;
    }

    public static bool IsCanUseRule => true;
}
