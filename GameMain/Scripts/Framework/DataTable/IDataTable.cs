using System;

/// <summary>
/// 数据表接口。
/// </summary>
/// <typeparam name="T">数据表行的类型。</typeparam>
public interface IDataTable<T>
{
    /// <summary>
    /// 获取数据表名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取数据表完整名称。
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// 获取数据表行的类型。
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// 获取数据表数据
    /// </summary>
    T Data { get; }

    /// <summary>
    /// 是否正在加载数据
    /// </summary>
    bool IsLoading { get; }

    void ReadData(string dataTableAssetName, Action<GameEventMessage> readDataSuccess, Action<GameEventMessage> readDataFailure);

    void Shutdown();
}
