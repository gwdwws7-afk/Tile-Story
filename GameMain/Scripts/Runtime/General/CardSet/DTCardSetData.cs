using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySelf.Model;
using Newtonsoft.Json;
using UnityEngine;

public class DTCardSetData
{
    public List<CardSet> CardSets;

    public List<CardSet> GetCurrentCardSetsByActivityID(int activityID)
    {
        List<CardSet> result = new List<CardSet>();
        foreach (CardSet cardSet in CardSets)
        {
            if (cardSet.ActivityID == activityID && cardSet.CardSetID != 0)
            {
                result.Add(cardSet);
            }
        }

        return result;
    }

    public (List<TotalItemData>, List<int>) GetCurrentFinalRewardByActivityID(int activityID)
    {
        var cardSet = CardSets.Find(cardSet => cardSet.ActivityID == activityID && cardSet.CardSetID == 0);
        return cardSet == null
            ? (new List<TotalItemData>(), new List<int>())
            : (cardSet.RewardTypeList, cardSet.RewardNumList);
    }
}

public class CardSet
{
    public int ActivityID;
    public int CardSetID;
    public string Reward;
    public string RewardNum;
    
    private List<TotalItemData> _rewardTypeList;
    private List<int> _rewardNumList;
    
    /// <summary>
    /// 奖励类型
    /// </summary>
    public List<TotalItemData> RewardTypeList
    {
        get
        {
            if (_rewardTypeList == null)
            {
                _rewardTypeList = new List<TotalItemData>();
                string[] splits = Reward.Split(',');
                foreach (var s in splits)
                {
                    if (int.TryParse(s, out int type))
                        _rewardTypeList.Add(TotalItemData.FromInt(type));
                }
            }

            return _rewardTypeList;
        }
    }

    /// <summary>
    /// 奖励数量
    /// </summary>
    public List<int> RewardNumList
    {
        get
        {
            if (_rewardNumList == null)
            {
                _rewardNumList = new List<int>();
                string[] splits = RewardNum.Split(',');
                foreach (var s in splits)
                {
                    if (int.TryParse(s, out int num))
                        _rewardNumList.Add(num);
                }
            }

            return _rewardNumList;
        }
    }

    
    private Dictionary<int, CardInfo> _cardDict;
    public Dictionary<int, CardInfo> CardDict
    {
        get
        {
            if (_cardDict == null)
            {
                _cardDict = new Dictionary<int, CardInfo>();
                List<CardInfo> cardList = GameManager.DataTable.GetDataTable<DTCardInfoData>().Data
                    .GetCardInfosByCardSetID(CardSetID);
                foreach (var card in cardList)
                {
                    if (!_cardDict.TryAdd(card.CardID, card))
                        _cardDict[card.CardID] = card;
                }
            }

            return _cardDict;
        }
    }
}
