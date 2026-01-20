using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MySelf.Model;

[Serializable]
public class DTEndlessTreasureData
{
	public List<EndlessTreasureDatas> EndlessTreasureDatas;
    

	//根据id获取对应数据信息
	public EndlessTreasureDatas GetEndlessTreasureDatasById(int id)
	{
		if (id > EndlessTreasureDatas.Count)
		{
			id=(id-EndlessTreasureDatas.Count-1)%24+(EndlessTreasureDatas.Count-24+1);
			return EndlessTreasureDatas[id-1];
		}
		return EndlessTreasureDatas[id-1];
	}

	/// <summary>
	/// 获取当前六个宝箱的数据信息
	/// </summary>
	/// <param name="curEndChestId"></param>
	/// <returns></returns>
	public List<EndlessTreasureDatas> GetEndlessTreasureDataListById(int curEndChestId)
	{
		List<EndlessTreasureDatas> list = new List<EndlessTreasureDatas>();
		for (int i = 0; i < 6; i++)
		{
			list.Add(GetEndlessTreasureDatasById(curEndChestId+i));
		}
		return list;
	}

	public Dictionary<TotalItemData, int> GetFreeRewardChest(int curChestId)
	{
		Dictionary<TotalItemData, int> dict = new Dictionary<TotalItemData, int>();
		while (true)
		{
			EndlessTreasureDatas data = GetEndlessTreasureDatasById(curChestId);
			if (data.ProductID > 0) break;
			if (data.ProductID == 0)
			{
				foreach (var reward in data.RewardIdsDict)
				{
					if(!dict.ContainsKey(reward.Key))dict.Add(reward.Key,0);

					dict[reward.Key] += reward.Value;
				}
			}
			curChestId += 1;
		}
		return dict;
	}
	
	public Dictionary<TotalItemData, int> GetNormalFreeRewardChest(int curChestId)
	{
		Dictionary<TotalItemData, int> dict = new Dictionary<TotalItemData, int>();
		while (true)
		{
			EndlessTreasureDatas data = GetEndlessTreasureDatasById(curChestId);
			if (data.ProductID > 0) break;
			if (data.ProductID == 0)
			{
				foreach (var reward in data.NormalRewardDict)
				{
					if(!dict.ContainsKey(reward.Key))dict.Add(reward.Key,0);

					dict[reward.Key] += reward.Value;
				}
			}
			curChestId += 1;
		}
		return dict;
	}
	
	public Dictionary<TotalItemData, int> GetCardFreeRewardChest(int curChestId)
	{
		Dictionary<TotalItemData, int> dict = new Dictionary<TotalItemData, int>();
		while (true)
		{
			EndlessTreasureDatas data = GetEndlessTreasureDatasById(curChestId);
			if (data.ProductID > 0) break;
			if (data.ProductID == 0)
			{
				foreach (var reward in data.CardRewardDict)
				{
					if(!dict.ContainsKey(reward.Key))dict.Add(reward.Key,0);

					dict[reward.Key] += reward.Value;
				}
			}
			curChestId += 1;
		}
		return dict;
	}
}

[Serializable]
public class EndlessTreasureDatas
{
	public int ActivityID; // 活动ID
	public int ProductID; // 商品ID
	public int Rewardlevel; // 奖励等级
	public string Reward; // 奖励编号
	public string RewardNum; // 奖励数量
	public string CardReward;
	public string CardRewardNum;
	public int RewardChestID; // 宝箱编号
	public string Column6;//备注

	private Dictionary<TotalItemData,int> _normalRewardDict;
	private Dictionary<TotalItemData,int> _cardRewardDict;

	//奖励
	public Dictionary<TotalItemData, int> RewardIdsDict =>
		CardModel.Instance.IsInCardActivity ? CardRewardDict : NormalRewardDict;

	public Dictionary<TotalItemData, int> NormalRewardDict
	{
		get
		{
			if (_normalRewardDict == null)
			{
				_normalRewardDict = new Dictionary<TotalItemData, int>();
				if (Reward != null && RewardNum != null)
				{
					string[] rewards = Reward.Split(',');
					string[] rewardNums = RewardNum.Split(',');
					for (int i = 0; i < rewards.Length; i++)
					{
						int id = int.Parse(rewards[i]);
						int num = int.Parse(rewardNums[i]);
						if (id > 0 && num > 0) _normalRewardDict.Add(TotalItemData.FromInt(id), num);
					}
				}
			}

			return _normalRewardDict;
		}
	}

	public Dictionary<TotalItemData, int> CardRewardDict
	{
		get
		{
			if (_cardRewardDict == null)
			{
				_cardRewardDict = new Dictionary<TotalItemData, int>();
				if (CardReward != null && CardRewardNum != null)
				{
					string[] rewards = CardReward.Split(',');
					string[] rewardNums = CardRewardNum.Split(',');
					for (int i = 0; i < rewards.Length; i++)
					{
						int id = int.Parse(rewards[i]);
						int num = int.Parse(rewardNums[i]);
						if (id > 0 && num > 0) _cardRewardDict.Add(TotalItemData.FromInt(id), num);
					}
				}
			}

			return _cardRewardDict;
		}
	}
	
	public bool IsHaveChest => RewardChestID > 0;
}
