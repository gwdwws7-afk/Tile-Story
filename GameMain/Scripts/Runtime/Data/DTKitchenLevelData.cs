using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

[Serializable]
public class DTKitchenLevelData
{
    public List<DTKitchenLevelDatas> KitchenLevelDatas;

    public DTKitchenLevelDatas GetKitchenLevelDataByLevel(int level)
    {
	    if(level < KitchenLevelDatas.Count)
			return KitchenLevelDatas[level];
	    return null;
    }
}

[Serializable]
public class DTKitchenLevelDatas
{
	public int ID;
	public int TargetCustomer;	// 目标顾客数量
	public int NormalTileNumber;// 默认关卡花色数量
	public int MinTileNumber;	// 最小花色数量
	public int FailedNumber;	// 花色减少所需的连续失败次数
	public int FoodTypeNumber;	// 食物最大需求种类
	public float EmotionReduce;	// 情绪削减的速度
	public int CoinProbability;// 金币出现概率
	public string FoodNumber;	// 需求食物数量
	public string FoodProbability;// 需求食物数量出现概率
	public string WaveCustomerNum;// 同时出现的顾客数量

	private List<int> foodNumberList;
	public List<int> FoodNumberList
	{
		get
		{
			if(foodNumberList!=null&&foodNumberList.Count>0)
				return foodNumberList;
			foodNumberList = new List<int>();
			var rewards = FoodNumber.Split(',');
			foreach (var reward in rewards)
			{
				var tmp = int.Parse(reward);
				foodNumberList.Add(tmp);
			}
			
			return foodNumberList;
		}
	}
	
	private List<int> foodProbabilityList;
	public List<int> FoodProbabilityList
	{
		get
		{
			if(foodProbabilityList!=null&&foodProbabilityList.Count>0)
				return foodProbabilityList;
			foodProbabilityList = new List<int>();
			var rewards = FoodProbability.Split(',');
			foreach (var reward in rewards)
			{
				var tmp = int.Parse(reward, CultureInfo.InvariantCulture);
				foodProbabilityList.Add(tmp);
			}
			
			return foodProbabilityList;
		}
	}

	private List<int> waveCustomerNumList;
	public List<int> WaveCustomerNumList
	{
		get
		{
			if(waveCustomerNumList!=null&&waveCustomerNumList.Count>0)
				return waveCustomerNumList;
			waveCustomerNumList = new List<int>();
			var rewards = WaveCustomerNum.Split(',');
			foreach (var reward in rewards)
			{
				var tmp = int.Parse(reward, CultureInfo.InvariantCulture);
				waveCustomerNumList.Add(tmp);
			}
			
			return waveCustomerNumList;
		}
	}
}

