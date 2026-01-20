

/// <summary>
/// 数据表组件
/// </summary>
public sealed class DataTableComponent : GameFrameworkComponent
{
    private IDataTableManager m_DataTableManager;

    protected override void Awake()
    {
        base.Awake();

        m_DataTableManager = GameFrameworkEntry.GetModule<DataTableManager>();
        if (m_DataTableManager == null)
        {
            Log.Fatal("Data table manager is invalid.");
            return;
        }
    }

    /// <summary>
    /// 是否存在数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>是否存在数据表。</returns>
    public bool HasDataTable<T>() where T : class
    {
        return m_DataTableManager.HasDataTable<T>();
    }

    /// <summary>
    /// 获取数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>要获取的数据表。</returns>
    public IDataTable<T> GetDataTable<T>() where T : class
    {
        return m_DataTableManager.GetDataTable<T>();
    }

    /// <summary>
    /// 创建数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>要创建的数据表。</returns>
    public IDataTable<T> CreateDataTable<T>() where T : class
    {
        return m_DataTableManager.CreateDataTable<T>();
    }

    /// <summary>
    /// 销毁数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    public bool DestroyDataTable<T>() where T : class
    {
        return m_DataTableManager.DestroyDataTable<T>();
    }
}
