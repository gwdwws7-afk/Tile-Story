using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Firebase.Analytics;
using Newtonsoft.Json;
using UnityEngine;
using MySelf.Model;
using Random = UnityEngine.Random;

namespace MySelf.Model
{
    #region HashSetConverter DictionaryHashSetConverter
    // 1. 简化转换器实现（避免泛型）
    public class DictionaryHashSetConverter : JsonConverter
    {
        // 明确指定只处理 Dictionary<int, HashSet<int>>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<int, HashSet<int>>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            // 反序列化为中间类型
            var temp = serializer.Deserialize<Dictionary<int, List<int>>>(reader);
            var result = new Dictionary<int, HashSet<int>>();

            if(temp!=null)
                foreach (var kvp in temp)
                {
                    result[kvp.Key] = new HashSet<int>(kvp.Value);
                }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // 转换为可序列化类型
            var dict = (Dictionary<int, HashSet<int>>)value;
            var temp = new Dictionary<int, List<int>>();

            if (dict != null)
                foreach (var kvp in dict)
                {
                    temp[kvp.Key] = new List<int>(kvp.Value);
                }

            serializer.Serialize(writer, temp);
        }
    }

    // 2. 专门针对 int 的 HashSet 转换器
    public class IntHashSetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HashSet<int>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var list = serializer.Deserialize<List<int>>(reader);
            
            return list==null?new HashSet<int>():new HashSet<int>(list);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var hashSet = (HashSet<int>)value;
            if (hashSet != null)
            {
                var list = new List<int>(hashSet.Count);
                foreach (int item in hashSet)
                {
                    list.Add(item);
                }

                serializer.Serialize(writer, list);
            }
        }
    }
    #endregion

    public class CardData
    {
        public int CardActivityID;
        public bool IsShowedPreviewPanel;
        public bool IsShowedStartPanel;
        public bool IsShowedCountdownPanel;
        public bool IsShowedEndPanel;
        public bool IsShowedGuide;
        public int CardExtraStarNum;
        public DateTime CoinToPackTime;

        [JsonConverter(typeof(DictionaryHashSetConverter))]
        public Dictionary<int, HashSet<int>> CardNewDict = new Dictionary<int, HashSet<int>>();

        [JsonConverter(typeof(DictionaryHashSetConverter))]
        public Dictionary<int, HashSet<int>> CardCollectDict = new Dictionary<int, HashSet<int>>();

        [JsonConverter(typeof(IntHashSetConverter))]
        public HashSet<int> CardCompletedSets = new HashSet<int>();

        public bool IsCompletedActivity;
    }

    public class CardModel : BaseModelService<CardModel, CardData>
    {
        #region Service

        public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
        {
            // return new Dictionary<string, object>
            // {
            //     { "CardModel.activityID", Data.activityID },
            //     { "CardModel.endTime", (Data.endTime - DateTime.MinValue).TotalMilliseconds },
            //     { "CardModel.showedPreviewPanel", Data.showedPreviewPanel },
            //     { "CardModel.showedStartPanel", Data.showedStartPanel },
            //     { "CardModel.showedCountdownPanel", Data.showedCountdownPanel },
            //     { "CardModel.showedEndPanel", Data.showedEndPanel },
            //     { "CardModel.extraStarNum", Data.extraStarNum },
            //     {
            //         "CardModel.newCardDict",
            //         JsonConvert.SerializeObject(Data.newCardDict, Formatting.None, new DictionaryHashSetConverter())
            //     },
            //     {
            //         "CardModel.collectCardDict",
            //         JsonConvert.SerializeObject(Data.collectCardDict, Formatting.None, new DictionaryHashSetConverter())
            //     },
            //     {
            //         "CardModel.completedCardSets",
            //         JsonConvert.SerializeObject(Data.completedCardSets, Formatting.None, new IntHashSetConverter())
            //     },
            //     { "CardModel.completedAll", Data.completedAll },
            // };
            return new Dictionary<string, object>
            {
                { "CardModelData", JsonConvert.SerializeObject(Data, Formatting.None) }
            };
        }

        public override void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary)
        {
            if (dictionary != null)
            {
                foreach (var item in dictionary)
                {
                    switch (item.Key)
                    {
                        case "CardModelData":
                            Data = JsonConvert.DeserializeObject<CardData>(Convert.ToString(item.Value));
                            break;
                    }
                }

                // foreach (var item in dictionary)
                // {
                //     switch (item.Key)
                //     {
                //         case "CardModel.activityID":
                //             Data.activityID = Convert.ToInt32(item.Value);
                //             break;
                //         case "CardModel.endTime":
                //             Data.endTime = DateTime.MinValue.AddMilliseconds(Convert.ToInt64(item.Value));
                //             break;
                //         case "CardModel.showedPreviewPanel":
                //             Data.showedPreviewPanel = Convert.ToBoolean(item.Value);
                //             break;
                //         case "CardModel.showedStartPanel":
                //             Data.showedStartPanel = Convert.ToBoolean(item.Value);
                //             break;
                //         case "CardModel.showedCountdownPanel":
                //             Data.showedCountdownPanel = Convert.ToBoolean(item.Value);
                //             break;
                //         case "CardModel.showedEndPanel":
                //             Data.showedEndPanel = Convert.ToBoolean(item.Value);
                //             break;
                //         case "CardModel.extraStarNum":
                //             Data.extraStarNum = Convert.ToInt32(item.Value);
                //             break;
                //         case "CardModel.newCardDict":
                //             Data.newCardDict =
                //                 JsonConvert.DeserializeObject<Dictionary<int, HashSet<int>>>(
                //                     Convert.ToString(item.Value), new DictionaryHashSetConverter());
                //             break;
                //         case "CardModel.collectCardDict":
                //             Data.collectCardDict =
                //                 JsonConvert.DeserializeObject<Dictionary<int, HashSet<int>>>(
                //                     Convert.ToString(item.Value), new DictionaryHashSetConverter());
                //             break;
                //         case "CardModel.completedCardSets":
                //             Data.completedCardSets =
                //                 JsonConvert.DeserializeObject<HashSet<int>>(Convert.ToString(item.Value),
                //                     new IntHashSetConverter());
                //             break;
                //         case "CardModel.completedAll":
                //             Data.completedAll = Convert.ToBoolean(item.Value);
                //             break;
                //     }
                // }

                SaveToLocal();
            }
        }

        #endregion
        
        public bool IsHaveCardAsset => AddressableUtils.IsHaveAsset($"CardSetMainMenu{CardActivityID}");
        
        public bool IsInCardActivity => IsHaveCardAsset &&
                                        GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockCardSetLevel &&
                                        ShowedStartPanel && 
                                        !ShowedEndPanel &&
                                        DateTime.Now < CardEndTime;

        //用于套组领奖界面
        public int CurrentCardSet { get; set; }

        //用于假入口伸出横条
        public int NewCompletedCardSet { get; set; }
        
        //用于播放音效
        public string BgmName
        {
            get
            {
                switch (Data.CardActivityID)
                {
                    case 200001:
                        return "Card_Collection_Summer_BGM";
                    case 200002:
                        return "Card_Collection_Animal_BGM";
                    case 200003:
                        return "HALLOWEEN_MAP";
                    case 200004:
                        return "Card_Collection_Winter_BGM";
                    case 200005:
                        return "Card_Collection_Spring_BGM";
                    default:
                        return "";
                }
            }
        }

        public int CardActivityID
        {
            get
            {
                if (Data.CardActivityID <= 0)
                {
                    Data.CardActivityID = GameManager.DataTable.GetDataTable<DTCardScheduleData>().Data
                        .GetNowActiveActivityID();
                    if (Data.CardActivityID > 0) SaveToLocal();
                }

                return Data.CardActivityID;
            }
        }

        private DateTime _cardStartTime = DateTime.MinValue;
        public DateTime CardStartTime
        {
            get
            {
                if (_cardStartTime == DateTime.MinValue)
                {
                    _cardStartTime = GameManager.DataTable.GetDataTable<DTCardScheduleData>().Data
                        .GetActivityStartTimeByID(CardActivityID);
                }

                return _cardStartTime;
            }
        }

        private DateTime _cardEndTime = DateTime.MinValue;
        public DateTime CardEndTime
        {
            get
            {
                if (_cardEndTime == DateTime.MinValue)
                {
                    _cardEndTime = GameManager.DataTable.GetDataTable<DTCardScheduleData>().Data
                        .GetActivityEndTimeByID(CardActivityID);
                }

                return _cardEndTime;
            }
        }

        #region panel

        public bool ShowedPreviewPanel
        {
            get => Data.IsShowedPreviewPanel;
            set
            {
                if (Data.IsShowedPreviewPanel != value)
                {
                    Data.IsShowedPreviewPanel = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedStartPanel
        {
            get => Data.IsShowedStartPanel;
            set
            {
                if (Data.IsShowedStartPanel != value)
                {
                    Data.IsShowedStartPanel = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedCountdownPanel
        {
            get => Data.IsShowedCountdownPanel;
            set
            {
                if (Data.IsShowedCountdownPanel != value)
                {
                    Data.IsShowedCountdownPanel = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedEndPanel
        {
            get => Data.IsShowedEndPanel;
            set
            {
                if (Data.IsShowedEndPanel != value)
                {
                    Data.IsShowedEndPanel = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedGuide
        {
            get => Data.IsShowedGuide;
            set
            {
                if (Data.IsShowedGuide != value)
                {
                    Data.IsShowedGuide = value;
                    SaveToLocal();
                }
            }
        }
        
        #endregion

        public int ExtraStarNum
        {
            get => Data.CardExtraStarNum;
            set
            {
                if (Data.CardExtraStarNum != value)
                {
                    Data.CardExtraStarNum = value;
                    SaveToLocal();
                }
            }
        }

        //用金币兑换卡包
        public DateTime CoinToPackTime
        {
            set
            {
                if (Data.CoinToPackTime != value)
                {
                    Data.CoinToPackTime = value;
                    SaveToLocal();
                }
            }
        }

        public bool CanUseCoinForPack
        {
            get
            {
                DateTime time = Data.CoinToPackTime.ToLocalTime();
                return time < DateTime.Today;
            }
        }

        //set走不进来，改值后需要SaveToLocal
        public Dictionary<int, HashSet<int>> NewCardDict
        {
            get => Data.CardNewDict ?? (NewCardDict = new Dictionary<int, HashSet<int>>());
            set
            {
                if (Data.CardNewDict != value)
                {
                    Data.CardNewDict = value;
                    SaveToLocal();
                }
            }
        }

        public Dictionary<int, HashSet<int>> CollectCardDict
        {
            get => Data.CardCollectDict ?? (CollectCardDict = new Dictionary<int, HashSet<int>>());
            set
            {
                if (Data.CardCollectDict != value)
                {
                    Data.CardCollectDict = value;
                    SaveToLocal();
                }
            }
        }

        public int CardSetCollectNum(int cardSetID)
        {
            CollectCardDict.TryGetValue(cardSetID, out var cards);
            return cards?.Count ?? 0;
        }

        public int TotalCollectNum
        {
            get
            {
                int count = 0;
                foreach (var kvp in CollectCardDict)
                {
                    count += kvp.Value.Count;
                }
                return count;
            }
        }

        public int TotalCardNum => 135;
        // {
        //     get
        //     {
        //         int count = 0;
        //         foreach (var kvp in CardSetDict)
        //         {
        //             count += kvp.Value.CardDict.Count;
        //         }
        //         return count;
        //     }
        // }

        public HashSet<int> CompletedCardSets
        {
            get => Data.CardCompletedSets ?? (CompletedCardSets = new HashSet<int>());
            set
            {
                if (Data.CardCompletedSets != value)
                {
                    Data.CardCompletedSets = value;
                    SaveToLocal();
                }
            }
        }

        public bool CompletedAll
        {
            get => Data.IsCompletedActivity;
            set
            {
                if (Data.IsCompletedActivity != value)
                {
                    Data.IsCompletedActivity = value;
                    SaveToLocal();
                }
            }
        }

        public void ResetData()
        {
            Data.CardActivityID = -1;
            _cardStartTime = DateTime.MinValue;
            _cardEndTime = DateTime.MinValue;
            Data.IsShowedPreviewPanel = false;
            Data.IsShowedStartPanel = false;
            Data.IsShowedCountdownPanel = false;
            Data.IsShowedEndPanel = false;
            Data.IsShowedGuide = false;
            Data.CardExtraStarNum = 0;
            Data.CoinToPackTime = DateTime.MinValue;
            Data.CardNewDict = null;
            Data.CardCollectDict = null;
            Data.CardCompletedSets = null;
            Data.IsCompletedActivity = false;
            SaveToLocal();

            _cardSetDict = null;
        }

        //卡组字典
        private Dictionary<int, CardSet> _cardSetDict;

        public Dictionary<int, CardSet> CardSetDict
        {
            get
            {
                if (_cardSetDict == null)
                {
                    _cardSetDict = new Dictionary<int, CardSet>();
                    List<CardSet> cardSetList = GameManager.DataTable.GetDataTable<DTCardSetData>().Data
                        .GetCurrentCardSetsByActivityID(CardActivityID);
                    foreach (var cardSet in cardSetList)
                    {
                        if (!_cardSetDict.TryAdd(cardSet.CardSetID, cardSet))
                            _cardSetDict[cardSet.CardSetID] = cardSet;
                    }
                }

                return _cardSetDict;
            }
        }

        //星级卡牌字典
        private Dictionary<int, List<CardInfo>> _starCardDict;

        public Dictionary<int, List<CardInfo>> StarCardDict
        {
            get
            {
                if (_starCardDict == null)
                {
                    DTCardInfoData cardInfoData = GameManager.DataTable.GetDataTable<DTCardInfoData>().Data;
                    _starCardDict = new Dictionary<int, List<CardInfo>>
                    {
                        { 1, cardInfoData.GetCardInfosByCardStar(1) },
                        { 2, cardInfoData.GetCardInfosByCardStar(2) },
                        { 3, cardInfoData.GetCardInfosByCardStar(3) },
                        { 4, cardInfoData.GetCardInfosByCardStar(4) },
                        { 5, cardInfoData.GetCardInfosByCardStar(5) }
                    };
                }

                return _starCardDict;
            }
        }
    }
}

public static class CardUtil
{
    public static int CardSetCollectNum(this CardSet cardSet)
    {
        return CardModel.Instance.CardSetCollectNum(cardSet.CardSetID);
    }

    public static bool IsCollected(this CardInfo cardInfo)
    {
        CardModel.Instance.CollectCardDict.TryGetValue(cardInfo.CardSetID, out var cards);
        return cards != null && cards.Contains(cardInfo.CardID);
    }

    public static bool IsNew(this CardInfo cardInfo)
    {
        CardModel.Instance.NewCardDict.TryGetValue(cardInfo.CardSetID, out var cards);
        return cards != null && cards.Contains(cardInfo.CardID);
    }

    public static void RemoveNew(this CardInfo cardInfo)
    {
        CardModel.Instance.NewCardDict.TryGetValue(cardInfo.CardSetID, out var cards);
        cards?.Remove(cardInfo.CardID);
        CardModel.Instance.SaveToLocal();
    }

    #region 开卡包

    public static List<CardInfo> OpenCardPack(int cardPackageID)
    {
        int[] cardStarList = GameManager.DataTable.GetDataTable<DTCardPackData>().Data
            .RandomGetCardStarListByPackageID(cardPackageID);

        // 预分配List容量，避免扩容开销
        List<CardInfo> cardInfos = new List<CardInfo>(cardStarList.Length);
        // 使用for循环替代LINQ的Select和ToList
        // 生成卡牌，确保不重复
        for (int i = 0; i < cardStarList.Length; i++)
        {
            cardInfos.Add(GetNonDuplicateCard(cardStarList[i], cardInfos));
        }

        if (cardPackageID == 5) cardInfos = CheckHighestPackContainsNewCard(cardInfos);
        cardInfos = CheckOnlyCompleteOneSet(cardInfos);
        // cardInfos = CheckNoRepeatNewCard(cardInfos);

        // 加星星（减少属性访问）
        int extraStarNum = 0;
        for (int i = 0; i < cardInfos.Count; i++)
        {
            var card = cardInfos[i];
            if (!card.isNewCollect)
                extraStarNum += card.CardStar;
        }

        CardModel.Instance.ExtraStarNum += extraStarNum;

        // 消耗卡包数量
        switch (cardPackageID)
        {
            case 1:
                GameManager.PlayerData.UseItem(TotalItemData.CardPack1, 1);
                break;
            case 2:
                GameManager.PlayerData.UseItem(TotalItemData.CardPack2, 1);
                break;
            case 3:
                GameManager.PlayerData.UseItem(TotalItemData.CardPack3, 1);
                break;
            case 4:
                GameManager.PlayerData.UseItem(TotalItemData.CardPack4, 1);
                break;
            case 5:
                GameManager.PlayerData.UseItem(TotalItemData.CardPack5, 1);
                break;
        }

        RecordCardPackageOpen(cardPackageID);

        return cardInfos;
    }

    /// <summary>
    /// 不重复开卡
    /// </summary>
    /// <param name="star"></param>
    /// <param name="currentCards"></param>
    /// <returns></returns>
    private static CardInfo GetNonDuplicateCard(int star, List<CardInfo> currentCards)
    {
        CardModel.Instance.StarCardDict.TryGetValue(star, out var cardInfos);
        if (cardInfos == null)
        {
            Debug.Log($"没有{star}星卡");
            return GameManager.DataTable.GetDataTable<DTCardInfoData>().Data.CardInfos.FirstOrDefault();
        }

        // 构建已开卡ID集合
        HashSet<int> openedIDs = new HashSet<int>();
        for (int i = 0; i < currentCards.Count; i++)
        {
            openedIDs.Add(currentCards[i].CardID);
        }

        CardInfo card;
        // 先尝试随机3次
        for (int attempt = 0; attempt < 3; attempt++)
        {
            card = RandomGetCardByStarType(star);
            if (!openedIDs.Contains(card.CardID))
            {
                SetNewCollectCardState(card);
                return card;
            }
        }

        // 随机失败，随机顺序查找对应星级的卡
        List<CardInfo> shuffledCards = new List<CardInfo>(cardInfos);
        ShuffleList(shuffledCards);

        for (int i = 0; i < shuffledCards.Count; i++)
        {
            card = shuffledCards[i];
            if (!openedIDs.Contains(card.CardID))
            {
                SetNewCollectCardState(card);
                return card;
            }
        }

        // 所有卡都重复了，返回随机卡
        Debug.Log($"{star}星卡里没有不重复的卡");
        card = RandomGetCardByStarType(star);
        SetNewCollectCardState(card);
        return card;
    }

    private static CardInfo RandomGetCardByStarType(int star)
    {
        CardModel.Instance.StarCardDict.TryGetValue(star, out var cardInfos);
        if (cardInfos != null)
        {
            // 用for循环替代LINQ
            List<float> weightList = new List<float>(cardInfos.Count);
            float totalWeight = 0f;
            for (int i = 0; i < cardInfos.Count; i++)
            {
                float weight = CardRealWeight(cardInfos[i]);
                weightList.Add(weight);
                totalWeight += weight;
            }

            float randomPoint = Random.Range(0, totalWeight);
            float accumulatedWeight = 0;
            for (int i = 0; i < weightList.Count; i++)
            {
                accumulatedWeight += weightList[i];
                if (randomPoint <= accumulatedWeight)
                {
                    Debug.Log($"抽{star}星卡，抽到{cardInfos[i].CardID}");
                    // SetNewCollectCardState(cardInfos[i]);
                    return cardInfos[i];
                }
            }

            // 保底返回最后一个
            var lastCard = cardInfos[^1];
            // SetNewCollectCardState(lastCard);
            return lastCard;
        }

        Debug.Log($"没有{star}星卡");
        return new CardInfo();
    }

    private static float CardRealWeight(CardInfo cardInfo)
    {
        int cardSetCollectNum = CardModel.Instance.CardSetCollectNum(cardInfo.CardSetID);

        if (cardSetCollectNum == CardModel.Instance.CardSetDict[cardInfo.CardSetID].CardDict.Count)
            return cardInfo.CardWeight - cardInfo.CardWeightReduce;
        if (cardSetCollectNum >= 3)
            return cardInfo.CardWeight + cardInfo.CardWeightIncrease;
        return cardInfo.CardWeight;
    }

    private static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    /// <summary>
    /// 设置刚抽到的卡的状态，并SaveToLocal
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private static void SetNewCollectCardState(CardInfo card)
    {
        card.isNewCollect = false;
        
        if (!CardModel.Instance.CollectCardDict.TryGetValue(card.CardSetID, out HashSet<int> collectSet))
        {
            collectSet = new HashSet<int>();
            CardModel.Instance.CollectCardDict[card.CardSetID] = collectSet;
        }

        //加拥有
        //未拥有，加new
        if (collectSet.Add(card.CardID))
        {
            if (!CardModel.Instance.NewCardDict.TryGetValue(card.CardSetID, out HashSet<int> newSet))
            {
                newSet = new HashSet<int>();
                CardModel.Instance.NewCardDict[card.CardSetID] = newSet;
            }

            if (newSet.Add(card.CardID))
            {
                //用于判断是否需要分解成星星
                card.isNewCollect = true;
            }

            CardModel.Instance.SaveToLocal();
        }
    }

    /// <summary>
    /// 保证五级卡包必开出至少一张新卡
    /// </summary>
    /// <param name="cardInfos"></param>
    /// <returns></returns>
    private static List<CardInfo> CheckHighestPackContainsNewCard(List<CardInfo> cardInfos)
    {
        Debug.Log("保证五级卡包必开出至少一张新卡");

        // 用for循环代替LINQ
        for (int i = 0; i < cardInfos.Count; i++)
        {
            if (cardInfos[i].isNewCollect)
            {
                Debug.Log("已有新卡");
                return cardInfos;
            }
        }

        if (CardModel.Instance.TotalCollectNum == CardModel.Instance.CardSetDict.Count * 9)
        {
            Debug.Log("本期已集齐");
            return cardInfos;
        }

        // 生成随机索引列表
        List<int> randomIndexes = new List<int>();
        for (int i = 0; i < cardInfos.Count; i++) randomIndexes.Add(i);
        ShuffleList(randomIndexes);
        for (int idx = 0; idx < randomIndexes.Count; idx++)
        {
            int i = randomIndexes[idx];
            int star = cardInfos[i].CardStar;

            CardModel.Instance.StarCardDict.TryGetValue(star, out var sameStarCards);
            if (sameStarCards == null) continue;

            // 随机顺序查找新卡
            List<CardInfo> randomCards = new List<CardInfo>(sameStarCards);
            ShuffleList(randomCards);

            for (int j = 0; j < randomCards.Count; j++)
            {
                if (!randomCards[j].IsCollected())
                {
                    Debug.Log($"把旧卡{cardInfos[i].CardID}换成新卡{randomCards[j].CardID}");
                    cardInfos[i] = randomCards[j];
                    SetNewCollectCardState(randomCards[j]);
                    return cardInfos;
                }
            }
        }

        // 如果同星级找不到新卡，尝试其他星级
        int[] allStars = { 1, 2, 3, 4, 5 };
        // 收集当前星级
        HashSet<int> currentStars = new HashSet<int>();
        for (int i = 0; i < cardInfos.Count; i++)
        {
            currentStars.Add(cardInfos[i].CardStar);
        }

        for (int star = 0; star < allStars.Length; star++)
        {
            int otherStar = allStars[star];
            if (currentStars.Contains(otherStar)) continue;

            CardModel.Instance.StarCardDict.TryGetValue(otherStar, out var otherStarCards);
            if (otherStarCards == null) continue;

            for (int idx = 0; idx < randomIndexes.Count; idx++)
            {
                int i = randomIndexes[idx];
                List<CardInfo> randomCards = new List<CardInfo>(otherStarCards);
                ShuffleList(randomCards);

                for (int j = 0; j < randomCards.Count; j++)
                {
                    if (!randomCards[j].IsCollected())
                    {
                        Debug.Log($"把旧卡{cardInfos[i].CardID}换成新卡{randomCards[j].CardID}");
                        cardInfos[i] = randomCards[j];
                        SetNewCollectCardState(randomCards[j]);
                        return cardInfos;
                    }
                }
            }
        }

        Debug.LogWarning("无法找到新卡替换");
        return cardInfos;


        bool isReplaced = false;
        var allStarTypes = new[] { 1, 2, 3, 4, 5 };
        HashSet<int> starTypeSet = new HashSet<int>();
        //不放回随机取元素，while最多循环6次
        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < cardInfos.Count; i++)
        {
            availableIndexes.Add(i);
            starTypeSet.Add(cardInfos[i].CardStar);
        }

        while (availableIndexes.Count > 0 && !isReplaced)
        {
            int randomIndexInAvail = Random.Range(0, availableIndexes.Count);
            int index = availableIndexes[randomIndexInAvail];
            availableIndexes.RemoveAt(randomIndexInAvail);

            foreach (var card in CardModel.Instance.StarCardDict[cardInfos[index].CardStar])
            {
                if (!card.IsCollected())
                {
                    Debug.Log($"把旧卡{cardInfos[index].CardID}换成新卡{card.CardID}");
                    cardInfos[index] = card;
                    SetNewCollectCardState(card);
                    isReplaced = true;
                    break;
                }
            }

            if (availableIndexes.Count == 0 && !isReplaced)
            {
                Debug.Log("！！循环完了但是没换成！！对应星级的卡都集齐了");
                foreach (var starType in allStarTypes)
                {
                    // 去掉LINQ
                    if (starTypeSet.Contains(starType)) continue;
                    Debug.Log($"再从{starType}星卡中找");
                    foreach (var card in CardModel.Instance.StarCardDict[starType])
                    {
                        if (!card.IsCollected())
                        {
                            Debug.Log($"把旧卡{cardInfos[index].CardID}换成新卡{card.CardID}");
                            cardInfos[index] = card;
                            SetNewCollectCardState(card);
                            isReplaced = true;
                            break;
                        }
                    }

                    if (isReplaced) break;
                }
            }
        }

        return cardInfos;
    }

    /// <summary>
    /// 保证单次开卡仅能集齐一个卡组
    /// </summary>
    /// <param name="cardInfos"></param>
    private static List<CardInfo> CheckOnlyCompleteOneSet(List<CardInfo> cardInfos)
    {
        Debug.Log("保证单次开卡仅能集齐一个卡组");
        bool completedOneSet = false;
        List<CardInfo> openedCards = new List<CardInfo>(cardInfos);
        
        for (int i = 0; i < cardInfos.Count; i++)
        {
            CardInfo card = cardInfos[i];
            //还未集齐一个卡组
            if (!completedOneSet)
            {
                //开到新卡且能集齐卡组，记录
                if (card.isNewCollect && CardModel.Instance.CollectCardDict[card.CardSetID].Count == 9)
                {
                    completedOneSet = true;
                    CardModel.Instance.NewCompletedCardSet = card.CardSetID;
                    RecordCardSetCollect(card.CardSetID);
                    RecordCollectFinished();
                    Debug.Log($"1.开到新卡{card.CardID}能集齐卡组{card.CardSetID}");
                }
            }
            //已经集齐一个卡组
            else
            {
                CardInfo temp = card;
                int count = 99;
                while (count > 0 &&
                       temp.CardSetID != CardModel.Instance.NewCompletedCardSet &&
                       temp.isNewCollect && CardModel.Instance.CollectCardDict[temp.CardSetID].Count == 9)
                {
                    count--;
                    Debug.Log($"2.开到新卡{temp.CardID}能集齐卡组{temp.CardSetID}");
                    //重选(移除状态，并SaveToLocal)
                    temp.isNewCollect = false;
                    if (CardModel.Instance.CollectCardDict.TryGetValue(temp.CardSetID, out var value1))
                        value1?.Remove(temp.CardID);
                    if (CardModel.Instance.NewCardDict.TryGetValue(temp.CardSetID, out var value2))
                        value2?.Remove(temp.CardID);
                    Debug.Log($"换掉重选");
                    temp = GetNonDuplicateCard(temp.CardStar, openedCards);
                    openedCards.Add(temp);
                    if (count == 0) Debug.LogError("！！死循环！！");
                }
                if (count < 99)
                {
                    CardModel.Instance.SaveToLocal();
                    cardInfos[i] = temp;
                    Debug.LogError($"为保证单次开卡仅能集齐一个卡组，把{card.CardID}换成{temp.CardID}");
                }
            }
        }

        return cardInfos;
    }

    /// <summary>
    /// 保证单次不会开出重复的新卡
    /// </summary>
    /// <returns></returns>
    private static List<CardInfo> CheckNoRepeatNewCard(List<CardInfo> cardInfos)
    {
        Debug.Log("保证单次不会开出重复的新卡");
        Dictionary<int, int> firstNewIndex = new Dictionary<int, int>();

        // 扫描记录首次出现的新卡位置
        for (int i = 0; i < cardInfos.Count; i++)
        {
            if (cardInfos[i].isNewCollect)
            {
                firstNewIndex.TryAdd(cardInfos[i].CardID, i);
            }
        }

        // 执行替换
        for (int i = 0; i < cardInfos.Count; i++)
        {
            if (firstNewIndex.TryGetValue(cardInfos[i].CardID, out int firstIndex) &&
                i != firstIndex) // 排除首张卡
            {
                // 换卡，换成不在此次抽出新卡之列的卡，且必须换已拥有的卡，不然可能会集齐新的卡组
                foreach (var card in CardModel.Instance.StarCardDict[cardInfos[i].CardStar])
                {
                    if (!firstNewIndex.ContainsKey(card.CardID) && card.IsCollected())
                    {
                        Debug.Log($"开到重复的新卡{cardInfos[i].CardID}，换成已拥有的卡{card.CardID}");
                        cardInfos[i] = card;
                        break;
                    }
                }
            }
        }

        return cardInfos;
    }

    #endregion

    #region 打点

    private static void RecordCardPackageOpen(int cardPackageID)
    {
        GameManager.Firebase.RecordMessageByEvent("Card_Package_Open", new Parameter("CardPackageID", cardPackageID));
    }

    private static void RecordCardSetCollect(int cardSetID)
    {
        GameManager.Firebase.RecordMessageByEvent("Card_Set_Collect", new Parameter("CardSetID", cardSetID));
    }

    private static void RecordCollectFinished()
    {
        if (CardModel.Instance.TotalCollectNum == CardModel.Instance.CardSetDict.Count * 9)
            GameManager.Firebase.RecordMessageByEvent("Card_Set_Collect_Finished");
    }

    public static void RecordUseStars(int boxID)
    {
        GameManager.Firebase.RecordMessageByEvent("Stars_Transfer_To_Box", new Parameter("BoxID", boxID));
    }

    public static void RecordUseCoin()
    {
        GameManager.Firebase.RecordMessageByEvent("Stars_Transfer_Coin_Buy_Card_Package");
    }

    public static void RecordFinishCards(int cardsNum)
    {
        GameManager.Firebase.RecordMessageByEvent("Card_Collect_Finished_Cards", new Parameter("Num", cardsNum));
    }

    public static void RecordFinishSets(int setsNum)
    {
        GameManager.Firebase.RecordMessageByEvent("Card_Collect_Finished_Sets", new Parameter("Num", setsNum));
    }

    #endregion

    /// <summary>
    /// 强制完成卡组
    /// </summary>
    /// <param name="cardSetID"></param>
    public static void CompleteCardSet(int cardSetID)
    {
        List<CardInfo> cardList = GameManager.DataTable.GetDataTable<DTCardInfoData>().Data
            .GetCardInfosByCardSetID(cardSetID);
        foreach (var cardInfo in cardList)
        {
            SetNewCollectCardState(cardInfo);
        }
    }
}
