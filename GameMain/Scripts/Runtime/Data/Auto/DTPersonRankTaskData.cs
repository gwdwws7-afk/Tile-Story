using System;
using System.Collections.Generic;
using MySelf.Model;

[Serializable]
public class DTPersonRankTaskData
{
    public List<PersonRankTaskData> PersonRankTaskDatas;
    public List<PersonRankRewardData> PersonRankRewardDatas;

    public PersonRankTaskData GetPersonRankTaskDataByLevel(PersonRankLevel level)
    {
        var levelInt = (int)level;
        return PersonRankTaskDatas.Find(x => x.PersonRankLevel == levelInt);
    }
    
    public Dictionary<TotalItemData,int> GetRewardsByLevel(PersonRankLevel level,int rank)
    {
        var rankLevel = 0;
        if (rank == 1) rankLevel = 0;
        else if (rank == 2) rankLevel = 1;
        else if (rank == 3) rankLevel = 2;
        else if (rank >= 4 && rank <= 10) rankLevel = 3;
        else if (rank >= 11 && rank <= 25) rankLevel = 4;
        else rankLevel = 5;
        var levelInt = (int)level;
        var rewardData = PersonRankRewardDatas.Find(x => x.PersonRankLevel == levelInt&&x.RewardLevel==rankLevel);
        var rewards = new Dictionary<TotalItemData, int>();
        for (var i = 0; i < rewardData.RewardTypeList.Count; i++)
        {
            rewards.Add(rewardData.RewardTypeList[i],rewardData.RewardNumList[i]);
        }
        return rewards;
    }
}

[Serializable]
public class PersonRankTaskData
{
    public int PersonRankLevel; // 段位
    public int UpRange; // 升段比例
}

[Serializable]
public class PersonRankRewardData
{
    public int PersonRankLevel; // 段位
    public int RewardLevel; // 奖励等级
    public string Rewards; // 奖励类型
    public string RewardsNum; // 奖励数量
    public string CardRewards;
    public string CardRewardsNum;

    private List<TotalItemData> _normalRewardTypeList;
    private List<int> _normalRewardNumList;

    private List<TotalItemData> _cardRewardTypeList;
    private List<int> _cardRewardNumList;

    public List<TotalItemData> RewardTypeList =>
        CardModel.Instance.IsInCardActivity ? CardRewardTypeList : NormalRewardTypeList;
    
    #region RewardTypeList
    
    private List<TotalItemData> NormalRewardTypeList
    {
        get
        {
            if (_normalRewardTypeList is { Count: > 0 })
                return _normalRewardTypeList;
            _normalRewardTypeList = new List<TotalItemData>();
            var rewards = Rewards.Split(',');
            foreach (var reward in rewards)
            {
                var tmp = TotalItemData.FromInt(int.Parse(reward));
                _normalRewardTypeList.Add(tmp);
            }

            return _normalRewardTypeList;
        }
    }
    
    private List<TotalItemData> CardRewardTypeList
    {
        get
        {
            if (_cardRewardTypeList is { Count: > 0 })
                return _cardRewardTypeList;
            _cardRewardTypeList = new List<TotalItemData>();
            var rewards = CardRewards.Split(',');
            foreach (var reward in rewards)
            {
                var tmp = TotalItemData.FromInt(int.Parse(reward));
                _cardRewardTypeList.Add(tmp);
            }

            return _cardRewardTypeList;
        }
    }
    
    #endregion

    public List<int> RewardNumList => CardModel.Instance.IsInCardActivity ? CardRewardNumList : NormalRewardNumList;

    #region RewardNumList
    
    private List<int> NormalRewardNumList
    {
        get
        {
            if (_normalRewardNumList is { Count: > 0 })
                return _normalRewardNumList;
            _normalRewardNumList = new List<int>();
            var rewards = RewardsNum.Split(',');
            foreach (var reward in rewards)
            {
                _normalRewardNumList.Add(int.Parse(reward));
            }

            return _normalRewardNumList;
        }
    }
    
    private List<int> CardRewardNumList
    {
        get
        {
            if (_cardRewardNumList is { Count: > 0 })
                return _cardRewardNumList;
            _cardRewardNumList = new List<int>();
            var rewards = CardRewardsNum.Split(',');
            foreach (var reward in rewards)
            {
                _cardRewardNumList.Add(int.Parse(reward));
            }

            return _cardRewardNumList;
        }
    }
    
    #endregion
}
