using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class DTBGID 
{
	public List<BGItemData> BGItemDatas;

	private Dictionary<int, BGItemData> bgItemDataDict;
	public Dictionary<int, BGItemData> BGItemDataDict
	{
		get
		{
			if (bgItemDataDict == null)
				bgItemDataDict = BGItemDatas.ToDictionary(a=>a.ID,b=>b);
			return bgItemDataDict;
		}
	}

	private Dictionary<int, List<BGItemData>> bgThemeDict;
	public Dictionary<int, List<BGItemData>> BGThemeDict
	{
		get
		{
			if (bgThemeDict == null)
			{
				bgThemeDict = new Dictionary<int, List<BGItemData>>();
				foreach (var bgItem in BGItemDatas)
				{
					if (!bgThemeDict.ContainsKey(bgItem.Theme)) bgThemeDict.Add(bgItem.Theme,new List<BGItemData>());
					bgThemeDict[bgItem.Theme].Add(bgItem);
				}
			}
			return bgThemeDict;
		}
	}
	
	private List<int> nonEventThemeList;
	public List<int> NonEventThemeList
	{
		get
		{
			if (nonEventThemeList == null)
			{
				nonEventThemeList = new List<int>();
				foreach (var bgItem in BGItemDatas)
				{
					if (bgItem.Theme == 0)
						continue;

					if (!nonEventThemeList.Contains(bgItem.Theme))
						nonEventThemeList.Add(bgItem.Theme);
				}
			}
			return nonEventThemeList;
		}
	}

	public bool IsNeedBuyBG(int id)
	{
		return BGItemDataDict[id].BGPrice > 0;
	}

	public object GetLevelBGData(int level)
	{
		var nextData = BGItemDatas.FirstOrDefault(d=>d.BGUnlockLevel>level&&d.BGPrice<=0);
		var lastData = BGItemDatas.LastOrDefault(d => d.BGUnlockLevel<=level && d.BGPrice <= 0);

		if (lastData == null || nextData == null) return null;
		return new
		{
			lastUnLockLevel = lastData == null ? 0 : lastData.BGUnlockLevel,
			nextUnLockLevel = nextData == null ? int.MaxValue : nextData.BGUnlockLevel,
			unlockBGID = nextData.ID,
		};
	}

	public (int ,int) GetLevelInterval(int themeID)
	{
		var list= BGThemeDict[themeID];
		return (list.First().BGUnlockLevel,list.Last().BGUnlockLevel);
	}
}
