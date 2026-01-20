namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// 行为树基类
    /// </summary>
    public abstract class BTTree
    {
        protected BTNode m_Root;
        private Database m_Database;
        private bool m_IsPaused;
        private bool m_IsDestroyed;

        public BTTree()
        {
            m_Root = null;
            m_Database = null;
            m_IsPaused = false;
            m_IsDestroyed = true;
        }

        /// <summary>
        /// 节点数据库
        /// </summary>
        public Database Database { get { return m_Database; } }

        /// <summary>
        /// 获取或配置暂停行为树
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return m_IsPaused;
            }
            set
            {
                m_IsPaused = value;
            }
        }

        /// <summary>
        /// 获取是否被破坏
        /// </summary>
        public bool IsDestroyed
        {
            get
            {
                return m_IsDestroyed;
            }
        }

        public void Init()
        {
            m_Database = new Database();

            OnInit();

            m_Root.Activate(m_Database);

            m_IsDestroyed = false;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_IsPaused)
                return;

            if (m_Root.Evaluate())
            {
                m_Root.Tick(elapseSeconds, realElapseSeconds);
            }

            OnUpdate(elapseSeconds, realElapseSeconds);
        }

        public void Clear()
        {
            if (m_Root != null)
            {
                m_Root.Clear();
                m_Root = null;
            }
            m_Database = null;
            m_IsPaused = false;
            m_IsDestroyed = true;

            OnClear();
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnClear()
        {
        }

        protected virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }
    }
}