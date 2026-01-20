using System;

public sealed class ConditionTriggerTaskAgent : ITaskAgent<ConditionTriggerTask>
{
    private ConditionTriggerTask m_Task;
    private float m_WaitTime;

    public ConditionTriggerTask Task { get => m_Task; }

    public float WaitTime { get => m_WaitTime; }

    public ConditionTriggerTaskAgent()
    {
        m_Task = null;
        m_WaitTime = 0;
    }

    public void Initialize()
    {
    }

    public void Reset()
    {
        m_Task = null;
        m_WaitTime = 0;
    }

    public void Shutdown()
    {
        Reset();
    }

    public void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (m_Task.Status == ConditionTriggerTaskStatus.Doing)
        {
            if (m_Task.CheckCompleteAction())
            {
                m_Task.Status = ConditionTriggerTaskStatus.Done;
                m_Task.CompleteAction?.Invoke();
                m_Task.Done = true;
                return;
            }

            if (m_Task.TimeOut > 0)
            {
                m_WaitTime += realElapseSeconds;
                if (m_WaitTime > m_Task.TimeOut)
                {
                    m_Task.Status = ConditionTriggerTaskStatus.Error;
                    m_Task.FailAction?.Invoke();
                    m_Task.Done = true;
                }
            }
        }
    }

    public StartTaskStatus Start(ConditionTriggerTask task)
    {
        if (task == null)
        {
            throw new Exception("Task is invalid.");
        }

        m_Task = task;
        m_Task.Status = ConditionTriggerTaskStatus.Doing;

        try
        {
            m_Task.StartAction?.Invoke();
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogError($"ConditionTriggerTaskAgent Start error - task {task.SerialId} start action error {e.Message}");

            return StartTaskStatus.UnknownError;
        }

        m_WaitTime = 0;
        return StartTaskStatus.CanResume;
    }
}
