using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class DTTileID
{
	public List<TileData> TileDatas;

	private Dictionary<int, TileData> tileDataDict;

	public Dictionary<int, TileData> TileDataDict
	{
		get
		{
			if (tileDataDict == null)
			{
				TileDatas.Sort(TileEqual);
				tileDataDict = TileDatas.ToDictionary(a => a.ID, b => b);
			}

			return tileDataDict;
		}
	}

	public TileData GetData(int id)
	{
		return TileDataDict[id];
	}

	public bool IsOwn(int id)
	{
		return TileDataDict[id].TilePrice <= 0;
	}

	private int TileEqual(TileData a,TileData b)
	{
		return a.TileSort.CompareTo(b.TileSort);
	}
}
