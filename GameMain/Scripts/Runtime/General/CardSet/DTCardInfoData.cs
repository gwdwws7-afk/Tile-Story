using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySelf.Model;
using UnityEngine;

public class DTCardInfoData
{
    public List<CardInfo> CardInfos;

    public List<CardInfo> GetCardInfosByCardSetID(int cardSetID)
    {
        List<CardInfo> result = new List<CardInfo>();
        foreach (CardInfo cardInfo in CardInfos)
        {
            if (cardInfo.CardSetID == cardSetID)
            {
                result.Add(cardInfo);
            }
        }

        return result;
    }

    public List<CardInfo> GetCardInfosByCardStar(int cardStar)
    {
        List<CardInfo> result = new List<CardInfo>();
        foreach (CardInfo cardInfo in CardInfos)
        {
            if (cardInfo.CardStar == cardStar)
            {
                result.Add(cardInfo);
            }
        }

        return result;
    }
}

[Serializable]
public class CardInfo
{
    public int CardID;
    public int CardSetID;
    public int CardStar;
    public float CardWeight;
    public float CardWeightIncrease;
    public float CardWeightReduce;

    public bool isNewCollect;
}
