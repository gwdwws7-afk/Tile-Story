using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// 行为树节点基类
    /// </summary>
    public abstract class BTNode
    {
        private string m_Name;
        private BTPrecondition m_Precondition;
        private Database m_Database;
        private float m_CooldownInterval;
        private float m_LastTimeEvaluated;
        private bool m_Activated;
        protected List<BTNode> m_Children;

        public BTNode() : this(null) { }

        public BTNode(BTPrecondition precondition)
        {
            m_Name = string.Empty;
            m_Precondition = precondition;
            m_Database = null;
            m_CooldownInterval = 0;
            m_LastTimeEvaluated = 0;
            m_Activated = false;
            m_Children = null;
        }

        /// <summary>
        /// 获取或设置节点名称
        /// </summary>
        public string Name { get { return m_Name; } set { m_Name = value; } }

        /// <summary>
        /// 获取节点数据库
        /// </summary>
        public Database Database { get { return m_Database; } }

        /// <summary>
        /// 获取或设置节点冷却时间
        /// </summary>
        public float CooldownInterval { get { return m_CooldownInterval; } set { m_CooldownInterval = value; } }

        /// <summary>
        /// 节点是否被激活
        /// </summary>
        public bool Activated { get { return m_Activated; } }

        public void Activate(Database database)
        {
            if (m_Activated)
                return;

            m_Database = database;

            if (m_Precondition != null)
            {
                m_Precondition.Activate(database);
            }

            if (m_Children != null)
            {
                foreach (BTNode child in m_Children)
                {
                    child.Activate(database);
                }
            }

            m_Activated = true;

            OnActivate();
        }

        protected virtual void OnActivate()
        {
        }

        public bool Evaluate()
        {
            bool coolDownOK = CheckTimer();

            return m_Activated && coolDownOK && (m_Precondition == null || m_Precondition.Check()) && DoEvaluate();
        }

        protected virtual bool DoEvaluate() { return true; }

        public virtual BTResult Tick(float elapseSeconds, float realElapseSeconds)
        {
            return BTResult.Ended;
        }

        public virtual void Clear() { }

        public virtual void AddChild(BTNode aNode)
        {
            if (m_Children == null)
            {
                m_Children = new List<BTNode>();
            }
            if (aNode != null)
            {
                m_Children.Add(aNode);
            }
        }

        public virtual void RemoveChild(BTNode aNode)
        {
            if (m_Children != null && aNode != null)
            {
                m_Children.Remove(aNode);
            }
        }

        // Check if cooldown is finished.
        private bool CheckTimer()
        {
            if (Time.time - m_LastTimeEvaluated > m_CooldownInterval)
            {
                m_LastTimeEvaluated = Time.time;
                return true;
            }
            return false;
        }
    }


    public enum BTResult
    {
        Ended = 1,
        Running = 2,
    }
}