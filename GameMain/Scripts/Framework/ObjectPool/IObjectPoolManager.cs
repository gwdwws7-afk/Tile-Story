
/// <summary>
/// 对象池管理器接口
/// </summary>
public interface IObjectPoolManager
{
    /// <summary>
    /// 获取对象池数量。
    /// </summary>
    int Count
    {
        get;
    }

    /// <summary>
    /// 检测是否存在对象池
    /// </summary>
    /// <param name="name">对象池名称</param>
    /// <returns>是否存在对象池</returns>
    bool HasObjectPool(string name);

    /// <summary>
    /// 获取对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="name">对象池名称。</param>
    /// <returns>要获取的对象池。</returns>
    IObjectPool<T> GetObjectPool<T>(string name) where T : ObjectBase;

    /// <summary>
    /// 创建对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="name">对象池名称。</param>
    /// <returns>创建的对象池。</returns>
    IObjectPool<T> CreateObjectPool<T>(string name) where T : ObjectBase;

    /// <summary>
    /// 创建对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="name">对象池名称。</param>
    /// <param name="capacity">对象池容量。</param>
    /// <returns>创建的对象池。</returns>
    IObjectPool<T> CreateObjectPool<T>(string name, int capacity) where T : ObjectBase;

    /// <summary>
    /// 创建对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="name">对象池名称。</param>
    /// <param name="autoReleaseInterval">对象池自动清理间隔。</param>
    /// <param name="capacity">对象池容量。</param>
    /// <param name="capacity">对象池对象过期时间。</param>
    /// <returns>创建的对象池。</returns>
    IObjectPool<T> CreateObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime) where T : ObjectBase;

    /// <summary>
    /// 关闭并清理对象池
    /// </summary>
    /// <param name="name">对象池名称</param>
    /// <returns>是否关闭成功</returns>
    bool DestroyObjectPool(string name);

    /// <summary>
    /// 关闭并清理所有对象池
    /// </summary>
    void DestroyAllObjectPool();

    /// <summary>
    /// 释放对象池中的可释放对象
    /// </summary>
    void Release();

    /// <summary>
    /// 释放对象池中的所有未使用对象
    /// </summary>
    void ReleaseAllUnused();
}
