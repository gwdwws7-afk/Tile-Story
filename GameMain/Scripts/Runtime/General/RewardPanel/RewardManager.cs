using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 奖励管理器
/// </summary>
public sealed partial class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    public RewardArea rewardArea;

    private RewardArea currentRewardArea;
    private RewardPanel currentRewardPanel;

    private readonly Dictionary<TotalItemData, int> needGetRewardsDic = new Dictionary<TotalItemData, int>();
    private readonly Dictionary<TotalItemData, int> gettingRewardDic = new Dictionary<TotalItemData, int>();
    private readonly Dictionary<RewardPanelType, RewardPanel> rewardPanelDic = new Dictionary<RewardPanelType, RewardPanel>();
    private readonly Dictionary<string, bool> loadedFlag = new Dictionary<string, bool>();
    private Transform cachedTransform;
    private Action onStartGetReward;
    private Action onGetRewardComplete;
    public Action onRewardPanelStartShow;
    private bool autoGetReward;
    private bool startShowLoading;
    private float autoGetRewardDelayTime = 0.2f;

    private Action tempCompleteAction = null;

    private LinkedList<ICoinFlyReceiver> coinFlyReceivers = new LinkedList<ICoinFlyReceiver>();
    private LinkedList<ILifeFlyReceiver> lifeFlyReceivers = new LinkedList<ILifeFlyReceiver>();
    private LinkedList<IItemFlyReceiver> itemFlyReceivers = new LinkedList<IItemFlyReceiver>();

    /// <summary>
    /// 已经注册的需要获取的奖励数量
    /// </summary>
    public int NeedGetRewardCount { get { return needGetRewardsDic.Count; } }

    /// <summary>
    /// 正在展示获取的奖励数量
    /// </summary>
    public int GettingRewardCount { get { return gettingRewardDic.Count; } }

    public bool ForceHideRewardPanelBg { get; set; }

    public bool ForbidHideRewardPanelBg { get; set; }

    public float AutoGetRewardDelayTime
    {
        get
        {
            return autoGetRewardDelayTime;
        }
        set
        {
            autoGetRewardDelayTime = value;
        }
    }

    public RewardArea RewardArea
    {
        get
        {
            if (currentRewardArea != null)
            {
                return currentRewardArea;
            }
            return rewardArea;
        }
    }

    public RewardPanel RewardPanel
    {
        get
        {
            return currentRewardPanel;
        }
    }

    /// <summary>
    /// 金币飞行接收对象
    /// </summary>
    /// <remarks>默认金币飞行对象集合中最新注册的对象</remarks>
    public ICoinFlyReceiver CoinFlyReceiver
    {
        get
        {
            var lastReceiver = coinFlyReceivers.Last;
            while (lastReceiver != null)
            {
                var obj = lastReceiver.Value.GetReceiverGameObject();
                if (obj != null && obj.activeInHierarchy)
                {
                    return lastReceiver.Value;
                }
                else
                {
                    lastReceiver= lastReceiver.Previous;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 生命飞行接收对象
    /// </summary>
    /// <remarks>默认生命飞行对象集合中最新注册的对象</remarks>
    public ILifeFlyReceiver LifeFlyReceiver
    {
        get
        {
            if (lifeFlyReceivers.Last != null)
                return lifeFlyReceivers.Last.Value;
            return null;
        }
    }


    public IItemFlyReceiver ItemFlyReceiver
    {
        get
        {
            var lastReceiver = itemFlyReceivers.Last;
            while (lastReceiver != null)
            {
                var obj = lastReceiver.Value.GetReceiverGameObject();
                if (obj != null && obj.activeInHierarchy)
                {
                    return lastReceiver.Value;
                }
                else
                {
                    lastReceiver= lastReceiver.Previous;
                }
            }
            return null;
        }
    }

    private void Awake()
    {
        Instance = this;
        cachedTransform = transform;
    }

    private void Update()
    {
        OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
    }

    public void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (startShowLoading && CheckLoadComplete())
        {
            startShowLoading = false;

            void animAction()
            {
                if (!autoGetReward)
                {
                    if (currentRewardPanel == null)
                    {
                        Log.Error("Can't click get reward because currentRewardPanel is null");
                        ShowGetRewardAnim();
                    }
                    else
                    {
                        currentRewardPanel.SetOnClickEvent(() =>
                        {
                            ShowGetRewardAnim();
                            
                            UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
                        });
                    }
                }
                else
                {
                    GameManager.Task.AddDelayTriggerTask(autoGetRewardDelayTime, () =>
                    {
                        ShowGetRewardAnim();
                    });
                }
            }

            if (currentRewardPanel != null)
            {
                currentRewardPanel.OnShow(RewardArea, animAction);
                onRewardPanelStartShow?.Invoke();
                onRewardPanelStartShow = null;
            }
            else
            {
                RewardArea.OnShow();
                animAction();
            }
        }
    }

    public void OnReset()
    {
        needGetRewardsDic.Clear();
        gettingRewardDic.Clear();
        loadedFlag.Clear();
        RewardArea.OnReset();

        currentRewardPanel = null;
        currentRewardArea = null;
        onStartGetReward = null;
        onGetRewardComplete = null;
        onRewardPanelStartShow = null;
        autoGetRewardDelayTime = 0.2f;

        foreach (var rewardPanelPair in rewardPanelDic)
        {
            rewardPanelPair.Value.OnReset();
        }
    }

    public void OnRelease()
    {
        needGetRewardsDic.Clear();
        gettingRewardDic.Clear();
        loadedFlag.Clear();
        RewardArea.OnRelease();

        currentRewardPanel = null;
        currentRewardArea = null;
        onStartGetReward = null;
        onGetRewardComplete = null;
        onRewardPanelStartShow = null;

        foreach (var rewardPanelPair in rewardPanelDic)
        {
            rewardPanelPair.Value.OnRelease();
        }

        if (rewardPanelDic.Count > 0)
        {
            foreach (var rewardPanelPair in rewardPanelDic)
            {
                Addressables.ReleaseInstance(rewardPanelPair.Value.gameObject);
            }
            rewardPanelDic.Clear();
        }

        GameManager.ObjectPool.DestroyObjectPool("DefaultFlyReward");
        GameManager.ObjectPool.DestroyObjectPool("CoinFlyReward");
        GameManager.ObjectPool.DestroyObjectPool("LifeFlyReward");
    }

    /// <summary>
    /// 添加获取的奖励
    /// </summary>
    /// <param name="productNameType">商品类型</param>
    public void AddNeedGetReward(ProductNameType productNameType)
    {
        ShopPackageData data = GameManager.DataTable.GetDataTable<DTShopPackageData>().Data.GetShopPackageData(productNameType);
        List<ItemData> itemDatas = data?.GetItemDatas();
        if(itemDatas!=null)
            for (int i = 0; i < itemDatas.Count; i++)
            {
                AddNeedGetReward(itemDatas[i].type, itemDatas[i].num);

                if (itemDatas[i].type == TotalItemData.Coin)
                {
                    GameManager.Firebase.RecordCoinGet("IAP_Buy", itemDatas[i].num);
                }
                else
                {
                    GameManager.Firebase.RecordLevelToolsGet("IAP_Buy", itemDatas[i].type, itemDatas[i].num);
                }
            }
    }
    
    /// <summary>
    /// 添加获取的奖励
    /// </summary>
    /// <param name="dict"></param>
    public void AddNeedGetReward(Dictionary<TotalItemData,int> dict)
    {
        foreach (var data in dict)
        {
            AddNeedGetReward(data.Key, data.Value);
        }
    }

    /// <summary>
    /// 添加获取的奖励
    /// </summary>
    /// <param name="rewardTypeList">奖励类型集合</param>
    /// <param name="rewardNumList">奖励数量集合</param>
    public void AddNeedGetReward(IList<TotalItemData> rewardTypeList, IList<int> rewardNumList)
    {
        if (rewardTypeList.Count != rewardNumList.Count)
        {
            Log.Error("AddNeedGetReward Fail.rewardTypeList count not match rewardNumList count");
            return;
        }

        for (int i = 0; i < rewardTypeList.Count; i++)
        {
            AddNeedGetReward(rewardTypeList[i], rewardNumList[i]);
        }
    }

    /// <summary>
    /// 添加获取的奖励
    /// </summary>
    /// <param name="rewardType">奖励类型</param>
    /// <param name="rewardNum">奖励数量</param>
    /// <param name="saveData">保存数据到本地</param>
    public void AddNeedGetReward(TotalItemData rewardType, int rewardNum, bool saveData = true)
    {
        InternalAddNeedGetReward(rewardType, rewardNum, saveData);
    }

    /// <summary>
    /// 增加外部的飞行奖励
    /// </summary>
    /// <param name="flyReward"></param>
    public void AddRewardFlyObject(FlyReward flyReward)
    {
        RewardArea.AddRewardFlyObject(flyReward);
    }

    /// <summary>
    /// 获取奖励页面
    /// </summary>
    public bool GetRewardPanel(RewardPanelType rewardPanelType, out RewardPanel rewardPanel)
    {
        if (rewardPanelDic.TryGetValue(rewardPanelType, out rewardPanel))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 创建奖励页面
    /// </summary>
    public void CreateRewardPanel(RewardPanelType rewardPanelType, Action<RewardPanel> onCreateComplete)
    {
        InternalCreateRewardPanel(rewardPanelType, onCreateComplete);
    }

    /// <summary>
    /// 展示获取的奖励
    /// </summary>
    public bool ShowNeedGetRewards(RewardPanelType panelType, bool autoGetReward, Action onGetRewardComplete, Action onStartGetReward = null, Action onCreatePanelSuccess = null)
    {
        if (gettingRewardDic.Count > 0)
        {
            Debug.LogError("Getting reward anim not finish");
            gettingRewardDic.Clear();
            this.onGetRewardComplete?.Invoke();
            this.onGetRewardComplete = null;
            // onCreatePanelSuccess?.Invoke();
            // return false;
        }

        if (needGetRewardsDic.Count <= 0)
        {
            Log.Debug("NeedGetRewardCount is less than 0");
            onGetRewardComplete?.Invoke();
            return false;
        }

        tempCompleteAction = null;
        Log.Debug("tempCompleteAction set Null！");
        
        string soundName;
        switch (panelType)
        {
            case RewardPanelType.SkillUnlockRewardPanel:
                soundName = SoundType.SFX_DecorationObjectFinished.ToString();
                break;
            case RewardPanelType.ClimbBeanstalkChestRewardPanel:
            case RewardPanelType.MergeDigChestRewardPanel:
            case RewardPanelType.CardSetRewardPanel:
            case RewardPanelType.CardFinalChestRewardPanel:
            case RewardPanelType.CardStarChestRewardPanel:
            case RewardPanelType.CardTransparentRewardPanel:
                soundName = "";
                break;
            default:
                soundName = "SFX_shopBuySuccess";
                break;
        }
        GameManager.Sound.PlayAudio(soundName);

        foreach (KeyValuePair<TotalItemData, int> rewardPair in needGetRewardsDic)
        {
            gettingRewardDic.Add(rewardPair.Key, rewardPair.Value);
        }
        needGetRewardsDic.Clear();

        if (panelType != RewardPanelType.None)
        {
            if (GetRewardPanel(panelType, out RewardPanel rewardPanel))
            {
                rewardPanel.OnInit(autoGetReward);
                currentRewardPanel = rewardPanel;
                currentRewardArea = rewardPanel.CustomRewardArea;

                currentRewardPanel.SetClearBgActive(true);
                RewardArea.OnInit(gettingRewardDic, autoGetReward);

                onCreatePanelSuccess?.Invoke();
            }
            else
            {
                InternalCreateRewardPanel(panelType, (panel) =>
                {
                    panel.OnInit(autoGetReward);
                    currentRewardPanel = panel;
                    currentRewardArea = panel.CustomRewardArea;
                    currentRewardPanel.SetClearBgActive(true);
                    RewardArea.OnInit(gettingRewardDic, autoGetReward);

                    onCreatePanelSuccess?.Invoke();
                });
            }
        }
        else
        {
            currentRewardPanel = null;
            currentRewardArea = null;
            RewardArea.OnInit(gettingRewardDic, autoGetReward);

            onCreatePanelSuccess?.Invoke();
        }

        this.autoGetReward = autoGetReward;
        this.onStartGetReward = onStartGetReward;
        this.onGetRewardComplete = onGetRewardComplete;

        startShowLoading = true;

        return true;
    }
    
    public bool ShowNeedGetRewards(RewardPanel rewardPanel, bool autoGetReward, Action onGetRewardComplete, Action onStartGetReward = null, Action onCreatePanelSuccess = null)
    {
        if (gettingRewardDic.Count > 0)
        {
            Debug.LogError("Getting reward anim not finish");
            onGetRewardComplete?.Invoke();
            return false;
        }

        if (needGetRewardsDic.Count <= 0)
        {
            Log.Debug("NeedGetRewardCount is less than 0");
            onGetRewardComplete?.Invoke();
            return false;
        }

        tempCompleteAction = null;

        foreach (KeyValuePair<TotalItemData, int> rewardPair in needGetRewardsDic)
        {
            gettingRewardDic.Add(rewardPair.Key, rewardPair.Value);
        }
        needGetRewardsDic.Clear();

        if (rewardPanel!=null)
        {
            rewardPanel.OnInit(autoGetReward);
            currentRewardPanel = rewardPanel;
            currentRewardArea = rewardPanel.CustomRewardArea;

            currentRewardPanel.SetClearBgActive(true);
            RewardArea.OnInit(gettingRewardDic, autoGetReward);

            onCreatePanelSuccess?.Invoke();
        }

        this.autoGetReward = autoGetReward;
        this.onStartGetReward = onStartGetReward;
        this.onGetRewardComplete = onGetRewardComplete;

        startShowLoading = true;

        return true;
    }
    
    public bool ShowNeedGetRewards(RewardPanelType panelType,Dictionary<TotalItemData,int> rewardDict, bool autoGetReward, Action onGetRewardComplete,
        Action onStartGetReward = null, Action onCreatePanelSuccess = null)
    {
        Dictionary<TotalItemData, int> rewardDict1 = new Dictionary<TotalItemData, int>();
        Dictionary<TotalItemType, int> additionalDict1 = new Dictionary<TotalItemType, int>();

        if (needGetRewardsDic.Count != ItemModel.Instance.Data.AdditionalDict.Count)
        {
            //数量不等
            string str = "";
            foreach (var item in needGetRewardsDic)
            {
                str += $"\n {item.Key}=={item.Value}";
            }
            Log.Error($"警告： needGetRewardsDic:{str}");
            str = "";
            foreach (var item in ItemModel.Instance.Data.AdditionalDict)
            {
                str += $"\n {item.Key}=={item.Value}";
            }
            Log.Error($"警告： needGetRewardsDic:{ItemModel.Instance.Data.AdditionalDict}");
        }

        foreach (var data in needGetRewardsDic)
        {
            rewardDict1.Add(data.Key,data.Value);
        }
        needGetRewardsDic.Clear();

        foreach (var data in ItemModel.Instance.Data.AdditionalDict)
        {
            additionalDict1.Add(data.Key, -data.Value);
        }
        ItemModel.Instance.Data.AdditionalDict.Clear();

        AddNeedGetReward(rewardDict);
        return ShowNeedGetRewards(panelType,autoGetReward, () =>
        {
            needGetRewardsDic.Clear();
            gettingRewardDic.Clear();
            
            onGetRewardComplete?.InvokeSafely();
            AddNeedGetReward(rewardDict1);
        },onStartGetReward, onCreatePanelSuccess);
    }

    /// <summary>
    /// 播放奖励获取的动画
    /// </summary>
    private void ShowGetRewardAnim()
    {
        onStartGetReward?.Invoke();

        tempCompleteAction = onGetRewardComplete;
        Action finish = () => 
        {
            GameManager.PlayerData.SyncAllItemData();//同步数据
            RewardArea.ShowGetRewardAnim(() =>
            {
                if(RewardPanel)RewardPanel.SetClearBgActive(false);

                if (tempCompleteAction != null)
                {
                    OnReset();
                }
                else
                {
                    gettingRewardDic.Clear();
                    loadedFlag.Clear();
                    RewardArea.OnReset();
                }

                tempCompleteAction?.Invoke();
            });
        };

        if (currentRewardPanel != null)
        {
            currentRewardPanel.ClearOnClickEvent();
            currentRewardPanel.OnHide(false,finish);
        }
        else
        {
            finish.Invoke();
            //RewardArea.ShowGetRewardAnim(finish);
        }
    }

    /// <summary>
    /// 资源是否加载完毕
    /// </summary>
    public bool CheckLoadComplete()
    {
        foreach (KeyValuePair<string, bool> flag in loadedFlag)
        {
            if (!flag.Value)
            {
                return false;
            }
        }
        return RewardArea.CheckLoadComplete();
    }

    public void SaveRewardData(ProductNameType productNameType)
    {
        ShopPackageData data = GameManager.DataTable.GetDataTable<DTShopPackageData>().Data.GetShopPackageData(productNameType);
        List<ItemData> itemDatas = data?.GetItemDatas();
        if (itemDatas != null)
            for (int i = 0; i < itemDatas.Count; i++)
            {
                SaveRewardData(itemDatas[i].type, itemDatas[i].num);

                if (itemDatas[i].type == TotalItemData.Coin)
                {
                    GameManager.Firebase.RecordCoinGet("IAP_Buy", itemDatas[i].num);
                }
                else
                {
                    GameManager.Firebase.RecordLevelToolsGet("IAP_Buy", itemDatas[i].type, itemDatas[i].num);
                }
            }
    }

    /// <summary>
    /// 保存奖励数据
    /// </summary>
    /// <param name="rewardType">奖励类型</param>
    /// <param name="rewardNum">奖励数量</param>
    public void SaveRewardData(TotalItemData rewardType, int rewardNum, bool isSyncData = false)
    {
        if (rewardType.TotalItemType == TotalItemType.RemoveAds)
        {
            GameManager.Ads.IsRemovePopupAds = true;
        }
        else if (rewardType.TotalItemType == TotalItemType.Item_BgID)
        {
            GameManager.PlayerData.BuyBGID(rewardType.RefID);
        }
        else if(rewardType.TotalItemType == TotalItemType.Item_TileID)
        {
            GameManager.PlayerData.BuyTileID(rewardType.RefID);
        }
        else
        {
            if (isSyncData)
            {
                GameManager.PlayerData.AddItemNum(rewardType, rewardNum);
                if (rewardType == TotalItemData.Coin)
                    GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(rewardNum, null));
                else if (rewardType == TotalItemData.InfiniteLifeTime)
                    GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(rewardNum, null));
            }
            else
            {
                GameManager.PlayerData.AddAdditionalItem(rewardType, rewardNum);

            }
            return;
        }
    }

    #region Receiver

    /// <summary>
    /// 注册金币接收对象
    /// </summary>
    /// <param name="coinFlyReceiver">金币接收对象</param>
    public void RegisterCoinFlyReceiver(ICoinFlyReceiver coinFlyReceiver)
    {
        foreach (ICoinFlyReceiver receiver in coinFlyReceivers)
        {
            if (receiver == coinFlyReceiver)
            {
                Log.Warning("coinflyreceiver already registered");
                return;
            }
        }

        coinFlyReceivers.AddLast(coinFlyReceiver);
    }

    /// <summary>
    /// 注销金币接收对象
    /// </summary>
    /// <param name="coinFlyReceiver">金币接收对象</param>
    public void UnregisterCoinFlyReceiver(ICoinFlyReceiver coinFlyReceiver)
    {
        coinFlyReceivers.Remove(coinFlyReceiver);
    }

    /// <summary>
    /// 注册生命接收对象
    /// </summary>
    /// <param name="lifeFlyReceiver">生命接收对象</param>
    public void RegisterLifeFlyReceiver(ILifeFlyReceiver lifeFlyReceiver)
    {
        foreach (ILifeFlyReceiver receiver in lifeFlyReceivers)
        {
            if (receiver == lifeFlyReceiver)
            {
                Log.Warning("lifeflyreceiver already registered");
                return;
            }
        }

        lifeFlyReceivers.AddLast(lifeFlyReceiver);
    }

    /// <summary>
    /// 注销生命接收对象
    /// </summary>
    /// <param name="lifeFlyReceiver">生命接收对象</param>
    public bool UnregisterLifeFlyReceiver(ILifeFlyReceiver lifeFlyReceiver)
    {
        foreach (ILifeFlyReceiver receiver in lifeFlyReceivers)
        {
            if (receiver == lifeFlyReceiver)
            {
                lifeFlyReceivers.Remove(receiver);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 注册宝石飞行接收器
    /// </summary>
    /// <param name="diamondFlyReceiver"></param>
    public void RegisterItemFlyReceiver(IItemFlyReceiver itemFlyReceiver)
    {
        foreach (var receiver in itemFlyReceivers)
        {
            if (receiver == itemFlyReceiver)
            {
                Log.Warning("ItemFlyReceiver already registered");
                return;
            }
        }
        itemFlyReceivers.AddLast(itemFlyReceiver);
    }


    /// <summary>
    /// 卸载宝石飞行接收器
    /// </summary>
    /// <param name="diamondFlyReceiver"></param>
    /// <returns></returns>
    public bool UnregisterItemFlyReceiver(IItemFlyReceiver itemFlyReceiver)
    {
        itemFlyReceivers.Remove(itemFlyReceiver);
        return true;
    }
    #endregion

    /// <summary>
    /// 生成奖励界面
    /// </summary>
    /// <param name="rewardPanelType">奖励界面类型</param>
    private void InternalCreateRewardPanel(RewardPanelType rewardPanelType, Action<RewardPanel> onCreateComplete)
    {
        string panelName = rewardPanelType.ToString();
        if (loadedFlag.TryGetValue(panelName, out bool isLoading))
        {
            if (!isLoading)
            {
                return;
            }
        }

        if (rewardPanelDic.ContainsKey(rewardPanelType))
        {
            return;
        }

        loadedFlag[panelName] = false;
        Addressables.InstantiateAsync(panelName, cachedTransform).Completed += (obj) =>
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                RewardPanel rewardPanel = obj.Result.GetComponent<RewardPanel>();
                rewardPanel.RewardPanelType = rewardPanelType;
                rewardPanel.OnHide(true, null);
                rewardPanelDic.Add(rewardPanelType, rewardPanel);

                onCreateComplete?.Invoke(rewardPanel);
                loadedFlag[panelName] = true;
            }
            else
            {
                Log.Error("load {0} fail", panelName);
            }
        };
    }

    private void InternalAddNeedGetReward(TotalItemData rewardType, int rewardNum, bool saveData)
    {
        if (rewardNum == 0) return;
        // 补充：如果不在卡牌活动中，卡包不加
        if (!CardModel.Instance.IsInCardActivity)
        {
            if (rewardType == TotalItemData.CardPack1 ||
                rewardType == TotalItemData.CardPack2 ||
                rewardType == TotalItemData.CardPack3 ||
                rewardType == TotalItemData.CardPack4 ||
                rewardType == TotalItemData.CardPack5)
                return;
        }
        if(saveData)
            SaveRewardData(rewardType, rewardNum);

        //if (rewardType != TotalItemData.RemoveAds)
        //{
        //    if (needGetRewardsDic.TryGetValue(rewardType, out int num))
        //    {
        //        needGetRewardsDic[rewardType] = rewardNum + num;
        //    }
        //    else
        //    {
        //        needGetRewardsDic.Add(rewardType, rewardNum);
        //    }
        //}
        if (needGetRewardsDic.TryGetValue(rewardType, out int num))
        {
            needGetRewardsDic[rewardType] = rewardNum + num;
        }
        else
        {
            needGetRewardsDic.Add(rewardType, rewardNum);
        }
    }

    public IItemFlyReceiver GetReceiverByItemType(TotalItemData type)
    {
        if (type == TotalItemData.Star)
        {
            return GetLastReceiverByType(ReceiverType.Star);
        }
        else if (type == TotalItemData.Prop_AddOneStep || type == TotalItemData.InfiniteAddOneStepBoost) 
        {
            var receiver = GetLastReceiverByType(ReceiverType.Boost2);
            if (receiver == null)
                receiver = GetLastReceiverByType(ReceiverType.Common);
            return receiver;
        }
        else if (type == TotalItemData.FireworkBoost || type == TotalItemData.InfiniteFireworkBoost)
        {
            var receiver = GetLastReceiverByType(ReceiverType.Boost3);
            if (receiver == null)
                receiver = GetLastReceiverByType(ReceiverType.Common);
            return receiver;
        }
        else if (type == TotalItemData.Pickaxe)
        {
            var receiver = GetLastReceiverByType(ReceiverType.Pickaxe);
            if (receiver == null)
                receiver = GetLastReceiverByType(ReceiverType.Common);
            return receiver;
        }
        else if (type == TotalItemData.MergeEnergyBox)
        {
            var receiver = GetLastReceiverByType(ReceiverType.MergeEnergyBox);
            if (receiver == null)
                receiver = GetLastReceiverByType(ReceiverType.Common);
            return receiver;
        }
        else
        {
            return GetLastReceiverByType(ReceiverType.Common);
        }
        //return type switch
        //{
        //    TotalItemData.Star => GetLastReceiverByType(ReceiverType.Star),
        //    _ => GetLastReceiverByType(ReceiverType.Common)
        //};
    }

    private IItemFlyReceiver GetLastReceiverByType(ReceiverType type)
    {
        var lastNode = itemFlyReceivers.Last;
        while (lastNode != null)
        {
            if ((lastNode.Value.ReceiverType & type) != 0) 
            {
                return lastNode.Value;
            }

            lastNode = lastNode.Previous;
        }

        return null;
    }
}
