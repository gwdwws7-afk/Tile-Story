
#if UNITY_ANDROID
using GoogleMobileAds.Common;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

namespace GoogleMobileAds.Android
{
	public class SelfInterstitialClient : AndroidJavaProxy, ISelfInterstitialClient
	{
		private AndroidJavaObject androidInterstitialAd;

		public event EventHandler<EventArgs> OnAdLoaded;

		public event EventHandler<LoadAdErrorClientEventArgs> OnAdFailedToLoad;

		public event EventHandler<AdErrorClientEventArgs> OnAdFailedToPresentFullScreenContent;

		public event EventHandler<EventArgs> OnAdDidDismissFullScreenContent;

		public event EventHandler<AdValue> OnPaidEvent;

		public SelfInterstitialClient()
			: base("linkdesks.pop.bubblegames.customads.InterstitialListener_Admob")
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			androidInterstitialAd = new AndroidJavaObject("linkdesks.pop.bubblegames.customads.Interstitial_Admob", @static, this);
		}

		public void CreateInterstitialAd()
		{
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
		
		public bool CanShowAd()
		{
			return androidInterstitialAd.Call<bool>("isAdAvailable");
		}
		
		public void LoadAd(string adUnitId,string bundleKey,string userGroupKey,string userGroupValue)
		{
			androidInterstitialAd.Call("loadAd", adUnitId,bundleKey,userGroupKey,userGroupValue);
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

		public void onAdClicked() { }

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
		
		public void onAdFailedToShow(AndroidJavaObject error)
		{
			if (this.OnAdFailedToPresentFullScreenContent != null)
			{
				AdErrorClientEventArgs loadAdErrorClientEventArgs = new AdErrorClientEventArgs();
				this.OnAdFailedToPresentFullScreenContent(this, loadAdErrorClientEventArgs);
			}
		}

		public void onAdLeftApplication() { }

		public void onAdOpened() { }
		
		public void onUserEarnedReward(){}

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
