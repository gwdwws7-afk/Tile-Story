using System;

namespace GameFramework.Fsm
{
    /// <summary>
    /// 有限状态机状态基类
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型</typeparam>
    public abstract class FsmState<T> where T : class
    {
        public FsmState()
        {
        }

        /// <summary>
        /// 状态机初始化时调用
        /// </summary>
        /// <param name="fsm">状态机引用</param>
        public virtual void OnInit(IFsm<T> fsm)
        {
        }

        /// <summary>
        /// 状态机状态进入时调用
        /// </summary>
        /// <param name="fsm">状态机引用</param>
        public virtual void OnEnter(IFsm<T> fsm)
        {
        }

        /// <summary>
        /// 状态机状态轮询时调用
        /// </summary>
        public virtual void OnUpate(IFsm<T> fsm, float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 状态机状态离开时调用
        /// </summary>
        public virtual void OnLeave(IFsm<T> fsm, bool isShutdown)
        {
        }

        /// <summary>
        /// 状态机状态销毁时调用
        /// </summary>
        public virtual void OnDestroy(IFsm<T> fsm)
        {
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        protected void ChangeState<TState>(IFsm<T> fsm) where TState : FsmState<T>
        {
            Fsm<T> fsmImplement = (Fsm<T>)fsm;
            if (fsmImplement == null)
            {
                Log.Error("需要切换状态的状态机为空，无法切换");
                return;
            }

            fsmImplement.ChangeState<TState>();
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        protected void ChangeState(IFsm<T> fsm, Type stateType)
        {
            Fsm<T> fsmImplement = (Fsm<T>)fsm;
            if (fsmImplement == null)
            {
                Log.Error("需要切换状态的状态机为空，无法切换");
                return;
            }

            if (stateType == null)
            {
                Log.Error("需要切换到的状态为空，无法切换");
                return;
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                Log.Error("要切换的状态没有直接或间接实现FsmState<T>，无法切换");
                return;
            }

            fsmImplement.ChangeState(stateType);
        }
    }
}