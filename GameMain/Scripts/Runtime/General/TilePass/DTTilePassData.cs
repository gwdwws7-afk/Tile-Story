using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using MySelf.Model;

[Serializable]
public class DTTilePassData
{
    public List<TilePassData> TilePassDatas;

    [JsonIgnore]
    private List<TilePassData> currentTilePassDatas;
    [JsonIgnore]
    public List<TilePassData> CurrentTilePassDatas
    {
        get
        {
            if (currentTilePassDatas == null)
            {
                GetDataTable();
            }
            return currentTilePassDatas;
        }
        set
        {
            if (currentTilePassDatas != value)
            {
                currentTilePassDatas = value;
            }
        }
    }

    /// <summary>
    /// 获取不同阶段的任务数据表
    /// </summary>
    public void GetDataTable()
    {
        int activityID = GameManager.DataTable.GetDataTable<DTTilePassScheduleData>().Data.GetNowActiveActivityID();
        if (activityID > 0)
        {
            currentTilePassDatas = TilePassDatas.FindAll(stage => stage.ActivityID == activityID);
        }
        else
        {
            currentTilePassDatas = TilePassDatas;
        }
    }

    public int GetTotalTargetNum(int index)
    {
        if (index < 0)
            return 0;
        int totalTargetNum = 0;
        for (int i = 0; i <= index; i++)
        {
            totalTargetNum += CurrentTilePassDatas[i].TargetNum;
        }
        return totalTargetNum;
    }
}

[Serializable]
public class TilePassData
{
    public int ActivityID;
    public int Index;
    public int TargetNum;
    public string FreeRewards;
    public string FreeRewardsNum;
    public string VipRewards;
    public string VipRewardsNum;
    public string CardFreeRewards;
    public string CardFreeRewardsNum;
    public string CardVipRewards;
    public string CardVipRewardsNum;

    private List<TotalItemData> normalFreeRewardList;
    private List<int> normalFreeRewardNumList;
    private List<TotalItemData> normalVipRewardList;
    private List<int> normalVipRewardNumList;
    
    private List<TotalItemData> cardFreeRewardList;
    private List<int> cardFreeRewardNumList;
    private List<TotalItemData> cardVipRewardList;
    private List<int> cardVipRewardNumList;
    
    public List<TotalItemData> FreeRewardList => CardModel.Instance.IsInCardActivity ? CardFreeRewardList : NormalFreeRewardList;

    #region FreeRewardList

    private List<TotalItemData> NormalFreeRewardList
    {
        get
        {
            if (normalFreeRewardList == null)
            {
                normalFreeRewardList = new List<TotalItemData>();
                string[] splits = FreeRewards.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int type))
                    {
                        normalFreeRewardList.Add(TotalItemData.FromInt(type));
                    }
                }
            }
            return normalFreeRewardList;
        }
    }
    
    private List<TotalItemData> CardFreeRewardList
    {
        get
        {
            if (cardFreeRewardList == null)
            {
                cardFreeRewardList = new List<TotalItemData>();
                string[] splits = CardFreeRewards.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int type))
                    {
                        cardFreeRewardList.Add(TotalItemData.FromInt(type));
                    }
                }
            }
            return cardFreeRewardList;
        }
    }

    #endregion

    public List<int> FreeRewardNumList => CardModel.Instance.IsInCardActivity ? CardFreeRewardNumList : NormalFreeRewardNumList;

    #region FreeRewardNumList

    private List<int> NormalFreeRewardNumList
    {
        get
        {
            if (normalFreeRewardNumList == null)
            {
                normalFreeRewardNumList = new List<int>();
                string[] splits = FreeRewardsNum.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int num))
                    {
                        normalFreeRewardNumList.Add(num);
                    }
                }
            }
            return normalFreeRewardNumList;
        }
    }
    
    private List<int> CardFreeRewardNumList
    {
        get
        {
            if (cardFreeRewardNumList == null)
            {
                cardFreeRewardNumList = new List<int>();
                string[] splits = CardFreeRewardsNum.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int num))
                    {
                        cardFreeRewardNumList.Add(num);
                    }
                }
            }
            return cardFreeRewardNumList;
        }
    }

    #endregion

    public List<TotalItemData> VIPRewardList => CardModel.Instance.IsInCardActivity ? CardVipRewardList : NormalVipRewardList;

    #region VIPRewardList
    
    private List<TotalItemData> NormalVipRewardList
    {
        get
        {
            if (normalVipRewardList == null)
            {
                normalVipRewardList = new List<TotalItemData>();
                string[] splits = VipRewards.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int type))
                    {
                        normalVipRewardList.Add(TotalItemData.FromInt(type));
                    }
                }
            }
            return normalVipRewardList;
        }
    }
    
    private List<TotalItemData> CardVipRewardList
    {
        get
        {
            if (cardVipRewardList == null)
            {
                cardVipRewardList = new List<TotalItemData>();
                string[] splits = CardVipRewards.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int type))
                    {
                        cardVipRewardList.Add(TotalItemData.FromInt(type));
                    }
                }
            }
            return cardVipRewardList;
        }
    }
    
    #endregion

    public List<int> VIPRewardNumList => CardModel.Instance.IsInCardActivity ? CardVipRewardNumList : NormalVipRewardNumList;

    #region VIPRewardNumList
    
    private List<int> NormalVipRewardNumList
    {
        get
        {
            if (normalVipRewardNumList == null)
            {
                normalVipRewardNumList = new List<int>();
                string[] splits = VipRewardsNum.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int num))
                    {
                        normalVipRewardNumList.Add(num);
                    }
                }
            }
            return normalVipRewardNumList;
        }
    }
    
    private List<int> CardVipRewardNumList
    {
        get
        {
            if (cardVipRewardNumList == null)
            {
                cardVipRewardNumList = new List<int>();
                string[] splits = CardVipRewardsNum.Split(',');
                for (int i = 0; i < splits.Length; i++)
                {
                    if (int.TryParse(splits[i], out int num))
                    {
                        cardVipRewardNumList.Add(num);
                    }
                }
            }
            return cardVipRewardNumList;
        }
    }
    
    #endregion
}
