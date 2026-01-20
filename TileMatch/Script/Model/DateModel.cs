using System;
using UnityEngine;
using Newtonsoft.Json;

namespace MySelf.Model
{
	public enum LoginStatus
	{
		PRE_DO = 0,
		TODAY,
		NEW_DAY,
		LONG_AGO
	}
	[JsonObject(MemberSerialization.OptOut)]
	public class DateModelData
	{
		public DateTime LastLoginDate= Constant.GameConfig.DateTimeMin;
		public DateTime FirstLoginDate= Constant.GameConfig.DateTimeMin;
		public int ContinuousLoginDay=0;

		[JsonIgnore] public LoginStatus LoginStatus;
		[JsonIgnore] public DateTime TodayEndTime= Constant.GameConfig.DateTimeMin;
	}
	public class DateModel : BaseModel<DateModel, DateModelData>
	{
        public void RefreshDate(Action newDayEvent)
		{
			if(Time.frameCount%60!=0)return;
			if (DateTime.Now >= Data.TodayEndTime)
			{
				try
				{
					if (CommonModel.Instance.Data.NewPlayer)
					{
						CommonModel.Instance.SetNewPlayer(false);
						Data.FirstLoginDate = DateTime.Now;
						GameManager.PlayerData.AddItemNum(TotalItemData.Life, GameManager.PlayerData.FullLifeNum);
						GameManager.DataNode.SetData("IsNewPlayer", true);
					}

					Data.TodayEndTime = DateTime.Now.AddDays(1) - DateTime.Now.TimeOfDay;

					Data.LoginStatus = CheckLoginStatus();

					if (Data.LoginStatus == LoginStatus.NEW_DAY) // ���յ�½���������״ε�½
					{
						Data.ContinuousLoginDay++;
						newDayEvent?.Invoke();
						GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Continuous_Login, 1);
					}
					else if (Data.LoginStatus == LoginStatus.LONG_AGO) // �����¼��������½��������
					{
						Data.ContinuousLoginDay = 0;
						newDayEvent?.Invoke();
						GameManager.Objective.SetObjectiveProgress(ObjectiveType.Continuous_Login, 1);
					}

					Data.LastLoginDate = DateTime.Now;
					GameManager.Objective.SetObjectiveProgress(ObjectiveType.Accumulated_Login, (DateTime.Now - GameManager.PlayerData.FirstDateTime).Days + 1);

					SaveToLocal();
				}
				catch (System.Exception e)
				{
					Log.Error($"RefreshDate fail :{e.Message}");
				}
			}
		}


		private LoginStatus CheckLoginStatus()
		{
			DateTime lastDate = new DateTime(
				Data.LastLoginDate.Year,
				Data.LastLoginDate.Month,
				Data.LastLoginDate.Day);
			int interval = (DateTime.Now - lastDate).Days;

			if (interval == 1)
			{
				return LoginStatus.NEW_DAY;
			}
			else if (interval == 0)
			{
				return LoginStatus.TODAY;
			}
			else
			{
				return LoginStatus.LONG_AGO;
			}
		}
	}
}

