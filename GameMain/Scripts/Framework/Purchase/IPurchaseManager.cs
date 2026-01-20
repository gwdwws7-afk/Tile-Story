
using System;
using UnityEngine.Purchasing;

public interface IPurchaseManager
{
    bool IsInitialized { get; }

    bool IsPurchasing { get; }

    bool IsInitOverTime { get; }

    void InitializePurchasing(float overTime);

    void BuyProduct(ProductNameType productType, Action successAction, Action<PurchaseFailureReason> failAction);

    string GetPrice(ProductNameType productType);

    void OnPurchaseSuccess(string productName);
}
