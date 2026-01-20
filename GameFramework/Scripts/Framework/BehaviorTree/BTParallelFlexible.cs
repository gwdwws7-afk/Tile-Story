using System.Collections.Generic;

namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// ParallelFlexible逻辑节点(同时执行各个子结点，当所有子结点的准入条件失败，它就不会执行，当所有子节点结束，它就结束)
    /// </summary>
    public class BTParallelFlexible : BTNode
    {

        private List<bool> _activeList = new List<bool>();


        public BTParallelFlexible(BTPrecondition precondition = null) : base(precondition) { }

        protected override bool DoEvaluate()
        {
            int numActiveChildren = 0;

            for (int i = 0; i < m_Children.Count; i++)
            {
                BTNode child = m_Children[i];
                if (child.Evaluate())
                {
                    _activeList[i] = true;
                    numActiveChildren++;
                }
                else
                {
                    _activeList[i] = false;
                }
            }

            if (numActiveChildren == 0)
            {
                return false;
            }

            return true;
        }

        public override BTResult Tick(float elapseSeconds, float realElapseSeconds)
        {
            int numRunningChildren = 0;

            for (int i = 0; i < m_Children.Count; i++)
            {
                bool active = _activeList[i];
                if (active)
                {
                    BTResult result = m_Children[i].Tick(elapseSeconds, realElapseSeconds);
                    if (result == BTResult.Running)
                    {
                        numRunningChildren++;
                    }
                }
            }

            if (numRunningChildren == 0)
            {
                return BTResult.Ended;
            }

            return BTResult.Running;
        }

        public override void AddChild(BTNode aNode)
        {
            base.AddChild(aNode);
            _activeList.Add(false);
        }

        public override void RemoveChild(BTNode aNode)
        {
            int index = m_Children.IndexOf(aNode);
            _activeList.RemoveAt(index);
            base.RemoveChild(aNode);
        }

        public override void Clear()
        {
            base.Clear();

            foreach (BTNode child in m_Children)
            {
                child.Clear();
            }
        }
    }

}