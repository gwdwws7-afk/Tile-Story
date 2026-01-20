using System.Collections.Generic;
using System;

[Serializable]
public class DTKitchenTaskData
{
    public List<DTKitchenTaskDatas> KitchenTaskDatas;

    public DTKitchenTaskDatas GetKitchenTaskDataById(int id)
    {
	    for (int i = 0; i < KitchenTaskDatas.Count; i++)
	    {
		    if (KitchenTaskDatas[i].ID == id)
			    return KitchenTaskDatas[i];
	    }
	    return null;
    }
    
    public int GetKitchenTaskIndex(int id)
    {
	    for (int i = 0; i < KitchenTaskDatas.Count; i++)
	    {
		    if (KitchenTaskDatas[i].ID == id)
			    return i;
	    }
	    return 9999999;
    }
}

[Serializable]
public class DTKitchenTaskDatas
{
	public int ID;	// 任务ID
	public int TargetPraise; // 任务目标点赞数
	public int ChallengeToolNumber;// 开启一局消耗的厨师帽数量
	public int StartingLineNum;//开始增加上移速度的行数
	public float StartingSpeed;//初始上移速度
	public float GrowthRate;//上移速度增长速率
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

