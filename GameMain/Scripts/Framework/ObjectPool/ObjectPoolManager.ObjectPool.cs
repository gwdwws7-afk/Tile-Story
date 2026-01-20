using GameFramework;
using System;
using System.Collections.Generic;

public sealed partial class ObjectPoolManager : GameFrameworkModule, IObjectPoolManager
{
    /// <summary>
    /// 对象池
    /// </summary>
    private sealed class ObjectPool<T> : ObjectPoolBase, IObjectPool<T> where T : ObjectBase
    {
        private readonly GameFrameworkMultiDictionary<string, Object<T>> objects;
        private readonly Dictionary<object, Object<T>> objectMap;
        private readonly ReleaseObjectFilterCallback<T> defaultReleaseObjectFilterCallback;
        private readonly List<T> cachedCanReleaseObjects;
        private readonly List<T> cachedToReleaseObjects;
        private float autoReleaseInterval;
        private int capacity;
        private float expireTime;
        private float autoReleaseTime;

        public ObjectPool(string name, float autoReleaseInterval, int capacity, float expireTime)
            :base(name)
        {
            objects = new GameFrameworkMultiDictionary<string, Object<T>>();
            objectMap = new Dictionary<object, Object<T>>();
            defaultReleaseObjectFilterCallback = DefaultReleaseObjectFilterCallback;
            cachedCanReleaseObjects = new List<T>();
            cachedToReleaseObjects = new List<T>();
            this.autoReleaseInterval = autoReleaseInterval;
            this.capacity = capacity;
            this.expireTime = expireTime;
            autoReleaseTime = 0;
        }

        /// <summary>
        /// 获取对象池对象类型。
        /// </summary>
        public override Type ObjectType { get => typeof(T); }

        /// <summary>
        /// 获取对象池中对象的数量。
        /// </summary>
        public override int Count { get => objectMap.Count; }

        /// <summary>
        /// 获取对象池中能被释放的对象的数量。
        /// </summary>
        public override int CanReleaseCount
        {
            get
            {
                GetCanReleaseObjects(cachedCanReleaseObjects);
                return cachedCanReleaseObjects.Count;
            }
        }

        /// <summary>
        /// 获取或设置对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public override float AutoReleaseInterval
        {
            get
            {
                return autoReleaseInterval;
            }
            set
            {
                autoReleaseInterval = value;
            }
        }

        /// <summary>
        /// 获取或设置对象池的容量。
        /// </summary>
        public override int Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                if (value < 0)
                {
                    Log.Error("Capacity is invalid");
                    return;
                }

                if (capacity == value)
                {
                    return;
                }

