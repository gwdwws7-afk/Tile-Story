using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

[Serializable]
public class DTFrogJumpData
{
    public List<FrogJumpRewardData> FrogJumpRewardDatas;
    
    public KeyValuePair<TotalItemData,int> GetReward(int level)
    {
        var data = FrogJumpRewardDatas.Find(x => x.Level == level);
        
        return CardModel.Instance.IsInCardActivity
            ? new KeyValuePair<TotalItemData, int>(TotalItemData.FromInt(data.CardRewardType), data.CardRewardNum)
            : new KeyValuePair<TotalItemData, int>(TotalItemData.FromInt(data.RewardType), data.RewardNum);
    }
    
    public int MaxLevel()
    {
        return FrogJumpRewardDatas.Count;
    }
}

[Serializable]
public class FrogJumpRewardData
{
    public int Level; // 任务序号
    public int RewardType; // 奖励
    public int RewardNum; // 奖励数量
    public int CardRewardType;
    public int CardRewardNum;
}
