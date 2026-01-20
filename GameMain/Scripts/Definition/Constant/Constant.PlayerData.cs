

public static partial class Constant
{
    /// <summary>
    /// 玩家数据配置
    /// </summary>
    public static class PlayerData
    {
        //cur level data
        public const string CurLevelRecordData = "PlayerData.Level.RecordData";

        //Config
        public const string Language = "PlayerData.Language";
        public const string NotificationForbidden = "PlayerData.NotificationForbidden";

        //Record
        public const string NewPlayer = "PlayerData.NewPlayer";
        public const string NowLevel = "PlayerData.NowLevel";
        public const string LastLoginDate = "PlayerData.LastLoginDate";
        public const string FirstLoginDate = "PlayerData.FirstLoginDate";
        public const string ContinuousLoginDay = "PlayerData.ContinuousLoginDay";
        public const string LastDayNowLevel = "PlayerData.LastDayNowLevel";
        public const string LastDayTurnbackTime = "PlayerData.LastDayTurnbackTime";
        public const string GetUnlockFreeBoost = "PlayerData.GetUnlockFreeBoost";
        public const string BehaviorScore = "PlayerData.BehaviorScore";
        public const string TodayShowedFreeMoveLimit = "PlayerData.TodayShowedFreeMoveLimit";
        public const string TodayShowedAdsBoostLimit = "PlayerData.TodayShowedAdsBoostLimit";
        public const string TodayShowedDailyQuestFinishReward = "PlayerData.TodayShowedDailyQuestFinishReward";
        public const string UseRecommendedBoostTime = "PlayerData.UseRecommendedBoostTime";

        //Rate
        public const string ClickedRateButton = "PlayerData.ClickedRateButton";

        public const string StarNum = "PlayerData.StarNum";
        public const string DiamondNum = "PlayerData.DiamondNum";
        public const string RealStarNum = "PlayerData.RealStarNum";
        public const string InfiniteLifeEndTime = "PlayerData.InfiniteLifeEndTime";
        public const string PrestoreInfiniteLifeTime = "PlayerData.PrestoreInfiniteLifeTime";
        public const string StartRecoverLifeTime = "PlayerData.StartRecoverLifeTime";
        public const string TransverseEliminationPropNum = "PlayerData.TransverseEliminationPropNum";
        public const string BombPropNum = "PlayerData.BombPropNum";
        public const string WildCardPropNum = "PlayerData.WildCardPropNum";
        public const string StraightPunchPropNum = "PlayerData.StraightPunchPropNum";
        public const string Boost1Num = "PlayerData.Boost1Num";
        public const string Boost2Num = "PlayerData.Boost2Num";
        public const string Boost3Num = "PlayerData.Boost3Num";
        public const string InfiniteBoostEndTimePrefix = "PlayerData.InfiniteBoostEndTime_";
        public const string PrestoreInfiniteBoostEndTimePrefix = "PlayerData.PrestoreInfiniteBoostEndTime_";
        public const string TurnbackPropNum = "PlayerData.TurnbackPropNum";
        public const string AutoPlusFiveStepBoostNum = "PlayerData.AutoPlusFiveStepBoostNum";
        public const string PrestoreDoubleRewardEndTime = "PlayerData.PrestoreDoubleRewardEndTime";
        public const string DoubleRewardEndTime = "PlayerData.DoubleRewardEndTime";

        //GrandPrizeTask
        public const string CurrentRewardTaskId = "PlayerData.CurrentRewardTaskId";
        public const string CurrentRewardTaskTargetNum = "PlayerData.CurrentRewardTaskTargetNum";
        public const string GrandPrizeTaskEndTime = "PlayerData.GrandPrizeTaskEndTime";

        //DailyTask
        public const string LevelFailTime = "PlayerData.LevelFailTime";
        public const string CurrentDailyTaskTargetNum = "PlayerData.CurrentDailyTaskTargetNum";
        public const string CurrentDailyTaskStage = "PlayerData.CurrentDailyTaskStage";
        public const string AutoShowedDailyTaskMenu = "PlayerData.AutoShowedDailyTaskMenu";
        public const string DailyTaskStart = "PlayerData.DailyTaskStart";
        public const string DailyFreePropNum = "PlayerData.DailyFreePropNum";

        //Ads
        public const string ShowInterstitialAdTime = "PlayerData.ShowInterstitialAdTime";

