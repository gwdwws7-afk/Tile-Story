
using System;

[Serializable]
public class BGItemData
{
	public int ID; // 编号
	public int Theme; // 主题ID
	public string ThemeName; // 主题名称
	public string StartSellTime;
	public int BGUnlockLevel; // 背景解锁等级
	public int BGPrice; // 背景价格
	public int BGSort; // 排序顺序
	public string UnlockTips;

	public DateTime StartSellTimeDT
	{
		get
		{
			if (string.IsNullOrEmpty(StartSellTime))
				return DateTime.MinValue;
			if (StartSellTime == "NotForSale")
				return DateTime.MaxValue;

			if (startSellTimeDT == DateTime.MinValue)
			{
				startSellTimeDT = DateTime.ParseExact(StartSellTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
			}
			return startSellTimeDT;
		}
	}
	private DateTime startSellTimeDT = DateTime.MinValue;
}