using GoogleMobileAds.Api;
using System;

namespace GoogleMobileAds.Common
{
    public interface INativeAdsClient
    {
        event EventHandler<EventArgs> OnAdLoaded;

        event EventHandler<LoadAdErrorClientEventArgs> OnAdFailedToLoad;

		event EventHandler<AdValue> OnPaidEvent;

		event EventHandler OnUserEarnedReward;

		event EventHandler<AdErrorClientEventArgs> OnAdFailedToPresentFullScreenContent;

		event EventHandler<EventArgs> OnAdDidPresentFullScreenContrnt;

		event EventHandler<EventArgs> OnAdDidDismissFullScreenContent;

		void LoadAd(string adUnitId);

		void Show();

		void Destroy();
		
		string GetAdUnitId();
		
		string GetMediationAdapterClassName();
		
		string GetAdSourceName();
	
		string GetAdSourceId();
	
		string GetAdSourceInstanceName();
	
		string GetAdSourceInstanceId();
	
		string GetMediationValueByKey(string key);
	}
}


