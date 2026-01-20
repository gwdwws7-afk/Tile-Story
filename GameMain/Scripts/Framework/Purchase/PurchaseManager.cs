using System;
using System.Collections.Generic;
using System.Globalization;
using Firebase.Analytics;
using MySelf.Model;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public sealed class PurchaseManager : GameFrameworkModule, IPurchaseManager, IStoreListener
{
    private IStoreController m_StoreController; // The Unity Purchasing system.
    private DTProductID productIDData;

    private Action purchaseSuccessAction;
    private Action<PurchaseFailureReason> purchaseFailAction;
    private bool isInitialized;
    private bool isInitOverTime;
    private bool startInit;
    private float overTime;
    private float timer;
    private bool isPurchasing;

    public bool IsInitialized
    {
        get
        {
            return isInitialized && m_StoreController != null && m_StoreController.products != null;
        }
    }

    public bool IsPurchasing
    {
        get
        {
            return isPurchasing;
        }
    }

    public bool IsInitOverTime => isInitOverTime;

    public PurchaseManager()
    {
        m_StoreController = null;
        productIDData = null;
        purchaseSuccessAction = null;
        purchaseFailAction = null;
        isInitialized = false;
        isInitOverTime = false;
        startInit = false;
        overTime = 2f;
        timer = 0;
        isPurchasing = false;
    }

    public void InitializePurchasing(float overTime)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Log.Warning("Network not reachable");
            isInitOverTime = true;
            return;
        }

        InternalInitializePurchasing();

        this.overTime = overTime;
        timer = 0;
        isInitOverTime = false;
        startInit = true;
    }

    private void InternalInitializePurchasing()
    {
        //Load ProductID Datatable
        productIDData = GameManager.DataTable.GetDataTable<DTProductID>().Data;
        if (productIDData == null)
        {
            Log.Error("productIDData is null");
            isInitOverTime = true;
            return;
        }
#if AmazonStore
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.AmazonAppStore));
#else
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
#endif

        //Add products that will be purchasable and indicate its type.
        for (int i = 0; i < productIDData.ProductIDDatas.Count; i++)
        {
            builder.AddProduct(productIDData.ProductIDDatas[i].ProductID, productIDData.ProductIDDatas[i].ProductType);
        }

#if AmazonStore
        builder.Configure<IAmazonConfiguration>();
