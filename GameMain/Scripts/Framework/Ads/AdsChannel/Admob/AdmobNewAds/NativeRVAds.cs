// GoogleMobileAds.Api.InterstitialAd
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine;

namespace GoogleMobileAds.Api
{
	public class NativeRVAd
	{
		private INativeAdsClient client;

		private string adUnitId;

		private bool isLoaded;

		public event EventHandler<EventArgs> OnAdLoaded;

		public event EventHandler<LoadAdErrorClientEventArgs> OnAdFailedToLoad;

		public event EventHandler<AdValue> OnPaidEvent;

		public event EventHandler<EventArgs> OnAdOpened;

		public event EventHandler OnUserEarnedReward;

		public event EventHandler<EventArgs> OnAdClosed;

		public event EventHandler<AdErrorClientEventArgs> OnAdFailedToShow;


		public NativeRVAd(string adUnitId)
		{
			this.adUnitId = adUnitId;
			isLoaded = false;

			client = GetINativeAdsClient();

			if (client != null)
			{
				client.OnAdLoaded += delegate (object sender, EventArgs args)
				{
					isLoaded = true;
					if (this.OnAdLoaded != null)
					{
						this.OnAdLoaded(this, args);
					}
				};
				client.OnAdFailedToLoad += delegate (object sender, LoadAdErrorClientEventArgs args)
				{
					if (this.OnAdFailedToLoad != null)
					{
						this.OnAdFailedToLoad(this, new LoadAdErrorClientEventArgs
						{
							LoadAdErrorClient = args.LoadAdErrorClient
						});
					}
				};

				client.OnPaidEvent += delegate (object sender, AdValue args)
				{
					if (this.OnPaidEvent != null)
					{
						this.OnPaidEvent(this, args);
					}
				};

				client.OnUserEarnedReward+=delegate (object sender,EventArgs args){

					if (this.OnUserEarnedReward != null)
					{
						this.OnUserEarnedReward(this, args);
					}
				};

				client.OnAdDidPresentFullScreenContrnt += delegate (object sender, EventArgs args)
				{

					if (this.OnAdOpened != null)
					{
						this.OnAdOpened(this, args);
					}
				};
				client.OnAdDidDismissFullScreenContent += delegate (object sender, EventArgs args)
				{
					if (this.OnAdClosed != null)
					{
						this.OnAdClosed(this, args);
					}
				};
				client.OnAdFailedToPresentFullScreenContent += delegate (object sender, AdErrorClientEventArgs args)
				{
					if (this.OnAdFailedToShow != null)
					{
						this.OnAdFailedToShow(this, new AdErrorClientEventArgs
						{
							AdErrorClient = args.AdErrorClient
						});
					}
				};
			}
		}

		internal INativeAdsClient GetINativeAdsClient()
		{
#if UNITY_ANDROID
            return new GoogleMobileAds.Android.NativeRVClient();
#endif
	        return null;
		}


		public void LoadAd()
		{
			if(client!=null) client.LoadAd(adUnitId);
		}

		public bool IsLoaded()
		{
			return isLoaded;
		}

		public void Show()
		{
			isLoaded = false;
			if(client!=null) client.Show();
		}

		public void Destroy()
		{
			if(client!=null) client.Destroy();
		}
		
		public string GetMediationAdapterClassName()
		{
			if (client != null) return client.GetMediationAdapterClassName();
			return null;
		}
		
		public string GetAdUnitId()
		{
			if (client != null) return client.GetAdUnitId();
			return null;
		}
		
		public string GetAdSourceName()
		{
			if (client != null) return client.GetAdSourceName();
			return null;
		}

		public string GetAdSourceId()
		{
			if (client != null) return client.GetAdSourceId();
			return null;
		}

		public string GetAdSourceInstanceName()
		{
			if (client != null) return client.GetAdSourceInstanceName();
			return null;
		}

		public string GetAdSourceInstanceId()
		{
			if (client != null) return client.GetAdSourceInstanceId();
			return null;
		}

		public string GetMediationValueByKey(string key)
		{
			if (client != null) return client.GetMediationValueByKey(key);
			return null;
		}
	}
}