        //Package
        public const string NewPlayerPackageEndDate = "PlayerData.NewPlayerPackageEndDate";
        public const string NewPlayerPackageAutoPopup = "PlayerData.NewPlayerPackageAutoPopup";
        public const string GiftPackAutoPopup = "PlayerData.GiftPackAutoPopup";

        //WinningStreak
        public const string WinningStreak = "PlayerData.WinningStreak";
        public const string ShowedWinningStreakStartMenu = "PlayerData.ShowedWinningStreakStartMenu";
        public const string WinningStreakGameUsing = "PlayerData.WinningStreakGameUsing";
        public const string WinningStreakEndTime = "PlayerData.WinningStreakEndTime";

        //DataSave
        public const string LastSavedPlayerData = "PlayerData.LastSavedPlayerData";
        public const string GoingToLogin = "PlayerData.GoingToLogin";
        public const string LastSaveSucceed = "PlayerData.LastSaveSucceed";
        public const string HasUIDInServer = "PlayerData.HasUIDInServer";
        public const string LoginType = "PlayerData.LoginType";
        public const string UserUID = "PlayerData.UserUID";
        public const string UserName = "PlayerData.UserName";
        public const string TmpUserName = "PlayerData.TmpUserName";
        public const string HasChangedName = "PlayerData.HasChangedName";
        public const string HasShownSettingLoginGuide = "PlayerData.HasShownSettingLoginGuide";
        public const string HasShownMapLoginGuide = "PlayerData.HasShownMapLoginGuide";
        public const string LoginFromTeam = "PlayerData.LoginFromTeam";
        public const string PullNowLevelSuccess = "PlayerData.PullNowLevelSuccess";
        public const string IsUsingFirestoreDataSave = "PlayerData.IsUsingFirestoreDataSave";

        //Team
        public const string JoinInTimestamp = "PlayerData.JoinInTimestamp";
        public const string MyTeamID = "PlayerData.MyTeamID";
        public const string TempTeamID = "PlayerData.TempTeamID";
        public const string MyTeamName = "PlayerData.MyTeamName";
        public const string MyTeamIconID = "PlayerData.MyTeamIconID";
        public const string LastUpdateLevel = "PlayerData.LastUpdateLevel";
        public const string IsLeader = "PlayerData.IsLeader";
        public const string IsCoLeader = "PlayerData.IsCoLeader";
        public const string MyTeamInfo = "PlayerData.MyTeamInfo";
        public const string MessageIDList = "PlayerData.MessageIDList";
        public const string HasShownTeamGuide = "PlayerData.HasShownTeamGuide";
        public const string HasShownUnlockTeamMenu = "PlayerData.HasShownUnlockTeamMenu";
        public const string HasShownUnlockMenuTeamGuide = "PlayerData.HasShownUnlockMenuTeamGuide";
        public const string RequestEndTime = "PlayerData.RequestEndTime";
        public const string TeamNetworkConnection = "PlayerData.TeamNetworkConnection";
        public const string FirstJoinATeam = "PlayerData.FirstJoinATeam";
        public const string TeamInfoListJson = "PlayerData.TeamInfoListJson";
        public const string MergeTeamListDate = "PlayerData.MergeTeamListDate";
        public const string ShowRankPanelAnimIng = "PlayerData.ShowRankPanelAnimIng";
        public const string IsMute = "PlayerData.IsMute";
        public const string DeletedMessageIDList = "PlayerData.DeletedMessageIDList";
        //Pass
        public const string HelpPassTriggerTime = "PlayerData.HelpPassTriggerTime";
        public const string HelpCurrentLevelTriggerTime = "PlayerData.HelpCurrentLevelTriggerTime";

        //FreeLife
        public const string FreeLifeStore = "PlayerData.HasShownMapLoginGuide";
        public const char SeparateChar = '⠁';
        public const char GoldenPassProviderChar = '⠂';

        //Guide
        public const string ToShowTurnBackGuide = "PlayerData.ToShowTurnBackGuide";
        public const string HasShownTurnBackGuide = "PlayerData.HasShownTurnBackGuide";

        //Robot
        public const string LocalRobots = "PlayerData.LocalRobots";