                capacity = value;
                Release();
            }
        }

        /// <summary>
        /// 获取或设置对象池对象过期秒数。
        /// </summary>
        public override float ExpireTime
        {
            get
            {
                return expireTime;
            }
            set
            {
                if (value < 0f)
                {
                    Log.Error("ExpireTime is invalid.");
                }

                if (ExpireTime == value)
                {
                    return;
                }

                expireTime = value;
                Release();
            }
        }

        /// <summary>
        /// 创建对象。
        /// </summary>
        /// <param name="obj">对象。</param>
        /// <param name="spawned">对象是否已被获取。</param>
        public void Register(T obj, bool spawned)
        {
            if (obj == null)
            {
                Log.Error("Object is invalid.");
            }

            Object<T> internalObject = Object<T>.Create(obj, spawned);
            objects.Add(obj.Name, internalObject);
            objectMap.Add(obj.Target, internalObject);
            //if (Count > capacity)
            //{
            //    Release();
            //}
        }

        /// <summary>
        /// 检查对象。
        /// </summary>
        /// <returns>要检查的对象是否存在。</returns>
        public bool CanSpawn()
        {
            return CanSpawn(string.Empty);
        }

        /// <summary>
        /// 检查对象。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <returns>要检查的对象是否存在。</returns>
        public bool CanSpawn(string name)
        {
            if (name == null)
            {
                throw new Exception("Name is invalid.");
            }

            GameFrameworkLinkedListRange<Object<T>> objectRange = default(GameFrameworkLinkedListRange<Object<T>>);
            if(objects.TryGetValue(name,out objectRange))
            {
                foreach (Object<T> internalObject in objectRange)
                {
                    if (!internalObject.IsInUse)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns>要获取的对象</returns>
        public T Spawn()
        {
            return Spawn(string.Empty);
        }

        /// <summary>
        /// 获取对象。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <returns>要获取的对象。</returns>
        public T Spawn(string name)
        {
            if (name == null)
            {
                throw new Exception("Name is invalid.");
            }

            GameFrameworkLinkedListRange<Object<T>> objectRange = default(GameFrameworkLinkedListRange<Object<T>>);
            if(objects.TryGetValue(name,out objectRange))
            {
                foreach (Object<T> internalObject in objectRange)
                {
                    if (!internalObject.IsInUse) 
                    {
                        return internalObject.Spawn();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 回收对象。
        /// </summary>
        /// <param name="obj">要回收的对象。</param>
        public void Unspawn(T obj)
        {
            if (obj == null)
            {
                throw new Exception("Object is invalid.");
            }
            Unspawn(obj.Target);
        }

        /// <summary>
        /// 回收对象。
        /// </summary>
        /// <param name="target">要回收的对象。</param>
        public void Unspawn(object target)
        {
            Object<T> internalObject = GetObject(target);
            if (internalObject != null)
            {
                if (!internalObject.IsInUse)
                {
                    Log.Warning("object {0} is not in use", internalObject.Name);
                    return;
                }
                internalObject.Unspawn();
                if (Count > capacity && internalObject.SpawnCount <= 0)
                {
                    Release();
                }
            }
            else
            {
                Log.Error($"Can not find target in object pool {target}");
            }
        }

        /// <summary>
        /// 释放对象。
        /// </summary>
        /// <param name="obj">要释放的对象。</param>
        /// <returns>释放对象是否成功。</returns>
        public bool ReleaseObject(T obj)
        {
            if (obj == null)
            {
                throw new Exception("Object is invalid.");
            }

            return ReleaseObject(obj.Target);
        }

        /// <summary>
        /// 释放对象。
        /// </summary>
        /// <param name="target">要释放的对象。</param>
        /// <returns>释放对象是否成功。</returns>
        public bool ReleaseObject(object target)
        {
            if (target == null)
            {
                throw new Exception("target is invalid.");
            }

            Object<T> internalObject = GetObject(target);
            if (internalObject == null)
            {
                return false;
            }

            if (internalObject.IsInUse)
            {
                return false;
            }

            objects.Remove(internalObject.Name, internalObject);
            objectMap.Remove(target);

            #if UNITY_EDITOR
            Log.Info("release object {0} success", internalObject.Name);
            #endif

            internalObject.Release(false);
            return true;
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        public override void Release()
        {
            Release(Count - capacity, defaultReleaseObjectFilterCallback);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        /// <param name="toReleaseCount">尝试释放对象数量。</param>
        public override void Release(int toReleaseCount)
        {
            Release(toReleaseCount, defaultReleaseObjectFilterCallback);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        /// <param name="releaseObjectFilterCallback">释放对象筛选函数。</param>
        public void Release(ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
        {
            Release(Count - capacity, releaseObjectFilterCallback);
        }

        /// <summary>
        /// 释放对象池中的可释放对象。
        /// </summary>
        /// <param name="toReleaseCount">尝试释放对象数量。</param>
        /// <param name="releaseObjectFilterCallback">释放对象筛选函数。</param>
        public void Release(int toReleaseCount, ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
        {
            //Log.Info("{0} check release", Name);

            if (releaseObjectFilterCallback == null)
            {
                throw new Exception("Release object filter callback is invalid.");
            }

            if (toReleaseCount < 0) 
            {
                toReleaseCount = 0;
            }

            DateTime time = Constant.GameConfig.DateTimeMin;
            if (expireTime < float.MaxValue)
            {
                time = DateTime.UtcNow.AddSeconds(-expireTime);
            }

            GetCanReleaseObjects(cachedCanReleaseObjects);
            List<T> toReleaseObjects = releaseObjectFilterCallback(cachedCanReleaseObjects, toReleaseCount, time);
            if (toReleaseObjects == null || toReleaseObjects.Count <= 0)
            {
                return;
            }

            foreach (T toReleaseObject in toReleaseObjects)
            {
                ReleaseObject(toReleaseObject);
            }
        }

        /// <summary>
        /// 释放对象池中的所有未使用对象。
        /// </summary>
        public override void ReleaseAllUnused()
        {
            autoReleaseTime = 0;
            GetCanReleaseObjects(cachedCanReleaseObjects);
            foreach (T toReleaseObject in cachedCanReleaseObjects)
            {
                ReleaseObject(toReleaseObject);
            }
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            autoReleaseTime += realElapseSeconds;
            if (autoReleaseTime >= autoReleaseInterval)
            {
                autoReleaseTime = 0;
                Release();
            }
        }

        public override void OnShutdown()
        {
            foreach (var objectInMap in objectMap)
            {
                objectInMap.Value.Release(true);
            }

            objects.Clear();
            objectMap.Clear();
            cachedCanReleaseObjects.Clear();
            cachedToReleaseObjects.Clear();

            Log.Info("Shutdown {0} success", Name);
        }

        private Object<T> GetObject(object target)
        {
            if (target == null)
            {
                throw new Exception("Target is invalid.");
            }

            if (objectMap.TryGetValue(target, out Object<T> internalObject))
            {
                return internalObject;
            }
            return null;
        }

        private void GetCanReleaseObjects(List<T> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<object, Object<T>> objectInMap in objectMap)
            {
                Object<T> internalObject = objectInMap.Value;
                if (internalObject.IsInUse)
                {
                    continue;
                }

                results.Add(internalObject.Peek());
            }
        }

        private List<T> DefaultReleaseObjectFilterCallback(List<T> candidateObjects, int toReleaseCount, DateTime expireTime)
        {
            cachedToReleaseObjects.Clear();

            if (expireTime > Constant.GameConfig.DateTimeMin)
            {
                for (int i = candidateObjects.Count - 1; i >= 0; i--) 
                {
                    if (candidateObjects[i].LastUseTime <= expireTime)
                    {
                        cachedToReleaseObjects.Add(candidateObjects[i]);
                        candidateObjects.RemoveAt(i);
                        continue;
                    }
                }

                //toReleaseCount -= cachedToReleaseObjects.Count;
            }

            return cachedToReleaseObjects;
        }
    }
}
