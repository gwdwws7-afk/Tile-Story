using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DTClimbBeanstalkScheduleData
{
    public List<ClimbBeanstalkScheduleDatas> ClimbBeanstalkScheduleDatas;

	/// <summary>
	/// 获取当前DateTime去表格中找到的 起效的ActivityID
	/// </summary>
	/// <returns></returns>
	public int GetNowActiveActivityID()
	{
		DateTime now = DateTime.Now;
		for (int i = 0; i < ClimbBeanstalkScheduleDatas.Count; ++i)
		{
			if (ClimbBeanstalkScheduleDatas[i].StartTimeDT <= now &&
				ClimbBeanstalkScheduleDatas[i].EndTimeDT >= now)
			{
				return ClimbBeanstalkScheduleDatas[i].ActivityID;
			}
		}

		return -1;
	}

	public DateTime GetNowActiveActivityEndTime()
    {
		DateTime now = DateTime.Now;
		for (int i = 0; i < ClimbBeanstalkScheduleDatas.Count; ++i)
		{
			if (ClimbBeanstalkScheduleDatas[i].StartTimeDT <= now &&
				ClimbBeanstalkScheduleDatas[i].EndTimeDT >= now)
			{
				return ClimbBeanstalkScheduleDatas[i].EndTimeDT;
			}
		}

		return DateTime.MinValue;
	}

	//在线进入到下一个活动阶段时 不会自动开始
}

[Serializable]
public class ClimbBeanstalkScheduleDatas
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

