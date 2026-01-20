using GameFramework;
using System.Collections.Generic;

public sealed partial class DataNodeManager: GameFrameworkModule, IDataNodeManager
{
    /// <summary>
    /// 数据结点
    /// </summary>
    public sealed class DataNode:IDataNode, IReference
    {
        private string name;
        private object data;
        private DataNode parent;
        private List<DataNode> childs;

        public DataNode()
        {
            name = null;
            data = null;
            parent = null;
            childs = null;
        }

        /// <summary>
        /// 创建数据结点。
        /// </summary>
        /// <param name="name">数据结点名称。</param>
        /// <param name="parent">父数据结点。</param>
        /// <returns>创建的数据结点。</returns>
        public static DataNode Create(string name, DataNode parent)
        {
            if (!IsVaildName(name))
            {
                throw new System.Exception("Name of data node is invalid.");
            }

            //DataNode node = ReferencePool.Acquire<DataNode>();
            DataNode dataNode = new DataNode();
            dataNode.name = name;
            dataNode.parent = parent;
            return dataNode;
        }

        /// <summary>
        /// 获取数据结点的名称。
        /// </summary>
        public string Name { get => name; }

        /// <summary>
        /// 获取数据结点的完整名称。
        /// </summary>
        public string FullName { get => string.Format("{0}{1}{2}", parent.name, PathSplitSeparator[0], name); }

        /// <summary>
        /// 获取父数据结点。
        /// </summary>
        public IDataNode Parent { get => parent; }

        /// <summary>
        /// 获取子数据结点的数量。
        /// </summary>
        public int ChildCount { get => childs != null ? childs.Count : 0; }

        /// <summary>
        /// 获取结点数据
        /// </summary>
        /// <typeparam name="T">要获取的数据类型。</typeparam>
        /// <returns>指定类型的数据。</returns>
        public T GetData<T>()
        {
            return (T)data;
        }

        /// <summary>
        /// 设置结点数据
        /// </summary>
        /// <returns>数据结点数据。</returns>
        public void SetData(object data)
        {
            this.data = data;
        }

        /// <summary>
        /// 根据索引获取子数据结点。
        /// </summary>
        /// <param name="index">子数据结点的索引。</param>
        /// <returns>指定索引的子数据结点，如果索引越界，则返回空。</returns>
        public IDataNode GetChild(int index)
        {
            return index < 0 || index >= ChildCount ? null : childs[index];
        }

        /// <summary>
        /// 根据名称获取子数据结点。
        /// </summary>
        /// <param name="name">子数据结点名称。</param>
        /// <returns>指定名称的子数据结点，如果没有找到，则返回空。</returns>
        public IDataNode GetChild(string name)
        {
            if (!IsVaildName(name))
            {
                throw new System.Exception("Name is invalid.");
            }

            if (childs == null)
            {
                return null;
            }

            for (int i = 0; i < childs.Count; i++)
            {
                if (childs[i].name == name)
                {
                    return childs[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 根据名称获取或增加子数据结点。
        /// </summary>
        /// <param name="name">子数据结点名称。</param>
        /// <returns>指定名称的子数据结点，如果对应名称的子数据结点已存在，则返回已存在的子数据结点，否则增加子数据结点。</returns>
        public IDataNode GetOrAddChild(string name)
        {
            DataNode dataNode = (DataNode)GetChild(name);
            if (dataNode != null)
            {
                return dataNode;
            }

            dataNode = Create(name, this);

            if (childs == null)
            {
                childs = new List<DataNode>();
            }
            childs.Add(dataNode);
            return dataNode;
        }

        /// <summary>
        /// 根据索引移除子数据结点。
        /// </summary>
        /// <param name="index">子数据结点的索引位置。</param>
        public void RemoveChild(int index)
        {
            DataNode dataNode = (DataNode)GetChild(index);
            if (dataNode == null)
            {
                return;
            }

            childs.Remove(dataNode);
            //ReferencePool.Release(dataNode);
        }

        /// <summary>
        /// 根据名称移除子数据结点。
        /// </summary>
        /// <param name="name">子数据结点名称。</param>
        public void RemoveChild(string name)
        {
            DataNode dataNode = (DataNode)GetChild(name);
            if (dataNode == null)
            {
                return;
            }

            childs.Remove(dataNode);
            //ReferencePool.Release(dataNode);
        }

        public void Clear()
        {
            if (data != null)
            {
                //ReferencePool.Release(m_Data);
                data = null;
            }

            if (childs != null)
            {
                //foreach (DataNode child in m_Childs)
                //{
                //    ReferencePool.Release(child);
                //}

                childs.Clear();
            }
        }

        /// <summary>
        /// 检测数据结点名称是否合法。
        /// </summary>
        /// <param name="name">要检测的数据结点名称。</param>
        /// <returns>是否是合法的数据结点名称。</returns>
        private static bool IsVaildName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            for (int i = 0; i < PathSplitSeparator.Length; i++)
            {
                if (name.Contains(PathSplitSeparator[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
