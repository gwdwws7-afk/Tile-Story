using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GoldTileUtil
{
    private const int GoldTileID = 17;
    private const int GoldAttachID = 3;
    private static int goldTileCount;
    private static List<(int, int)> uncoveredGoldTiles;
    
    /// <summary>
    /// 找出数量最多的花色
    /// </summary>
    /// <param name="tileIndexCountDict">全部花色索引数量字典</param>
    /// <returns></returns>
    private static int GetMaxTileID(Dictionary<int, List<int>> tileIndexCountDict)
    {
        var orderedEnum = tileIndexCountDict.OrderByDescending(kvp => kvp.Value.Count);
        List<int> orderedIndexList = orderedEnum.Select(kvp => kvp.Key).ToList();
        orderedIndexList.RemoveAll(index => index <= 1 || index == 20);
        int maxTileID = orderedIndexList[0];

        Log.Info("要替换的图案是：" + maxTileID);
        return maxTileID;
    }

    /// <summary>
    /// 判断块是否被覆盖
    /// </summary>
    /// <param name="layer">层</param>
    /// <param name="mapIndex">位置</param>
    /// <param name="allLayerTileDict">字典</param>
    /// <returns></returns>
    private static bool CheckTileIsCovered(int layer, int mapIndex, Dictionary<int, Dictionary<int, TileInfo>> allLayerTileDict)
    {
        var (coverIndexes, beCoverIndexes) = TileMatchUtil.GetLinkMaskByLayer(layer, mapIndex, allLayerTileDict);
        foreach (var kvp in beCoverIndexes)
        {
            if (kvp.Value is { Count: > 0 })
                return true;
        }

        return false;
    }
    
    /// <summary>
    /// 用金块替换数量最多的花色
    /// </summary>
    /// <param name="tileIndexCountDict">全部花色索引数量字典</param>
    /// <param name="allLayerTileDict"></param>
    /// <returns></returns>
    public static int ReplaceWithGoldTile(Dictionary<int, List<int>> tileIndexCountDict,
        Dictionary<int, Dictionary<int, TileInfo>> allLayerTileDict)
    {
        if (!DTLevelUtil.IsSpecialGoldTile(GameManager.PlayerData.RealLevel())) return 0;

        goldTileCount = 0;
        uncoveredGoldTiles = new List<(int, int)>();
        
        int maxTileID = GetMaxTileID(tileIndexCountDict);
        foreach (var kvp in allLayerTileDict)
        {
            int layer = kvp.Key;
            Dictionary<int, TileInfo> map = kvp.Value;
            for (int i = 0; i < map.Count; i++)
            {
                if (map.ElementAt(i).Value.TileID == maxTileID)
                {
                    int key = map.ElementAt(i).Key;
                    map[key] = new TileInfo(GoldTileID, GoldAttachID, map.ElementAt(i).Value.DirectionType);
                    goldTileCount++;

                    if (!CheckTileIsCovered(layer, key, allLayerTileDict))
                    {
                        uncoveredGoldTiles.Add((layer, key));
                    }
                }
            }
        }
        
        while (uncoveredGoldTiles.Count > 4)
        {
            //Debug.LogError("将多余的未被遮盖的金块与最下层块交换");
            int index = Random.Range(0, uncoveredGoldTiles.Count);
            (int, int) goldTilePos = uncoveredGoldTiles[index];
            uncoveredGoldTiles.Remove(goldTilePos);

            bool isReplaced = false;
            foreach (var kvp in allLayerTileDict)
            {
                int layer = kvp.Key;
                Dictionary<int, TileInfo> map = kvp.Value;
                for (int i = 0; i < map.Count; i++)
                {
                    if (map.ElementAt(i).Value.TileID != GoldTileID && CheckTileIsCovered(layer, map.ElementAt(i).Key, allLayerTileDict))
                    {
                        allLayerTileDict[goldTilePos.Item1][goldTilePos.Item2] = map.ElementAt(i).Value;
                        allLayerTileDict[layer][map.ElementAt(i).Key] = new TileInfo(GoldTileID, GoldAttachID, map.ElementAt(i).Value.DirectionType);
                        isReplaced = true;
                        break;
                    }
                }
                if (isReplaced) break;
            }
        }

        return goldTileCount;
    }
}
