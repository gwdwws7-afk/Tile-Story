using DG.Tweening;
using Firebase.Analytics;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// 商店界面管理器
/// </summary>
public sealed class ShopMenuManager : PopupMenuForm
{
    public GameObject topPanel;
    public DelayButton closeButton;
    public DelayButton moreButton;
    public ScrollRect scrollRect;
    public VerticalLayoutGroup layoutGroup;
    public ContentSizeFitter contentSizeFitter;
    public Transform packageBarsRoot;
    public Transform shopBarPoolRoot;
    public Transform ShopAdsBar;

    public static int RecordSourceIndex = 0;//1：接关开启商店，2：购买道具开启商店
    public static bool RecordBuySuccess = false;//记录有购买成功

    private List<ShopBar> shopBars = new List<ShopBar>();
    private LinkedList<ShopBar> shopBarPool = new LinkedList<ShopBar>();
    private List<AsyncOperationHandle<GameObject>> asyncOperationHandles = new List<AsyncOperationHandle<GameObject>>();
    private bool isInitComplete;
    private bool isReset;
    private bool isUnfold;

    /// <summary>
    /// 是否展开
    /// </summary>
    public bool IsUnfold
    {
        get
        {
            return isUnfold;
        }
    }

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction, object userData)
    {
        base.OnInit(uiGroup, initCompleteAction, userData);

        //只有接关进商店 或者金币不足购买道具进商店 才打点 
        if (RecordSourceIndex == 1 || RecordSourceIndex == 2)
        {
            GameManager.Firebase.RecordMessageByEvent(
                "Shop_Open",
                new Parameter("Source", RecordSourceIndex == 1 ? "BuyContinue" : " BuyProps"));
        }

        moreButton.OnInit(OnMoreButtonClick);
        closeButton.OnInit(OnClose);

        bool isShowAdsBar = !GameManager.PlayerData.IsRemoveAds;
        ShopAdsBar.gameObject.SetActive(isShowAdsBar);

        InitShopBars(GetShopPackagesData(1), 0, () =>
        {
            isInitComplete = true;

            if (UIGroup != null && UIGroup.GroupName == "PopupUI")
            {
                RefreshShopBars();
            }
        });

        isReset = false;
        isUnfold = false;

        if (UIGroup != null)
        {
            closeButton.gameObject.SetActive(UIGroup.GroupName == "PopupUI");

            if (UIGroup.GroupName == "Area1")
            {
                Vector2 rect = Screen.safeArea.position;
                if (rect.y > 0)
                {
                    layoutGroup.padding.bottom = 200 + (int)(rect.y);
                }
            }
        }
    }

    public override void OnReset()
    {
        isReset = true;
        isUnfold = false;
        isInitComplete = false;

        Transform moreButtonTrans = moreButton.transform;
        moreButtonTrans.DOKill();
        moreButtonTrans.position = new Vector3(0, moreButtonTrans.position.y, 0);
        scrollRect.verticalNormalizedPosition = 1;

        for (int i = 0; i < shopBars.Count; i++)
        {
            RecycleShopBar(shopBars[i]);
        }
        shopBars.Clear();

        base.OnReset();
    }

    public override void OnRelease()
    {
        //只有接关进商店 或者金币不足购买道具进商店 才打点  【并且没有发生内购的情况下】
        if (RecordBuySuccess && (RecordSourceIndex == 1 || RecordSourceIndex == 2))
        {
            GameManager.Firebase.RecordMessageByEvent(
                "IAP_Succeed",
                new Parameter("Source", RecordSourceIndex == 1 ? "BuyContinue" : " BuyProps"));
        }
        else if (!RecordBuySuccess && (RecordSourceIndex == 1 || RecordSourceIndex == 2))
        {
            GameManager.Firebase.RecordMessageByEvent(
                "Shop_Close",
                new Parameter("Source", RecordSourceIndex == 1 ? "BuyContinue" : " BuyProps"));
        }

        isReset = true;
        isUnfold = false;

        for (int i = 0; i < shopBars.Count; i++)
        {
            shopBars[i].OnRelease();
        }
        shopBars.Clear();

        foreach (ShopBar bar in shopBarPool)
        {
            bar.OnRelease();
        }
        shopBarPool.Clear();

        for (int i = 0; i < asyncOperationHandles.Count; i++)
        {
            if (asyncOperationHandles[i].IsValid())
            {
                Addressables.ReleaseInstance(asyncOperationHandles[i]);
            }
        }
        asyncOperationHandles.Clear();

        RecordSourceIndex = 0;
        RecordBuySuccess = false;
        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        topPanel.SetActive(true);
        gameObject.SetActive(true);
        GameManager.Sound.PlayUIOpenSound();

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        OnReset();
        GameManager.Sound.PlayUICloseSound();
        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnClose()
    {
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ContinueLevelTime));
        GameManager.UI.HideUIForm(this);
        if(GameManager.PlayerData.CoinNum>=800&&
           GameManager.PlayerData.CoinNum<900)
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.ShopMenuClose_Coin_800_900);
    }

    public override bool CheckInitComplete()
    {
        return isInitComplete;
    }

    public override void OnRefocus()
    {
        RefreshShopBars();
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
        if (depthInUIGroup == uiGroupDepth)
        {
            transform.SetAsLastSibling();
        }

        base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
    }

    public void RefreshShopBars()
    {
        for (int i = 0; i < shopBars.Count; i++)
        {
            RecycleShopBar(shopBars[i]);
        }
        shopBars.Clear();

        if (!isUnfold)
        {
            List<ShopPackageData> packageDatas = GetShopPackagesData(1);
            InitShopBars(packageDatas, 0, () =>
            {
                ShowShopBars(packageDatas, true);
            });
        }
        else
        {
            List<ShopPackageData> packageDatas = GetShopPackagesData(2);

            InitShopBars(packageDatas, 0, () =>
            {
                ShowShopBars(packageDatas, false);
            });
        }
    }

    private ShopBar GetShopBar(string barName)
    {
        LinkedListNode<ShopBar> current = shopBarPool.First;
        while (current != null)
        {
            LinkedListNode<ShopBar> next = current.Next;
            if (current.Value.BarName == barName)
            {
                shopBarPool.Remove(current);
                return current.Value;
            }

            current = next;
        }

        return null;
    }

    private void RecycleShopBar(ShopBar shopBar)
    {
        if (shopBar != null)
        {
            shopBar.OnReset();
            shopBar.OnHide();
            shopBar.transform.SetParent(shopBarPoolRoot);
            shopBarPool.AddLast(shopBar);
        }
    }

    private void InitShopBars(List<ShopPackageData> packageDatas, int index, Action callback)
    {
        if (index >= packageDatas.Count)
        {
            for (int i = shopBars.Count - 1; i >= index + 1; i--)  
            {
                RecycleShopBar(shopBars[i]);
                shopBars.RemoveAt(i);
            }

            callback?.Invoke();
            return;
        }

        string barName = packageDatas[index].BarName;
        int replaceIndex = -1;

        if (shopBars.Count > index)
        {
            try
            {
                if (shopBars[index].BarName == barName)
                {
                    shopBars[index].OnReset();
                    InitShopBars(packageDatas, index + 1, callback);
                    return;
                }
                else
                {
                    replaceIndex = index;
                    RecycleShopBar(shopBars[replaceIndex]);
                    shopBars[replaceIndex] = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        ShopBar targetBar = GetShopBar(barName);
        if (targetBar == null)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(barName, packageBarsRoot);
            handle.Completed += (obj) =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    if (isReset)
                    {
                        RecycleShopBar(obj.Result.GetComponent<ShopBar>());
                        return;
                    }

                    if (replaceIndex == -1)
                    {
                        shopBars.Add(obj.Result.GetComponent<ShopBar>());
                    }
                    else
                    {
                        if (replaceIndex >= shopBars.Count)
                        {
                            return;
                        }
                        shopBars[replaceIndex] = obj.Result.GetComponent<ShopBar>();
                    }
                    InitShopBars(packageDatas, index + 1, callback);
                }
                else
                {
                    Log.Error("load {0} asset fail", barName);
                }
            };
            asyncOperationHandles.Add(handle);
        }
        else
        {
            targetBar.transform.SetParent(packageBarsRoot);
            if (replaceIndex == -1)
            {
                shopBars.Add(targetBar.GetComponent<ShopBar>());
            }
            else
            {
                shopBars[replaceIndex] = targetBar.GetComponent<ShopBar>();
            }
            InitShopBars(packageDatas, index + 1, callback);
        }
    }

    private void ShowShopBars(List<ShopPackageData> packageDatas, bool isSimple)
    {
        layoutGroup.enabled = true;
        contentSizeFitter.enabled = true;

        bool skipAnim = false;

        int count = 0;
        for (int i = 0; i < packageDatas.Count; i++)
        {
            if (packageDatas[i] == null) continue;
            if (i > shopBars.Count - 1) 
            {
                break;
            }

            shopBars[i].OnInit(packageDatas[i]);

            shopBars[i].OnShow(0.04f * count, skipAnim);
            count++;
        }

        if (isSimple)
        {
            Transform moreButtonParent = moreButton.transform.parent;
            moreButtonParent.transform.SetAsLastSibling();

            if (!skipAnim)
            {
                Transform moreButtonTrans = moreButton.transform;
                float originalPosX = 0;
                moreButtonTrans.localPosition += new Vector3(800f, 0);
                moreButtonParent.gameObject.SetActive(true);
                moreButtonTrans.DOLocalMoveX(originalPosX - 50f, 0.2f).SetDelay(0.04f * (count + 1)).onComplete = () =>
                {
                    moreButtonTrans.DOLocalMoveX(originalPosX, 0.2f);
                };
            }
            else
            {
                moreButton.transform.localPosition = new Vector3(0, moreButton.transform.localPosition.y);
                moreButtonParent.gameObject.SetActive(true);
            }
        }

        Invoke("CloseLayoutGroup", 0.5f);
    }

    private void OnMoreButtonClick()
    {
        moreButton.transform.parent.gameObject.SetActive(false);

        for (int i = 0; i < shopBars.Count; i++)
        {
            RecycleShopBar(shopBars[i]);
        }
        shopBars.Clear();

        List<ShopPackageData> totalPackageDatas= GetShopPackagesData(2);

        InitShopBars(totalPackageDatas, 0, () =>
         {
             ShowShopBars(totalPackageDatas, false);
         });

        isUnfold = true;
    }

    private void CloseLayoutGroup()
    {
        if (contentSizeFitter != null && layoutGroup != null) 
        {
            contentSizeFitter.enabled = false;
            layoutGroup.enabled = false;
        }
    }

    /// <summary>
    /// list
    /// </summary>
    /// <param name="packageDataList"></param>
    /// <param name="type"></param> 1:短 2：长
    private List<ShopPackageData> GetShopPackagesData(int type)
    {
        List<ShopPackageData> dataList = GameManager.DataTable.GetDataTable<DTShopPackageData>().Data.GetShopPackageDatas(type);
        dataList.Sort(delegate (ShopPackageData x, ShopPackageData y)
        {
            return x.Sort.CompareTo(y.Sort);
        });

        var removeAdsData = GameManager.DataTable.GetDataTable<DTShopPackageData>().Data.GetShopPackageData(ProductNameType.Remove_Ads_New_Pro);

        //SpecialOffer礼包只能购买一次
        bool everBoughtSpecialOffer = GameManager.PlayerData.GetEverBoughtSpecialOfferPack();
        //if (everBoughtSpecialOffer)
        //    dataList.RemoveAll(data => data.ID == 30001);

        List<ShopPackageData> returnList = new List<ShopPackageData>();
        //短模式 最多只显示两个礼包类Bar
        if (type == 1)
        {
            int packageBarCount = 0;
            for (int i = 0; i < dataList.Count; ++i)
            {
                if (everBoughtSpecialOffer && dataList[i].BarName == "ShopSpecialOfferBar")
                    continue;

                if (dataList[i].BarName == "ShopSpecialOfferBar" ||
                    dataList[i].BarName == "ShopPackageBar")
                {
                    if (packageBarCount < 2)
                    {
                        packageBarCount++;
                        returnList.Add(dataList[i]);
                    }
                }
                else
                {
                    returnList.Add(dataList[i]);
                }
            }
            return returnList;
        }
        //长模式显示几乎所有获取出来的礼包列表
        else if (type == 2)
        {
            for (int i = 0; i < dataList.Count; ++i)
            {
                if (everBoughtSpecialOffer && dataList[i].BarName == "ShopSpecialOfferBar")
                    continue;
                returnList.Add(dataList[i]);
            }

            if (!GameManager.Ads.IsRemovePopupAds && removeAdsData != null)
                returnList.Add(removeAdsData);

            return returnList;
        }
        else
        {
            Log.Error("Unexpected");
            return dataList;
        }
    }
}
