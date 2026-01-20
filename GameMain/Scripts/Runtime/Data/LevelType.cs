
using System;

[Serializable]
public class LevelType
{
	public int ModeID; // 编号
	public int StarNum; // 获得星星数量
	public int CoinNum; // 获得金币数量
	public int ADButton; // 是否有看广告翻倍按钮（0-无，1-有）
	public int CoinsTimesNum; // 看广告金币翻倍倍数
}