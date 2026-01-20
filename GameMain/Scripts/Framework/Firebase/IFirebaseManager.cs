using Firebase.Analytics;
using System;
public interface IFirebaseManager
{
    bool IsInitFirebaseApp { get; }
    bool IsInitRemoteConfig { get; }
    bool IsFetchRemoteConfig { get; }
    void InitFirebaseApp();
    void InitRemoteConfig();
    void FetchRemoteConfigDataAsync();
    void RecordMessageByEventSelectContent(string contentType, string ItemIds = null);
    void RecordMessageByEvent(string recordName, params Parameter[] parameters);
    void RecordOnPaidEvent(string source, long value, string currencyCode, string precisionType, string adsUnitId, string mediationAdapterClassName);
    void RecordOnPaidEvent(string source, double value, string currencyCode, string precisionType, string adsUnitId, string mediationAdapterClassName);
    bool GetBool(string key, bool defaultValue = false);
    long GetLong(string key, long defaultValue = 0);
    double GetDouble(string key, double defaultValue = 0);
    string GetString(string key, string defaultValue = null);
    void SigninWithFacebook(Action<bool> callback = null);
    void SigninWithGoogle(Action<bool> callback = null);
    void SignOut();
}
