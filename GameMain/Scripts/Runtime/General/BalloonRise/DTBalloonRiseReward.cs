using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

[Serializable]
public class DTBalloonRiseReward
{
    public List<BalloonRiseStage> BalloonRiseStages;

    [JsonIgnore]
    private List<BalloonRiseStage> currentBalloonRiseStages;
    [JsonIgnore]
    public List<BalloonRiseStage> CurrentBalloonRiseStages
    {
        get
        {
            if (currentBalloonRiseStages == null)
            {
                GetDataTable();
            }
            return currentBalloonRiseStages;
        }
        set
        {
            if (currentBalloonRiseStages != value)
            {
                currentBalloonRiseStages = value;
            }
        }
    }

    /// <summary>
    /// 获取不同阶段的任务数据表
    /// </summary>
    public void GetDataTable()
    {
        int activityID = GameManager.DataTable.GetDataTable<DTBalloonRiseScheduleData>().Data.GetNowActiveActivityID();
        if (activityID > 0)
        {
            currentBalloonRiseStages = BalloonRiseStages.FindAll(stage => stage.ActivityID == activityID);
        }
        else
        {
            currentBalloonRiseStages = BalloonRiseStages;
        }
    }

    public BalloonRiseStage GetRewardByStageNum(int stageNum)
    {
        int level = GameManager.Task.BalloonRiseManager.StageStartLevel;
        int levelCondition = (level / 50 + 1) * 50;
        if (levelCondition > 150) levelCondition = 9999;
        BalloonRiseStage stage = CurrentBalloonRiseStages.Find(stage =>
            stage.LevelCondition == levelCondition && stage.Stage == stageNum);
        return stage;
    }
}

[Serializable]
public class BalloonRiseStage
{
    public int Index;
    public int ActivityID;
    public int LevelCondition;
    public int Stage;
    public string Reward;
    public string RewardNum;
    public string CardReward;
    public string CardRewardNum;

    private List<TotalItemData> normalRewardTypeList;
    private List<int> normalRewardNumList;
    
    private List<TotalItemData> cardRewardTypeList;
    private List<int> cardRewardNumList;
    
    
    /// <summary>
    /// 奖励类型
    /// </summary>
    public List<TotalItemData> RewardTypeList => CardModel.Instance.IsInCardActivity ? CardRewardTypeList : NormalRewardTypeList;

    #region RewardTypeList

    private List<TotalItemData> NormalRewardTypeList
    {
        get
        {
            if (normalRewardTypeList == null)
            {
                normalRewardTypeList = new List<TotalItemData>();
                string[] splits = Reward.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int type))
                    {
                        normalRewardTypeList.Add(TotalItemData.FromInt(type));
                    }
                }
            }

            return normalRewardTypeList;
        }
    }
    
    private List<TotalItemData> CardRewardTypeList
    {
        get
        {
            if (cardRewardTypeList == null)
            {
                cardRewardTypeList = new List<TotalItemData>();
                string[] splits = CardReward.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int type))
                    {
                        cardRewardTypeList.Add(TotalItemData.FromInt(type));
                    }
                }
            }
            return cardRewardTypeList;
        }
    }

    #endregion
    

    /// <summary>
    /// 奖励数量
    /// </summary>
    public List<int> RewardNumList => CardModel.Instance.IsInCardActivity ? CardRewardNumList : NormalRewardNumList;

    #region RewardNumList

    private List<int> NormalRewardNumList
    {
        get
        {
            if (normalRewardNumList == null)
            {
                normalRewardNumList = new List<int>();
                string[] splits = RewardNum.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int num))
                    {
                        normalRewardNumList.Add(num);
                    }
                }
            }
            return normalRewardNumList;
        }
    }
    
    private List<int> CardRewardNumList
    {
        get
        {
            if (cardRewardNumList == null)
            {
                cardRewardNumList = new List<int>();
                string[] splits = CardRewardNum.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int num))
                    {
                        cardRewardNumList.Add(num);
                    }
                }
            }
            return cardRewardNumList;
        }
    }

    #endregion
}
