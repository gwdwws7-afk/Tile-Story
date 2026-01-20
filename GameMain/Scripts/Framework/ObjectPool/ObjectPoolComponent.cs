using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 对象池组件
/// </summary>
public sealed class ObjectPoolComponent : GameFrameworkComponent
{
    private IObjectPoolManager m_ObjectPoolManager;

    protected override void Awake()
    {
        base.Awake();

        m_ObjectPoolManager = GameFrameworkEntry.GetModule<ObjectPoolManager>();
        if (m_ObjectPoolManager == null)
        {
            Log.Fatal("Object pool manager is invalid");
            return;
        }
    }

    /// <summary>
    /// 是否存在对象池
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    public bool HasObjectPool(string poolName)
    {
        return m_ObjectPoolManager.HasObjectPool(poolName);
    }

    /// <summary>
    /// 获取对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="poolName">对象池名称。</param>
    /// <returns>要获取的对象池。</returns>
    public IObjectPool<T> GetObjectPool<T>(string poolName) where T : ObjectBase, new()
    {
        return m_ObjectPoolManager.GetObjectPool<T>(poolName);
    }

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    public IObjectPool<T> CreateObjectPool<T>(string poolName) where T : ObjectBase, new()
    {
        return m_ObjectPoolManager.CreateObjectPool<T>(poolName);
    }

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    /// <param name="capacity">容量</param>
    public IObjectPool<T> CreateObjectPool<T>(string poolName, int capacity) where T : ObjectBase, new()
    {
        return m_ObjectPoolManager.CreateObjectPool<T>(poolName, capacity);
    }

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="poolName">对象池名称</param>
    /// <param name="autoReleaseInterval">自动释放的时间间隔</param>
    /// <param name="capacity">对象池容量</param>
    /// <param name="expireTime">对象过期时间</param>
    /// <returns></returns>
    public IObjectPool<T> CreateObjectPool<T>(string poolName, float autoReleaseInterval, int capacity, float expireTime) where T : ObjectBase, new()
    {
        return m_ObjectPoolManager.CreateObjectPool<T>(poolName, autoReleaseInterval, capacity, expireTime);
    }

    /// <summary>
    /// 预加载对象池资源
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="poolName">对象池名称</param>
    /// <param name="preloadNum">预加载数量</param>
    public List<AsyncOperationHandle<GameObject>> PreloadObjectPool<T>(string poolName, string assetName, Transform parent, int preloadNum) where T : ObjectBase, new()
    {
        return PreloadObjectPool<T>(poolName, assetName, parent, preloadNum,null);
    }

    /// <summary>
    /// 预加载对象池资源
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="assetName"></param>
    /// <param name="parent"></param>
    /// <param name="preloadNum"></param>
    /// <param name="finishAction"></param>
    public List<AsyncOperationHandle<GameObject>> PreloadObjectPool<T>(string poolName, string assetName, Transform parent, int preloadNum,Action<GameObject> finishAction) where T : ObjectBase, new()
    {
        IObjectPool<T> pool = GetObjectPool<T>(poolName);
        if (pool == null)
        {
            pool = CreateObjectPool<T>(poolName, preloadNum);
        }
        else if (preloadNum > pool.Capacity)
        {
            pool.Capacity = preloadNum;
        }

        List<AsyncOperationHandle<GameObject>> asyncOperationHandles = new List<AsyncOperationHandle<GameObject>>(preloadNum);
        for (int i = 0; i < preloadNum; i++)
        {
            AsyncOperationHandle<GameObject> handle = UnityUtility.InstantiateAsync(assetName, parent, obj =>
            {
                T objectBase = new T();
                objectBase.Initialize(assetName, obj);
                pool.Register(objectBase, false);
                finishAction?.Invoke(obj);
            });
            asyncOperationHandles.Add(handle);
        }

        return asyncOperationHandles;
    }

    /// <summary>
    /// 关闭并清理对象池
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    /// <returns>是否成功关闭</returns>
    public bool DestroyObjectPool(string poolName)
    {
        return m_ObjectPoolManager.DestroyObjectPool(poolName);
    }

    /// <summary>
    /// 关闭并清理所有对象池
    /// </summary>
    public void DestroyAllObjectPool()
    {
        m_ObjectPoolManager.DestroyAllObjectPool();
    }

    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    /// <param name="assetName">对象名称</param>
    /// <param name="poolName">对象池名称</param>
    /// <param name="position">位置</param>
    /// <param name="quaternion">四元数</param>
    /// <param name="parent">父类</param>
    /// <param name="completeAction">获取后执行的函数</param>
    public AsyncOperationHandle<GameObject> Spawn<T>(string assetName, string poolName, Vector3 position, Quaternion quaternion, Transform parent, Action<T> completeAction) where T : ObjectBase, new()
    {
        IObjectPool<T> objectPool = m_ObjectPoolManager.GetObjectPool<T>(poolName);
        if (objectPool == null)
        {
            objectPool = m_ObjectPoolManager.CreateObjectPool<T>(poolName);
        }
        return Spawn(assetName, objectPool, position, quaternion, parent, completeAction);
    }

    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="assetName">对象名称</param>
    /// <param name="objectPool">对象池</param>
    /// <param name="position">位置</param>
    /// <param name="quaternion">四元数</param>
    /// <param name="parent">父类</param>
    /// <param name="completeAction">获取后执行的函数</param>
    public AsyncOperationHandle<GameObject> Spawn<T>(string assetName, IObjectPool<T> objectPool, Vector3 position, Quaternion quaternion, Transform parent, Action<T> completeAction) where T : ObjectBase, new()
    {
        T spawnedObj = objectPool.Spawn(assetName);
        if (spawnedObj != null)
        {
            GameObject target = (GameObject)spawnedObj.Target;
            Transform targetTrans = target.transform;
            targetTrans.SetParent(parent);
            targetTrans.position = position;
            targetTrans.rotation = quaternion;
            completeAction.Invoke(spawnedObj);

            return default;
        }
        else
        {
            AsyncOperationHandle<GameObject> handle = UnityUtility.InstantiateAsync(assetName, position, quaternion, parent, obj =>
             {
                 if (obj == null)
                 {
                     Log.Error($"ObjectPool spawn asset {assetName} fail!");

                     return;
                 }

                 T objectBase = new T();
                 objectBase.Initialize(assetName, obj);
                 objectPool.Register(objectBase, true);
                 GameManager.Task.AddDelayTriggerTask(0, () =>
                 {
                     completeAction.Invoke(objectBase);
                 });
             });

            return handle;
        }
    }
    
