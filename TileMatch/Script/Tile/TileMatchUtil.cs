using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MySelf.Model;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public static class TileMatchUtil
{
    public const int EachWidth = 134 / 2;
    public const int EachHeight = 136 / 2;

    public const float ChangeScale = 7 / 8f;

    public static float NewChangeScale = 1f;

    public static int CurTileImageIndex => GameManager.PlayerData == null ? 1 : GameManager.PlayerData.TileImageIndex;

    //private static Dictionary<int, AsyncOperationHandle> handleDict = new Dictionary<int, AsyncOperationHandle>();
    private static Dictionary<int, UnityEngine.U2D.SpriteAtlas> atlasDict = new Dictionary<int, SpriteAtlas>();

    private static Dictionary<int, Dictionary<int, Sprite>> allTileImageDict = new Dictionary<int, Dictionary<int, Sprite>>();
    public static Dictionary<int, Sprite> AllTileMatchSprites(int tileImageIndex = 1)
    {
        if (allTileImageDict.TryGetValue(tileImageIndex, out Dictionary<int, Sprite> dict))
        {
            return dict;
        }

        Dictionary<int, Sprite> spriteDict = new Dictionary<int, Sprite>();

        int tileAtlasId = tileImageIndex;

        if (atlasDict.TryGetValue(tileImageIndex, out UnityEngine.U2D.SpriteAtlas atlas))
        {

        }
        else
        {
            string spriteKey = $"TileIcons{tileAtlasId}";
            //AsyncOperationHandle async = new AsyncOperationHandle();
            //atlas = AddressableUtils.LoadAsset<UnityEngine.U2D.SpriteAtlas>(spriteKey, ref async);
            var async = Addressables.LoadAssetAsync<SpriteAtlas>(spriteKey);
            atlas = async.WaitForCompletion();
            //handleDict[tileImageIndex] = async;
        }

        int index = -3;
        while (true)
        {
            if (atlas == null) break;
            var sprite = atlas.GetSprite(index.ToString());
            if (sprite == null) break;

            spriteDict.Add(index, sprite);
            index += 1;
        }
        allTileImageDict[tileImageIndex] = spriteDict;
        return spriteDict;
    }

    private static int curTileMatchSpriteID = 0;
    private static Dictionary<int, Sprite> tileMatchSprites;
    public static Dictionary<int, Sprite> TileMatchSprites
    {
        get
        {
            if (tileMatchSprites == null || curTileMatchSpriteID != CurTileImageIndex)
            {
                curTileMatchSpriteID = CurTileImageIndex;
                tileMatchSprites = new Dictionary<int, Sprite>();
                foreach (var child in AllTileMatchSprites(CurTileImageIndex))
                {
                    if (child.Key > 1 && !IsNonRandomized(child.Key))
                        tileMatchSprites.Add(child.Key, child.Value);
                }
            }
            return tileMatchSprites;
        }
    }

    public static void ClearTileSprite()
    {
        if (curTileMatchSpriteID != CurTileImageIndex)
        {
            tileMatchSprites = null;
            curTileMatchSpriteID = 0;
        }

        allTileImageDict = allTileImageDict.Where(a => a.Key == CurTileImageIndex)
            .ToDictionary(a => a.Key, b => b.Value);
        for (int i = 0; i < atlasDict.Count; i++)
        {
            var key = atlasDict.ElementAt(i).Key;
            if (key != CurTileImageIndex)
            {
                var atlas = atlasDict.ElementAt(i).Value;
                atlasDict[key] = null;
                Addressables.Release(atlas);
            }
        }
        atlasDict = atlasDict.Where(a => a.Key == CurTileImageIndex)
            .ToDictionary(a => a.Key, b => b.Value);
        // for (int j = 0; j < handleDict.Count; j++)
        // {
        //     var key = handleDict.ElementAt(j).Key;
        //     if (key != CurTileImageIndex)
        //     {
        //         var async = handleDict.ElementAt(j).Value;
        //         Addressables.Release(async);
        //     }
        // }
        // handleDict = handleDict.Where(a => a.Key == CurTileImageIndex)
        //     .ToDictionary(a => a.Key, b => b.Value);
    }

    public static void ClearAllTileSprite()
    {
        tileMatchSprites = null;
        curTileMatchSpriteID = 0;

        allTileImageDict.Clear();

        foreach (var atlasPair in atlasDict)
        {
            Addressables.Release(atlasPair.Value);
        }
        atlasDict.Clear();

        // foreach (var handlePair in handleDict)
        // {
        //     Addressables.Release(handlePair.Value);
        // }
        // handleDict.Clear();
    }

    public static Sprite GetTileSprite(int tileID)
    {
        return AllTileMatchSprites(CurTileImageIndex)[tileID];
    }

    public static Sprite GetTileSprite(int tileImageIndex, int tileID)
    {
        return AllTileMatchSprites(tileImageIndex)[tileID];
    }

    public static List<int> GetLinkTileIndex(int mapIndex)
    {
        List<int> list = new List<int>(9);
        list.Add(mapIndex);
        list.Add(mapIndex - 1);
        list.Add(mapIndex + 1);
        list.Add(mapIndex + 15);
        list.Add(mapIndex - 15);
        list.Add(mapIndex + 16);
        list.Add(mapIndex - 16);
        list.Add(mapIndex + 17);
        list.Add(mapIndex - 17);
        return list;
    }

    public static Dictionary<int, List<int>> GetLinkTileIndexByLayer(int curLayer, int mapIndex, Dictionary<int, Dictionary<int, int>> mapDict)
    {
        Dictionary<int, List<int>> dict = new Dictionary<int, List<int>>();

        List<int> linkList = GetLinkTileIndex(mapIndex);
        foreach (var layer in mapDict)
        {
            if (layer.Key > curLayer)
            {
                dict.Add(layer.Key, linkList.Intersect(layer.Value.Keys).ToList());
            }
        }
        return dict;
    }

    public static (Dictionary<int, List<int>>, Dictionary<int, List<int>>) GetLinkMaskByLayer(int curLayer, int mapIndex, Dictionary<int, Dictionary<int, TileInfo>> mapDict)
    {
        (Dictionary<int, List<int>>, Dictionary<int, List<int>>) dict = (new Dictionary<int, List<int>>(), new Dictionary<int, List<int>>());

        List<int> linkList = GetLinkTileIndex(mapIndex);
        foreach (var layer in mapDict)
        {
            if (layer.Key > curLayer)
            {
                dict.Item2.Add(layer.Key, IntersectWithDictionary(linkList, layer.Value));
            }
            else if (layer.Key < curLayer)
            {
                dict.Item1.Add(layer.Key, IntersectWithDictionary(linkList, layer.Value));
            }
        }
        return dict;
    }
    
    static List<int> IntersectWithDictionary<TValue>(List<int> linkList, IDictionary<int, TValue> layerDict)
    {
        // 预分配结果容量为较小值，减少内部扩容
        List<int> result = new List<int>(Math.Min(linkList.Count, layerDict.Count));
        // 遍历 linkList（通常较小且需要保留 linkList 的顺序）
        for (int i = 0, n = linkList.Count; i < n; ++i)
        {
            int idx = linkList[i];
            // O(1) 查找，无额外分配
            if (layerDict.ContainsKey(idx))
                result.Add(idx);
        }
        return result;
    }

    public static TileMatch_LevelData GetNewData(int levelNum, TileMatch_LevelData data, ref Dictionary<int, Dictionary<int, TileMoveDirectionType>> moveDict)
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

        Dictionary<int, List<int>> recordTileTexIndexDict = new Dictionary<int, List<int>>();

        foreach (var layer in data.AllLayerTileDict)
        {
            foreach (var map in layer.Value)
            {
                if (!recordTileTexIndexDict.ContainsKey(map.Value.TileID))
                    recordTileTexIndexDict.Add(map.Value.TileID, new List<int>());

                recordTileTexIndexDict[map.Value.TileID].Add(map.Value.TileID);
            }
        }

        moveDict.Clear();

        if (IsNeedRandom(recordTileTexIndexDict))
        {
            int randomTotalNum = NeedRandomTotalNum(recordTileTexIndexDict);
            int needTexTotalNum = Mathf.Max(1, data.TotalItemNum);
            List<int> randomTexIndex = new List<int>();
            List<int> curTexTypeList = recordTileTexIndexDict.Keys.ToList();
            curTexTypeList.RemoveAll(a => a <= 1 || IsNonRandomized(a));

            CallBack<int> addRandomAciton = (i) =>
            {
                randomTotalNum -= 3;
                curTexTypeList.Add(i);
                randomTexIndex.Add(i); randomTexIndex.Add(i); randomTexIndex.Add(i);
                if (!recordTileTexIndexDict.ContainsKey(i))
                    recordTileTexIndexDict.Add(i, new List<int>());
                recordTileTexIndexDict[i].Add(i); recordTileTexIndexDict[i].Add(i); recordTileTexIndexDict[i].Add(i);
            };

            if (data.TileItemArray != null)
            {
                foreach (int TileID in data.TileItemArray)
                {
                    if (TileID > 1 && !IsNonRandomized(TileID) && !curTexTypeList.Contains(TileID))
                    {
                        curTexTypeList.Add(TileID);
                    }
                }
            }

            //修正未凑够3个数量的元素
            foreach (KeyValuePair<int, List<int>> pair in recordTileTexIndexDict)
            {
                if (pair.Key > 1 && !IsSpecial(pair.Key) && pair.Value.Count % 3 != 0)
                {
                    int remainder = 3 - pair.Value.Count % 3;
                    for (int i = 0; i < remainder; i++)
                    {
                        randomTotalNum--;
                        randomTexIndex.Add(pair.Key);
                        recordTileTexIndexDict[pair.Key].Add(pair.Key);
                    }
                }
            }

            if (randomTotalNum % 3 != 0)
                Log.Error("randomTotalNum is invalid");

            List<int> pseudoRandom = new List<int>();
            curTexTypeList.ForEach(index => pseudoRandom.Add(index));
            while (randomTotalNum > 0 /*|| curTexTypeList.Count < needTexTotalNum*/)
            {
                int randomIndex = 0;
                if (curTexTypeList.Count < needTexTotalNum)
                {
                    var list = TileMatchSprites.Keys.ToList().Except(curTexTypeList).ToList();
                    randomIndex = list[Random.Range(0, list.Count)];
                }
                else
                {
                    if (pseudoRandom.Count <= 0)
                        curTexTypeList.ForEach(index => pseudoRandom.Add(index));
                    randomIndex = pseudoRandom[Random.Range(0, pseudoRandom.Count)];
                    pseudoRandom.Remove(randomIndex);
                }
                addRandomAciton(randomIndex);
            }

            for (int i = 0; i < data.AllLayerTileDict.Count; i++)
            {
                int layer = data.AllLayerTileDict.ElementAt(i).Key;
                var map = data.AllLayerTileDict.ElementAt(i).Value;
                for (int j = 0; j < map.Count; j++)
                {
                    if (IsNeedRandom(map.ElementAt(j).Value.TileID))
                    {
                        if (!moveDict.ContainsKey(layer)) moveDict.Add(layer, new Dictionary<int, TileMoveDirectionType>());
                        moveDict[layer].Add(map.ElementAt(j).Key, (TileMoveDirectionType)map.ElementAt(j).Value.TileID);

                        int random = randomTexIndex[Random.Range(0, randomTexIndex.Count)];
                        data.AllLayerTileDict[layer][map.ElementAt(j).Key] = new TileInfo(random, map.ElementAt(j).Value.AttachID, map.ElementAt(j).Value.DirectionType);
                        randomTexIndex.Remove(random);
                    }
                }
            }
        }

        int goldTileCount = GoldTileUtil.ReplaceWithGoldTile(recordTileTexIndexDict, data.AllLayerTileDict);
        data.GoldTileCount = goldTileCount;
        return data;
    }

    /// <summary>
    /// 金块关找出最少的图案
    /// </summary>
    /// <param name="TileIndexCountDict">全部图案索引数量字典</param>
    /// <returns></returns>
    private static int GoldLevelFindMaxTileID(Dictionary<int, List<int>> TileIndexCountDict)
    {
        var orderedEnum = TileIndexCountDict.OrderByDescending(kvp => kvp.Value.Count);
        Dictionary<int, List<int>> orderedDic = orderedEnum.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        List<int> orderedIndexList = orderedDic.Keys.ToList();
        orderedIndexList.RemoveAll(index => index <= 1 || IsSpecial(index));
        int maxTileID = orderedIndexList[0];

        Log.Info("要替换的图案是：" + maxTileID);
        return maxTileID;
    }

    public static int[] ArrayRemove(this int[] array, int index)
    {
        if (array != null && array.Contains(index))
        {
            array.ToList().Remove(index);
            return array.ToArray();
        }
        else return array;
    }

    public static Vector3 GetTilePos(int layer, int mapIndex, TileMoveDirectionType moveIndex, Dictionary<int, Dictionary<int, TileMoveDirectionType>> mapDict)
    {
        return
            GetTileMove(layer, mapIndex, moveIndex, mapDict)
            + new Vector3((mapIndex % 16) * EachWidth, -mapIndex / 16 * EachHeight)
            - new Vector3(EachWidth * 7, -EachWidth * 7);
    }

    public static Vector3 GetTileMove(int layer, int mapIndex, TileMoveDirectionType moveIndex, Dictionary<int, Dictionary<int, TileMoveDirectionType>> mapDict)
    {
        int recordLayer = 0;
        while (layer > 0)
        {
            recordLayer++;
            layer--;
            if (mapDict.ContainsKey(layer) && mapDict[layer].TryGetValue(mapIndex, out TileMoveDirectionType type) && type == moveIndex) { }
            else break;
        }
        switch (moveIndex)
        {
            case TileMoveDirectionType.Up:
                return 12 * (recordLayer - 1) * Vector3.up;
            case TileMoveDirectionType.Down:
                return 12 * (recordLayer - 1) * Vector3.down;
            case TileMoveDirectionType.Left:
                return 12 * (recordLayer - 1) * Vector3.left;
            case TileMoveDirectionType.Right:
                return 12 * (recordLayer - 1) * Vector3.right;
            default:
                return Vector3.zero;
        }
    }

    public static bool IsNeedRandom(Dictionary<int, List<int>> dict)
    {
        return dict.ContainsKey(-3)
            || dict.ContainsKey(-2)
            || dict.ContainsKey(-1)
            || dict.ContainsKey(0)
            || dict.ContainsKey(1);
    }

    public static bool IsNeedRandom(int tileID)
    {
        return tileID >= -3 && tileID <= 1;
    }

    /// <summary>
    /// 是否是特殊棋子
    /// </summary>
    public static bool IsSpecial(int tileID)
    {
        return tileID >= 20;
    }

    /// <summary>
    /// 是否是不可随机棋子
    /// </summary>
    public static bool IsNonRandomized(int tileID)
    {
        return IsSpecial(tileID) || tileID == 17;
    }

    public static string GetTilePrefabName(int tileId)
    {
        switch (tileId)
        {
            case 20:
                return "TileItemPrefab_Firework";
            default:
                return "TileItemPrefab";
        }
    }

    public static int NeedRandomTotalNum(Dictionary<int, List<int>> dict)
    {
        int totalNum = 0;
        totalNum += dict.ContainsKey(-3) ? dict[-3].Count : 0;
        totalNum += dict.ContainsKey(-2) ? dict[-2].Count : 0;
        totalNum += dict.ContainsKey(-1) ? dict[-1].Count : 0;
        totalNum += dict.ContainsKey(0) ? dict[0].Count : 0;
        totalNum += dict.ContainsKey(1) ? dict[1].Count : 0;
        return totalNum;
    }

    public static List<int> GetRandmonList(int count)
    {
        if (count <= 1) return null;
        List<int> list = new List<int>();
        int num = 1;
        while (num <= count)
        {
            list.Add(num);
            num++;
        }

        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, list.Count);
            if (randomIndex == i)
                randomIndex = Random.Range(0, list.Count);
            int nowValue = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = nowValue;
        }
        return list;
    }

    #region 放过机制处理

    public static bool EditorOpenEnterLevel = false;

    private static int FirstLevelReduceTypeNum => (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.First_Level_Reduce_Type_Num_By_Day, 2);
    /// <summary>
    /// 放过机制  获取新的关卡信息
    /// </summary>
    /// <param name="failCount"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static TileMatch_LevelData GetTileMatchLevelDataByEnterCount(this TileMatch_LevelData data, int levelId, int enterCount)
    {
#if UNITY_EDITOR
        if (!EditorOpenEnterLevel) return data;
#endif

        LevelItemData levelData = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetLevelData(levelId);
        if (levelData != null)
        {
            Log.Info($"放过机制Start:enterCount:{enterCount}：level：{levelId}:TypeCountMin:{levelData.TypeCountMin}:" +
                     $"TryCount:{levelData.TryCount}:TotalItemNum:{data.TotalItemNum}:TileItemArray.Length:{data.TileItemArray.Length}");
            
            int reduceTypeNum = 0;
			
            bool isFirstEnterGame = PlayerBehaviorModel.Instance.Data.LevelData.GameToMapCountByDay == 0;
            if (isFirstEnterGame)
            {
                PlayerBehaviorModel.Instance.Data.LevelData.GameToMapCountByDay++;
                PlayerBehaviorModel.Instance.SaveToLocal();

                reduceTypeNum = FirstLevelReduceTypeNum;
            }
            else
            {
                //计算出需要的元素种类数量
                reduceTypeNum =enterCount-levelData.TryCount;
            }
            if (reduceTypeNum > 0)
            {
                //最小的资源数量
                int minItemTypeNum = levelData.TypeCountMin;
                data.TotalItemNum = Math.Max(minItemTypeNum, data.TotalItemNum - reduceTypeNum);
                if (data.TileItemArray.Length > data.TotalItemNum)
                {
                    //截取0~TotalItemNum 这段
                    data.TileItemArray = data.TileItemArray.OrderBy(a => a).ToArray();
                    data.TileItemArray = data.TileItemArray.Skip(0).Take(data.TotalItemNum).ToArray();
                }
            }
            Log.Info($"放过机制End:enterCount:{enterCount}：level：{levelId}:TypeCountMin:{levelData.TypeCountMin}:" +
                     $"TryCount:{levelData.TryCount}:TotalItemNum:{data.TotalItemNum}:TileItemArray.Length:{data.TileItemArray.Length}");
        }
        return data;
    }
    #endregion
    
    #region 给关卡数据添加挂载物信息
    /// <summary>
    /// 添加油桶挂载物【添加的挂载物在最底层】
    /// </summary>
    /// <param name="data"></param>
    /// <param name="addNum"></param>
    /// <returns></returns>
    public static TileMatch_LevelData AddOilDrumAttachItem(this TileMatch_LevelData data,int addNum)
    {
        Log.Info($"增加油桶开始");
        if (addNum > 0)
        {
            try
            {
                List<TileInfo> recordBeCoverTileItemList = new List<TileInfo>();
                foreach (var dict in data.AllLayerTileDict)
                {
                    foreach (var element in dict.Value)
                    {
                        if (element.Value.AttachID <= 0)
                        {
                            //如果是被覆盖的 
                            if(IsBeCover(dict.Key,element.Key,data.AllLayerTileDict))
                            {
                                recordBeCoverTileItemList.Add(element.Value);
                            }
                        }
                    }
                }
                if (recordBeCoverTileItemList.Count == 0) return data;
                int[] addItemIndexs = GenerateRandomSample(recordBeCoverTileItemList.Count,addNum);
                for (int i = 0; i < addItemIndexs.Length; i++)
                {
                    recordBeCoverTileItemList[addItemIndexs[i]].AttachID=4;
                    Log.Info($"增加油桶");
                }
            }
            catch (Exception e)
            {
                Log.Error($"AddOilDrumAttachItem:{e.Message}");
            }
        }
        return data;
    }

    public static bool IsBeCover(int curLayer, int mapIndex, Dictionary<int, Dictionary<int, TileInfo>> mapDict)
    {
        List<int> linkList = GetLinkTileIndex(mapIndex);
        foreach (var layer in mapDict)
        {
            if (layer.Key > curLayer)
            {
                return linkList.Intersect(layer.Value.Keys).ToArray().Length > 0;
            }
        }
        return false;
    }
    
    public static int[] GenerateRandomSample(int n, int m)
    {
        m = Math.Min(n, m);
        System.Random random = new System.Random();
        return Enumerable.Range(0, n).OrderBy(x => random.Next()).Take(m).ToArray();
    }
    #endregion

	// 时间模式测试标志位
    public static bool OpenCalendarTimeLimitTest = false;
}