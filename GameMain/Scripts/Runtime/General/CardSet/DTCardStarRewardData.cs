using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class DTCardStarRewardData
{
    public List<CardStarReward> CardStarRewards;

    public CardStarReward GetRewardByID(int id)
    {
        return CardStarRewards.Find(reward => reward.ID == id);
    }
}

public class CardStarReward
{
    public int ID;
    public int StarNum;
    public string Reward;
    public string RewardNum;
    
    private List<TotalItemData> _rewardTypeList;
    private List<int> _rewardNumList;
    
    /// <summary>
    /// 奖励类型
    /// </summary>
    public List<TotalItemData> RewardTypeList
    {
        get
        {
            if (_rewardTypeList == null)
            {
                _rewardTypeList = new List<TotalItemData>();
                string[] splits = Reward.Split(',');
                foreach (var s in splits)
                {
                    if (int.TryParse(s, out int type))
                        _rewardTypeList.Add(TotalItemData.FromInt(type));
                }
            }

            return _rewardTypeList;
        }
    }

    /// <summary>
    /// 奖励数量
    /// </summary>
    public List<int> RewardNumList
    {
        get
        {
            if (_rewardNumList == null)
            {
                _rewardNumList = new List<int>();
                string[] splits = RewardNum.Split(',');
                foreach (var s in splits)
                {
                    if (int.TryParse(s, out int num))
                        _rewardNumList.Add(num);
                }
            }

            return _rewardNumList;
        }
    }
}