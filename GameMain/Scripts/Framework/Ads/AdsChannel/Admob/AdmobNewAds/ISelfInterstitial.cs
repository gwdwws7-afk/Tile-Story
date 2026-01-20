using GoogleMobileAds.Api;
using System;
using GoogleMobileAds.Common;

public interface ISelfInterstitialClient
{
	event EventHandler<EventArgs> OnAdLoaded;

	event EventHandler<LoadAdErrorClientEventArgs> OnAdFailedToLoad;

	event EventHandler<AdErrorClientEventArgs> OnAdFailedToPresentFullScreenContent;

	event EventHandler<EventArgs> OnAdDidDismissFullScreenContent;

	event EventHandler<AdValue> OnPaidEvent;

	void LoadAd(string adUnitID, string bundleKey,string userGroupKey,string userGroupValue);

	void Show();

	void Destroy();

	string GetAdUnitId();

	string GetMediationAdapterClassName();

	string GetAdSourceName();
	
	string GetAdSourceId();
	
	string GetAdSourceInstanceName();
	
	string GetAdSourceInstanceId();
	
	string GetMediationValueByKey(string key);

	bool CanShowAd();
}


