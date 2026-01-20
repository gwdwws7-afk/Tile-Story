using Mycom.Target.Unity.Ads;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class BannerAds_YandexMyTargetMix : AdUnitBase
{
	private Banner bannerAd_Yandex = null;
	private MyTargetView bannerAd_MyTarget = null;

	private const int BannerHeighDp = 50;

	private string recordSuccessAdsId;

	private bool isShow = true;//记录展示状态

	public override bool IsLoaded()
	{
		return (bannerAd_Yandex != null || bannerAd_MyTarget != null) && IsLoad;
	}

	public override bool Show(object userData)
	{
		if (IsLoaded())
		{
			if (bannerAd_Yandex != null)
				bannerAd_Yandex.Show();
			else if (bannerAd_MyTarget != null)
				bannerAd_MyTarget.Start();
			return true;
		}
		return false;
	}

	public override void Dispose()
	{
		showBannerTime = -1f;
		recordSuccessAdsId = null;
		
		IsLoad = false;
		if(bannerAd_Yandex!=null)bannerAd_Yandex.Destroy();
		bannerAd_Yandex = null;

        if (bannerAd_MyTarget != null)
        {
			bannerAd_MyTarget.AdLoadCompleted -= LoadCompletedAction;
			bannerAd_MyTarget.AdLoadFailed -= LoadFailedAction;

			bannerAd_MyTarget.Dispose();
			bannerAd_MyTarget = null;
		}

		base.Dispose();
	}

	public override void LoadAd(string[] ids=null)
	{
		ids = ids == null ? base.AdsIds : ids;
		if (ids == null || ids.Length == 0)
		{
			Log.Debug("YandexMyTargetMix BannerAd adUnitId is invalid");
			return;
		}

		if (IsLoaded())
		{
			Log.Debug($"YandexMyTargetMix BannerAd is has load");
			return;
		}

		base.SetAdsIds(ids);
		base.LoadAd(ids);

		if (base.CurAdsId==null)
		{
			Log.Debug($"YandexMyTargetMix BannerAd Can Load Id is Null! Id :{base.CurAdsId}");
			return;
		}

		Log.Debug($"YandexMyTargetMix BannerAd Start Load...{base.CurAdsId}");
		GameManager.Ads.IsRequestingBannerAd = true;

        if (bannerAd_Yandex != null)
        {
			bannerAd_Yandex.Destroy();
			bannerAd_Yandex = null;
		}

		if (bannerAd_MyTarget != null)
		{
			bannerAd_MyTarget.AdLoadCompleted -= LoadCompletedAction;
			bannerAd_MyTarget.AdLoadFailed -= LoadFailedAction;

			bannerAd_MyTarget.Dispose();
			bannerAd_MyTarget = null;
		}

		bool isYandex = base.CurAdsId.Contains("R-M-");
        if (isYandex)
        {
			BannerAdSize bannerMaxSize = BannerAdSize.InlineSize(GetScreenWidthDp(), BannerHeighDp);
			bannerAd_Yandex = new Banner(base.CurAdsId, bannerMaxSize, AdPosition.BottomCenter);
			bannerAd_Yandex.OnAdLoaded += HandleBannerAdLoaded;
			bannerAd_Yandex.OnAdFailedToLoad += HandleBannerAdFailedToLoad;

			AdRequest request = new AdRequest.Builder().Build();
			bannerAd_Yandex.LoadAd(request);
		}
        else
        {
			bannerAd_MyTarget = new MyTargetView(uint.Parse(base.CurAdsId));

			bannerAd_MyTarget.AdLoadCompleted -= LoadCompletedAction;
			bannerAd_MyTarget.AdLoadCompleted += LoadCompletedAction;

			bannerAd_MyTarget.AdLoadFailed -= LoadFailedAction;
			bannerAd_MyTarget.AdLoadFailed += LoadFailedAction;

			bannerAd_MyTarget.X = 0;
			bannerAd_MyTarget.Y = GetSafeAreaBottomY() - BannerHeighDp; // 贴紧安全区域底部
			bannerAd_MyTarget.Load();
		}
	}

	// 获取安全区域底部的Y位置（单位：dp）
	private int GetSafeAreaBottomY()
	{
		// 获取屏幕安全区域（左下角坐标系）
		Rect safeArea = Screen.safeArea;

		// 计算安全区域底部的Y坐标（转换为左上角坐标系）
		float bottomY = Screen.height - safeArea.yMin;

		// 转换为DP单位（考虑DPI）
		float dpi = Screen.dpi;
		if (dpi <= 0) dpi = 160; // 避免DPI为0的情况
		float pixelsPerDp = dpi / 160f;

		return (int)(bottomY / pixelsPerDp);
	}

	private float showBannerTime = -1f;
	private Banner lastBanner=null; 
	private long RefreshBannerTime =>
		GameManager.Firebase.GetLong(Constant.RemoteConfig.Yandex_Banner_Refresh_Time, 30l);
	public override void Update(float elapseSeconds, float realElapseSeconds)
	{
		base.Update(elapseSeconds, realElapseSeconds);

		if (bannerAd_Yandex != null&&IsLoad&&!string.IsNullOrEmpty(recordSuccessAdsId)&&isShow)
		{
			if (showBannerTime > 0)
			{
				showBannerTime -= elapseSeconds;
				if (showBannerTime <= 0)
				{
					Log.Info($"Yandex BannerAd ReFresh!");
					IsLoad=false;
					lastBanner = bannerAd_Yandex;
					
					BannerAdSize bannerMaxSize = BannerAdSize.InlineSize(GetScreenWidthDp(), BannerHeighDp);
					bannerAd_Yandex = new Banner(recordSuccessAdsId, bannerMaxSize, AdPosition.BottomCenter);
					bannerAd_Yandex.OnAdLoaded += HandleBannerAdLoaded;
					bannerAd_Yandex.OnAdFailedToLoad += HandleBannerAdFailedToLoad;
					AdRequest request = new AdRequest.Builder().Build();
					bannerAd_Yandex.LoadAd(request);
				}
			}
		}
	}

	public override void ShowBanner()
	{
		isShow = true;
		if (bannerAd_Yandex != null) bannerAd_Yandex.Show();
		if (bannerAd_MyTarget != null) bannerAd_MyTarget.Start();
		if(lastBanner!=null)lastBanner.Show();
	}

	public override void HideBanner()
	{
		isShow = false;
		if (bannerAd_Yandex != null) bannerAd_Yandex.Hide();
		if (bannerAd_MyTarget != null) bannerAd_MyTarget.Stop();
		if(lastBanner!=null)lastBanner.Hide();
	}

	private void HandleBannerAdLoaded(object sender, EventArgs e)
	{
		eventQueue.Enqueue(AdsEvent.Create(() =>
		{
			recordSuccessAdsId = this.CurAdsId;//记录id
			
			AdsIdModel.LoadSuccess();
			base.IsLoad = true;
			Log.Info("Yandex BannerAd Loaded...");
			GameManager.Ads.IsRequestingBannerAd = false;

			bannerAd_Yandex.Show();
			
			if (GameManager.Ads.IsBannerAdInHideStatus)GameManager.Ads.HideBanner();
			
			showBannerTime = RefreshBannerTime;
			if (lastBanner != null)
            {
                lastBanner.Destroy();
                lastBanner = null;
            }
		}));
	}

	private void HandleBannerAdFailedToLoad(object sender, AdFailureEventArgs e)
	{
		eventQueue.Enqueue(AdsEvent.Create(() =>
		{
			UnityEngine.Debug.Log($"Yandex BannerAd Failed To Load,Reason {base.CurAdsId}...Error:{e.Message}");

			if (!IsLoad && !string.IsNullOrEmpty(base.CurAdsId))
			{
				GameManager.Ads.IsRequestingBannerAd = false;
				Dispose();
				AdsIdModel.LoadFail();
				eventQueue.Enqueue(AdsEvent.Create(() =>
				{
					LoadAd();
				}, AdsDelayLoadTime));
			}
		}));
	}

	private int GetScreenWidthDp()
	{
		int screenWidth = (int)Screen.safeArea.width;
		return ScreenUtils.ConvertPixelsToDp(screenWidth);
	}

	private int GetBannerHeightPixel()
	{
		return ScreenUtils.ConvertDpToPixels(BannerHeighDp);
	}

	private int heightRatio;
	public override int GetHeight()
	{
		if (bannerAd_Yandex != null && IsLoad)
		{
			if (heightRatio == 0)
			{
				float ratio = ((float)Screen.height / (float)Screen.width) > (1920f / 1080f) ?  1080f/Screen.width :  1920f/Screen.height;
				heightRatio = Math.Max(0, (int)(GetBannerHeightPixel()* ratio));
			}
			return heightRatio;
		}
		return 0;
	}

    #region MyTarget

    void LoadCompletedAction(System.Object a, EventArgs b)
	{
		eventQueue.Enqueue(AdsEvent.Create(() =>
		{
			AdsIdModel.LoadSuccess();
			base.IsLoad = true;
			Log.Info("MyTarget BannerAd Loaded...");
			GameManager.Ads.IsRequestingBannerAd = false;

			bannerAd_MyTarget.Start();

			if (GameManager.Ads.IsBannerAdInHideStatus)
				GameManager.Ads.HideBanner();
            
	        if (lastBanner != null)
            {
                lastBanner.Destroy();
                lastBanner = null;
            }
		}));
	}

	void LoadFailedAction(System.Object a, ErrorEventArgs b)
	{
		eventQueue.Enqueue(AdsEvent.Create(() =>
		{
			UnityEngine.Debug.Log($"MyTarget BannerAd Failed To Load,Reason {base.CurAdsId}...Error:{b.Message}");

			if (!IsLoad && !string.IsNullOrEmpty(base.CurAdsId))
			{
				GameManager.Ads.IsRequestingBannerAd = false;
				Dispose();
				AdsIdModel.LoadFail();
				eventQueue.Enqueue(AdsEvent.Create(() =>
				{
					LoadAd();
				}, AdsDelayLoadTime));
			}
		}));
	}

    #endregion
}

