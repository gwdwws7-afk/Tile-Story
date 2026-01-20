using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;

[Serializable]
public class DTNormalTurntable
{
    public List<TurntableReward> turntableRewardData;

    public TurntableReward GetTurntableRewardByIndex(int index)
    {
        if (index < 0 || index >= turntableRewardData.Count)
            return null;
        return turntableRewardData[index];
    }
}

[Serializable]
public class TurntableReward
{
    public int Index;
    public string Rewards;
    public string RewardsNum;
    public string CardRewards;
    public string CardRewardsNum;
    public float Probability;
    
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
                string[] splits = Rewards.Split(',');
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
                string[] splits = CardRewards.Split(',');
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
                string[] splits = RewardsNum.Split(',');
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
                string[] splits = CardRewardsNum.Split(',');
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
