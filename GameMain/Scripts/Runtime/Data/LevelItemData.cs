
using System;

[Serializable]
public class LevelItemData
{
	public int LevelID; // 关卡编号
	public int ModeID; // 模式名称（默认值=1）
	public int SpecialTile; //金块关
    public int TryCount;//超过几次失败之后花色 每次花色减一
    public int TypeCountMin;//花色种类最小数量
    public int TimeLimit;//时间模式的关卡通过时长
    public string Column5;
}