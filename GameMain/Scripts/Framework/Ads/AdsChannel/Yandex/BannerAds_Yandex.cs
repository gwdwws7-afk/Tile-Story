using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class BannerAds_Yandex: AdUnitBase
{
	// ~BannerAds_Yandex()
	// {
	// 	if (bannerAd != null)
	// 	{
	// 		bannerAd.Destroy();
	// 		bannerAd = null;
	// 	}
	// }
	
	private Banner bannerAd=null;

	private const int BannerHeighDp = 50;

	private float showBannerTime = -1f;

    private bool isShow = true;//记录展示状态

    private long RefreshBannerTime =>
		GameManager.Firebase.GetLong(Constant.RemoteConfig.Yandex_Banner_Refresh_Time, 30l);

	public override bool IsLoaded()
	{
		return bannerAd!=null&& IsLoad;
	}

	public override bool Show(object userData)
	{
		if (IsLoaded())
		{
			bannerAd.Show();
			return true;
		}
		return false;
	}

	public override void Dispose()
	{
		showBannerTime = -1f;
		IsLoad = false;
		if(bannerAd!=null)bannerAd.Destroy();
		bannerAd = null;
		base.Dispose();
	}

	public override void LoadAd(string[] ids=null)
	{
		ids = ids == null ? base.AdsIds : ids;
		if (ids == null || ids.Length == 0)
		{
			Log.Debug("Yandex BannerAd adUnitId is invalid");
			return;
		}

		if (IsLoaded())
		{
			Log.Debug($"Yandex BannerAd is has load");
			return;
		}

		base.SetAdsIds(ids);
		base.LoadAd(ids);

		if (base.CurAdsId==null)
		{
			Log.Debug($"Yandex BannerAd Can Load Id is Null! Id :{base.CurAdsId}");
			return;
		}

		Log.Debug($"Yandex BannerAd Start Load...{base.CurAdsId}");
		GameManager.Ads.IsRequestingBannerAd = true;

		if (bannerAd == null)
		{
			BannerAdSize  bannerMaxSize = BannerAdSize.InlineSize(GetScreenWidthDp(), BannerHeighDp);
			bannerAd = new Banner(base.CurAdsId, bannerMaxSize, AdPosition.BottomCenter);
			bannerAd.OnAdLoaded += HandleBannerAdLoaded;
			bannerAd.OnAdFailedToLoad += HandleBannerAdFailedToLoad;
		}

		AdRequest request = new AdRequest.Builder().Build();
		bannerAd.LoadAd(request);
	}

    public override void ShowBanner()
    {
        isShow = true;
        if (bannerAd != null) bannerAd.Show();
    }

    public override void HideBanner()
    {
        isShow = false;
        if (bannerAd != null) bannerAd.Hide();
    }

    private void HandleBannerAdLoaded(object sender, EventArgs e)
	{
		eventQueue.Enqueue(AdsEvent.Create(() =>
		{
			AdsIdModel.LoadSuccess();
			base.IsLoad = true;
			Log.Info("Yandex BannerAd Loaded...");
			GameManager.Ads.IsRequestingBannerAd = false;

			bannerAd.Show();

			showBannerTime = RefreshBannerTime;
			if(lastBanner!=null)lastBanner.Destroy();

			if (GameManager.Ads.IsBannerAdInHideStatus)
				GameManager.Ads.HideBanner();
		}));
	}

	private Banner lastBanner=null; 
	public override void Update(float elapseSeconds, float realElapseSeconds)
	{
		base.Update(elapseSeconds, realElapseSeconds);
		
		if (showBannerTime > 0)
		{
			showBannerTime -= elapseSeconds;
			if (showBannerTime <= 0)
			{
				Log.Info($"Yandex BannerAd ReFresh!");
				IsLoad=false;
				lastBanner = bannerAd;
				bannerAd = null;
				LoadAd();
			}
		}
	}

	private void HandleBannerAdFailedToLoad(object sender, AdFailureEventArgs e)
	{
		eventQueue.Enqueue(AdsEvent.Create(() =>
		{
			UnityEngine.Debug.Log($"Yandex BannerAd Failed To Load,Reason {base.CurAdsId}...Error:{e.Message}");

			GameManager.Ads.IsRequestingBannerAd = false;
			if (!IsLoad && !string.IsNullOrEmpty(base.CurAdsId))
			{
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
		if (bannerAd != null && IsLoad)
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
}

