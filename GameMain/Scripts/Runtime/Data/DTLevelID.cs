using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MySelf.Model;

[Serializable]
public class DTLevelID 
{
	public List<LevelItemData> LevelItemDatas;
	public List<LevelItemData> LevelChallengeDatas;

    public LevelItemData GetLevelData(int level)
    {
		foreach (var obj in LevelItemDatas)
		{
			if (obj.LevelID == level)
			{
				return obj;
			}
		}

		return null;
	}

    public int GetLevelModelType(int level)
    {
	    int count = LevelItemDatas.Count;
	    if (count == 0)
		    return 1; // 安全防护，避免空列表异常

	    int max = LevelItemDatas[count - 1].LevelID;
	    
	    if (level > max)
	    {
		    level = (level - max) % 100 + (max - 100);
	    }
	    
	    for (int i = 0; i < count; i++)
	    {
		    var obj = LevelItemDatas[i];
		    if (obj.LevelID == level)
		    {
			    return obj.ModeID >= 1 ? obj.ModeID : 1;
		    }
	    }
	    
	    return 1;
    }

	public Queue<int> GetChallengeLevelList(bool isHard, bool isTimeLimit)
	{
		// var ans = LevelChallengeDatas
		// 	.Where(level => level.ModeID == (isHard ? 102 : 101) &&
		// 	                (isTimeLimit ? level.TimeLimit != 0 : level.TimeLimit == 0)).Select(obj => obj.LevelID)
		// 	.ToList();
		//
		// for (int i = 0; i < ans.Count; i++)
		// {
		// 	int index = UnityEngine.Random.Range(i, ans.Count);
		// 	(ans[i], ans[index]) = (ans[index], ans[i]);
		// }
		// return new Queue<int>(ans);
		int targetMode = isHard ? 102 : 101;

		// 假设 LevelChallengeDatas 是 List<LevelChallengeData>（或其他有 Count 和索引的集合）
		var ids = new List<int>(LevelChallengeDatas.Count);
		for (int i = 0, n = LevelChallengeDatas.Count; i < n; i++)
		{
			var item = LevelChallengeDatas[i];
			if (item.ModeID != targetMode) continue;

			// 保持与原逻辑一致：isTimeLimit 为 true 要求 TimeLimit != 0，否则 TimeLimit == 0
			if (isTimeLimit)
			{
				if (item.TimeLimit == 0) continue;
			}
			else
			{
				if (item.TimeLimit != 0) continue;
			}

			ids.Add(item.LevelID);
		}

		// 原来使用的是从 i 到 Count 的 Range（Unity 的 Random.Range minInclusive, maxExclusive）
		// 这是一个 Fisher–Yates 洗牌（从前向后也可以），复杂度 O(n)
		for (int i = 0, n = ids.Count; i < n; i++)
		{
			int j = UnityEngine.Random.Range(i, n); // j in [i, n-1]
			int tmp = ids[i];
			ids[i] = ids[j];
			ids[j] = tmp;
		}

		return new Queue<int>(ids);
	}

	public void SetAsSpecialGoldTile(int level)
	{
		LevelItemData data = null;
		foreach (var obj in LevelItemDatas)
		{
			if (obj.LevelID == level)
			{
				data = obj;
				break;
			}
		}

		if (data != null)
		{
			data.SpecialTile = 1;
		}
	}
	
	public bool GetIsSpecialGoldTile(int level)
	{
		LevelItemData data = null;
        foreach (var obj in LevelItemDatas)
        {
			if (obj.LevelID == level)
            {
				data = obj;
				break;
			}
        }

        if (data != null)
            if (data.SpecialTile > 0)
                return true;
        return false;
    }

	private List<int> normalLevels = null;

	public List<int> NormalLevels
	{
		get
		{
			if (normalLevels == null)
			{
				// normalLevels = LevelItemDatas
				// 	.Where(a => a.LevelID >= LevelModel.RandomLevelMin && a.LevelID <= LevelModel.TotalLevelNum && a.ModeID <= 3)
				// 	.Select(b => b.LevelID).ToList();
				normalLevels = new List<int>(LevelItemDatas.Count);
				for (int i = 0, n = LevelItemDatas.Count; i < n; i++)
				{
					var item = LevelItemDatas[i];
					if (item.LevelID >= LevelModel.RandomLevelMin &&
					    item.LevelID <= LevelModel.TotalLevelNum &&
					    item.ModeID <= 3)
					{
						normalLevels.Add(item.LevelID);
					}
				}
			}
			return normalLevels;
		}
	}

    private List<int> hardLevels = null;

