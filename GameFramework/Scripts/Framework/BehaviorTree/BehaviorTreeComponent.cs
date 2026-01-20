using GameFramework.BehaviorTree;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 行为树模块
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/BehaviorTree")]
    public sealed class BehaviorTreeComponent : GameFrameworkComponent
    {
        private readonly Dictionary<string, BTTree> m_BTTreeDic = new Dictionary<string, BTTree>();
        private readonly List<BTTree> m_TempBTTreeList = new List<BTTree>();

        /// <summary>
        /// 获取行为树的数量
        /// </summary>
        public int Count
        {
            get
            {
                return m_BTTreeDic.Count;
            }
        }

        private void Update()
        {
            if (m_BTTreeDic.Count <= 0)
                return;

            m_TempBTTreeList.Clear();
            foreach (KeyValuePair<string, BTTree> treeKeyValuePair in m_BTTreeDic)
            {
                m_TempBTTreeList.Add(treeKeyValuePair.Value);
            }

            for (int i = 0; i < m_TempBTTreeList.Count; i++)
            {
                if (m_TempBTTreeList[i].IsDestroyed)
                    continue;

                m_TempBTTreeList[i].Update(Time.deltaTime, Time.unscaledDeltaTime);
            }
        }

        /// <summary>
        /// 关闭并清理行为树管理器
        /// </summary>
        public void Shutdown()
        {
            foreach (var tree in m_BTTreeDic)
            {
                tree.Value.Clear();
            }

            m_BTTreeDic.Clear();
            m_TempBTTreeList.Clear();
        }

        /// <summary>
        /// 获取行为树
        /// </summary>
        /// <param name="name">行为树名称</param>
        /// <returns>要获取的行为树</returns>
        public BTTree GetBehaviorTree(string name)
        {
            BTTree result = null;
            if (m_BTTreeDic.TryGetValue(name, out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// 创建行为树
        /// </summary>
        /// <typeparam name="T">行为树类型</typeparam>
        /// <param name="name">行为树名称</param>
        /// <returns>要创建的行为树</returns>
        public T CreateBehaviorTree<T>(string name) where T : BTTree, new()
        {
            if (m_BTTreeDic.ContainsKey(name))
            {
                Log.Error("Behavior Tree {0} has already exist", name);
                return null;
            }

            T btTree = new T();
            btTree.Init();
            m_BTTreeDic.Add(name, btTree);

            return btTree;
        }

        /// <summary>
        /// 销毁行为树
        /// </summary>
        /// <param name="name">行为树名称</param>
        /// <returns>是否销毁成功</returns>
        public bool DestroyBehaviorTree(string name)
        {
            BTTree result = null;
            if (m_BTTreeDic.TryGetValue(name, out result))
            {
                result.Clear();
                m_BTTreeDic.Remove(name);
                return true;
            }

            return false;
        }
    }
}