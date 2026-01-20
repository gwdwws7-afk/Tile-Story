using System;
using System.Collections.Generic;

/// <summary>
/// 成就目标数据表
/// </summary>
[Serializable]
public class DTAllTimeObjective
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
