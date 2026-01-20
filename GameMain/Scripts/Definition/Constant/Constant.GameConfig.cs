using System;

public static partial class Constant
{
    /// <summary>
    /// 游戏配置
    /// </summary>
    public static class GameConfig
    {
        //最大关卡(levelmodel里TotalLevelNum也要改)
        public const int MaxLevel = 1900;

        //最大装修区域
        internal const int MaxDecorationArea = 48;
        //开始需要下载的装修区域
        internal const int StartNeedDownloadArea = 4;

        // 恢复一条生命的时间（秒）
        internal const int RecoverLifeInterval = 1800;
        
        internal const int AdsSpinLimitTime = 100;
        internal const int FreeSpinLimitTime = 0;
        internal const int CoinSpinLimitTime = 1000000;

        internal const string DefaultDateTimeFormet = "yyyy-MM-dd HH:mm:ss";
        internal static DateTime DateTimeMin = new DateTime(2000, 1, 1);

        #region Unlock
        
        internal const int UnlockChangeBgLevel = 5;
        internal const int UnlockNormalTurntableLevel = 8;
        internal const int UnlockCalendarChallengeLevel = 68;
        internal const int UnlockDailyWatchAdsLevel = 9;
        internal const int UnlockRemoveAdsButtonLevel = 11;
        internal const int UnlockPersonRankButtonLevel = 21;
        internal const int UnlockPersonRankGameLevel = 25;
        internal const int UnlockPackLevel = 30;
        internal const int UnlockClimbBeanstalkEventLevel = 55;
        internal const int UnlockGameAdsPropLevel = 24;
        internal const int UnlockLoginGiftLevel = 18;
        internal const int UnlockMagnifierBoostLevel = 7;
        internal const int UnlockAddOneStepBoostLevel = 10;
        internal const int UnlockGameAddOneStepBoostLevel = 1;
        internal const int UnlockFireworkBoost = 27;
        internal const int UnlockAdBoostLevel = 33;
        internal const int UnlockTilePassLevel = 32;
        internal const int UnlockGoldCollectionButtonLevel = 36;
        internal const int UnlockGoldCollectionLevel = 41;
        internal const int UnlockPreviewBalloonRiseLevel = 43;
        internal const int UnlockBalloonRiseLevel = 48;
        internal const int UnlockPreviewGlacierQuestLevel = 43;
        internal const int UnlockGlacierQuestLevel = 48;
        // 每天获取冰川副本大奖的最大次数
        internal const int GlacierQuestOpenCountMax = 3;
        // 无尽模式入口展示等级
        internal const int UnlockKitchenButtonLevel = 12;
        // 无尽模式活动解锁等级
        internal const int UnlockKitchenLevel = 35;
        // 感恩节餐厅活动解锁等级
        internal const int UnlockHarvestKitchenLevel = 21;
        //周末礼包
        internal const int UnlockWeekendPack = 49;
        //青蛙
        internal const int UnlockFrogJumpLevel = 12;
        internal const int UnlockCardSetLevel = 30;
        #endregion

        #region Element

        internal const int UnlockElementIceLevel = 11;
        internal const int UnlockElementGlueLevel = 21;
        internal const int UnlockElementFireworksLevel = 100000;
        internal const int UnlockElementCurtainLevel = 1000000;

        #endregion

        internal const int CalendarBgIndex = 900001;

    }
}
