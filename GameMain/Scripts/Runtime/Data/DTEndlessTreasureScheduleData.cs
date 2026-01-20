using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DTEndlessTreasureScheduleData
{
	public List<EndlessTreasureScheduleData> EndlessTreasureScheduleDatas;

	public EndlessTreasureScheduleData GetEndlessDataByDateTimeNow()
	{
		foreach (var data in EndlessTreasureScheduleDatas)
		{
			if (DateTime.Now >= data.StartDateTime && DateTime.Now <= data.EndDateTime)
			{
				return data;
			}
		}
		return null;
	}

	public EndlessTreasureScheduleData GetEndlessDataByActivityId(int activityId)
	{
		return EndlessTreasureScheduleDatas.Find(obj => obj.ActivityID == activityId);
	}
}

[Serializable]
public class EndlessTreasureScheduleData
{
	public int ActivityID; // 活动ID
	public string StartTime; // 开始时间
	public string EndTime; // 结束时间

	private DateTime startDateTime=DateTime.MinValue;
	public DateTime StartDateTime
	{
		get
		{
			if (startDateTime == DateTime.MinValue)
			{
				startDateTime =  DateTime.ParseExact(StartTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
			}

			return startDateTime;
		}
	}

	private DateTime endDateTime=DateTime.MinValue;
	public DateTime EndDateTime
	{
		get
		{
			if (endDateTime == DateTime.MinValue)
			{
				endDateTime =  DateTime.ParseExact(EndTime, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
			}

			return endDateTime;
		}
	}

	public bool IsOver => DateTime.Now > EndDateTime;
}