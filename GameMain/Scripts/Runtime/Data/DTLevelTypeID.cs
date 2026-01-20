using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DTLevelTypeID
{
	public List<LevelType> LevelTypes;
	public List<LevelType> LevelTypes2;

	public LevelType GetLevelTypeData(int type)
	{
		bool isUseMultiple = GameManager.Firebase.GetBool(Constant.RemoteConfig.Level_Win_Rewards_RV_Multiple, false);
		List<LevelType> targetTypes = LevelTypes;
		if (isUseMultiple)
			targetTypes = LevelTypes2;

		foreach (var obj in targetTypes)
		{
			if (obj.ModeID == type)
			{
				return obj;
			}
		}

		foreach (var obj in targetTypes)
		{
			if (obj.ModeID == 1)
			{
				return obj;
			}
		}

		return null;
	}
}