#endif

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Log.Info("In-App Purchasing successfully initialized");
        m_StoreController = controller;
        isInitialized = true;

        RestorePurchases();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Log.Error($"In-App Purchasing initialize failed: {error}");
        isInitialized = true;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Log.Error($"In-App Purchasing initialize failed: {error}.Message:{message}");
        isInitialized = true;
    }

    public void BuyProduct(ProductNameType productType, Action successAction, Action<PurchaseFailureReason> failAction)
    {
        if (!isInitialized)
        {
            Log.Warning("buy product fail because not initialize");
            return;
        }

        if (m_StoreController == null || m_StoreController.products == null)
        {
            Log.Warning("buy product fail because StoreController is null");
            return;
        }

        purchaseSuccessAction = successAction;
        purchaseFailAction = failAction;

        GameManager.Firebase.RecordMessageByEvent(
            Constant.AnalyticsEvent.In_App_Purchase, 
            new Parameter("ProductName", (int)productType));
        GameManager.Firebase.RecordMessageByEvent(
            Constant.AnalyticsEvent.Purchase_Level,
            new Parameter("ProductName", (int)productType),
            new Parameter("LevelNum", GameManager.PlayerData.NowLevel));

        isPurchasing = true;

        GameManager.CurState = "BuyProduct";
        m_StoreController.InitiatePurchase(productIDData.GetProductID(productType));
    }

    public string GetPrice(ProductNameType productType)
    {
        if (!isInitialized)
        {
            Log.Warning("get price fail because not initialize");
            return string.Empty;
        }

        if (m_StoreController == null || m_StoreController.products == null)
        {
            Log.Warning("get price fail because StoreController is null");
            return string.Empty;
        }

        return GetPrice(productIDData.GetProductID(productType));
    }

    private string GetPrice(string productID)
    {
        try
        {
            Product product = m_StoreController.products.WithID(productID);
            if (product != null)
            {
                if (product.metadata.localizedPrice > new decimal(0.01))
                {
                    return product.metadata.localizedPrice.ToString() + " " + product.metadata.isoCurrencyCode;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                Log.Warning("get price fail because product {0} is null", productID);
                return string.Empty;
            }
        }
        catch (Exception e)
        {
            Log.Error("get price fail because {0}", e.Message);
            return string.Empty;
        }
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        //Retrieve the purchased product
        var product = purchaseEvent.purchasedProduct;

        //Add the purchased product to the players inventory
        string productName = productIDData.GetProductName(product.definition.id);
        if (!string.IsNullOrEmpty(productName))
        {
            bool validPurchase = CheckPurchaseValidation(purchaseEvent);
#if UNITY_IOS || UNITY_IPHONE
            validPurchase = true;
#endif
            OnPurchaseSuccess(productName);

            if (validPurchase)
            {
                Dictionary<string, string> eventValues = new Dictionary<string, string>();
                eventValues.Add("af_currency", product.metadata.isoCurrencyCode);
                eventValues.Add("af_revenue", product.metadata.localizedPrice.ToString(CultureInfo.InvariantCulture));
                eventValues.Add("af_quantity", "1");
                eventValues.Add("af_content_type", purchaseEvent.purchasedProduct.definition.type.ToString());
                eventValues.Add("af_content_id", purchaseEvent.purchasedProduct.definition.id);
                eventValues.Add("af_order_id", product.transactionID);
                GameManager.AppsFlyer.SendPurchaseEvent(eventValues);
                
                #if UNITY_ANDROID&&!AmazonStore
                double tablePrice = GameManager.DataTable.GetDataTable<DTProductID>().Data.GetProductPriceByCode(purchaseEvent.purchasedProduct.definition.id,product.metadata.isoCurrencyCode);
                GameManager.Firebase.RecordMessageByEvent(
                    Constant.AnalyticsEvent.IAPrevenue, 
                    new Parameter("quantity","1"),
                    new Parameter("currency","USD"),
                    new Parameter("value",tablePrice),
                    new Parameter("price",tablePrice),
                    new Parameter("product_id",purchaseEvent.purchasedProduct.definition.id));
                
                Debug.Log($"IAPrevenue:quantity:{1} " +$"currency:{"USD"} "+$"value:{tablePrice} "+$"price:{tablePrice} "+$"product_id:{purchaseEvent.purchasedProduct.definition.id}," +
                          $"product.metadata.isoCurrencyCode:{product.metadata.isoCurrencyCode}");
            #endif
            }
            else
            {
                Dictionary<string, string> eventValues = new Dictionary<string, string>();
                eventValues.Add("af_purchase_cheater", productName);
                GameManager.AppsFlyer.SendPurchaseEvent(eventValues);

                //OnPurchaseFailed(product, PurchaseFailureReason.Unknown);
            }
        }
        else
        {
            Log.Error("product id not match {0}", product.definition.id);

            OnPurchaseFailed(product, PurchaseFailureReason.ProductUnavailable);
        }

        Log.Info("Purchase Complete - Product: {0}", product.definition.id);

        //We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.
        return PurchaseProcessingResult.Complete;
    }

    private bool CheckPurchaseValidation(PurchaseEventArgs args)
    {
        bool validPurchase = false; // Presume valid for platforms with no R.V.

        // Unity IAP's validation logic is only included on these platforms.
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        // Prepare the validator with the secrets we prepared in the Editor
        // obfuscation window.
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            AppleTangle.Data(), Application.identifier);

        try
        {
            if (args.purchasedProduct == null || args.purchasedProduct.receipt == null) return validPurchase;
            // On Google Play, result has a single product ID.
            // On Apple stores, receipts contain multiple products.
            var result = validator.Validate(args.purchasedProduct.receipt);
            // For informational purposes, we list the receipt(s)
            Log.Info("Receipt is valid. Contents:");
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Log.Info(productReceipt.productID);
                Log.Info(productReceipt.purchaseDate);
                if (productReceipt.transactionID == null||productReceipt.transactionID.StartsWith("GPA"))
                {
                    validPurchase = true;
                }
            }
        }
        catch (IAPSecurityException e)
        {
            Log.Error($"Invalid receipt, not unlocking content:{e.Message}");
        }
