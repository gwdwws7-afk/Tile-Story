using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DTLevelReward
{
    public List<LevelRewardData> LevelRewardDatas;

    public LevelRewardData GetNextUnlockLevelReward(int curLevel)
    {
        for (int i = 0; i < LevelRewardDatas.Count; i++)
        {
            if (curLevel < LevelRewardDatas[i].RewardGetLevel)
                return LevelRewardDatas[i];
        }

        return null;
    }

    public LevelRewardData GetLevelReward(int id)
    {
        for (int i = 0; i < LevelRewardDatas.Count; i++)
        {
            if (LevelRewardDatas[i].ID == id) 
                return LevelRewardDatas[i];
        }

        return null;
    }

    public int GetLastUnlockLevel(int curLevel)
    {
        int result = 1;
        for (int i = 0; i < LevelRewardDatas.Count; i++)
        {
            if (curLevel >= LevelRewardDatas[i].RewardGetLevel)
                result = LevelRewardDatas[i].RewardGetLevel;
            else
                break;
        }

        return result;
    }
}
