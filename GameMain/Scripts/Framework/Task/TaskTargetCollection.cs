using System.Collections.Generic;

/// <summary>
/// 任务目标收集类
/// </summary>
public sealed class TaskTargetCollection
{
    private readonly Dictionary<TaskTarget, int> targetCollectDic;

    public TaskTargetCollection()
    {
        targetCollectDic = new Dictionary<TaskTarget, int>();
    }

    /// <summary>
    /// 获取任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    public int GetTargetCollectNum(TaskTarget rewardTaskTarget)
    {
        if (targetCollectDic.TryGetValue(rewardTaskTarget, out int collectNum)) 
        {
            return collectNum;
        }
        else
        {
            if (targetCollectDic.Count != 0)
            {
                Log.Warning("task target not match");
            }
            return 0;
        }
    }

    /// <summary>
    /// 增加任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    /// <param name="collectNum">收集数量</param>
    public void AddTargetCollectNum(TaskTarget rewardTaskTarget, int collectNum)
    {
        if (targetCollectDic.TryGetValue(rewardTaskTarget, out int num))
        {
            targetCollectDic[rewardTaskTarget] = collectNum + num;
        }
        else
        {
            targetCollectDic.Add(rewardTaskTarget, collectNum);
        }
    }

    /// <summary>
    /// 清空任务目标收集数
    /// </summary>
    /// <param name="rewardTaskTarget">任务目标</param>
    public void ClearTargetCollectNum(TaskTarget rewardTaskTarget)
    {
        if(targetCollectDic.ContainsKey(rewardTaskTarget))
            targetCollectDic.Remove(rewardTaskTarget);
    }

    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        targetCollectDic.Clear();
    }
}
