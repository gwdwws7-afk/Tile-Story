using MySelf;
using System;
using System.Collections.Generic;

[Serializable]
public class DecorateArea
{
    public int ID;
    public string Name;
    public string RewardString;    //1:300,2:1,3:1,5:1,6:1,4:1

    private List<Reward> reward;
    public List<Reward> Reward
    {
        get
        {
            if (reward != null && reward.Count > 0)
                return reward;
            reward = new List<Reward>();

            string[] splitString = RewardString.Split(',');
            for (int i = 0; i < splitString.Length; ++i)
            {
                Reward newReward = new Reward(splitString[i]);
                reward.Add(newReward);
            }
            return reward;
        }
    }
}
