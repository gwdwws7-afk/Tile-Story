using System;

/// <summary>
/// 数据结点管理器
/// </summary>
public sealed partial class DataNodeManager: GameFrameworkModule, IDataNodeManager
{
    private static readonly string[] EmptyStringArray = new string[] { };
    private static readonly string[] PathSplitSeparator = new string[] { ".", "/", "\\" };

    private const string rootName = "<Root>";
    private DataNode root;

    public DataNodeManager()
    {
        root = DataNode.Create(rootName, null);
    }

    public IDataNode Root { get => root; }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
    }

    public override void Shutdown()
    {
        //ReferencePool.Release(root);
        root = null;
    }

    /// <summary>
    /// 数据结点是否存在
    /// </summary>
    /// <param name="path">查找路径</param>
    public bool HasData(string path)
    {
        IDataNode current = GetNode(path, null);

        return current != null;
    }

    /// <summary>
    /// 根据类型获取数据结点的数据。
    /// </summary>
    /// <typeparam name="T">要获取的数据类型。</typeparam>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <returns>指定类型的数据。</returns>
    public T GetData<T>(string path, T defaultValue)
    {
        return GetData<T>(path, null, defaultValue);
    }

    /// <summary>
    /// 根据类型获取数据结点的数据。
    /// </summary>
    /// <typeparam name="T">要获取的数据类型。</typeparam>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <param name="node">查找起始结点。</param>
    /// <returns>指定类型的数据。</returns>
    public T GetData<T>(string path, IDataNode node, T defaultValue)
    {
        IDataNode current = GetNode(path, node);
        if (current == null)
        {
            Log.Info("Data node is not exist,Path is {0}", path);
            return defaultValue;
        }
        return current.GetData<T>();
    }

    /// <summary>
    /// 设置数据结点的数据。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <param name="data">要设置的数据。</param>
    public void SetData<T>(string path, T data)
    {
        SetData(path, data, null);
    }

    /// <summary>
    /// 设置数据结点的数据。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <param name="data">要设置的数据。</param>
    /// <param name="node">查找起始结点。</param>
    public void SetData<T>(string path, T data, IDataNode node)
    {
        IDataNode current = GetOrAddNode(path, node);
        current.SetData(data);
    }

    /// <summary>
    /// 获取数据结点。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <returns>指定位置的数据结点，如果没有找到，则返回空。</returns>
    public IDataNode GetNode(string path)
    {
        return GetNode(path, null);
    }

    /// <summary>
    /// 获取数据结点。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <param name="node">查找起始结点。</param>
    /// <returns>指定位置的数据结点，如果没有找到，则返回空。</returns>
    public IDataNode GetNode(string path, IDataNode node)
    {
        IDataNode current = node ?? root;

        current = current.GetChild(path);
        if (current == null)
        {
            return null;
        }

        //string[] splitedPath = GetSplitedPath(path);

        //for (int i = 0; i < splitedPath.Length; i++)
        //{
        //    current = current.GetChild(splitedPath[i]);
        //    if (current == null)
        //    {
        //        return null;
        //    }
        //}

        return current;
    }

    /// <summary>
    /// 获取或增加数据结点。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <returns>指定位置的数据结点，如果没有找到，则增加相应的数据结点。</returns>
    public IDataNode GetOrAddNode(string path)
    {
        return GetOrAddNode(path, null);
    }

    /// <summary>
    /// 获取或增加数据结点。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <param name="node">查找起始结点。</param>
    /// <returns>指定位置的数据结点，如果没有找到，则增加相应的数据结点。</returns>
    public IDataNode GetOrAddNode(string path, IDataNode node)
    {
        IDataNode current = node ?? root;
        //string[] splitedPath = GetSplitedPath(path);

        //for (int i = 0; i < splitedPath.Length; i++)
        //{
        //    current = current.GetOrAddChild(splitedPath[i]);
        //}
        current = current.GetOrAddChild(path);

        return current;
    }

    /// <summary>
    /// 移除数据结点。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    public void RemoveNode(string path)
    {
        RemoveNode(path, null);
    }

    /// <summary>
    /// 移除数据结点。
    /// </summary>
    /// <param name="path">相对于 node 的查找路径。</param>
    /// <param name="node">查找起始结点。</param>
    public void RemoveNode(string path, IDataNode node)
    {
        IDataNode current = node ?? root;
        IDataNode parent = current.Parent;
        //string[] splitedPath = GetSplitedPath(path);
        //for (int i = 0; i < splitedPath.Length; i++)
        //{
        //    parent = current;
        //    current = current.GetChild(splitedPath[i]);
        //    if (current == null)
        //    {
        //        return;
        //    }
        //}
        parent = current;
        current = current.GetChild(path);
        if (current == null)
        {
            return;
        }

        if (parent != null)
        {
            parent.RemoveChild(current.Name);
        }
    }

    public void Clear()
    {
        root.Clear();
    }

    /// <summary>
    /// 数据结点路径切分工具函数。
    /// </summary>
    /// <param name="path">要切分的数据结点路径。</param>
    /// <returns>切分后的字符串数组。</returns>
    private static string[] GetSplitedPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return EmptyStringArray;
        }

        return path.Split(PathSplitSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

    public void UsedOnlyForAOTCodeGeneration()
    {
        // IL2CPP needs only this line.
        GetData<int>("AOT", 0);

        // Include an exception so we can be sure to know if this method is ever called.
        throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
    }
}
