using System;
using System.Collections.Generic;

/// <summary>
/// 商品礼包的数据
/// </summary>
[Serializable]
public class DTShopPackageData
{
    public List<ShopPackageData> ShopPackageDatas;

    /// <summary>
    /// 获取shopmenu展示类
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<ShopPackageData> GetShopPackageDatas(int type)
    {
        List<ShopPackageData> list = new List<ShopPackageData>();
        foreach (var child in ShopPackageDatas)
        {
            if (child.ShowType> ProductPosType.None && child.ShowType <= (ProductPosType)type)
            {
                list.Add(child);
            }
        }
        return list;
    }

    /// <summary>
    /// 获取礼包数据
    /// </summary>
    public ShopPackageData GetShopPackageData(ProductNameType packageType)
    {
        foreach (var child in ShopPackageDatas)
        {
            if (child.Type == packageType)
            {
                return child;
            }
        }
        return null;
    }

    public List<ItemData> GetShopPackageItemDatas(ProductNameType packageType)
    {
        var data = GetShopPackageData(packageType);
        if (data != null) return data.GetItemDatas();
        return null;
    }
}

