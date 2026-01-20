using System.Collections.Generic;
using System;

[Serializable]
public class DTHarvestKitchenChestData
{
    public List<DTHarvestKitchenChestDatas> KitchenChestDatas;

    public DTHarvestKitchenChestDatas GetKitchenChestDataById(int id)
    {
	    for (int i = 0; i < KitchenChestDatas.Count; i++)
	    {
		    if (KitchenChestDatas[i].ID == id)
			    return KitchenChestDatas[i];
	    }
	    return null;
    }
}

[Serializable]
public class DTHarvestKitchenChestDatas
{
	public int ID;	// 宝箱编号
	public int TargetDishNum; // 该阶段需要完成餐点数量
	public string Rewards;	 // 奖励类型
	public string RewardsNum;// 各个奖励的数量

	private List<TotalItemData> rewardTypeList;
	public List<TotalItemData> RewardsList
	{
		get
		{
			if (rewardTypeList == null)
			{
				rewardTypeList = new List<TotalItemData>();
				string[] splits = Rewards.Split(',');
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
	
	private List<int> rewardsNumList;
	public List<int> RewardsNumList
	{
		get
		{
			if (rewardsNumList != null && rewardsNumList.Count > 0)
				return rewardsNumList;
			rewardsNumList = new List<int>();
			var rewards = RewardsNum.Split(',');
			foreach (var reward in rewards)
			{
				var tmp = int.Parse(reward);
				rewardsNumList.Add(tmp);
			}
			
			return rewardsNumList;
		}
	}
}