    /// <summary>
    /// 立即生成
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="poolName"></param>
    /// <param name="position"></param>
    /// <param name="quaternion"></param>
    /// <param name="parent"></param>
    /// <param name="completeAction"></param>
    /// <typeparam name="T"></typeparam>
    public void SpawnByImmediately<T>(string assetName, string poolName, Vector3 position, Quaternion quaternion, Transform parent, Action<T> completeAction) where T : ObjectBase, new()
    {
        IObjectPool<T> objectPool = m_ObjectPoolManager.GetObjectPool<T>(poolName);
        if (objectPool == null)
        {
            objectPool = m_ObjectPoolManager.CreateObjectPool<T>(poolName);
        }
        SpawnByImmediately(assetName, objectPool, position, quaternion, parent, completeAction);
    }
    
    /// <summary>
    /// 同步加载
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="objectPool"></param>
    /// <param name="position"></param>
    /// <param name="quaternion"></param>
    /// <param name="parent"></param>
    /// <param name="completeAction"></param>
    /// <typeparam name="T"></typeparam>
    public void SpawnByImmediately<T>(string assetName, IObjectPool<T> objectPool, Vector3 position, Quaternion quaternion, Transform parent, Action<T> completeAction) where T : ObjectBase, new()
    {
        T spawnedObj = objectPool.Spawn(assetName);
        if (spawnedObj != null)
        {
            GameObject target = (GameObject)spawnedObj.Target;
            Transform targetTrans = target.transform;
            targetTrans.SetParent(parent);
            targetTrans.position = position;
            targetTrans.rotation = quaternion;
            completeAction.Invoke(spawnedObj);
        }
        else
        {
            var obj = UnityUtility.InstantiateSync(assetName,position,quaternion,parent);
            if (obj != null)
            {
                T objectBase = new T();
                objectBase.Initialize(assetName, obj);
                objectPool.Register(objectBase, true);
                completeAction.Invoke(objectBase);
            }
        }
    }

    /// <summary>
    /// 从对象池中获取对象 附带回收时间
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="assetName">对象名称</param>
    /// <param name="poolName">对象池名称</param>
    /// <param name="useTime">回收时间</param>
    /// <param name="position">位置</param>
    /// <param name="quaternion">四元数</param>
    /// <param name="parent">父类</param>
    /// <param name="completeAction">获取后执行的函数</param>
    public void SpawnWithRecycle<T>(string assetName, string poolName, float useTime, Vector3 position, Quaternion quaternion, Transform parent=null, Action<T> completeAction=null) where T : ObjectBase, new()
    {
        IObjectPool<T> objectPool = m_ObjectPoolManager.GetObjectPool<T>(poolName);
        if (objectPool == null)
        {
            objectPool = m_ObjectPoolManager.CreateObjectPool<T>(poolName);
        }
        Spawn<T>(assetName,objectPool,position,quaternion,parent,(t)=> 
        {
            completeAction?.Invoke(t);
            if (useTime > 0)
            {
                var obj = t?.Target as GameObject;
                obj.gameObject.SetActive(true);
                GameManager.Task.AddDelayTriggerTask(useTime, () =>
                {
                    GameManager.ObjectPool.Unspawn(objectPool, t?.Target as GameObject);
                }, true);
            }
        });
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    /// <param name="poolName">对象池名称</param>
    /// <param name="target">对象实例</param>
    public void Unspawn<T>(string poolName, GameObject target) where T : ObjectBase, new()
    {
        IObjectPool<T> objectPool = m_ObjectPoolManager.GetObjectPool<T>(poolName);
        if (objectPool != null)
        {
            Unspawn(objectPool, target);
        }
        else
        {
            Log.Error($"Can not find {poolName} Object Pool");
            UnityUtility.UnloadInstance(target);
        }
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    /// <param name="objectPool">对象池</param>
    /// <param name="target">对象实例</param>
    public void Unspawn<T>(IObjectPool<T> objectPool, GameObject target) where T : ObjectBase, new()
    {
        if (objectPool != null && target != null)
        {
            target.SetActive(false);
            target.transform.SetParent(transform,true);
            objectPool.Unspawn(target);
        }
    }

    /// <summary>
    /// 释放对象池中的可释放对象。
    /// </summary>
    public void Release()
    {
        Log.Info("Object pool release...");
        m_ObjectPoolManager.Release();
    }

    /// <summary>
    /// 释放对象池中的所有未使用对象。
    /// </summary>
    public void ReleaseAllUnused()
    {
        Log.Info("Object pool release all unused...");
        m_ObjectPoolManager.ReleaseAllUnused();
    }
}
