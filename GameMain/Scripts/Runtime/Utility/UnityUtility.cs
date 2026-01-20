using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static partial class UnityUtility
{
    public static string GetSpriteKey(string spriteKey,string atlasName=Constant.ResourceConfig.TotalItemAtlas)
    {
        return $"{atlasName}[{spriteKey}]";
    }

    /// <summary>
    /// 异步生成实例
    /// </summary>
    public static AsyncOperationHandle<GameObject> InstantiateAsync(string assetAddress, Transform parent = null, Action<GameObject> completeAction = null)
    {
        AsyncOperationHandle<GameObject> asyncHandle = Addressables.InstantiateAsync(assetAddress, parent);
        asyncHandle.Completed += (obj) =>
        {
            if (obj.Status != AsyncOperationStatus.Succeeded)
            {
                Log.Error("Instantiate {0} Object fail", assetAddress);
            }
            completeAction?.Invoke(obj.Result);
        };
        return asyncHandle;
    }

    /// <summary>
    /// 异步生成实例
    /// </summary>
    public static AsyncOperationHandle<GameObject> InstantiateAsync(string assetAddress, Vector3 position, Quaternion rotation, Transform parent = null, Action<GameObject> completeAction = null)
    {
        AsyncOperationHandle<GameObject> asyncHandle = Addressables.InstantiateAsync(assetAddress, position, rotation, parent);
        asyncHandle.Completed += (obj) =>
        {
            if (obj.Status != AsyncOperationStatus.Succeeded)
            {
                Log.Error("Instantiate {0} Object fail", assetAddress);
            }
            completeAction?.Invoke(obj.Result);
        };
        return asyncHandle;
    }

    /// <summary>
    /// 释放实例
    /// </summary>
    public static void UnloadInstance(AsyncOperationHandle asyncHandle)
    {
        if (asyncHandle.IsValid())
        {
            Addressables.ReleaseInstance(asyncHandle);
        }
    }

    /// <summary>
    /// 释放实例
    /// </summary>
    public static void UnloadInstance(GameObject instance)
    {
        Addressables.ReleaseInstance(instance);
    }

    /// <summary>
    /// 异步加载公用图片资源
    /// </summary>
    public static AsyncOperationHandle LoadGeneralSpriteAsync(string spriteKey, Action<Sprite> completeAction)
    {
        return LoadSpriteAsync(spriteKey, Constant.ResourceConfig.TotalItemAtlas, completeAction);
    }

    /// <summary>
    /// 异步加载图片资源
    /// </summary>
    public static AsyncOperationHandle LoadSpriteAsync(string spriteKey, string atlasName, Action<Sprite> completeAction)
    {
        string atlasedSpriteAddress = $"{atlasName}[{spriteKey}]";
        return LoadAssetAsync(atlasedSpriteAddress, completeAction);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    public static AsyncOperationHandle LoadAssetAsync<T>(string assetAddress, Action<T> completeAction)
    {
        AsyncOperationHandle<T> asyncHandle = Addressables.LoadAssetAsync<T>(assetAddress);
        asyncHandle.Completed += (obj) =>
        {
            if (obj.Status != AsyncOperationStatus.Succeeded)
            {
                Log.Error("load {0} targetImage fail", assetAddress);
            }

            try
            {
                completeAction?.Invoke(obj.Result);
            }
            catch (Exception e)
            {
                GameManager.Firebase.RecordMessageByEvent(
                    Constant.AnalyticsEvent.LoadAsset_CompleteError,
                    new Parameter( "assetAddress", assetAddress));
            }
        };
        return asyncHandle;
    }
    
    /// <summary>
    /// 同步加载 GameObject
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static GameObject InstantiateSync(string assetName,Vector3 position,Quaternion rotation,Transform parent)
    {
        try
        {
            var handle = Addressables.InstantiateAsync(assetName,position, rotation, parent);
            if (!handle.IsDone)
            {
                handle.WaitForCompletion();
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }
            else
            {
                Log.Error($"LoadAsset无效句柄：assetName：{assetName}");
                Addressables.Release(handle);
                return null;
            }
        }
        catch (System.Exception e)
        {
            Log.Error($"InstantiateSync:{e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 异步卸载资源
    /// </summary>
    public static void UnloadAssetAsync(AsyncOperationHandle asyncHandle)
    {
        if (asyncHandle.IsValid())
        {
            Addressables.Release(asyncHandle);
        }
    }

    public static string GetRewardSpriteKey(TotalItemData type, int num)
    {
        if (type.TotalItemType == TotalItemType.Item_BgID ||
            type.TotalItemType == TotalItemType.Item_TileID ||
            type.TotalItemType == TotalItemType.Item_PortraitID)
        {
            return type.ID.ToString();
        }
        if (type.TotalItemType == TotalItemType.Coin)
        {
            if (num >= 1000)
            {
                return "Coin3";
            }

            return "Coin1";

        }
        else if (type == TotalItemData.MergeEnergyBox)
        {
            return type.TotalItemType.ToString() + "_" + Merge.MergeManager.Instance.Theme.ToString();
        }
        else
        {
            return type.TotalItemType.ToString();
        }
    }

    public static string GetTaskObjectSpriteName(TaskTarget taskTarget)
    {
        string rewardKey = null;
        switch (taskTarget)
        {
            case TaskTarget.DestroyBalls:
                rewardKey = "DestroyBallsTaskIcon";
                break;
            case TaskTarget.GenerateCannons:
                rewardKey = "GenerateCannonsTaskIcon";
                break;
            case TaskTarget.FirstPass:
                rewardKey = "FirstPassTaskIcon";
                break;
            case TaskTarget.GenerateBombs:
                rewardKey = "GenerateBombsTaskIcon";
                break;
            case TaskTarget.LevelPass:
                rewardKey = "LevelPassTaskIcon";
                break;
            case TaskTarget.RemainSteps:
                rewardKey = "RemainStepsTaskIcon";
                break;
            case TaskTarget.GetStars:
                rewardKey = "GetStarsTaskIcon";
                break;
            case TaskTarget.FallBalls:
                rewardKey = "DropTaskIcon";
                break;
            case TaskTarget.CollectCandies:
                rewardKey = "PinataTaskIcon";
                break;
            default:
                rewardKey = taskTarget.ToString() + "TaskIcon";
                break;
        }

        return rewardKey;
    }

    public static string GetAltasSpriteName(string spriteName, string atlasName)
    {
        return atlasName + "[" + spriteName + "]";
    }

    /// <summary>
    /// 获取目标日期的结束时间
    /// </summary>
    /// <param name="dayOfWeeks">星期几结束</param>
    public static DateTime GetTargetDateEndTime(params DayOfWeek[] dayOfWeeks)
    {
        if (dayOfWeeks.Length == 0)
        {
            throw new Exception("Target Date can't be null");
        }

        DayOfWeek now = DateTime.Now.DayOfWeek;
        int minDeltaDay = int.MaxValue;
        for (int i = 0; i < dayOfWeeks.Length; i++)
        {
            if (dayOfWeeks[i] - now >= 0 && minDeltaDay > dayOfWeeks[i] - now)
            {
                minDeltaDay = dayOfWeeks[i] - now;
            }
        }

        if (minDeltaDay == int.MaxValue)
        {
            for (int i = 0; i < dayOfWeeks.Length; i++)
            {
                if (dayOfWeeks[i] - now + 7 >= 0 && minDeltaDay > dayOfWeeks[i] - now + 7)
                {
                    minDeltaDay = dayOfWeeks[i] - now + 7;
                }
            }
        }

        if (minDeltaDay == int.MaxValue)
        {
            throw new Exception("minDeltaDay is invalid");
        }

        TimeSpan timeToZero = DateTime.Now.AddDays(1).Date - DateTime.Now;
        DateTime endTime = DateTime.Now.AddDays(minDeltaDay) + timeToZero;
        return endTime;
    }

    /// <summary>
    /// 获取平均位置
    /// </summary>
    /// <param name="centerPos">中心点</param>
    /// <param name="count">位置数量</param>
    public static Vector3[] GetAveragePosition(Vector3 centerPos, Vector3 deltaPos, int count)
    {
        if (count <= 1)
        {
            return new Vector3[] { centerPos };
        }

        Vector3[] result = new Vector3[count];

        if (count % 2 == 0)
        {
            int half = count / 2;

            for (int i = 0; i < count; i++)
            {
                if (i < half)
                {
                    result[i] = centerPos - (half - i - 0.5f) * deltaPos;
                }
                else
                {
                    result[i] = centerPos + (i - half + 0.5f) * deltaPos;
                }
            }
        }
        else
        {
            int half = count / 2;

            for (int i = 0; i < count; i++)
            {
                result[i] = centerPos - (half - i) * deltaPos;
            }
        }

        return result;
    }

    /// <summary>
    /// 获取新锚点的位置坐标
    /// </summary>
    public static Vector3 GetNewAnchorPosition(RectTransform rt, Vector2 newPivot)
    {
        Vector3 op = new Vector3(rt.rect.width * newPivot.x - rt.rect.width * rt.pivot.x, rt.rect.height * newPivot.y - rt.rect.height * rt.pivot.y, 0);
        Vector3 pt = rt.TransformPoint(op);
        return rt.parent.InverseTransformPoint(pt);
    }

    public static void SetBtnEvent(this UnityEngine.UI.Button btn, UnityEngine.Events.UnityAction callBack)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(callBack);
    }


    public static void SetBtnEvent(this DelayButton btn, UnityEngine.Events.UnityAction callBack)
    {
        btn.OnInit(callBack);
    }

    public static void SetBtnEvent(this TileDelayButton btn, UnityEngine.Events.UnityAction callBack)
    {
        btn.OnInit(callBack);
    }

    public static void SetBtnEvent(this UnityEngine.UI.Toggle toggle, UnityEngine.Events.UnityAction<bool> callBack)
    {
        toggle.onValueChanged.RemoveListener(callBack);
        toggle.onValueChanged.AddListener(callBack);
    }

    public static void SetBtnEvent(this UnityEngine.UI.ExtensionsToggle toggle, UnityEngine.Events.UnityAction<bool> callBack)
    {
        toggle.onValueChanged.RemoveListener(callBack);
        toggle.onValueChanged.AddListener(callBack);
    }

    public static void DestroyAllChild(this Transform trans)
    {
        foreach (var child in trans.GetComponentsInChildren<Transform>())
        {
            if(trans!=child)
                GameObject.Destroy(child.gameObject);
        }
    }

    public static void PlayerVibrator(this UnityUtil.EVibatorType type)
    {
        if (GameManager.PlayerData != null &&
           !GameManager.PlayerData.ShakeMuted)
            UnityUtil.UnityVibrator.PlayerVibrator(type);
    }

    public static double GetSystemMemory()
    {
        var systemMemory = SystemInfo.systemMemorySize;
        var ans = systemMemory / 1024d / 1024d / 1024d;
        var numRounded = Math.Round(ans * 100d) / 100d;
        return numRounded;
    }
    
    private static readonly Dictionary<Type, string> s_UIFormNameDic = new();

    public static string GetTypeName(Type type)
    {
        if (s_UIFormNameDic.TryGetValue(type, out var typeName))
        {
            return typeName;
        }
        var n = type.Name;
        s_UIFormNameDic.Add(type, n);
        return n;
    }
    
    private static readonly Dictionary<UIFormType, string> s_EnumNameDic = new();

    public static string GetEnumName(UIFormType e)
    {
        if (s_EnumNameDic.TryGetValue(e, out var typeName))
        {
            return typeName;
        }
        var n = e.ToString();
        s_EnumNameDic.Add(e, n);
        return n;
    }
}

#region 加载预制体扩展
public static partial class UnityUtility
{
    public delegate void GridBinder<T>(int index, T comp) where T : Component;
    public static void AddChild<T>(this GameObject parent, string sampleName, int count, GridBinder<T> binder = null) where T : Component
    {
        if (!parent)
        {
            throw new System.ArgumentNullException("parent is null");
        }

        int allocCount = count - parent.transform.childCount;
        for (int i = 0; i < allocCount; i++)
        {
            if (String.IsNullOrEmpty(sampleName))
            {
                throw new System.ArgumentNullException("sampleName is null");
            }
            var sample = Resources.Load<T>(sampleName);
            if (!sample)
            {
                throw new System.ArgumentNullException("sample is null");
            }
            var child = GameObject.Instantiate<T>(sample);
            child.transform.SetParent(parent.transform);
        }

        for (int i = 0; i < count; i++)
        {
            var child = parent.transform.GetChild(i);
            child.gameObject.SetActive(true);
            binder?.Invoke(i, child.GetComponent<T>());
        }

        //隐藏多余的
        for (int i = count; i < parent.transform.childCount; i++)
        {
            var child = parent.transform.GetChild(i);
            child.gameObject.SetActive(true);
        }
    }

    public static void AddChild<T>(this GameObject parent, T sample, int count, GridBinder<T> binder = null) where T : Component
    {
        if (!parent)
        {
            throw new System.ArgumentNullException("parent is null");
        }

        int allocCount = count - parent.transform.childCount;
        for (int i = 0; i < allocCount; i++)
        {
            if (!sample)
            {
                throw new System.ArgumentNullException("sample is null");
            }
            var child = GameObject.Instantiate<T>(sample);
            child.transform.SetParent(parent.transform);
            child.transform.localScale = Vector3.one;
            child.transform.localPosition = Vector3.zero;
        }

        for (int i = 0; i < count; i++)
        {
            var child = parent.transform.GetChild(i);
            child.gameObject.SetActive(true);
            binder?.Invoke(i, child.GetComponent<T>());
        }

        //隐藏多余的
        for (int i = count; i < parent.transform.childCount; i++)
        {
            var child = parent.transform.GetChild(i);
            child.gameObject.SetActive(false);
        }
    }

    public static void FillGameObjectWithFirstChild<T>(GameObject parent, int count, GridBinder<T> binder = null) where T : Component
    {
        if (!parent)
        {
            throw new ArgumentNullException("UIGrid is null");
        }
        int num = parent.transform.childCount;
        if (num == 0)
        {
            throw new System.ArgumentNullException("child is null");
        }
        else
        {
            var sample = parent.transform.GetChild(0).GetComponent<T>();
            AddChild(parent, sample, count, binder);
        }
    }
}
#endregion

public static partial class UnityUtility
{
    public static void InvokeSafely(this Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception e)
        {
            Log.Error($"{e}");
        }
    }
    
    public static void InvokeSafely<T>(this Action<T> action,T t)
    {
        try
        {
            action?.Invoke(t);
        }
        catch (Exception e)
        {
            Log.Error($"{e}");
        }
    }
    
    public static void InvokeSafely<T,K>(this Action<T,K> action,T t,K k)
    {
        try
        {
            action?.Invoke(t,k);
        }
        catch (Exception e)
        {
            Log.Error($"{e}");
        }
    }
}

public partial class UnityUtility
{
    public static void SetChildActive(this Transform trans, bool isActive, string exclusiveName)
    {
        foreach (Transform child in trans)
        {
            bool isExclusive = child.name.Contains(exclusiveName);
            child.gameObject.SetActive(isExclusive?!isActive:isActive);
        }
    }
    
    public static void SetChildActive(this Transform trans, bool isActive, GameObject exclusiveObj)
    {
        foreach (Transform child in trans)
        {
            bool isExclusive=child.gameObject.Equals(exclusiveObj);
            child.gameObject.SetActive(isExclusive?!isActive:isActive);
        }
    }
}

public partial class UnityUtility
{
    public static T Get<T>(this GameObject obj) where T : MonoBehaviour
    {
        var t = obj.GetComponent<T>();
        if (t == null)
        {
            t = obj.AddComponent<T>();
        }
        return t;
    }
}
