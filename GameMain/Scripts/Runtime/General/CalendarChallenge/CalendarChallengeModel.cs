using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MySelf.Model
{
    public class CalendarChallengeModel : BaseModelService<CalendarChallengeModel, CalendarChallengeData>
    {
        #region Service
        public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
        {
            return new Dictionary<string, object>()
        {
            { "CalendarChallengeModel.FinishDays",JsonConvert.SerializeObject(Data.FinishDays, Formatting.Indented)},
            { "CalendarChallengeModel.MonthlyChestStatus",JsonConvert.SerializeObject(Data.MonthlyChestStatus, Formatting.Indented)},
            { "CalendarChallengeModel.DailyLevelNum",JsonConvert.SerializeObject(Data.DailyLevelNum, Formatting.Indented)},
            { "CalendarChallengeModel.RandomLevelQueue",JsonConvert.SerializeObject(Data.RandomLevelQueue, Formatting.Indented)},
        };
        }

        public override void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary)
        {
            if (dictionary == null) return;
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "CalendarChallengeModel.FinishDays":
                        Data.FinishDays = JsonConvert.DeserializeObject<Dictionary<string, int>>((string)item.Value);
                        break;
                    case "CalendarChallengeModel.MonthlyChestStatus":
                        Data.MonthlyChestStatus = JsonConvert.DeserializeObject<Dictionary<string, int>>((string)item.Value);
                        break;
                    case "CalendarChallengeModel.DailyLevelNum":
                        Data.DailyLevelNum = JsonConvert.DeserializeObject<Dictionary<string, int>>((string)item.Value);
                        break;
                    case "CalendarChallengeModel.RandomLevelQueue":
                        Data.RandomLevelQueue = JsonConvert.DeserializeObject<List<int>>((string)item.Value);
                        break;
                }
            }
            SaveToLocal();
        }
        #endregion

        public Dictionary<string, int> FinishDays
        {
            get => Data.FinishDays;
            set
            {
                Data.FinishDays = value;
                SaveToLocal();
            }
        }

        public Dictionary<string, int> MonthlyChestStatus
        {
            get => Data.MonthlyChestStatus;
            set
            {
                Data.MonthlyChestStatus = value;
                SaveToLocal();
            }
        }

        public Dictionary<string, int> DailyLevelNum
        {
            get => Data.DailyLevelNum;
            set
            {
                Data.DailyLevelNum = value;
                SaveToLocal();
            }
        }

        public List<int> RandomLevelQueue
        {
            get => Data.RandomLevelQueue;
            set
            {
                Data.RandomLevelQueue = value;
                SaveToLocal();
            }
        }

        public Queue<int> EasyRandomLevelQueue
        {
            get => Data.EasyRandomLevelQueue;
            set
            {
                Data.EasyRandomLevelQueue = value;
                SaveToLocal();
            }
        }
        public Queue<int> HardRandomLevelQueue
        {
            get => Data.HardRandomLevelQueue;
            set
            {
                Data.HardRandomLevelQueue = value;
                SaveToLocal();
            }
        }

        public Queue<int> EasyRandomTimeLevelQueue
        {
            get => Data.EasyRandomTimeLevelQueue;
            set
            {
                Data.EasyRandomTimeLevelQueue = value;
                SaveToLocal();
            }
        }
        public Queue<int> HardRandomTimeLevelQueue
        {
            get => Data.HardRandomTimeLevelQueue;
            set
            {
                Data.HardRandomTimeLevelQueue = value;
                SaveToLocal();
            }
        }

        public DateTime LastWinDate
        {
            get => Data.LastWinDate;
            // get => new DateTime(2023,11,18);
            set
            {
                Data.LastWinDate = value;
                SaveToLocal();
            }
        }

        public int RandomCount
        {
            get => Data.RandomCount;
            // get => new DateTime(2023,11,18);
            set
            {
                Data.RandomCount = value;
                SaveToLocal();
            }
        }

        public Dictionary<string, int> LevelPlayedTimes = new Dictionary<string, int>();

        public void Init()
        {
            Data = GetLocalModel();
        }
    }

    public class CalendarChallengeData
    {
        public Dictionary<string, int> FinishDays = new Dictionary<string, int>();
        public Dictionary<string, int> MonthlyChestStatus = new Dictionary<string, int>();
        public Dictionary<string, int> DailyLevelNum = new Dictionary<string, int>();
        public List<int> RandomLevelQueue = new List<int>();
        public Queue<int> EasyRandomLevelQueue = new Queue<int>();
        public Queue<int> HardRandomLevelQueue = new Queue<int>();
        public Queue<int> EasyRandomTimeLevelQueue = new Queue<int>();
        public Queue<int> HardRandomTimeLevelQueue = new Queue<int>();
        public DateTime LastWinDate = DateTime.MinValue;
        public int RandomCount = 0;
    }
}