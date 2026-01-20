

/// <summary>
/// 数据保存接口
/// </summary>
public interface ISave
{
    /// <summary>
    /// 数据保存
    /// </summary>
    StoreData Save();

    /// <summary>
    /// 数据恢复
    /// </summary>
    void Restore(StoreData storeData);
}
