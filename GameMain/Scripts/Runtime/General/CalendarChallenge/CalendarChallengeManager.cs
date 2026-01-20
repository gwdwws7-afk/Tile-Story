using Firebase.Analytics;
using MySelf.Model;
using System;
using System.Collections.Generic;
using System.Globalization;

public class CalendarChallengeManager : TaskManagerBase
{
    public Dictionary<string, int> FinishDays
    {
        get => CalendarChallengeModel.Instance.FinishDays;
        set => CalendarChallengeModel.Instance.FinishDays = value;
    }

    public bool HasUsedTodayFreeChance
    {
        get => PlayerBehaviorModel.Instance.HasUsedTodayFreeChance();
        set => PlayerBehaviorModel.Instance.RecordHasUsedTodayFreeChance(value);
    }

    public bool HasShownCalendarSwitchGuide
    {
        get => PlayerBehaviorModel.Instance.HasShownCalendarSwitchGuide();
        set => PlayerBehaviorModel.Instance.RecordHasShownCalendarSwitchGuide(value);
    }

    public bool HasShowedCalendarChallengeGuide
    {
        get => PlayerBehaviorModel.Instance.HasShownCalendarChallengeGuide();
        set => PlayerBehaviorModel.Instance.RecordHasShownCalendarChallengeGuide(value);
    }

    public Dictionary<string, int> MonthlyChestStatus
    {
        get => CalendarChallengeModel.Instance.MonthlyChestStatus;
        set => CalendarChallengeModel.Instance.MonthlyChestStatus = value;
    }

    public Dictionary<string, int> DailyLevelNum
    {
        get => CalendarChallengeModel.Instance.DailyLevelNum;
        set => CalendarChallengeModel.Instance.DailyLevelNum = value;
    }

    public DateTime LastWinDate
    {
        get => CalendarChallengeModel.Instance.LastWinDate;
        set => CalendarChallengeModel.Instance.LastWinDate = value;
    }

    public bool IsPlayingCalendarChallenge => GameManager.DataNode.HasData("CalendarChallengeDate");

    public bool NeedToShowAnim => LastWinDate != DateTime.MinValue;

    public DateTime LastFailDate
    {
        get;
        set;
    }

    public int PlayingLevel
    {
        get
        {
            var date = GameManager.DataNode.GetData("CalendarChallengeDate", DateTime.MinValue);
            if (date == DateTime.MinValue)
            {
                return 0;
            }
            var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (DailyLevelNum.ContainsKey(dateString))
            {
                return DailyLevelNum[dateString];
            }
            return 0;
        }
    }

    private List<int> RandomLevelQueue
    {
        get => CalendarChallengeModel.Instance.RandomLevelQueue;
        set => CalendarChallengeModel.Instance.RandomLevelQueue = value;
    }

    private Queue<int> EasyRandomLevelQueue
    {
        get => CalendarChallengeModel.Instance.EasyRandomLevelQueue;
        set => CalendarChallengeModel.Instance.EasyRandomLevelQueue = value;
    }

    private Queue<int> HardRandomLevelQueue
    {
        get => CalendarChallengeModel.Instance.HardRandomLevelQueue;
        set => CalendarChallengeModel.Instance.HardRandomLevelQueue = value;
    }

    private Queue<int> EasyRandomTimeLevelQueue
    {
        get => CalendarChallengeModel.Instance.EasyRandomTimeLevelQueue;
        set => CalendarChallengeModel.Instance.EasyRandomTimeLevelQueue = value;
    }

    private Queue<int> HardRandomTimeLevelQueue
    {
        get => CalendarChallengeModel.Instance.HardRandomTimeLevelQueue;
        set => CalendarChallengeModel.Instance.HardRandomTimeLevelQueue = value;
    }

    public Dictionary<string, int> LevelPlayedTimes
    {
        get => CalendarChallengeModel.Instance.LevelPlayedTimes;
        set => CalendarChallengeModel.Instance.LevelPlayedTimes = value;
    }

    private int RandomCount
    {
        get => CalendarChallengeModel.Instance.RandomCount;
        set => CalendarChallengeModel.Instance.RandomCount = value;
    }

