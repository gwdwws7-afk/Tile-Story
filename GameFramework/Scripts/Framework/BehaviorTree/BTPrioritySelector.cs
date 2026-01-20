namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// Priority Selector逻辑节点(先有序地遍历子节点，然后执行符合准入条件的第一个子结点)
    /// </summary>
    public class BTPrioritySelector : BTNode
    {
        private BTNode m_ActiveChild;

        public BTPrioritySelector(BTPrecondition precondition = null) : base(precondition) { }

        // selects the active child
        protected override bool DoEvaluate()
        {
            foreach (BTNode child in m_Children)
            {
                if (child.Evaluate())
                {
                    if (m_ActiveChild != null && m_ActiveChild != child)
                    {
                        m_ActiveChild.Clear();
                    }
                    m_ActiveChild = child;
                    return true;
                }
            }

            if (m_ActiveChild != null)
            {
                m_ActiveChild.Clear();
                m_ActiveChild = null;
            }

            return false;
        }

        public override void Clear()
        {
            if (m_ActiveChild != null)
            {
                m_ActiveChild.Clear();
                m_ActiveChild = null;
            }
        }

        public override BTResult Tick(float elapseSeconds, float realElapseSeconds)
        {
            if (m_ActiveChild == null)
            {
                return BTResult.Ended;
            }

            BTResult result = m_ActiveChild.Tick(elapseSeconds, realElapseSeconds);
            if (result != BTResult.Running)
            {
                m_ActiveChild.Clear();
                m_ActiveChild = null;
            }
            return result;
        }
    }
}