        //RushRank
        public const string RushRankEndTime = "PlayerData.RushRankEndTime";
        public const string RushRankStartTime = "PlayerData.RushRankStartTime";
        public const string RushRankLastLevelSuccess = "PlayerData.RushRankLastLevelSuccess";
        public const string RushRankInited = "PlayerData.RushRankInited";
        public const string RushRankFinished = "PlayerData.RushRankFinished";
        public const string RushRankStart = "PlayerData.RushRankStart";
        public const string RushRankLastLevelScore = "PlayerData.RushRankLastLevelScore";
        public const string RushRankSetName = "PlayerData.RushRankSetName";

        //Notification
        public const string NotificationScheduledWeek = "PlayerData.NotificationScheduledWeek";

        //Network
        public const string NetworkTimeDefaultUrl = "PlayerData.NetworkTimeDefaultUrl";
        public const string NetworkTimeChangeEnabled = "PlayerData.NetworkTimeChangeEnabled";
        public const string NetworkCurrentTime = "PlayerData.NetworkCurrentTime";

        //SaveBox
        public const string SaveBoxActivityInited = "PlayerData.SaveBoxActivityInited";
        public const string SaveBoxActivityFinished = "PlayerData.SaveBoxActivityFinished";
        public const string SaveBoxIsFull = "PlayerData.SaveBoxIsFull";
        public const string SaveBoxCurrentLevel = "PlayerData.SaveBoxCurrentLevel";
        public const string SaveBoxEndTime = "PlayerData.SaveBoxEndTime";
        public const string SaveBoxCurrentCoinNum = "PlayerData.SaveBoxCurrentCoinNum";
        public const string SaveBoxDailyShowPopup = "PlayerData.SaveBoxDailyShowPopup";
        public const string SaveBoxCountDownTime = "PlayerData.SaveBoxCountDownTime";

        //DailyCoin
        public const string ShowedDailyCoinUnlockMenu = "PlayerData.ShowedDailyCoinUnlockMenu";

        //Entry Gift Pack
        public const string EntryGiftPackShowedLevel = "PlayerData.EntryGiftPackShowedLevel";

        //TeamRank
        public const string TeamRankSerialNumber = "PlayerData.TeamRankSerialNumber";
        public const string TeamRankInited = "PlayerData.TeamRankInited";
        public const string TeamRankNeedToSendRewards = "PlayerData.TeamRankNeedToSendRewards";
        public const string TeamRankLastScore = "PlayerData.TeamRankLastScore";
        public const string TeamRankLastLevelSuccess = "PlayerData.TeamRankLastLevelSuccess";
        public const string TeamRankLeaveTeamDuringBattle = "PlayerData.TeamRankLeaveTeamDuringBattle";
        public const string NetworkTimeStatus = "PlayerData.NetworkTimeStatus";
        public const string TeamRankFirstShownTerm = "PlayerData.TeamRankFirstShownTerm";
        public const string TeamRankJoinTeamSuccess = "PlayerData.TeamRankJoinTeamSuccess";
        public const string TeamRankPlayedInThisTerm = "PlayerData.TeamRankPlayedInThisTerm";

        //TeamRankPackage
        public const string TeamRankBasicPackagePurchased = "PlayerData.TeamRankBasicPackagePurchased";
        public const string TeamRankAdvancedPackagePurchased = "PlayerData.TeamRankAdvancedPackagePurchased";
        public const string TeamRankPremiumPackagePurchased = "PlayerData.TeamRankPremiumPackagePurchased";
        public const string TeamRankPackageTerm = "PlayerData.TeamRankPackageTerm";

        //PeekRank
        public const string PeekRankSerialNumber = "PlayerData.PeekRankSerialNumber";
        public const string PeekRankInited = "PlayerData.PeekRankInited";
        public const string PeekRankFinished = "PlayerData.PeekRankFinished";
        public const string PeekRankLastScore = "PlayerData.PeekRankLastScore";
        public const string PeekRankScore = "PlayerData.PeekRankScore";
        public const string PeekRankLastTerm = "PlayerData.PeekRankLastTerm";
        public const string PeekRankNeedToSendRewards = "PlayerData.PeekRankNeedToSendRewards";
        public const string PeekRankLastLevelIsSuccessful = "PlayerData.PeekRankLastLevelIsSuccessful";
        public const string PeekRankPlayedGames = "PlayerData.PeekRankPlayedGames";


        public const string ResetAllUnGetRewards = "PlayerData.ResetAllUnGetRewards";
        public const string DuringDoubleRewardTime = "PlayerData.DuringDoubleRewardTime";

        //MapLoopBG
        public const string LastWinLevelNum = "PlayerData.LastWinLevelNum";
    }
}
