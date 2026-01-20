

/// <summary>
/// 数据结点接口
/// </summary>
public interface IDataNode
{
    /// <summary>
    /// 获取数据结点的名称。
    /// </summary>
    string Name
    {
        get;
    }

    /// <summary>
    /// 获取数据结点的完整名称。
    /// </summary>
    string FullName
    {
        get;
    }

    /// <summary>
    /// 获取父数据结点。
    /// </summary>
    IDataNode Parent
    {
        get;
    }

    /// <summary>
    /// 获取子数据结点的数量。
    /// </summary>
    int ChildCount
    {
        get;
    }

    /// <summary>
    /// 获取结点数据
    /// </summary>
    /// <typeparam name="T">要获取的数据类型。</typeparam>
    /// <returns>指定类型的数据。</returns>
    T GetData<T>();

    /// <summary>
    /// 设置结点数据
    /// </summary>
    /// <returns>数据结点数据。</returns>
    void SetData(object data);

    /// <summary>
    /// 根据索引获取子数据结点。
    /// </summary>
    /// <param name="index">子数据结点的索引。</param>
    /// <returns>指定索引的子数据结点，如果索引越界，则返回空。</returns>
    IDataNode GetChild(int index);

    /// <summary>
    /// 根据名称获取子数据结点。
    /// </summary>
    /// <param name="name">子数据结点名称。</param>
    /// <returns>指定名称的子数据结点，如果没有找到，则返回空。</returns>
    IDataNode GetChild(string name);

    /// <summary>
    /// 根据名称获取或增加子数据结点。
    /// </summary>
    /// <param name="name">子数据结点名称。</param>
    /// <returns>指定名称的子数据结点，如果对应名称的子数据结点已存在，则返回已存在的子数据结点，否则增加子数据结点。</returns>
    IDataNode GetOrAddChild(string name);

    /// <summary>
    /// 根据索引移除子数据结点。
    /// </summary>
    /// <param name="index">子数据结点的索引位置。</param>
    void RemoveChild(int index);

    /// <summary>
    /// 根据名称移除子数据结点。
    /// </summary>
    /// <param name="name">子数据结点名称。</param>
    void RemoveChild(string name);
}
