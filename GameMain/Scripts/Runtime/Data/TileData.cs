using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData
{
	public int ID; // 编号
	public string TileName; // 棋子图名称
	public int TileUnlockLevel; // 棋子解锁等级
	public int TilePrice; // 棋子价格
	public int TileSort; // 排序顺序
}
