using System;
using UnityEngine.Purchasing;

/// <summary>
/// 商品ID数据
/// </summary>
[Serializable]
public class ProductIDData
{
    public int ID;
    public string ProductName;
    public string ProductID;
    public ProductType ProductType;
    public double Price;//价格 美元
    
    public double GetPriceByCode(string code)
    {
        return Price;
    }
}
