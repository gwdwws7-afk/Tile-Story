using System.Collections.Generic;
using UnityEngine;
using FrameworkLog;
using System;
using UnityGameFramework.Runtime;

/// <summary>
/// 游戏管理器
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    private static readonly LinkedList<GameFrameworkComponent> m_GameManagerComponents = new LinkedList<GameFrameworkComponent>();

    /// <summary>
    /// 获取数据表组件
    /// </summary>
    public static DataTableComponent DataTable { get; private set; }

    /// <summary>
    /// 获取数据结点组件
    /// </summary>
    public static DataNodeComponent DataNode { get; private set; }

    /// <summary>
    /// 获取有限状态机组件
    /// </summary>
    public static FsmComponent Fsm { get; private set; }

    /// <summary>
    /// 获取流程组件
    /// </summary>
    public static ProcedureComponent Procedure { get; private set; }

    /// <summary>
    /// 获取对象池组件
    /// </summary>
    public static ObjectPoolComponent ObjectPool { get; private set; }

    /// <summary>
    /// 获取游戏数据组件
    /// </summary>
    public static PlayerDataComponent PlayerData { get; private set; }

    /// <summary>
    /// 下载组件
    /// </summary>
    public static DownloadComponent Download { get; private set; }

    /// <summary>
    /// 事件组件
    /// </summary>
    public static EventComponent Event { get; private set; }

    /// <summary>
    /// 获取声音组件
    /// </summary>
    public static SoundComponent Sound { get; private set; }

    /// <summary>
    /// 获取场景组件
    /// </summary>
    public static SceneComponent Scene { get; private set; }

    /// <summary>
    /// 获取广告组件
    /// </summary>
    public static AdsComponent Ads { get; private set; }

    /// <summary>
    /// 获取任务组件
    /// </summary>
    public static TaskComponent Task { get; private set; }

    /// <summary>
    /// 获取进程组件
    /// </summary>
    public static ProcessComponent Process { get; private set; }

    /// <summary>
    /// 获取本地化组件
    /// </summary>
    public static LocalizationComponent Localization { get; private set; }

    /// <summary>
    /// 获取Firebase组件
    /// </summary>
    public static FirebaseComponent Firebase { get; private set; }

    /// <summary>
    /// 获取购买组件
    /// </summary>
    public static PurchaseComponent Purchase { get; private set; }

    /// <summary>
    /// AppsFlyer组件
    /// </summary>
    public static AppsFlyerComponent AppsFlyer { get; private set; }

    /// <summary>
    /// UI组件
    /// </summary>
    public static GameUIComponent UI { get; private set; }

    /// <summary>
    /// 成就目标组件
    /// </summary>
    public static ObjectiveComponent Objective { get; private set; }

    /// <summary>
    /// 通知模块
    /// </summary>
    public static NotificationComponent Notification { get; private set; }

    /// <summary>
    /// 活动模块
    /// </summary>
    public static ActivityComponent Activity { get; private set; }

    /// <summary>
    /// 网络模块
    /// </summary>
    public static NetworkComponent Network { get; private set; }

    /// <summary>
    /// 是否暂停
    /// </summary>
    public static bool IsPause { get; set; }

    /// <summary>
    /// 用来记录当前状态
    /// </summary>
    public static string CurState { get; set; }

    /// <summary>
    /// 游戏是否暂停
    /// </summary>
    public static bool IsOnApplicationPase { get; set; }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        GameFrameworkLog.SetLogHelper(new DefaultLogHelper());

        GameFrameworkComponent[] gameComponents = GetComponentsInChildren<GameFrameworkComponent>();
        DataTable = SearchComponent<DataTableComponent>(gameComponents);
        DataNode = SearchComponent<DataNodeComponent>(gameComponents);
        Fsm = SearchComponent<FsmComponent>(gameComponents);
        Procedure = SearchComponent<ProcedureComponent>(gameComponents);
        ObjectPool = SearchComponent<ObjectPoolComponent>(gameComponents);
        PlayerData = SearchComponent<PlayerDataComponent>(gameComponents);
        Download = SearchComponent<DownloadComponent>(gameComponents);
        Event = SearchComponent<EventComponent>(gameComponents);
        Sound = SearchComponent<SoundComponent>(gameComponents);
        Scene = SearchComponent<SceneComponent>(gameComponents);
        Ads = SearchComponent<AdsComponent>(gameComponents);
        Process = SearchComponent<ProcessComponent>(gameComponents);
        Localization = SearchComponent<LocalizationComponent>(gameComponents);
        Firebase = SearchComponent<FirebaseComponent>(gameComponents);
        Purchase = SearchComponent<PurchaseComponent>(gameComponents);
        AppsFlyer = SearchComponent<AppsFlyerComponent>(gameComponents);
        UI = SearchComponent<GameUIComponent>(gameComponents);
        Task= SearchComponent<TaskComponent>(gameComponents);
        Objective = SearchComponent<ObjectiveComponent>(gameComponents);
        Notification = SearchComponent<NotificationComponent>(gameComponents);
        Activity = SearchComponent<ActivityComponent>(gameComponents);
        Network = SearchComponent<NetworkComponent>(gameComponents);

        Application.lowMemory += OnLowMemory;
    }

    public void Update()
    {
    #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (GameManager.UI.GetUIForm("TileMatch_LevelTypePanel") != null)
            {
                GameManager.UI.HideUIForm("TileMatch_LevelTypePanel");
                if (GameManager.UI.GetUIForm("TileMatchPanel") != null)
                {
                    GameManager.UI.HideUIForm("TileMatchPanel");
                    GameManager.UI.ShowUIForm("TileMatchPanel");
                }
            }
            else
            {
                GameManager.UI.ShowUIForm("TileMatch_LevelTypePanel");
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack5, 3);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
            });
        }
    #endif

        if (IsPause)
            return;

        GameFrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
    }

    public void Shutdown()
    {
        GameFrameworkEntry.Shutdown();

        m_GameManagerComponents.Clear();

        Destroy(gameObject);
    }

    /// <summary>
    /// 获取游戏管理组件
    /// </summary>
    /// <typeparam name="T">要获取游戏管理组件类型</typeparam>
    /// <returns>要获取游戏管理组件</returns>
    public static T GetGameComponent<T>() where T:GameFrameworkComponent
    {
        return (T)GetGameComponent(typeof(T));
    }

    /// <summary>
    /// 获取游戏管理组件
    /// </summary>
    /// <param name="type">要获取游戏管理组件类型</param>
    /// <returns>要获取游戏管理组件</returns>
    public static GameFrameworkComponent GetGameComponent(Type type)
    {
        LinkedListNode<GameFrameworkComponent> current = m_GameManagerComponents.First;
        while (current != null)
        {
            if (current.Value.GetType() == type)
            {
                return current.Value;
            }

            current = current.Next;
        }

        return null;
    }

    /// <summary>
    /// 获取游戏管理组件
    /// </summary>
    /// <param name="typeName">要获取游戏管理组件类型名称</param>
    /// <returns>要获取游戏管理组件</returns>
    public static GameFrameworkComponent GetGameComponent(string typeName)
    {
        LinkedListNode<GameFrameworkComponent> current = m_GameManagerComponents.First;
        while (current != null)
        {
            Type type = current.Value.GetType();
            if (type.FullName == typeName || type.Name == typeName) 
            {
                return current.Value;
            }

            current = current.Next;
        }

        return null;
    }

    /// <summary>
    /// 注册游戏框架组件
    /// </summary>
    /// <param name="component">要注册的游戏框架组件</param>
    public static void RegisterComponent(GameFrameworkComponent component)
    {
        if (component == null)
        {
            Log.Error("GameManagerComponent is invaild");
            return;
        }

        Type type = component.GetType();

        LinkedListNode<GameFrameworkComponent> current = m_GameManagerComponents.First;
        while (current != null)
        {
            if (current.Value.GetType() == type)
            {
                Log.Error("GameManagerComponent type '{0}' is already exist.", type.FullName);
                return;
            }

            current = current.Next;
        }

        m_GameManagerComponents.AddLast(component);
        Log.Info("Register {0} success", type.FullName);
    }

    /// <summary>
    /// 注销游戏框架组件
    /// </summary>
    /// <param name="component">要注销的游戏框架组件</param>
    public static void UnregisterComponent(GameFrameworkComponent component)
    {
        Type type = component.GetType();

        LinkedListNode<GameFrameworkComponent> current = m_GameManagerComponents.First;
        while (current != null)
        {
            if (current.Value.GetType() == type)
            {
                Log.Info("Unregister {0} success", type.FullName);
                m_GameManagerComponents.Remove(current);
                return;
            }

            current = current.Next;
        }
        Log.Warning("{0} not register", type.FullName);
    }

    private T SearchComponent<T>(GameFrameworkComponent[] gameComponents) where T : GameFrameworkComponent
    {
        for (int i = 0; i < gameComponents.Length; i++)
        {
            if (gameComponents[i].GetType() == typeof(T))
            {
                return (T)gameComponents[i];
            }
        }

        return null;
    }

    //private float cdTime = 0;

    private void OnLowMemory()
    {
        //如果小于2g的，做处理
        if (SystemInfoManager.IsLowMemorySize)
        {
            SystemInfoManager.IsLowMemory = true;

            //if (Application.targetFrameRate != 30) Application.targetFrameRate = 30;

            //if (cdTime < Time.realtimeSinceStartup)
            //{
            //    cdTime = Time.realtimeSinceStartup + 20;

            //    if (ObjectPool != null)
            //    {
            //        ObjectPool.ReleaseAllUnused();
            //    }

            //    //Resources.UnloadUnusedAssets();
            //    //Task.AddDelayTriggerTask(0.1f, () =>
            //    //{
            //    //    GC.Collect();
            //    //});
            //}
        }
    }
}
