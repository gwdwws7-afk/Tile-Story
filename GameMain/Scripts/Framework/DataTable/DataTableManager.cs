using System.Collections.Generic;

/// <summary>
/// 数据表管理器。
/// </summary>
public sealed partial class DataTableManager: GameFrameworkModule, IDataTableManager
{
    private readonly Dictionary<TypeNamePair, DataTableBase> dataTables;

    public DataTableManager()
    {
        dataTables = new Dictionary<TypeNamePair, DataTableBase>();
    }

    /// <summary>
    /// 获取数据表数量。
    /// </summary>
    public int Count { get => dataTables.Count; }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
    }

    public override void Shutdown()
    {
        foreach (var dataTable in dataTables)
        {
            dataTable.Value.Shutdown();
        }
        dataTables.Clear();
    }

    /// <summary>
    /// 是否存在数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>是否存在数据表。</returns>
    public bool HasDataTable<T>() where T:class
    {
        return InternalHasDataTable(new TypeNamePair(typeof(T)));
    }

    /// <summary>
    /// 获取数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>要获取的数据表。</returns>
    public IDataTable<T> GetDataTable<T>() where T : class
    {
        return (IDataTable<T>)InternalGetDataTable(new TypeNamePair(typeof(T)));
    }

    /// <summary>
    /// 创建数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <returns>要创建的数据表。</returns>
    public IDataTable<T> CreateDataTable<T>() where T : class
    {
        return CreateDataTable<T>(string.Empty);
    }

    /// <summary>
    /// 创建数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    /// <param name="name">数据表名称。</param>
    /// <returns>要创建的数据表。</returns>
    public IDataTable<T> CreateDataTable<T>(string name) where T : class
    {
        TypeNamePair typeNamePair = new TypeNamePair(typeof(T), name);
        if (HasDataTable<T>())
        {
            throw new System.Exception(string.Format("Already exist data table '{0}'.", typeNamePair));
        }

        DataTable<T> dataTable = new DataTable<T>(name);
        dataTables.Add(typeNamePair, dataTable);
        return dataTable;
    }
    /// <summary>
    /// 销毁数据表。
    /// </summary>
    /// <typeparam name="T">数据表行的类型。</typeparam>
    public bool DestroyDataTable<T>() where T : class
    {
        return InternalDestroyDataTable(new TypeNamePair(typeof(T)));
    }

    private bool InternalHasDataTable(TypeNamePair typeNamePair)
    {
        return dataTables.ContainsKey(typeNamePair);
    }

    private DataTableBase InternalGetDataTable(TypeNamePair typeNamePair)
    {
        if (dataTables.TryGetValue(typeNamePair,out DataTableBase dataTable))
        {
            return dataTable;
        }
        return null;
    }

    private bool InternalDestroyDataTable(TypeNamePair typeNamePair)
    {
        if (dataTables.TryGetValue(typeNamePair, out DataTableBase dataTable))
        {
            dataTable.Shutdown();
            return dataTables.Remove(typeNamePair);
        }
        return false;
    }
}
