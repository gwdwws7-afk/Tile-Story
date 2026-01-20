namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// Sequence逻辑节点(有序地执行各个子结点，当一个子结点结束后才执行下一个)
    /// </summary>
    public class BTSequence : BTNode
    {
        private BTNode _activeChild;
        private int _activeIndex = -1;


        public BTSequence(BTPrecondition precondition = null) : base(precondition) { }

        protected override bool DoEvaluate()
        {
            if (_activeChild != null)
            {
                bool result = _activeChild.Evaluate();
                if (!result)
                {
                    _activeChild.Clear();
                    _activeChild = null;
                    _activeIndex = -1;
                }
                return result;
            }
            else
            {
                return m_Children[0].Evaluate();
            }
        }

        public override BTResult Tick(float elapseSeconds, float realElapseSeconds)
        {
            // first time
            if (_activeChild == null)
            {
                _activeChild = m_Children[0];
                _activeIndex = 0;
            }

            BTResult result = _activeChild.Tick(elapseSeconds, realElapseSeconds);
            if (result == BTResult.Ended)
            {   // Current active node over
                _activeIndex++;
                if (_activeIndex >= m_Children.Count)
                {   // sequence is over
                    _activeChild.Clear();
                    _activeChild = null;
                    _activeIndex = -1;
                }
                else
                {   // next node
                    _activeChild.Clear();
                    _activeChild = m_Children[_activeIndex];
                    result = BTResult.Running;
                }
            }
            return result;
        }

        public override void Clear()
        {
            if (_activeChild != null)
            {
                _activeChild = null;
                _activeIndex = -1;
            }

            foreach (BTNode child in m_Children)
            {
                child.Clear();
            }
        }
    }

}