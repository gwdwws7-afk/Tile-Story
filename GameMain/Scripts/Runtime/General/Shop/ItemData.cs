using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 物品数据类
/// </summary>
public class ItemData
{
    public TotalItemData type;
    public int num;

    public ItemData(TotalItemData type, int num)
    {
        this.type = type;
        this.num = num;
    }
}
