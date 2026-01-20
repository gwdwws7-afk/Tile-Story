using System;
using System.Collections.Generic;

/// <summary>
/// 每日目标数据表
/// </summary>
[Serializable]
public class DTDailyObjective
{
    public List<ObjectiveData> ObjectiveDatas;

    public ObjectiveData GetData(int id)
    {
        for (int i = 0; i < ObjectiveDatas.Count; i++)
        {
            if (ObjectiveDatas[i].ID == id)
                return ObjectiveDatas[i];
        }

        return null;
    }
}
