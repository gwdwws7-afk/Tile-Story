using System;

/// <summary>
/// 成就目标数据类
/// </summary>
[Serializable]
public class ObjectiveData
{
    public int ID;
    public ObjectiveType Type;
    public int TargetNum;
    public TotalItemType RewardType;
    public int RewardNum;
    public int PreObjective;
    public int TypeId;
}
