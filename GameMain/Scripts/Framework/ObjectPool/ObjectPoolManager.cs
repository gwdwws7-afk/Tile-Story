using System.Collections.Generic;

/// <summary>
/// 对象池管理器
/// </summary>
public sealed partial class ObjectPoolManager : GameFrameworkModule, IObjectPoolManager
{
    private const int DefaultCapacity = 50;
    private const float DefaultExpireTime = 12;

    private readonly Dictionary<string, ObjectPoolBase> objectPools;
    private readonly List<ObjectPoolBase> cachedAllObjectPools;

    public ObjectPoolManager()
    {
        objectPools = new Dictionary<string, ObjectPoolBase>();
        cachedAllObjectPools = new List<ObjectPoolBase>();
    }

    public override int Priority
    {
        get
        {
            return 6;
        }
    }

    /// <summary>
    /// 获取对象池数量。
    /// </summary>
    public int Count
    {
        get
        {
            return objectPools.Count;
        }
    }

    /// <summary>
    /// 对象池管理器轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
        foreach (var objectPool in objectPools)
        {
            objectPool.Value.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }

    /// <summary>
    /// 关闭并清理对象池管理器。
    /// </summary>
    public override void Shutdown()
    {
        foreach (var objectPool in objectPools)
        {
            objectPool.Value.OnShutdown();
        }

        objectPools.Clear();
        cachedAllObjectPools.Clear();
    }

    /// <summary>
    /// 检测是否存在对象池
    /// </summary>
    /// <param name="name">对象池名称</param>
    /// <returns>是否存在对象池</returns>
    public bool HasObjectPool(string name)
    {
        return objectPools.ContainsKey(name);
    }

    /// <summary>
    /// 获取对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="name">对象池名称。</param>
    /// <returns>要获取的对象池。</returns>
    public IObjectPool<T> GetObjectPool<T>(string name) where T : ObjectBase
    {
        if(objectPools.TryGetValue(name,out ObjectPoolBase objectPoolBase))
        {
            return (IObjectPool<T>)objectPoolBase;
        }
        return null;
    }

    /// <summary>
    /// 获取所有对象池
    /// </summary>
    /// <returns>所有对象池</returns>
    public List<ObjectPoolBase> GetAllObjectPools()
    {
        List<ObjectPoolBase> results = new List<ObjectPoolBase>();

        foreach (KeyValuePair<string, ObjectPoolBase> objectPoolPair in objectPools)
        {
            results.Add(objectPoolPair.Value);
        }

        return results;
    }

    /// <summary>
    /// 获取所有对象池
    /// </summary>
    /// <returns>所有对象池</returns>
    public void GetAllObjectPools(List<ObjectPoolBase> results)
    {
        results.Clear();
        foreach (KeyValuePair<string, ObjectPoolBase> objectPoolPair in objectPools)
        {
            results.Add(objectPoolPair.Value);
        }
    }

    /// <summary>
    /// 创建对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="name">对象池名称。</param>
    /// <returns>创建的对象池。</returns>
    public IObjectPool<T> CreateObjectPool<T>(string name) where T : ObjectBase
    {
        return CreateObjectPool<T>(name, DefaultExpireTime, DefaultCapacity, DefaultExpireTime);
    }

    /// <summary>
    /// 创建对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="name">对象池名称。</param>
    /// <param name="capacity">对象池容量。</param>
    /// <returns>创建的对象池。</returns>
    public IObjectPool<T> CreateObjectPool<T>(string name,int capacity) where T : ObjectBase
    {
        return CreateObjectPool<T>(name, DefaultExpireTime, capacity, DefaultExpireTime);
    }

    /// <summary>
    /// 创建对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="name">对象池名称。</param>
    /// <param name="autoReleaseInterval">对象池自动清理间隔。</param>
    /// <param name="capacity">对象池容量。</param>
    /// <param name="capacity">对象池对象过期时间。</param>
    /// <returns>创建的对象池。</returns>
    public IObjectPool<T> CreateObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime) where T : ObjectBase
    {
        if (HasObjectPool(name))
        {
            Log.Info($"Already exist object pool '{name}'.");
            return GetObjectPool<T>(name);
        }
        ObjectPool<T> objectPool = new ObjectPool<T>(name, autoReleaseInterval, capacity, expireTime);
        objectPools.Add(name, objectPool);
        Log.Info("create {0} success", name);
        return objectPool;
    }

    /// <summary>
    /// 关闭并清理对象池
    /// </summary>
    /// <param name="name">对象池名称</param>
    /// <returns>是否关闭成功</returns>
    public bool DestroyObjectPool(string name)
    {
        if (objectPools.TryGetValue(name, out ObjectPoolBase objectPool))
        {
            objectPool.OnShutdown();
            return objectPools.Remove(name);
        }

        return false;
    }

    /// <summary>
    /// 关闭并清理所有对象池
    /// </summary>
    public void DestroyAllObjectPool()
    {
        Shutdown();
    }

    /// <summary>
    /// 释放对象池中的可释放对象
    /// </summary>
    public void Release()
    {
        GetAllObjectPools(cachedAllObjectPools);
        foreach (ObjectPoolBase objectPool in cachedAllObjectPools)
        {
            objectPool.Release();
        }
    }

    /// <summary>
    /// 释放对象池中的所有未使用对象
    /// </summary>
    public void ReleaseAllUnused()
    {
        GetAllObjectPools(cachedAllObjectPools);
        foreach (ObjectPoolBase objectPool in cachedAllObjectPools)
        {
            objectPool.ReleaseAllUnused();
        }
    }
}
