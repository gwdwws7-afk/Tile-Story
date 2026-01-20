using System;

namespace MySelf.Model
{
	public class ActivityModelData
	{
		public int CurPeriodID = 0;
		public int CurActivityID = 0;
        public DateTime CurActivityStartTime=Constant.GameConfig.DateTimeMin;
		public DateTime CurActivityEndTime = Constant.GameConfig.DateTimeMin;
	}

	public class ActivityModel : BaseModel<ActivityModel, ActivityModelData>
	{
        public int CurPeriodID
        {
            get => Data.CurPeriodID;
            set
            {
                Data.CurPeriodID = value;
                SaveToLocal();
            }
        }

        public int CurActivityID
        {
            get => Data.CurActivityID;
            set
            {
                Data.CurActivityID = value;
                SaveToLocal();
            }
        }

        public DateTime CurActivityEndTime
        {
            get => Data.CurActivityEndTime;
            set
            {
                Data.CurActivityEndTime = value;
                SaveToLocal();
            }
        }

        public DateTime CurActivityStartTime
        {
            get => Data.CurActivityStartTime;
            set
            {
                Data.CurActivityStartTime = value;
                SaveToLocal();
            }
        }
    }
}

