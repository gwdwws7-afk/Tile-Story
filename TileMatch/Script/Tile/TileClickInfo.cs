using System.Collections.Generic;
using UnityEngine;

public sealed class TileClickInfo
{
    public int Layer;
    public int MapID;
    public TileMoveDirectionType moveIndex;
    public Dictionary<int, List<int>> CoverIndexs;
    public Dictionary<int, List<int>> BeCoverIndexs;

    public static TileClickInfo Create(int layer,int mapId, TileMoveDirectionType moveIndex,Dictionary<int, List<int>> coverIndexs,Dictionary<int, List<int>> beCoverIndexs)
    {
        TileClickInfo info = new TileClickInfo();
        info.Layer = layer;
        info.MapID = mapId;
        info.moveIndex = moveIndex;
        info.CoverIndexs = coverIndexs;
        info.BeCoverIndexs = beCoverIndexs;
        return info;
    }
}
