//
// Auto Generated Code By excel2json
// https://neil3d.gitee.io/coding/excel2json.html
// 1. 每个 Sheet 形成一个 Struct 定义, Sheet 的名称作为 Struct 的名称
// 2. 表格约定：第一行是变量名称，第二行是变量类型

// Generate From C:\UnityProject\BubbleShooter_ZL\branches\branch_1.0\src\TileMatch\TileMatch\documents\DTIdleDialogue.xlsx.xlsx

using System;

[Serializable]
public class IdleDialogueData
{
	public int ID; // 编号
	public int Role; // 场景spine序号
	public string Dialogue; // 对话内容
	public int Chapter; // 章节
	public int BuildScheduleStart; // 进度条件下限
	public int BuildScheduleEnd; // 进度条件上限
	public string SoundEffect; // 播放音效
}


// End of Auto Generated Code
