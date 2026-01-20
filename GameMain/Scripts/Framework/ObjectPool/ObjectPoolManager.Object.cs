using GameFramework;
using System;

public sealed partial class ObjectPoolManager : GameFrameworkModule, IObjectPoolManager
{
    /// <summary>
    /// 内部对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    private sealed class Object<T> : IReference where T : ObjectBase
    {
        private T target;
        private int spawnCount;

        public Object()
        {
            target = null;
            spawnCount = 0;
        }

        /// <summary>
        /// 获取对象名称
        /// </summary>
        public string Name { get => target.Name; }

        /// <summary>
        /// 获取对象上次使用时间
        /// </summary>
        public DateTime LastUseTime { get => target.LastUseTime; }

        /// <summary>
        /// 获取对象是否正在被使用
        /// </summary>
        public bool IsInUse { get => spawnCount > 0; }

        /// <summary>
        /// 获取对象的生成计数
        /// </summary>
        public int SpawnCount { get => spawnCount; }

        /// <summary>
        /// 创建内部对象。
        /// </summary>
        /// <param name="obj">对象。</param>
        /// <param name="spawned">对象是否已被获取。</param>
        /// <returns>创建的内部对象。</returns>
        public static Object<T> Create(T target, bool spawned)
        {
            if (target == null)
            {
                throw new Exception("Object is invalid.");
            }

            Object<T> internalObject = ReferencePool.Acquire<Object<T>>();
            //Object<T> internalObject = new Object<T>();
            internalObject.target = target;
            internalObject.spawnCount = spawned ? 1 : 0;
            if (spawned)
            {
                target.OnSpawn();
            }
            return internalObject;
        }

        /// <summary>
        /// 清理内部对象。
        /// </summary>
        public void Clear()
        {
            target = null;
            spawnCount = 0;
        }

        /// <summary>
        /// 查看对象。
        /// </summary>
        /// <returns>对象。</returns>
        public T Peek()
        {
            return target;
        }

        /// <summary>
        /// 获取对象。
        /// </summary>
        /// <returns>对象。</returns>
        public T Spawn()
        {
            spawnCount++;
            target.LastUseTime = DateTime.UtcNow;
            target.OnSpawn();
            return target;
        }

        /// <summary>
        /// 回收对象。
        /// </summary>
        public void Unspawn()
        {
            target.OnUnspawn();
            target.LastUseTime = DateTime.UtcNow;
            spawnCount--;
            if (spawnCount < 0)
            {
                throw new Exception($"Object '{Name}' spawn count is less than 0.");
            }
        }

        /// <summary>
        /// 释放对象。
        /// </summary>
        /// <param name="isShutdown">是否是关闭对象池时触发。</param>
        public void Release(bool isShutdown)
        {
            target.Release(isShutdown);
            target =null;
            ReferencePool.Release(this);
        }
    }
}
