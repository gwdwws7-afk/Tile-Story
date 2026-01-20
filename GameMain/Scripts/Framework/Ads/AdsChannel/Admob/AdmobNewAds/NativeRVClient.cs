
#if UNITY_ANDROID
using GoogleMobileAds.Android;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine;

namespace GoogleMobileAds.Android
{
	public class NativeRVClient : AndroidJavaProxy, INativeAdsClient
	{
		private AndroidJavaObject androidInterstitialAd;
		
		public event EventHandler<EventArgs> OnAdLoaded;

		public event EventHandler<LoadAdErrorClientEventArgs> OnAdFailedToLoad;

		public event EventHandler<AdValue> OnPaidEvent;

		public event EventHandler OnUserEarnedReward;

		public event EventHandler<AdErrorClientEventArgs> OnAdFailedToPresentFullScreenContent;

		public event EventHandler<EventArgs> OnAdDidPresentFullScreenContrnt;

		public event EventHandler<EventArgs> OnAdDidDismissFullScreenContent;


		public NativeRVClient()
			: base("linkdesks.pop.bubblegames.customads.InterstitialListener_Admob")
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			androidInterstitialAd = new AndroidJavaObject("linkdesks.pop.bubblegames.customads.Reward_Native_Admob", @static, this);
		}
		
		public string GetAdUnitId()
		{
			return androidInterstitialAd.Call<string>("getAdUnitId");
		}
		
		public string GetMediationAdapterClassName()
		{
			return androidInterstitialAd.Call<string>("getMediationAdapterClassName");
		}
		
		public string GetAdSourceName()
		{
			return androidInterstitialAd.Call<string>("getAdSourceName");
		}

		public string GetAdSourceId()
		{
			return androidInterstitialAd.Call<string>("getAdSourceId");
		}

		public string GetAdSourceInstanceName()
		{
			return androidInterstitialAd.Call<string>("getAdSourceInstanceName");
		}

		public string GetAdSourceInstanceId()
		{
			return androidInterstitialAd.Call<string>("getAdSourceInstanceId");
		}

		public string GetMediationValueByKey(string key)
		{
			return androidInterstitialAd.Call<string>("getMediationValueByKey",key);
		}

		public void LoadAd(string adUnitId)
		{
			androidInterstitialAd.Call("loadAd", adUnitId);
		}

		public void Show()
		{
			androidInterstitialAd.Call("show");
		}

		public void Destroy()
		{
			androidInterstitialAd.Call("destroy");
		}

		public void onAdLoaded()
		{
			if (this.OnAdLoaded != null)
			{
				this.OnAdLoaded(this, EventArgs.Empty);
			}
		}

		public void onAdClosed()
		{
			if (this.OnAdDidDismissFullScreenContent != null)
			{
				this.OnAdDidDismissFullScreenContent(this, EventArgs.Empty);
			}
		}

		public void onAdFailedToLoad(AndroidJavaObject error)
		{
			if (this.OnAdFailedToLoad != null)
			{
				LoadAdErrorClientEventArgs loadAdErrorClientEventArgs = new LoadAdErrorClientEventArgs();
				this.OnAdFailedToLoad(this, loadAdErrorClientEventArgs);
			}
		}

		public void onAdOpened()
		{
			if (this.OnAdDidPresentFullScreenContrnt != null)
			{
				this.OnAdDidPresentFullScreenContrnt(this, EventArgs.Empty);
			}
		}

		public void onAdFailedToShow(AndroidJavaObject error)
		{
			if (this.OnAdFailedToPresentFullScreenContent != null)
			{
				if (this.OnAdFailedToPresentFullScreenContent != null)
				{
					this.OnAdFailedToPresentFullScreenContent(this, new AdErrorClientEventArgs());
				}
			}
		}

		public void onUserEarnedReward()
		{
			if (this.OnUserEarnedReward != null)
			{
				this.OnUserEarnedReward(this, EventArgs.Empty);
			}
		}

		public void onPaidEvent(int precision, long valueInMicros, string currencyCode)
		{
			if (this.OnPaidEvent != null)
			{
				AdValue adValue = new AdValue()
				{
					Precision = (AdValue.PrecisionType)precision,
					Value = valueInMicros,
					CurrencyCode = currencyCode,
				};
				this.OnPaidEvent(this, adValue);
			}
		}
	}
}
#endif
