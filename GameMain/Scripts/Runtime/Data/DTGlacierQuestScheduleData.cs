using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class DTGlacierQuestScheduleData
{
    public List<GlacierQuestScheduleDatas> GlacierQuestScheduleDatas;

	/// <summary>
	/// 获取当前DateTime去表格中找到的 起效的ActivityID
	/// </summary>
	/// <returns></returns>
	public int GetNowActiveActivityID()
	{
		DateTime now = DateTime.Now;
		for (int i = 0; i < GlacierQuestScheduleDatas.Count; ++i)
		{
			if (GlacierQuestScheduleDatas[i].StartTimeDT <= now &&
			    GlacierQuestScheduleDatas[i].EndTimeDT >= now)
			{
				return GlacierQuestScheduleDatas[i].ActivityID;
			}
		}

		return -1;
	}

	public DateTime GetNowActiveActivityEndTime()
    {
		DateTime now = DateTime.Now;
		for (int i = 0; i < GlacierQuestScheduleDatas.Count; ++i)
		{
			if (GlacierQuestScheduleDatas[i].StartTimeDT <= now &&
			    GlacierQuestScheduleDatas[i].EndTimeDT >= now)
			{
				return GlacierQuestScheduleDatas[i].EndTimeDT;
			}
		}

		return DateTime.MinValue;
	}

	public DateTime GetActiveEndTimeByActivityID(int id)
	{
		for (int i = 0; i < GlacierQuestScheduleDatas.Count; ++i)
		{
			if (GlacierQuestScheduleDatas[i].ActivityID == id)
			{
				return GlacierQuestScheduleDatas[i].EndTimeDT;
			}
		}
		return DateTime.MinValue;
	}
}

[Serializable]
public class GlacierQuestScheduleDatas
{
	public int ID;
	public int ActivityID; // 活动ID
	public string StartTime; // 开始时间
	public string EndTime; // 结束时间

	public DateTime StartTimeDT
    {
		get
		{
			if (startTimeDT == DateTime.MinValue)
			{
				startTimeDT = DateTime.ParseExact(StartTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
			}
			return startTimeDT;
		}
	}
	private DateTime startTimeDT = DateTime.MinValue;

	public DateTime EndTimeDT
    {
		get
		{
			if (endTimeDT == DateTime.MinValue)
			{
				endTimeDT = DateTime.ParseExact(EndTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
			}
			return endTimeDT;
		}
	}
	private DateTime endTimeDT = DateTime.MinValue;
}

