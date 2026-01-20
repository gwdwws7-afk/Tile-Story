using GameFramework.Fsm;
using System;

namespace GameFramework.Procedure
{
    /// <summary>
    /// 流程管理器
    /// </summary>
    public sealed class ProcedureManager : GameFrameworkModule, IProcedureManager
    {
        private IFsmManager fsmManager;
        private IFsm<ProcedureManager> procedureFsm;

        public ProcedureManager()
        {
            fsmManager = null;
            procedureFsm = null;
        }

        /// <summary>
        /// 获取游戏框架模块优先级
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行</remarks>
        public override int Priority
        {
            get
            {
                return -2;
            }
        }

        /// <summary>
        /// 当前流程
        /// </summary>
        public ProcedureBase CurrentProcedure
        {
            get
            {
                if (procedureFsm == null)
                {
                    throw new Exception("You must initialize procedure first.");
                }
                return (ProcedureBase)procedureFsm.CurrentState;
            }
        }

        /// <summary>
        /// 获取当前流程持续时间
        /// </summary>
        public float CurrentProcedureTime
        {
            get
            {
                if (procedureFsm == null)
                {
                    throw new Exception("You must initialize procedure first.");
                }
                return procedureFsm.CurrentStateTime;
            }
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override void Shutdown()
        {
            if (fsmManager != null)
            {
                if (procedureFsm != null)
                {
                    fsmManager.DestroyFsm(procedureFsm);
                    procedureFsm = null;
                }
                fsmManager = null;
            }
        }

        /// <summary>
        /// 初始化流程管理器
        /// </summary>
        /// <param name="fsmManager">有限状态机管理器</param>
        /// <param name="procedureBases">流程管理器包含的流程</param>
        public void Initialize(IFsmManager fsmManager, params ProcedureBase[] procedureBases)
        {
            if (fsmManager == null)
            {
                throw new Exception("FSM manager is invalid.");
            }

            this.fsmManager = fsmManager;
            procedureFsm = fsmManager.CreateFsm(this, procedureBases);
        }

        /// <summary>
        /// 开始流程
        /// </summary>
        /// <typeparam name="T">要开始的流程类型</typeparam>
        public void StartProcedure<T>() where T : ProcedureBase
        {
            if (procedureFsm == null)
            {
                throw new Exception("You must initialize procedure first.");
            }

            procedureFsm.Start<T>();
        }

        /// <summary>
        /// 开始流程
        /// </summary>
        /// <param name="procedureType">要开始的流程类型</param>
        public void StartProcedure(Type procedureType)
        {
            if (procedureFsm == null)
            {
                throw new Exception("You must initialize procedure first.");
            }

            procedureFsm.Start(procedureType);
        }

        /// <summary>
        /// 是否存在流程
        /// </summary>
        /// <typeparam name="T">要检查的流程类型</typeparam>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure<T>() where T : ProcedureBase
        {
            if (procedureFsm == null)
            {
                throw new Exception("You must initialize procedure first.");
            }

            return procedureFsm.HasState<T>();
        }

        /// <summary>
        /// 是否存在流程
        /// </summary>
        /// <param name="procedureType">要检查的流程类型</param>
        /// <returns>是否存在流程</returns>
        public bool HasProcedure(Type procedureType)
        {
            if (procedureFsm == null)
            {
                throw new Exception("You must initialize procedure first.");
            }

            return procedureFsm.HasState(procedureType);
        }

        /// <summary>
        /// 获取流程
        /// </summary>
        /// <typeparam name="T">要获取的流程类型</typeparam>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            if (procedureFsm == null)
            {
                throw new Exception("You must initialize procedure first.");
            }

            return procedureFsm.GetState<T>();
        }

        /// <summary>
        /// 获取流程
        /// </summary>
        /// <param name="procedureType">要获取的流程类型</param>
        /// <returns>要获取的流程</returns>
        public ProcedureBase GetProcedure(Type procedureType)
        {
            if (procedureFsm == null)
            {
                throw new Exception("You must initialize procedure first.");
            }

            return (ProcedureBase)procedureFsm.GetState(procedureType);
        }
        
    }
}