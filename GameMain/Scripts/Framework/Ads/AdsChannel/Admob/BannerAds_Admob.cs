using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Common;
using UnityEngine;

public class BannerAds_Admob : AdUnitBase
{
	private BannerView bannerAd = null;

    private bool isShow = true;//记录展示状态

    public override bool IsLoaded()
	{
		return bannerAd != null && IsLoad;
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
		IsLoad = false;
		if (bannerAd != null)
		{
			bannerAd.Destroy();
			bannerAd = null;
		}
		base.Dispose();
	}

	public override void LoadAd(string[] ids = null)
	{
		ids = ids == null ? base.AdsIds : ids;
		if (ids == null || ids.Length == 0)
		{
			Log.Debug("Admob BannerAd adUnitId is invalid");
			return;
		}

		if (IsLoaded())
		{
			Log.Debug($"Admob BannerAd is has load");
			return;
		}

		base.SetAdsIds(ids);
		base.LoadAd(ids);

		if (base.CurAdsId == null)
		{
			Log.Debug($"Admob BannerAd Can Load Id is Null! Id :{base.CurAdsId}");
			return;
		}

		Log.Debug($"Admob BannerAd Start Load...{base.CurAdsId}");
		GameManager.Ads.IsRequestingBannerAd = true;

		if (bannerAd == null)
		{
			bannerAd = new BannerView(base.CurAdsId, GetAdSize(), AdPosition.Bottom);
			ListenToAdEvents();
		}

		bannerAd.LoadAd(new AdRequest());
	}

    public override void ShowBanner()
    {
        isShow = true;
        if (bannerAd != null)
        {
            bannerAd.Show();
        }
    }

    public override void HideBanner()
    {
        isShow = false;
        if (bannerAd != null)
        {
            bannerAd.Hide();
        }
    }

    /// <summary>
    /// listen to events the banner may raise.
    /// </summary>
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
			GameManager.Ads.IsRequestingBannerAd = false;
			Log.Info($"Admob BannerAd Loaded...:{base.CurAdsId}");

			if (GameManager.Ads.IsBannerAdInHideStatus)
				GameManager.Ads.HideBanner();
		});
	}

	private void HandleBannerAdFailedToLoad(LoadAdError e)
	{
		AdmobUtils.ConcurrentQueue.Enqueue(() =>
		{
			UnityEngine.Debug.Log($"Admob BannerAd Failed To Load,Reason {base.CurAdsId}...Error:{(e!=null?e.GetMessage():null)}");

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
						GameManager.Firebase.RecordOnPaidEvent("BannerAd", e.Value, e.CurrencyCode, e.Precision.ToString(), info.GetResponseId(), info.GetMediationAdapterClassName());
						Log.Debug($"HandleBannerAdPaidEvent:{info.GetMediationAdapterClassName()}");
					}
				}
			}
			catch { }
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
				float ratio = ((float)Screen.height / (float)Screen.width) > (1920f / 1080f) ? 1080f / Screen.width : 1920f / Screen.height;
				heightRatio = Math.Max(0, (int)(bannerAd.GetHeightInPixels() * ratio));
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

