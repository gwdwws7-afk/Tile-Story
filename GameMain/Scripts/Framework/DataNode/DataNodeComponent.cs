

/// <summary>
/// 数据结点组件
/// </summary>
public sealed class DataNodeComponent : GameFrameworkComponent
{
    private IDataNodeManager m_DataNodeManager;

    protected override void Awake()
    {
        base.Awake();

        m_DataNodeManager = GameFrameworkEntry.GetModule<DataNodeManager>();
        if (m_DataNodeManager == null)
        {
            Log.Fatal("Data node manager is invalid.");
            return;
        }
    }

    /// <summary>
    /// 数据结点是否存在
    /// </summary>
    /// <param name="path">查找路径</param>
    public bool HasData(string path)
    {
        return m_DataNodeManager.HasData(path);
    }

    /// <summary>
    /// 根据类型获取数据结点的数据。
    /// </summary>
    /// <typeparam name="T">要获取的数据类型。</typeparam>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <returns>指定类型的数据。</returns>
    public T GetData<T>(string path, T defaultValue)
    {
        return m_DataNodeManager.GetData<T>(path, null, defaultValue);
    }

    /// <summary>
    /// 设置数据结点的数据。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <param name="data">要设置的数据。</param>
    public void SetData<T>(string path, T data)
    {
        m_DataNodeManager.SetData(path, data, null);
    }

    /// <summary>
    /// 移除数据结点。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    public void RemoveNode(string path)
    {
        m_DataNodeManager.RemoveNode(path, null);
    }

    public void Clear()
    {
        m_DataNodeManager.Clear();
    }
}