    public List<int> HardLevels
    {
        get
        {
            if (hardLevels == null)
            {
                // hardLevels = LevelItemDatas
                //     .Where(a => a.LevelID >= LevelModel.RandomLevelMin && a.LevelID <= LevelModel.TotalLevelNum && a.ModeID == 4)
                //     .Select(b => b.LevelID).ToList();
                hardLevels = new List<int>(LevelItemDatas.Count);
                for (int i = 0, n = LevelItemDatas.Count; i < n; i++)
                {
	                var item = LevelItemDatas[i];
	                if (item.LevelID >= LevelModel.RandomLevelMin &&
	                    item.LevelID <= LevelModel.TotalLevelNum &&
	                    item.ModeID == 4)
	                {
		                hardLevels.Add(item.LevelID);
	                }
                }
            }
            return hardLevels;
        }
    }

    private List<int> superHardLevels = null;

    public List<int> SuperHardLevels
    {
        get
        {
            if (superHardLevels == null)
            {
                // superHardLevels = LevelItemDatas
                //     .Where(a => a.LevelID >= LevelModel.RandomLevelMin && a.LevelID <= LevelModel.TotalLevelNum && a.ModeID == 5)
                //     .Select(b => b.LevelID).ToList();
                superHardLevels = new List<int>(LevelItemDatas.Count);
                for (int i = 0, n = LevelItemDatas.Count; i < n; i++)
                {
	                var item = LevelItemDatas[i];
	                if (item.LevelID >= LevelModel.RandomLevelMin &&
	                    item.LevelID <= LevelModel.TotalLevelNum &&
	                    item.ModeID == 5)
	                {
		                superHardLevels.Add(item.LevelID);
	                }
                }
            }
            return superHardLevels;
        }
    }

    public int GetLevelLimitTime(int level)
    {
	    level -= 9000000;

		foreach (var obj in LevelChallengeDatas)
		{
			if (obj.LevelID == level)
			{
				return obj.TimeLimit;
			}
		}

	    return 0;
    }
}

public static class DTLevelUtil
{
	public static LevelType GetLevelType(int levelNum)
	{
		int levelModelType = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelModelType(levelNum);
		return GameManager.DataTable.GetDataTable<DTLevelTypeID>().Data.GetLevelTypeData(levelModelType);
	}

	public static int GetLevelHard(int levelNum)
	{
		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
		{
			return 0;
		}
		int levelModelType = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelModelType(levelNum);
		var levelType = GameManager.DataTable.GetDataTable<DTLevelTypeID>().Data.GetLevelTypeData(levelModelType);
		switch (levelType.ModeID)
		{
			case 1:
			case 2:
			case 3:
				return 0;
			case 4:
				return 1;
			case 5:
				return 2;
			default:
				return 0;
		}
	}

	public static string GetSkillBGImageName(int levelNum)
	{
		var hardIndex = GetLevelHard(levelNum);
		return $"{hardIndex}_SkillBG";
	}

	public static string GetAddOneStepImageName(int levelNum)
	{
		var hardIndex = GetLevelHard(levelNum);
		return $"{hardIndex}_AddOneStep";
	}

	public static string GetAddOneStepMaterialName(int levelNum)
	{
		var hardIndex = GetLevelHard(levelNum);
		switch (hardIndex)
		{
			case 1:
				return "Btn_Purple";
			case 2:
				return "Btn_Red";
			default:
				return "Btn_Blue";
		}
	}

	public static bool IsSpecialGoldTile(int levelNum)
	{
		if (!GameManager.Task.GoldCollectionTaskManager.ShowedFirstMenu)
			return false;
		if (GameManager.Task.GoldCollectionTaskManager.ShowedLastMenu)
			return false;
		if (GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge)
			return false;
		return GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetIsSpecialGoldTile(levelNum);
	}

	public static int GetRandomLevel(int levelNum)
	{
		levelNum = GetLevelNumByLoop(levelNum);

		var dataTable = GameManager.DataTable.GetDataTable<DTLevelID>().Data;
		int levelModelType = dataTable.GetLevelModelType(levelNum);
		bool isSpecialGoldTile = dataTable.GetIsSpecialGoldTile(levelNum);
		List<int> levelIDs;
		switch (levelModelType)
		{
			case 4:
				levelIDs = dataTable.HardLevels;
				break;
			case 5:
                levelIDs = dataTable.SuperHardLevels;
                break;
			default:
                levelIDs = dataTable.NormalLevels;
                break;
        }
		if (isSpecialGoldTile)
		{
			int levelID = levelIDs[UnityEngine.Random.Range(0, levelIDs.Count)];
			while (!dataTable.GetIsSpecialGoldTile(levelID))
			{
				levelID = levelIDs[UnityEngine.Random.Range(0, levelIDs.Count)];
            }
			return levelID;
		}
        return levelIDs[UnityEngine.Random.Range(0, levelIDs.Count)];
	}
	
	public static int GetLevelNumByLoop(int levelNum,int maxLevelNum=3000,int eachLoopLevelCount=20)
	{
		if (levelNum > maxLevelNum)
		{
			levelNum=(levelNum-maxLevelNum-1)%eachLoopLevelCount+(maxLevelNum-eachLoopLevelCount+1);
		}
		return levelNum;
	}
}
