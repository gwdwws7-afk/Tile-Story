using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 商品ID数据表
/// </summary>
[Serializable]
public class DTProductID
{
    public List<ProductIDData> ProductIDDatas;

    public string GetProductID(ProductNameType productType)
    {
        string productName = productType.ToString();
        for (int i = 0; i < ProductIDDatas.Count; i++)
        {
            if (ProductIDDatas[i].ProductName == productName) 
            {
                return ProductIDDatas[i].ProductID;
            }
        }
        return null;
    }
    
    public string GetProductID(int ID)
    {
        for (int i = 0; i < ProductIDDatas.Count; i++)
        {
            if (ProductIDDatas[i].ID == ID)
            {
                return ProductIDDatas[i].ProductID;
            }
        }
        return null;
    }

    public string GetProductName(string productID)
    {
        for (int i = 0; i < ProductIDDatas.Count; i++)
        {
            if (ProductIDDatas[i].ProductID == productID)
            {
                return ProductIDDatas[i].ProductName;
            }
        }
        return null;
    }
    
    public string GetProductNameById(int id)
    {
        if (id <= 0) return String.Empty;
        for (int i = 0; i < ProductIDDatas.Count; i++)
        {
            if (ProductIDDatas[i].ID == id)
            {
                return ProductIDDatas[i].ProductName;
            }
        }
        return null;
    }

    public UnityEngine.Purchasing.ProductType GetProductType(string productID)
    {
        if (ProductIDDatas != null)
        {
            for (int i = 0; i < ProductIDDatas.Count; i++)
            {
                if (ProductIDDatas[i].ProductID == productID)
                {
                    return ProductIDDatas[i].ProductType;
                }
            }
        }

        return UnityEngine.Purchasing.ProductType.Consumable;
    }
    
    public double GetProductPriceByCode(string productID,string code)
    {
        foreach (var product in ProductIDDatas)
        {
            if (product.ProductID == productID)
            {
                return product.GetPriceByCode(code);
            }
        }
        return 0;
    }
}
