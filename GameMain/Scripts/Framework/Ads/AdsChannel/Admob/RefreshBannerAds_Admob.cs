using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Common;
using UnityEngine;

public class RefreshBannerAds_Admob : AdUnitBase
{
	// ~RefreshBannerAds_Admob()
	// {
	// 	if (bannerAd != null)
	// 	{
	// 		bannerAd.Destroy();
	// 		bannerAd = null;
	// 	}
	// }
	
	private BannerView bannerAd=null;

	private int recordShowFrameCount;

	private int refreshFrameCount = -1;

	private int RefreshFrameCount
	{
		get
		{
			if (refreshFrameCount < 0)
			{
				refreshFrameCount = Math.Max(600, (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Refresh_Banner_FrameCount, 600));
			}
			return refreshFrameCount;
		}
	}

	public override void Update(float elapseSeconds, float realElapseSeconds)
	{
		//自刷新  记录开始展示的时间 定时加载banner
		base.Update(elapseSeconds, realElapseSeconds);
		if (recordShowFrameCount > 0 && (Time.frameCount - recordShowFrameCount) > RefreshFrameCount)
		{
			recordShowFrameCount = 0;
			Clear();
			LoadAd();
		}
	}

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
		recordShowFrameCount = 0;
		IsLoad = false;
		if (bannerAd != null)
		{
			bannerAd.OnBannerAdLoaded -= HandleBannerAdLoaded;
			bannerAd.OnBannerAdLoadFailed -= HandleBannerAdFailedToLoad;
			bannerAd.OnAdPaid -= HandleBannerAdPaidEvent;

			bannerAd.Destroy();
			bannerAd = null;
		}
		base.Dispose();
	}

	private void Clear()
	{
		recordShowFrameCount = 0;
		base.Dispose();
	}

	public override void LoadAd(string[] ids=null)
	{
		ids = ids == null ? base.AdsIds : ids;
		if (ids == null || ids.Length == 0)
		{
			Log.Error("Admob BannerAd adUnitId is invalid");
			return;
		}

		//if (IsLoaded())
		//{
		//	Log.Debug($"Admob BannerAd is has load");
		//	return;
		//}

		base.SetAdsIds(ids);
		base.LoadAd(ids);

		if (base.CurAdsId==null)
		{
			UnityEngine.Debug.Log($"Admob BannerAd Can Load Id is Null! Id :{base.CurAdsId}");
			return;
		}

		Log.Debug($"Admob BannerAd Start Load...{base.CurAdsId}");

		if (bannerAd == null)
		{
			bannerAd = new BannerView(base.CurAdsId, GetAdSize(), AdPosition.Bottom);
			ListenToAdEvents();
		}

#if UNITY_EDITOR
		if (IsLoaded())
		{
			bannerAd.Hide();
		}
#endif
		bannerAd.LoadAd(new AdRequest());
	}

	private void ListenToAdEvents()
	{
		bannerAd.OnBannerAdLoaded -= HandleBannerAdLoaded;
		bannerAd.OnBannerAdLoaded += HandleBannerAdLoaded;

		bannerAd.OnBannerAdLoadFailed -= HandleBannerAdFailedToLoad;
		bannerAd.OnBannerAdLoadFailed += HandleBannerAdFailedToLoad;

		bannerAd.OnAdPaid -= HandleBannerAdPaidEvent;
		bannerAd.OnAdPaid += HandleBannerAdPaidEvent;
	}

	private void HandleBannerAdLoaded()
	{
		AdmobUtils.ConcurrentQueue.Enqueue(() =>
		{
			AdsIdModel.LoadSuccess();
			base.IsLoad = true;
			recordShowFrameCount = Time.frameCount;
			Log.Info("Admob BannerAd Loaded Success!");

			if (GameManager.Ads.IsBannerAdInHideStatus)
				GameManager.Ads.HideBanner();
		});
	}

	private void HandleBannerAdFailedToLoad(LoadAdError e)
	{
		AdmobUtils.ConcurrentQueue.Enqueue(() =>
		{
			Log.Info($"Admob BannerAd Failed To Load,Reason {base.CurAdsId}...Error:{(e!=null?e.GetMessage():null)}");

			if (!string.IsNullOrEmpty(base.CurAdsId))
			{
				Clear();
				AdsIdModel.LoadFail();
				eventQueue.Enqueue(AdsEvent.Create(() =>
				{
					LoadAd();
				}, AdsDelayLoadTime));
			}
		});
	}

	private void HandleBannerAdPaidEvent(AdValue e)
	{
		AdmobUtils.ConcurrentQueue.Enqueue(() =>
		{
			if (e == null) return;

			try 
			{
				if (AddLocalPriceLog(e.Precision.ToString(), e.Value, e.CurrencyCode))
				{
					if (bannerAd != null)
					{
						ResponseInfo info = bannerAd.GetResponseInfo();
						GameManager.Firebase.RecordOnPaidEvent("RewardedAd", e.Value, e.CurrencyCode, e.Precision.ToString(), info.GetResponseId(), info.GetMediationAdapterClassName());
					}
				}
			} catch { }
		});
	}

	private bool AddLocalPriceLog(string precision, long value, string currencyCode)
	{
		if (string.IsNullOrEmpty(precision) || value <= 0 || string.IsNullOrEmpty(currencyCode))
			return false;
		
		AdsTaichiRecordManager.OnAdRevenuePaidEvent(value,currencyCode);

		if (currencyCode.Equals("USD") && precision != "Unknown")
		{
			return true;
		}
		return false;
	}

	private int heightRatio;
	public override int GetHeight()
	{
		if (bannerAd != null && IsLoad)
		{
			if (heightRatio == 0)
			{
				float ratio = ((float)Screen.height / (float)Screen.width) > (1920f / 1080f) ?  1080f/Screen.width :  1920f/Screen.height;
				heightRatio = Math.Max(0, (int)(bannerAd.GetHeightInPixels()* ratio));
			}
			return heightRatio;
		}

		return 0;
	}

	AdSize recordAdSize = new AdSize(0, 120);
	private AdSize GetAdSize()
	{
		float widthInPixels = /*Screen.safeArea.width > 0 ? Screen.safeArea.width :*/ Screen.width;
		int width = (int)(widthInPixels / MobileAds.Utils.GetDeviceScale());
		recordAdSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(width);
		return recordAdSize;
	}
}

