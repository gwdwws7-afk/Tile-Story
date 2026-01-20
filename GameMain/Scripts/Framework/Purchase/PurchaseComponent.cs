using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

/// <summary>
/// 购买组件
/// </summary>
public sealed class PurchaseComponent : GameFrameworkComponent
{
    private IPurchaseManager purchaseManager = null;
    private bool isTryInitialize = false;
    private bool debuggerMode = false;

    const string environment = "production";

    public bool DebuggerMode { get => debuggerMode; set => debuggerMode = value; }

    public bool IsPurchasing { get => purchaseManager.IsPurchasing; }

    protected override void Awake()
    {
        base.Awake();

        purchaseManager = GameFrameworkEntry.GetModule<PurchaseManager>();
        if (purchaseManager == null)
        {
            Log.Fatal("Purchase manager is invalid.");
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        InitializePurchasing(1f);
    }

    /// <summary>
    /// 购买商品
    /// </summary>
    public void BuyProduct(ProductNameType productType, Action successAction = null, Action<PurchaseFailureReason> failAction = null)
    {
#if UNITY_EDITOR
        debuggerMode = true;    
#endif

        if (debuggerMode)
        {
            purchaseManager.OnPurchaseSuccess(productType.ToString());
            successAction?.Invoke();
            return;
        }

        if (!CheckInternetReachable())
        {
            ShowInaccessiableMenu();
            failAction?.Invoke(PurchaseFailureReason.PurchasingUnavailable);
            return;
        }

        if (!purchaseManager.IsInitialized) 
        {
            if (!isTryInitialize)
            {
                isTryInitialize = true;
                GameManager.UI.ShowUIForm("LoadingMenu",obj =>
                {
                    StartCoroutine(TryInitializePurchasing(15, success =>
                    {
                        isTryInitialize = false;
                        GameManager.UI.HideUIForm("LoadingMenu");
                        if (success)
                        {
                            purchaseManager.BuyProduct(productType, successAction, failAction);
                        }
                        else
                        {
                            Log.Warning("Try Initialize Purchasing Fail");
                            ShowInaccessiableMenu();
                        }
                    }));
                }, userData: 20f);
            }
            else
            {
                Log.Warning("Already Try Initialize Purchasing");
            }
            return;
        }

        GameManager.UI.ShowUIForm("LoadingMenu",obj =>
        {
            purchaseManager.BuyProduct(productType, successAction, failAction);
        }, userData: 20f);
    }

    /// <summary>
    /// 获取商品价格
    /// </summary>
    public string GetPrice(ProductNameType productType)
    {
        return purchaseManager.GetPrice(productType);
    }

    /// <summary>
    /// 是否初始化完毕
    /// </summary>
    public bool CheckInitComplete()
    {
        return purchaseManager.IsInitialized || purchaseManager.IsInitOverTime;
    }

    public async void InitializeUnityServices()
    {
        try
        {
            var options = new InitializationOptions().SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);

            OnUnityServicesInitializeSuccess();
        }
        catch (Exception exception)
        {
            OnUnityServicesInitializeError(exception.Message);
        }
    }

    public void OnUnityServicesInitializeSuccess()
    {
        Log.Info("Unity Gaming Services has been successfully initialized.");
    }

    public void OnUnityServicesInitializeError(string message)
    {
        Log.Error("Unity Gaming Services failed to initialize with error: {0}", message);
    }

    private void InitializePurchasing(float overTime)
    {
        Log.Info("Init In-App Purchasing...");
        
        InitializeUnityServices();

        purchaseManager.InitializePurchasing(overTime);
    }

    IEnumerator TryInitializePurchasing(int timeout, Action<bool> onDone, float frequency = 10f)
    {
        Log.Info("IAP Start Initialize...");
        if (purchaseManager.IsInitialized)
        {
            onDone(true);
            yield break;
        }

        int totalWaitCount = (int)(timeout * frequency);
        float interval = 1f / frequency;
        InitializePurchasing(10f);
        WaitForSeconds delay = new WaitForSeconds(interval);
        for (int i = 0; i < totalWaitCount; i++)
        {
            yield return delay;
            if (purchaseManager.IsInitialized)
            {
                onDone(true);
                yield break;
            }
        }
        onDone(false);
    }

    private void ShowInaccessiableMenu()
    {
        GameManager.UI.ShowUIForm("PurchaseCancelMenu");
    }

    private bool CheckInternetReachable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}
