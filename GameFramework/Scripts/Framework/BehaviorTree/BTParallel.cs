using System.Collections.Generic;

namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// Parallel逻辑节点(同时执行各个子结点，每当任一子结点的准入条件失败，它就不会执行)
    /// </summary>
    public class BTParallel : BTNode
    {
        protected List<BTResult> _results;
        protected ParallelFunction _func;

        public BTParallel(ParallelFunction func) : this(func, null) { }

        public BTParallel(ParallelFunction func, BTPrecondition precondition) : base(precondition)
        {
            _results = new List<BTResult>();
            this._func = func;
        }

        protected override bool DoEvaluate()
        {
            foreach (BTNode child in m_Children)
            {
                if (!child.Evaluate())
                {
                    return false;
                }
            }
            return true;
        }

        public override BTResult Tick(float elapseSeconds, float realElapseSeconds)
        {
            int endingResultCount = 0;

            for (int i = 0; i < m_Children.Count; i++)
            {

                if (_func == ParallelFunction.And)
                {
                    if (_results[i] == BTResult.Running)
                    {
                        _results[i] = m_Children[i].Tick(elapseSeconds, realElapseSeconds);
                    }
                    if (_results[i] != BTResult.Running)
                    {
                        endingResultCount++;
                    }
                }
                else
                {
                    if (_results[i] == BTResult.Running)
                    {
                        _results[i] = m_Children[i].Tick(elapseSeconds, realElapseSeconds);
                    }
                    if (_results[i] != BTResult.Running)
                    {
                        ResetResults();
                        return BTResult.Ended;
                    }
                }
            }
            if (endingResultCount == m_Children.Count)
            {   // only apply to AND func
                ResetResults();
                return BTResult.Ended;
            }
            return BTResult.Running;
        }

        public override void Clear()
        {
            ResetResults();

            foreach (BTNode child in m_Children)
            {
                child.Clear();
            }
        }

        public override void AddChild(BTNode aNode)
        {
            base.AddChild(aNode);
            _results.Add(BTResult.Running);
        }

        public override void RemoveChild(BTNode aNode)
        {
            int index = m_Children.IndexOf(aNode);
            _results.RemoveAt(index);
            base.RemoveChild(aNode);
        }

        private void ResetResults()
        {
            for (int i = 0; i < _results.Count; i++)
            {
                _results[i] = BTResult.Running;
            }
        }

        public enum ParallelFunction
        {
            And = 1,    // returns Ended when all results are not running
            Or = 2,     // returns Ended when any result is not running
        }
    }

}