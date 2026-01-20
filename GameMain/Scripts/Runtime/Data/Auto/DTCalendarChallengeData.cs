using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DTCalendarChallengeData
{
    public List<CalendarChallengeRewardData> CalendarChallengeRewardDatas;

    
    public Dictionary<TotalItemData,int> GetRewardsByLevel(int rewardLevel)
    {
        var rewardData = CalendarChallengeRewardDatas.Find(x => x.RewardLevel==rewardLevel);
        var rewards = new Dictionary<TotalItemData, int>();
        for (var i = 0; i < rewardData.RewardTypeList.Count; i++)
        {
            rewards.Add(rewardData.RewardTypeList[i],rewardData.RewardNumList[i]);
        }
        return rewards;
    }
}

[Serializable]
public class CalendarChallengeRewardData
{
    public int RewardLevel; // 奖励等级
    public string Rewards; // 奖励类型
    public string RewardsNum; // 奖励数量

    private List<TotalItemData> rewardTypeList;
    public List<TotalItemData> RewardTypeList
    {
        get
        {
            if(rewardTypeList!=null&&rewardTypeList.Count>0)
                return rewardTypeList;
            rewardTypeList = new List<TotalItemData>();
            var rewards = Rewards.Split(',');
            foreach (var reward in rewards)
            {
                var tmp = TotalItemData.FromInt(int.Parse(reward));
                rewardTypeList.Add(tmp);
            }

            return rewardTypeList;
        }
    }
    
    private List<int> rewardNumList;
    public List<int> RewardNumList
    {
        get
        {
            if(rewardNumList!=null&&rewardNumList.Count>0)
                return rewardNumList;
            rewardNumList = new List<int>();
            var rewards = RewardsNum.Split(',');
            foreach (var reward in rewards)
            {
                rewardNumList.Add(int.Parse(reward));
            }

            return rewardNumList;
        }
    }
}
