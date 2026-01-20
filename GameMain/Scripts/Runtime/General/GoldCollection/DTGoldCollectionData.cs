using System;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

[Serializable]
public class DTGoldCollectionData
{
    public List<GoldCollectionStage> GoldCollectionStages;

    public GoldCollectionStage GetStageTaskByIndex(int index)
    {
        if (index - 1 < 0 || index - 1 >= GoldCollectionStages.Count)
            return null;
        return GoldCollectionStages[index - 1];
    }

    public int GetNeedCollectNum(int index)
    {
        if (index <= 0 || index > GoldCollectionStages.Count)
            return 0;
        int count = 0;
        for (int i = 0; i < index; i++)
        {
            count += GoldCollectionStages[i].TargetNum;
        }
        return count;
    }
}

[Serializable]
public class GoldCollectionStage
{
    public int ActivityID;
    public int Index;
    public TaskTarget Target;
    public int TargetNum;
    public string Reward;
    public string RewardNum;
    public string CardReward;
    public string CardRewardNum;

    private List<TotalItemData> normalRewardTypeList;
    private List<int> normalRewardNumList;
    
    private List<TotalItemData> cardRewardTypeList;
    private List<int> cardRewardNumList;

    public int StageID
    {
        get
        {
            string index = Index < 10 ? "00" + Index.ToString() : "0" + Index.ToString();
            return int.Parse(ActivityID.ToString() + index);
        }
    }

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
