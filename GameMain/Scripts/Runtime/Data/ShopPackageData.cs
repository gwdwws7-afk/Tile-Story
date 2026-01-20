using System;
using System.Collections.Generic;

/// <summary>
/// 商品礼包数据类
/// </summary>
[Serializable]
public class ShopPackageData
{
    /// <summary>
    /// 编号
    /// </summary>
    public int ID;
    public ProductNameType Type;
    public string ProductNameTerm;
    public string Items;
    public string Nums;
    public ProductPosType ShowType;
    public string BarName;
    public int Sort;
    public string SideMarkTerm;

    List<ItemData> itemDatas =null;
    public List<ItemData> GetItemDatas()
    {
        if (itemDatas == null)
        {
            string[] totalId = Items.Split(new char[1] { ',' });
            string[] totalNums = Nums.Split(new char[1] { ',' });
            if (totalId.Length != totalNums.Length)
            {
                Log.Error("Load {0} False,Split Wrong", Type);
                return null;
            }
            itemDatas = new List<ItemData>(totalId.Length);
            for (int i = 0; i < totalId.Length; i++)
            {
                int type = int.Parse(totalId[i]);
                int num = int.Parse(totalNums[i]);
                itemDatas.Add(new ItemData(TotalItemData.FromInt(type), num));
            }
        }
        return itemDatas;
    }
}