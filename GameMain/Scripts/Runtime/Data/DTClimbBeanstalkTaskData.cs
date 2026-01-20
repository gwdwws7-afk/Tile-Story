using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DTClimbBeanstalkTaskData
{
    public List<ClimbBeanstalkTaskDatas> ClimbBeanstalkTaskDatas;

    private List<ClimbBeanstalkTaskDatas> RecentClimbBeanstalkTaskDatas = new List<ClimbBeanstalkTaskDatas>();

    public void FilterTaskDatas(int inputActivityID, int inputPhaseID)
    {
        RecentClimbBeanstalkTaskDatas.Clear();

        Queue<ClimbBeanstalkTaskDatas> queue = new Queue<ClimbBeanstalkTaskDatas>();
        int maxTargetNum = -1;
        for (int i = 0; i < ClimbBeanstalkTaskDatas.Count; ++i)
        {
            if (ClimbBeanstalkTaskDatas[i].ActivityID == inputActivityID &&
                ClimbBeanstalkTaskDatas[i].PhaseID == inputPhaseID)
            {
                queue.Enqueue(ClimbBeanstalkTaskDatas[i]);
                if (ClimbBeanstalkTaskDatas[i].TargetNum > maxTargetNum)
                    maxTargetNum = ClimbBeanstalkTaskDatas[i].TargetNum;
            }
        }

        if (maxTargetNum <= 0)
        {
            Log.Error($"DTClimbBeanstalkTaskData Unexpected inputActivityID = {inputActivityID}");
            RecentClimbBeanstalkTaskDatas.AddRange(ClimbBeanstalkTaskDatas);
            return;
        }

        for (int i = 1; i < maxTargetNum + 1; ++i)
        {
            if (queue.Peek().TargetNum == i)
                RecentClimbBeanstalkTaskDatas.Add(queue.Dequeue());
            else
                RecentClimbBeanstalkTaskDatas.Add(new ClimbBeanstalkTaskDatas(inputActivityID, i));
        }
    }

    public bool CheckTargetActivityIDAndPhaseIDExist(int inputActivityID, int inputPhaseID)
    {
        for (int i = 0; i < ClimbBeanstalkTaskDatas.Count; ++i)
        {
            if (ClimbBeanstalkTaskDatas[i].ActivityID == inputActivityID &&
                ClimbBeanstalkTaskDatas[i].PhaseID == inputPhaseID)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// 获取当前生效的任务列表 由ActivityID + PhaseID 过滤得到
    /// </summary>
    public List<ClimbBeanstalkTaskDatas> GetRecentClimbBeanstalkTaskDatas()
    {
        //CheckRecentClimbBeanstalkTaskDatas();
        return RecentClimbBeanstalkTaskDatas;
    }

    //private void CheckRecentClimbBeanstalkTaskDatas()
    //{
    //    if (RecentClimbBeanstalkTaskDatas.Count == 0)
    //        RecentClimbBeanstalkTaskDatas.AddRange(ClimbBeanstalkTaskDatas);
    //}


}

[Serializable]
public class ClimbBeanstalkTaskDatas
{
    /// <summary>
    /// 编号(公式生成)
    /// </summary>
    public long ID;

    /// <summary>
    /// 活动ID
    /// </summary>
    public int ActivityID;

    /// <summary>
    /// 阶段ID
    /// </summary>
    public int PhaseID;

    /// <summary>
    /// 阶段序号
    /// </summary>
    public int Index;

    /// <summary>
    /// 任务目标
    /// </summary>
    public TaskTarget Target;

    /// <summary>
    /// 目标数量
    /// </summary>
    public int TargetNum;

    public string Reward;
    public string RewardNum;

    private List<TotalItemData> rewardTypeList;
    private List<int> rewardNumList;

    public int ChestType;

    /// <summary>
    /// 奖励类型
    /// </summary>
    public List<TotalItemData> RewardTypeList
    {
        get
        {
            if (rewardTypeList == null)
            {
                rewardTypeList = new List<TotalItemData>();
                string[] splits = Reward.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int type))
                    {
                        rewardTypeList.Add(TotalItemData.FromInt(type));
                    }
                }
            }

            return rewardTypeList;
        }
    }


    /// <summary>
    /// 奖励数量
    /// </summary>
    public List<int> RewardNumList
    {
        get
        {
            if (rewardNumList == null)
            {
                rewardNumList = new List<int>();
                string[] splits = RewardNum.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int num))
                    {
                        rewardNumList.Add(num);
                    }
                }
            }

            return rewardNumList;
        }
    }

    public ClimbBeanstalkTaskDatas(int inputActivityID, int inputTargetNum)
    {
        ID = -1;//for generated Non-Chest Data
        ActivityID = inputActivityID;
        Index = -1;//
        Target = TaskTarget.LevelPass;
        TargetNum = inputTargetNum;

        Reward = string.Empty;
        RewardNum = string.Empty;

        ChestType = -1;//
    }
}
