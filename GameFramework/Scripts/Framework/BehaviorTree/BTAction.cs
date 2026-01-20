namespace GameFramework.BehaviorTree
{
    /// <summary>
    /// 行为树行为节点基类
    /// </summary>
    public class BTAction : BTNode
    {
        private BTActionStatus m_Status = BTActionStatus.Ready;

        public BTAction(BTPrecondition precondition = null) : base(precondition) { }


        protected virtual void Enter()
        {
        }

        protected virtual void Exit()
        {
        }

        protected virtual BTResult Execute(float elapseSeconds, float realElapseSeconds)
        {
            return BTResult.Running;
        }

        public override void Clear()
        {
            if (m_Status != BTActionStatus.Ready)
            {
                Exit();
                m_Status = BTActionStatus.Ready;
            }
        }

        public override BTResult Tick(float elapseSeconds, float realElapseSeconds)
        {
            BTResult result = BTResult.Ended;
            if (m_Status == BTActionStatus.Ready)
            {
                Enter();
                m_Status = BTActionStatus.Running;
            }
            if (m_Status == BTActionStatus.Running)
            {
                result = Execute(elapseSeconds, realElapseSeconds);
                if (result != BTResult.Running)
                {
                    Exit();
                    m_Status = BTActionStatus.Ready;
                }
            }
            return result;
        }

        public override void AddChild(BTNode aNode)
        {
            Log.Error("BTAction: Cannot add a node into BTAction.");
        }

        public override void RemoveChild(BTNode aNode)
        {
            Log.Error("BTAction: Cannot remove a node into BTAction.");
        }


        private enum BTActionStatus
        {
            Ready = 1,
            Running = 2,
        }
    }
}