    //0:Free, 1:Ad, 2:Paid
    public int ChallengeStartType
    {
        get;
        set;
    }

    /// <summary>
    /// 开始每日挑战
    /// </summary>
    /// <param name="dateTime">挑战日期</param>
    /// <param name="startType">开始挑战的方式</param>
    public void StartCalendarChallenge(DateTime dateTime, int startType = 0)
    {
        ChallengeStartType = startType;
        GameManager.DataNode.SetData("CalendarChallengeDate", dateTime);
        if (dateTime.Date == DateTime.Today)
        {
            HasUsedTodayFreeChance = true;
        }

        if (!PlayerBehaviorModel.Instance.DailyFirstStartLevel())
        {
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Start_One_Level_Day);
            PlayerBehaviorModel.Instance.RecordDailyFirstStartLevel(true);
        }
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ChangBGImageID, Constant.GameConfig.CalendarBgIndex));
        ProcedureUtil.ProcedureMapToGame();
    }

    public bool CheckIfNeedToShowFreeSkillProb()
    {
        if (!IsPlayingCalendarChallenge) return false;
        var date = GameManager.DataNode.GetData("CalendarChallengeDate", DateTime.MinValue);
        var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (LevelPlayedTimes.ContainsKey(dateString))
        {
            return LevelPlayedTimes[dateString] >= 1;
        }

        return false;
    }

    public static string GetSelectedDateString(DateTime dateTime)
    {
        return dateTime.ToString("M", GetCultureInfoFromLanguage(GameManager.Localization.Language));
    }

    /// <summary>
    /// 每日挑战胜利
    /// </summary>
    public void CalendarChallengeWin()
    {
        var dateTime = GameManager.DataNode.GetData<DateTime>("CalendarChallengeDate", DateTime.MinValue);
        if (dateTime == DateTime.MinValue)
        {
            return;
        }
        GameManager.DataNode.SetData<int>("ContinueLevelCount", 0);
        GameManager.DataNode.RemoveNode("CalendarChallengeDate");
        var dateString = dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        LastWinDate = dateTime;
        if (FinishDays.ContainsKey(dateString))
        {
            FinishDays[dateString]++;
            CalendarChallengeModel.Instance.SaveToLocal();
        }
        else
        {
            FinishDays.Add(dateString, 1);
            CalendarChallengeModel.Instance.SaveToLocal();
        }
    }

    public void CalendarChallengeFail()
    {
        GameManager.DataNode.SetData<int>("ContinueLevelCount", 0);
        var date = GameManager.DataNode.GetData("CalendarChallengeDate", DateTime.MinValue);
        GameManager.DataNode.RemoveNode("CalendarChallengeDate");
        if (date == DateTime.MinValue)
        {
            return;
        }
        var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        LevelPlayedTimes.TryAdd(dateString, 1);

        LastFailDate = date;
    }

    public void FinishAllCalendarChallenge()
    {
        var lastMonthFirstDay = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddMonths(-1);
        // var searchLastDay = lastMonthFirstDay>StartDate?lastMonthFirstDay:StartDate;

        for (var date = DateTime.Now; date >= lastMonthFirstDay; date = date.AddDays(-1))
        {
            var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            FinishDays.TryAdd(dateString, 1);
        }
        CalendarChallengeModel.Instance.SaveToLocal();
    }

    public void FinishAllCalendarChallengeOfThisMonth()
    {
        var lastMonthFirstDay = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
        // var searchLastDay = lastMonthFirstDay>StartDate?lastMonthFirstDay:StartDate;

        for (var date = DateTime.Now; date >= lastMonthFirstDay; date = date.AddDays(-1))
        {
            var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            FinishDays.TryAdd(dateString, 1);
        }
        CalendarChallengeModel.Instance.SaveToLocal();
    }

    /// <summary>
    /// 检查当日任务是否已完成
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns>true:已完成, false:未完成</returns>
    public bool CheckFinishDay(DateTime dateTime)
    {
        var dateString = dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return FinishDays.ContainsKey(dateString) && FinishDays[dateString] > 0;
    }

    public int GetRandomLevel()
    {
        var date = GameManager.DataNode.GetData("CalendarChallengeDate", DateTime.MinValue);
        var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        // 增加时间模式测试代码
        if (TileMatchUtil.OpenCalendarTimeLimitTest)
        {
            if (DailyLevelNum.ContainsKey(dateString))
            {
                return DailyLevelNum[dateString];
            }
            else
            {
                int lv = UnityEngine.Random.Range(81, 111) + 9000000;
                RandomLevelQueue.Add(lv);
                DailyLevelNum.Add(dateString, lv);
                CalendarChallengeModel.Instance.SaveToLocal();
                return lv;
            }
        }

        if (date == DateTime.MinValue)
        {
            return UnityEngine.Random.Range(1, 31) + 9000000;
        }

        if (DailyLevelNum.ContainsKey(dateString))
        {
            return DailyLevelNum[dateString];
        }

        if (EasyRandomLevelQueue.Count == 0)
        {
            EasyRandomLevelQueue = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetChallengeLevelList(false, false);
        }
        if (HardRandomLevelQueue.Count == 0)
        {
            HardRandomLevelQueue = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetChallengeLevelList(true, false);
        }
        if (EasyRandomTimeLevelQueue.Count == 0)
        {
            EasyRandomTimeLevelQueue = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetChallengeLevelList(false, true);
        }
        if (HardRandomTimeLevelQueue.Count == 0)
        {
            HardRandomTimeLevelQueue = GameManager.DataTable.GetDataTable<DTLevelID>().Data.GetChallengeLevelList(true, true);
        }

        int level;
        bool isTimeLevel = false;
        switch (RandomCount)
        {
            case 0:
            case 6:
                level = EasyRandomLevelQueue.Dequeue();
                break;
            case 1:
            case 2:
            case 5:
            case 8:
            case 10:
                level = HardRandomLevelQueue.Dequeue();
                break;
            case 3:
            case 9:
                if (EasyRandomTimeLevelQueue.Count == 0)
                    level = EasyRandomLevelQueue.Dequeue();
                else
                    level = EasyRandomTimeLevelQueue.Dequeue();
                isTimeLevel = true;
                break;
            case 4:
            case 7:
            case 11:
                if (HardRandomTimeLevelQueue.Count == 0)
                    level = HardRandomLevelQueue.Dequeue();
                else
                    level = HardRandomTimeLevelQueue.Dequeue();
                isTimeLevel = true;
                break;
            default://默认使用普通简单关
                level = EasyRandomLevelQueue.Dequeue();
                break;
        }

        // var level = RandomCount == 0 ? EasyRandomLevelQueue.Dequeue() : HardRandomLevelQueue.Dequeue();
        Log.Info($"RandomLevel {level}, IsHard: {RandomCount % 3 != 0}, IsTime：{isTimeLevel} RandomCount: {RandomCount}");
        RandomCount = (RandomCount + 1) % 12;

        level += 9000000;
        RandomLevelQueue.Add(level);
        DailyLevelNum.Add(dateString, level);
        CalendarChallengeModel.Instance.SaveToLocal();

        return level;
    }

    public DateTime GetNearestUncompletedLevel()
    {
        var lastMonthFirstDay = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddMonths(-1);
        // var searchLastDay = lastMonthFirstDay>StartDate?lastMonthFirstDay:StartDate;
        for (var date = DateTime.Now; date >= lastMonthFirstDay; date = date.AddDays(-1))
        {
            var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (!FinishDays.ContainsKey(dateString) || FinishDays[dateString] == 0)
            {
                return date;
            }
        }

        return DateTime.MinValue;
    }

    public DateTime LastDay()
    {
        var lastMonthFirstDay = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddMonths(-1);
        // var searchLastDay = lastMonthFirstDay>StartDate?lastMonthFirstDay:StartDate;
        return lastMonthFirstDay;
    }

    public int GetChallengeProgressByMonth(DateTime dateTime)
    {
        var month = dateTime.Month;
        var year = dateTime.Year;
        var days = DateTime.DaysInMonth(year, month);
        var count = 0;
        for (var i = 1; i <= days; i++)
        {
            var date = new DateTime(year, month, i);
            if (CheckFinishDay(date))
            {
                count++;
            }
        }

        // return 7;

        return count;
    }

    /// <summary>
    /// 每个月奖励需要的天数
    /// </summary>
    /// <param name="dateTime">日期</param>
    /// <returns></returns>
    public int[] GetRewardNeedDays(DateTime dateTime)
    {
        var month = dateTime.Month;
        var year = dateTime.Year;
        var days = DateTime.DaysInMonth(year, month);
        var states = new[] { 7, 14, 21, days };
        return states;
    }

    /// <summary>
    /// 发送奖励
    /// </summary>
    /// <param name="rewardLevel">奖励等级</param>
    /// <param name="callback">发奖成功后的回调</param>
    public void SendRewards(int rewardLevel, Action onPanelCreateSuccess, Action<bool> callback)
    {
        var dateString = LastWinDate.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        if (rewardLevel >= 4 || rewardLevel < 0)
        {
            callback?.Invoke(false);
            return;
        }

        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Open_Chest,
            new Parameter("ChestLevel", rewardLevel));
        var rewards = GameManager.DataTable.GetDataTable<DTCalendarChallengeData>().Data.GetRewardsByLevel(rewardLevel);
        foreach (var reward in rewards)
        {
            RewardManager.Instance.AddNeedGetReward(reward.Key, reward.Value);
        }

        GameManager.DataNode.SetData("CalendarChallengeRewardLevel", rewardLevel);
        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CalendarRewardPanel, false,
            () =>
            {
                callback?.Invoke(true);

            }, null, onPanelCreateSuccess);
        if (MonthlyChestStatus.ContainsKey(dateString))
        {
            MonthlyChestStatus[dateString] += 1;
        }
        else
        {
            MonthlyChestStatus.Add(dateString, 1);
        }

        LastWinDate = DateTime.MinValue;
    }


    public override void OnInit()
    {
        CalendarChallengeModel.Instance.Init();
    }

    public string GetMonthString(int month, bool isShort)
    {
        switch (month)
        {
            case 1:
                return isShort ? "Jan" : "January";
            case 2:
                return isShort ? "Feb" : "February";
            case 3:
                return isShort ? "Mar" : "March";
            case 4:
                return isShort ? "Apr" : "April";
            case 5:
                return "May";
            case 6:
                return isShort ? "Jun" : "June";
            case 7:
                return isShort ? "Jul" : "July";
            case 8:
                return isShort ? "Aug" : "August";
            case 9:
                return isShort ? "Sept" : "September";
            case 10:
                return isShort ? "Oct" : "October";
            case 11:
                return isShort ? "Nov" : "November";
            case 12:
                return isShort ? "Dec" : "December";
        }

        return string.Empty;
    }

    public override void OnReset()
    {
    }

    public override void ConfirmTargetCollection(TaskTargetCollection collection)
    {
    }

    public override TaskTarget GetTaskTarget()
    {
        return TaskTarget.None;
    }

    public static CultureInfo GetCultureInfoFromLanguage(Language language)
    {
        switch (language)
        {
            case Language.Unspecified:
                return CultureInfo.InvariantCulture;
            case Language.Afrikaans:
                return CultureInfo.GetCultureInfo("af-ZA");
            case Language.Albanian:
                return CultureInfo.GetCultureInfo("sq-AL");
            case Language.Arabic:
                return CultureInfo.GetCultureInfo("ar-SA");
            case Language.Basque:
                return CultureInfo.GetCultureInfo("eu-ES");
            case Language.Belarusian:
                return CultureInfo.GetCultureInfo("be-BY");
            case Language.Bulgarian:
                return CultureInfo.GetCultureInfo("bg-BG");
            case Language.Catalan:
                return CultureInfo.GetCultureInfo("ca-ES");
            case Language.ChineseSimplified:
                return CultureInfo.GetCultureInfo("zh-CN");
            case Language.ChineseTraditional:
                return CultureInfo.GetCultureInfo("zh-TW");
            case Language.Croatian:
                return CultureInfo.GetCultureInfo("hr-HR");
            case Language.Czech:
                return CultureInfo.GetCultureInfo("cs-CZ");
            case Language.Danish:
                return CultureInfo.GetCultureInfo("da-DK");
            case Language.Dutch:
                return CultureInfo.GetCultureInfo("nl-NL");
            case Language.English:
                return CultureInfo.GetCultureInfo("en-US");
            case Language.Estonian:
                return CultureInfo.GetCultureInfo("et-EE");
            case Language.Faroese:
                return CultureInfo.GetCultureInfo("fo-FO");
            case Language.Finnish:
                return CultureInfo.GetCultureInfo("fi-FI");
            case Language.French:
                return CultureInfo.GetCultureInfo("fr-FR");
            case Language.Georgian:
                return CultureInfo.GetCultureInfo("ka-GE");
            case Language.German:
                return CultureInfo.GetCultureInfo("de-DE");
            case Language.Greek:
                return CultureInfo.GetCultureInfo("el-GR");
            case Language.Hebrew:
                return CultureInfo.GetCultureInfo("he-IL");
            case Language.Hungarian:
                return CultureInfo.GetCultureInfo("hu-HU");
            case Language.Icelandic:
                return CultureInfo.GetCultureInfo("is-IS");
            case Language.Indonesian:
                return CultureInfo.GetCultureInfo("id-ID");
            case Language.Italian:
                return CultureInfo.GetCultureInfo("it-IT");
            case Language.Japanese:
                return CultureInfo.GetCultureInfo("ja-JP");
            case Language.Korean:
                return CultureInfo.GetCultureInfo("ko-KR");
            case Language.Latvian:
                return CultureInfo.GetCultureInfo("lv-LV");
            case Language.Lithuanian:
                return CultureInfo.GetCultureInfo("lt-LT");
            case Language.Macedonian:
                return CultureInfo.GetCultureInfo("mk-MK");
            case Language.Malayalam:
                return CultureInfo.GetCultureInfo("ml-IN");
            case Language.Norwegian:
                return CultureInfo.GetCultureInfo("nb-NO");
            case Language.Persian:
                return CultureInfo.GetCultureInfo("fa-IR");
            case Language.Polish:
                return CultureInfo.GetCultureInfo("pl-PL");
            case Language.PortugueseBrazil:
                return CultureInfo.GetCultureInfo("pt-BR");
            case Language.PortuguesePortugal:
                return CultureInfo.GetCultureInfo("pt-PT");
            case Language.Romanian:
                return CultureInfo.GetCultureInfo("ro-RO");
            case Language.Russian:
                return CultureInfo.GetCultureInfo("ru-RU");
            case Language.SerboCroatian:
                return CultureInfo.GetCultureInfo("sh-HR");
            case Language.SerbianCyrillic:
                return CultureInfo.GetCultureInfo("sr-Cyrl-RS");
            case Language.SerbianLatin:
                return CultureInfo.GetCultureInfo("sr-Latn-RS");
            case Language.Slovak:
                return CultureInfo.GetCultureInfo("sk-SK");
            case Language.Slovenian:
                return CultureInfo.GetCultureInfo("sl-SI");
            case Language.Spanish:
                return CultureInfo.GetCultureInfo("es-ES");
            case Language.Swedish:
                return CultureInfo.GetCultureInfo("sv-SE");
            case Language.Thai:
                return CultureInfo.GetCultureInfo("th-TH");
            case Language.Turkish:
                return CultureInfo.GetCultureInfo("tr-TR");
            case Language.Ukrainian:
                return CultureInfo.GetCultureInfo("uk-UA");
            case Language.Vietnamese:
                return CultureInfo.GetCultureInfo("vi-VN");
            case Language.Hindi:
                return CultureInfo.GetCultureInfo("hi-IN");
            default:
                return CultureInfo.InvariantCulture;
        }
    }
}