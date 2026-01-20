using GameFramework;
using System;

public sealed partial class ProcessManager: GameFrameworkModule, IProcessManager
{
    /// <summary>
    /// 进程数据
    /// </summary>
    private sealed class Process : IReference
    {
        private string m_Name;
        private int m_Priority;
        private Action m_StartAction;
        private Action m_FinishAction;
        private Func<bool> m_CheckLockFunc;
        private ProcessStatus m_Status;
        private float m_CurrentProcessTime;

        public Process()
        {
            m_Name = null;
            m_Priority = 0;
            m_StartAction = null;
            m_FinishAction = null;
            m_CheckLockFunc = null;
            m_Status = ProcessStatus.None;
            m_CurrentProcessTime = 0;
        }

        /// <summary>
        /// 进程类型
        /// </summary>
        public string Name { get => m_Name; }

        /// <summary>
        /// 进程优先级
        /// </summary>
        public int Priority { get => m_Priority; }

        /// <summary>
        /// 进程状态
        /// </summary>
        public ProcessStatus Status { get => m_Status; set => m_Status = value; }

        /// <summary>
        /// 进程进行的时间
        /// </summary>
        public float CurrentProcessTime { get => m_CurrentProcessTime; }

        /// <summary>
        /// 进程是否被锁
        /// </summary>
        public bool IsLock 
        {
            get
            {
                return m_CheckLockFunc != null ? m_CheckLockFunc.Invoke() : false;
            }
        }

        public static Process Create(string name, int priority, Action startAction, Action finishAction, ProcessStatus status, Func<bool> checkLockFunc)
        {
            Process processData = new Process();

            processData.m_Name = name;
            processData.m_Priority = priority;
            processData.m_StartAction = startAction;
            processData.m_FinishAction = finishAction;
            processData.m_CheckLockFunc = checkLockFunc;
            processData.m_Status = status;

            return processData;
        }

        public void StartProcess(Action action=null)
        {
            if (m_Status != ProcessStatus.Wait)
            {
                Log.Warning("Start process state {0} fail.Process status is {1}", m_Name, m_Status);
                return;
            }

            Log.Info("Start process {0}", m_Name);

            action?.Invoke();
            
            m_Status = ProcessStatus.Playing;

#if UNITY_EDITOR
            m_StartAction?.Invoke();
#else
            try
            {
                m_StartAction?.Invoke();
            }
            catch (Exception e)
            {
                Log.Error($"Start Process:{m_Name} error! Exception:{e.Message}");
                GameManager.Process.EndProcess(m_Name);
            }
#endif
        }

        public void EndProcess(Action action=null)
        {
            if (m_Status != ProcessStatus.Playing && m_Status != ProcessStatus.End) 
            {
                Log.Warning("End process state {0} fail.Process status is {1}", m_Name, m_Status);
                return;
            }

            Log.Info("End process {0}", m_Name);


            m_Status = ProcessStatus.None;

#if UNITY_EDITOR
            m_FinishAction?.Invoke();
#else
            try
            {
                m_FinishAction?.Invoke();
            }
            catch (Exception e)
            {
                Log.Warning($"End Process Exception:{e.Message}");
            }
#endif
            
            action?.Invoke();
        }

        public void Clear()
        {
            m_Name = null;
            m_Priority = 0;
            m_StartAction = null;
            m_FinishAction = null;
            m_CheckLockFunc = null;
            m_Status = ProcessStatus.None;
            m_CurrentProcessTime = 0;
        }
    }
}
