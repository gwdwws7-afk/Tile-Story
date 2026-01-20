using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DTCardPackData
{
    public List<CardPack> CardPacks;

    /// <summary>
    /// 通过卡包类型获取所有的抽卡可能性
    /// </summary>
    /// <param name="cardPackageID"></param>
    /// <returns></returns>
    private List<CardPack> GetCardPacksByPackageID(int cardPackageID)
    {
        // return CardPacks.Where(pack => pack.CardPackageID == cardPackageID).ToList();
        
        // 使用for循环替代LINQ，减少内存分配
        var result = new List<CardPack>();
        for (int i = 0; i < CardPacks.Count; i++)
        {
            var pack = CardPacks[i];
            if (pack.CardPackageID == cardPackageID)
            {
                result.Add(pack);
            }
        }

        return result;
    }
     
    //获取卡包抽到的卡牌星级列表
    public int[] RandomGetCardStarListByPackageID(int cardPackageID)
    {
        List<CardPack> packs = GetCardPacksByPackageID(cardPackageID);
        int packCount = packs.Count;
        if (packCount == 0) return Array.Empty<int>();
        
        // float totalWeight = packs.Sum(pack => pack.StarListWeight);
        float totalWeight = 1f;
        
        float randomPoint = Random.Range(0, totalWeight);
        float accumulatedWeight = 0;
        for (int i = 0; i < packCount; i++)
        {
            var pack = packs[i];
            accumulatedWeight += pack.CardStarArrayWeight;
            if (randomPoint <= accumulatedWeight)
            {
                Debug.Log($"开{cardPackageID}级卡包");
                return pack.CardStarArray;
            }
        }

        return packs[packCount - 1].CardStarArray;
    }
}

public class CardPack
{
    public int CardPackageID;
    public float CardStarArrayWeight;
    public int[] CardStarArray;
    
    // public float StarListWeight
    // {
    //     get
    //     {
    //         if (_starListWeight == 0)
    //         {
    //             string[] numberStrings = StarWeightSequence.Split(',', StringSplitOptions.RemoveEmptyEntries);
    //             for (int i = 0; i < numberStrings.Length; i++)
    //             {
    //                 numberStrings[i] = numberStrings[i].Trim();
    //             }
    //             _starListWeight = 1.0f;
    //             foreach (string numStr in numberStrings)
    //             {
    //                 if (float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
    //                     _starListWeight *= number;
    //             }
    //         }
    //
    //         return _starListWeight;
    //     }
    // }
}
