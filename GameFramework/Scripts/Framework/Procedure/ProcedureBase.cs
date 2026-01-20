using GameFramework.Fsm;

namespace GameFramework.Procedure
{
    /// <summary>
    /// 流程基类
    /// </summary>
    public abstract class ProcedureBase : FsmState<ProcedureManager>
    {
        /// <summary>
        /// 流程名称
        /// </summary>
        public abstract string ProcedureName { get; }

        public override void OnEnter(IFsm<ProcedureManager> fsm)
        {
            base.OnEnter(fsm);
            Log.Info("进入流程：" + ProcedureName);
        }

        public override void OnLeave(IFsm<ProcedureManager> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);
            Log.Info("离开流程：" + ProcedureName);
        }
    }
}