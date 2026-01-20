using GameFramework;
using System.Collections.Generic;
/// <summary>
/// 奖励任务
/// </summary>
public sealed class RewardTask : TaskBase
{
    private int index;
    private TaskTarget rewardTaskTarget;
    private int targetNum;
    private List<TotalItemData> reward;
    private List<int> rewardNum;

    public RewardTask()
    {
        index = 0;
        rewardTaskTarget = TaskTarget.None;
        targetNum = 0;
        reward = null;
        rewardNum = null;
    }

    /// <summary>
    /// 任务序号
    /// </summary>
    public int Index { get => index; }

    /// <summary>
    /// 任务目标
    /// </summary>
    public TaskTarget Target { get => rewardTaskTarget; }

    /// <summary>
    /// 目标数量
    /// </summary>
    public int TargetNum { get => targetNum; }

    /// <summary>
    /// 任务奖励
    /// </summary>
    public List<TotalItemData> Reward { get => reward; }

    /// <summary>
    /// 奖励数量
    /// </summary>
    public List<int> RewardNum { get => rewardNum; }

    public static RewardTask Create(int id, int index, TaskTarget target, int targetNum, TotalItemData reward, int rewardNum, object userData)
    {
        RewardTask rewardTask = new RewardTask();
        rewardTask.Initialize(id, "RewardTask", 0, userData);
        rewardTask.index = index;
        rewardTask.rewardTaskTarget = target;
        rewardTask.targetNum = targetNum;

        rewardTask.reward = new List<TotalItemData>();
        rewardTask.reward.Add(reward);

        rewardTask.rewardNum = new List<int>();
        rewardTask.rewardNum.Add(rewardNum);
        return rewardTask;
    }

    public static RewardTask Create(int id, int index, TaskTarget target, int targetNum, List<TotalItemData> rewardList, List<int> rewardNumList, object userData)
    {
        RewardTask rewardTask = new RewardTask();
        rewardTask.Initialize(id, "RewardTask", 0, userData);
        rewardTask.index = index;
        rewardTask.rewardTaskTarget = target;
        rewardTask.targetNum = targetNum;
        rewardTask.reward = rewardList;
        rewardTask.rewardNum = rewardNumList;
        return rewardTask;
    }

    public override void Clear()
    {
        base.Clear();
        index = 0;
        rewardTaskTarget = TaskTarget.None;
        targetNum = 0;
        reward = null;
        rewardNum = null;
    }
}
