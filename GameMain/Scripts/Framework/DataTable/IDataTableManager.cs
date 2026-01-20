
/// <summary>
/// 数据表管理器接口
/// </summary>
public interface IDataTableManager
{
    /// <summary>
    /// 获取数据表数量。
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 是否存在数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>是否存在数据表。</returns>
    bool HasDataTable<T>() where T : class;

    /// <summary>
    /// 获取数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>要获取的数据表。</returns>
    IDataTable<T> GetDataTable<T>() where T : class;

    /// <summary>
    /// 创建数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>要创建的数据表。</returns>
    IDataTable<T> CreateDataTable<T>() where T : class;

    /// <summary>
    /// 销毁数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    bool DestroyDataTable<T>() where T : class;
}
