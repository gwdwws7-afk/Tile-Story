using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class DTHelp 
{
	public List<HelpData> HelpDatas;

	public Dictionary<int, HelpData> GetDictByBuildSchedule(int chapterId,int buildSchedule)
	{
		Dictionary<int, HelpData> dict = new Dictionary<int, HelpData>();
		int index = 0;
		foreach (var data in HelpDatas)
		{
			if (data.Chapter == chapterId && data.BuildSchedule == buildSchedule)
			{
				index++;
				dict.Add(index,data);
			}
		}
		return dict;
	}

	public bool IsShowStory(int chapterId,int buildSchedule)
	{
		foreach (var data in HelpDatas)
		{
			if (data.Chapter == chapterId && data.BuildSchedule == buildSchedule) return true;
		}

		return false;
	}
}