#endif

        return validPurchase;
    }

    public void OnPurchaseSuccess(string productName)
    {
        GameManager.CurState = null;
        GameManager.UI.HideUIForm("LoadingMenu");

        bool productValid = true;
        if (Enum.TryParse(productName, out ProductNameType productNameType)) 
        {
            Log.Debug($"OnPurchaseSuccess :{productName} ");
            if (productNameType == ProductNameType.Halloween_Merge_Offer_1 ||
                productNameType == ProductNameType.Halloween_Merge_Offer_2 ||
                productNameType == ProductNameType.Halloween_Merge_Offer_3 ||
                productNameType == ProductNameType.Halloween_Merge_Offer_4 ||
                productNameType == ProductNameType.Halloween_Merge_Offer_5 ||
                productNameType == ProductNameType.Halloween_Merge_Offer_6 ||
                productNameType == ProductNameType.Halloween_Merge_Offer_7 ||
                productNameType == ProductNameType.Halloween_Merge_Offer_8 ||
                productNameType == ProductNameType.Halloween_Merge_Offer_9)
            {
                RewardManager.Instance.SaveRewardData(productNameType);
            }
            else
            {
                RewardManager.Instance.AddNeedGetReward(productNameType);
            }
            purchaseSuccessAction?.Invoke();
            PurchaseModel.Instance.RecordBuyItem(productNameType);
        }
        else
        {
            productValid = false;
            Log.Error($"productName {productName} is invalid");
        }

        if (productValid)
        {
            //购买任何商品都会去广告
            GameManager.Ads.IsRemovePopupAds = true;
            GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.IsRemovePopupAds));

            GameManager.Firebase.RecordMessageByEvent(
                Constant.AnalyticsEvent.Buy_Purchase_Success, 
                new Parameter("ProductName", productName));
        }

        Clear();
        isPurchasing = false;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        GameManager.CurState = null;
        Log.Warning($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");

        GameManager.UI.HideUIForm("LoadingMenu");

        purchaseFailAction?.Invoke(failureReason);
        Clear();

        //if (failureReason == PurchaseFailureReason.UserCancelled)
        //{
        //    GameManager.UI.ShowUIForm<LoadingMenu>();
        //}

        isPurchasing = false;
    }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (startInit)
        {
            if (IsInitialized)
            {
                timer = 0;
                startInit = false;
                isInitOverTime = false;
                return;
            }

            if (timer < overTime)
            {
                timer += elapseSeconds;
            }
            else
            {
                timer = 0;
                startInit = false;
                isInitOverTime = true;
                Log.Info("In-App Purchasing initialize over time");
            }
        }
    }

    public override void Shutdown()
    {
    }

    public void Clear()
    {
        purchaseSuccessAction = null;
        purchaseFailAction = null;
    }

    /// <summary>
    /// 恢复不可消耗商品
    /// </summary>
    private void RestorePurchases()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            //Log.Info("RestorePurchases products all {0}", m_StoreController.products.all.Length);
            foreach (var product in m_StoreController.products.all)
            {
                //Log.Info("RestorePurchases product {0}|{1}", product.definition.id.ToString(), product.definition.type.ToString());
                if (!string.IsNullOrEmpty(product.receipt))
                {
                    //Log.Info("RestorePurchases product receipt {0},Id is{1}", product.receipt, product.definition.id);

                    if (product.definition.type == ProductType.NonConsumable) 
                    {
                        //Log.Info("RestorePurchases Get NonConsumable {0}", product.definition.id);

                        if (product.definition.id.Equals(productIDData.GetProductID(ProductNameType.Remove_Ads))
                            || product.definition.id.Equals(productIDData.GetProductID(ProductNameType.Remove_Ads_New_1))
                            || product.definition.id.Equals(productIDData.GetProductID(ProductNameType.Remove_Ads_New_Pro)))
                        {
                            GameManager.Ads.IsRemovePopupAds = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
