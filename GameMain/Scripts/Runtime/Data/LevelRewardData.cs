using System;
using System.Collections.Generic;

[Serializable]
public class LevelRewardData
{
    public int ID;// 编号
    public int RewardGetLevel;//奖励获取的关卡
    public string RewardIDs;//奖励编号
    public string RewardNums;//奖励数量
    public string RewardTypes;//奖励类型

    private List<ItemData> m_ItemDatas = null;

    public List<ItemData> GetRewardDatas()
    {
        if (m_ItemDatas == null)
        {
            m_ItemDatas = new List<ItemData>();
            string[] split_id = RewardIDs.Split(new char[1] { ',' });
            string[] split_Num = RewardNums.Split(new char[1] { ',' });
            string[] split_Type = RewardTypes.Split(new char[1] { ',' });

            for (int i = 0; i < split_id.Length; i++)
            {
                int id = int.Parse(split_id[i]);
                int num = int.Parse(split_Num[i]);
                int type = int.Parse(split_Type[i]);

                if (type == 1)
                    m_ItemDatas.Add(new ItemData(TotalItemData.FromInt(id), num));
                else if (type == 2)
                    m_ItemDatas.Add(new ItemData(new TotalItemData(id, 0, (int)TotalItemType.Item_BgID), num));
            }
        }

        return m_ItemDatas;
    }
}
