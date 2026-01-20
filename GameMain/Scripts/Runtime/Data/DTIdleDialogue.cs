using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class DTIdleDialogue 
{
	public List<IdleDialogueData> IdleDialogueData;

	public List<IdleDialogueData> GetAllAvailableIdleDialogueData(int areaID, int progress, int characterIndex)
	{
		List<IdleDialogueData> returnList = new List<IdleDialogueData>();

		foreach (IdleDialogueData data in IdleDialogueData)
		{
			if (data.Chapter == areaID &&
				data.BuildScheduleStart <= progress && data.BuildScheduleEnd >= progress &&
				data.Role == characterIndex)
			{
				returnList.Add(data);
			}
		}
		return returnList;
	}

	public IdleDialogueData GetDefaultDialogueData()
    {
		return IdleDialogueData[0];
	}
}
