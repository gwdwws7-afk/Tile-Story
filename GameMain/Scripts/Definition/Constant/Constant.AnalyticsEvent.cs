

public static partial class Constant
{
    /// <summary>
    /// firebase分析事件
    /// </summary>
    public static class AnalyticsEvent
    {
        //Level
        public const string Level_Start = "Level_Start";
        public const string Level_Setting = "Level_Setting";
        public const string Level_Restart = "Level_Restart";
        public const string Level_Home = "Level_Home";
        public const string Level_Setting_Restart = "Level_Setting_Restart";
        public const string Level_Setting_Home = "Level_Setting_Home";
        public const string Level_Win = "Level_Win";
        public const string Level_Fail = "Level_Fail";
        public const string Level_AD_Continue = "Level_AD_Continue";
        public const string Level_Buy_Continue = "Level_Buy_Continue";
        public const string Level_AD_Continue_Sucess = "Level_AD_Continue_Sucess";
        public const string Level_Bonus_Reward_Show = "Level_Bonus_Reward_Show";
        public const string Level_Bonus_Reward_Click_AD = "Level_Bonus_Reward_Click_AD";
        public const string Level_BG_Use = "Level_BG_Use";
        public const string Level_BG_Collect = "Level_BG_Collect";
        public const string Theme_Background_Purchase = "Theme_Background_Purchase";

        public const string Level_Prop_Use = "Level_Prop_Use";
        public const string Level_Prop_Purchase = "Level_Prop_Purchase";
        public const string Level_Prop_Purchase_Fail = "Level_Prop_Purchase_Fail";

        public const string Rate_Menu_Show = "Rate_Menu_Show";
        public const string Rate_Menu_Close = "Rate_Menu_Close";
        public const string Rate_Stars = "Rate_Stars";

        public const string Edit_Avatar_Count = "Edit_Avatar_Count";
        public const string Level_Pass_First_Time = "Level_Pass_First_Time";

        //Purchase
        public const string In_App_Purchase = "In_App_Purchase";
        public const string Buy_Purchase_Success = "Buy_Purchase_Success";
        public const string Purchase_Level = "Purchase_Level";
        public const string ShopMenuClose_Coin_800_900="ShopMenuClose_Coin_800_900";

        //Ads
        public const string Reward_Ads_Complete = "Reward_Ads_Complete";
        public const string Reward_Ads_Show = "Reward_Ads_Show";
        public const string Show_Interstitial = "Show_Interstitial";
        public const string Show_Native = "Show_Native";
        public const string Ad_Time_Out = "Ad_Time_Out";
        public const string Daliy_Ad_Coin_Click = "Shop_Ad_Coin_Click";
        public const string Daliy_Ad_Coin_Sucess = "Shop_Ad_Coin_Sucess";
        public const string Ads_For_Props = "Ads_For_Props";
        public const string should_show_Int = "should_show_Int";
        //DaliyGift
        public const string Daily_Gift_Claim = "Daily_Gift_Claim";
        public const string Daily_Gift_Ad_Double = "Daily_Gift_Ad_Double";
        //Turntable
        public const string Ads_For_Spin = "Ads_For_Spin";
        public const string Coin_For_Spin = "Coin_For_Spin";
        public const string AdNum_For_Spin = "AdNum_For_Spin";
        public const string CoinNum_For_Spin = "CoinNum_For_Spin";
        public const string Ads_For_Spin_Reward = "Ads_For_Spin_Reward";
        public const string Coin_For_Spin_Reward = "Coin_For_Spin_Reward";
        public const string Coin_For_LuxurySpin = "Coin_For_LuxurySpin";
        public const string LuckSpin_FiveTime_Reward = "LuckSpin_FiveTime_Reward";

        //Task
        public const string BombTask_NeedLevel = "BombTask_NeedLevel";
        public const string BubbleTask_NeedLevel = "BubbleTask_NeedLevel";
        public const string BombTask_Completed = "BombTask_Completed";
        public const string BubbleTask_Completed = "BubbleTask_Completed";
        public const string DailyTask_Completed = "DailyTask_Completed";
        public const string DailyTask_Click = "DailyTask_Click";
        public const string DailyTask_Start = "DailyTask_Start";
        public const string DailyTask_Play = "DailyTask_Play";

        //RoyalPass
        public const string RoyalPass_Completed = "RoyalPass_Completed";
        public const string RoyalPass_Purchase = "RoyalPass_Purchase";
        public const string RoyalPass_Guide_Purchase = "RoyalPass_Guide_Purchase";
        public const string RoyalPass_Way = "RoyalPass_Way";

        //Error
        public const string LoadAsset_CompleteError = "LoadAsset_CompleteError";
        public const string DateTimeConvertError = "DateTimeConvertError";

        //Team
        public const string Team_JoinClick = "Team_JoinClick";
        public const string Team_JoinClickSituation = "Team_JoinClickSituation";
        public const string Team_Join_login = "Team_Join_login";
        public const string Team_Create = "Team_Create";
        public const string Team_CreateCoinClick = "Team_CreateCoinClick";
        public const string Team_JoinTeam = "Team_JoinTeam";

        //Pass
        public const string Help_Passlevel = "Help_Passlevel";
        public const string HelpNum_Passlevel = "HelpNum_Passlevel";

        //Notification
        public const string Notification_On = "Notification_On";
        public const string Notification_Open = "Notification_Open";
        public const string Enter_Game_By_EightMorning_Notification = "Enter_Game_By_EightMorning_Notification";
        public const string Enter_Game_By_SevenEvening_Notification = "Enter_Game_By_SevenEvening_Notification";
        public const string Enter_Game_By_Notification_NoMessage = "Enter_Game_By_Notification_NoMessage";

        //CoinsBank
        public const string CoinBank_Purchase = "CoinBank_Purchase";

        //Entry Gift Pack
        public const string HardLevel_PackShow = "HardLevel_PackShow";
        public const string HardLevel_PackPurchase = "HardLevel_PackPurchase";
        public const string HeroicLevel_PackShow = "HeroicLevel_PackShow";
        public const string HeroicLevel_PackPurchase = "HeroicLevel_PackPurchase";

        //Boost
        public const string Recommend_Boost = "Recommend_Boost";
        public const string Ads_For_Boost = "Ads_For_Boost";

        //Coin
        public const string Coin_Get = "Coin_Get";
        public const string Coin_Spend = "Coin_Spend";
        public const string Level_Tools_Get = "Level_Tools_Get";
        public const string Coin_Not_Enough = "Coin_Not_Enough";
        public const string Coin_Not_Enough_FirstTime = "Coin_Not_Enough_FirstTime";

        //TeamRank
        public const string TeamBattle_Join_Popup = "TeamBattle_Join_Popup";
        public const string TeamBattle_Join_Team = "TeamBattle_Join_Team";
        public const string TeamBattle_GetShields = "TeamBattle_GetShields";

        //Decoration
        public const string Decorate_Click = "Decorate_Click";
        public const string Decoration_Stage = "Decoration_Stage";
        public const string Decoration_Reward = "Decoration_Reward";
        public const string Decoration_Complete_ = "Decoration_Complete_";
        
        //Ads
        public const string Rewarded_Ads_Paid = "RV_Ads_Income_Memosize";
        public const string Interstitial_Ads_Paid = "Int_Ads_Income_Memosize";
        public const string Rewarded_KV_Ads_Paid = "Rewarded_KV_Ads_Paid";
        public const string Interstitial_KV_Ads_Paid = "Interstitial_KV_Ads_Paid";
        
        //Turntable
        public const string Spin_Play_ByAds = "Spin_Play_ByAds";
        public const string Spin_Play_ByCoins_Normal = "Spin_Play_ByCoins_Normal";
        public const string Normal_Spin_Progress_Rewards = "Normal_Spin_Progress_Rewards";
        public const string Spin_Double = "Spin_Double";
        public const string Spin_Jackpot = "Spin_Jackpot";

        //tile theme
        public const string Theme_Tile_Buy = "Theme_Tile_Buy";
        public const string Theme_Tile_Use = "Theme_Tile_Use";
        
        //PersonRank
        public const string PersonRank_Start = "PersonRank_Start";
        public const string PersonRank_IconClick = "PersonRank_IconClick";
        public const string PersonRank_IconPreview = "PersonRank_IconPreview";
        public const string PersonRank_NamingGuide = "PersonRank_NamingGuide";
        public const string PersonRank_Reward_Claim = "PersonRank_Reward_Claim";
        public const string PersonRank_TrophiesNumbuer_Get = "PersonRank_TrophiesNumbuer_Get";

        //Rate
        public const string Rate_FiveStar = "Rate_FiveStar";

        //ClimbBeanstalk
        public const string WinStreak_Open = "WinStreak_Open";
        public const string WinStreak_StageReward = "WinStreak_StageReward";
        
        //CalendarChallenge
        public const string DailyChallenge_First_Open = "DailyChallenge_First_Open";
        public const string DailyChallenge_Spend_Coin_Start = "DailyChallenge_Spend_Coin_Start";
        public const string DailyChallenge_Watch_AD_Start = "DailyChallenge_Watch_AD_Start";
        public const string DailyChallenge_Spend_Coin_More_Time = "DailyChallenge_Spend_Coin_More_Time";
        public const string DailyChallenge_Watch_AD_More_Time = "DailyChallenge_Watch_AD_More_Time";
        public const string DailyChallenge_Open_Chest = "DailyChallenge_Open_Chest";
        public const string DailyChallenge_Start_One_Level_Day = "DailyChallenge_Start_One_Level_Day";

        public const string DailyChallenge_Level_Start = "DailyChallenge_Level_Start";
        public const string DailyChallenge_Level_Fail = "DailyChallenge_Level_Fail";
        public const string DailyChallenge_Level_Win = "DailyChallenge_Level_Win";
        public const string DailyChallenge_Level_Prop_Use = "DailyChallenge_Level_Prop_Use";
        public const string DailyChallenge_Level_Prop_Purchase = "DailyChallenge_Level_Prop_Purchase";

        //Player Item data
        public const string Player_Item_Remain = "Player_Item_Remain";

        //Life
        public const string Lives_None = "Lives_None";
        public const string Lives_Buy = "Lives_Buy";

        //Login Gift
        public const string Sign_In_Day_GetReward = "Sign_In_Day_GetReward";
        public const string Sign_In_WatchAd_DoubleReward = "Sign_In_WatchAd_DoubleReward";

        //Boost
        public const string Level_Booster_Use = "Level_Booster_Use";
        public const string Level_Booster_Purchase = "Level_Booster_Purchase";
        public const string Level_Booster_Purchase_Fail = "Level_Booster_Purchase_Fail";

        //Gold Collection
        public const string Lucky_Collect_Task = "Lucky_Collect_Task";
        public const string Lucky_Collect_Activity_Open = "Lucky_Collect_Activity_Open";
        
        //AdLTV_OneDay_Top10Percent
        public const string AdLTV_OneDay_Top10Percent = "AdLTV_OneDay_Top10Percent";
        public const string AdLTV_OneDay_Top20Percent = "AdLTV_OneDay_Top20Percent";
        public const string AdLTV_OneDay_Top30Percent = "AdLTV_OneDay_Top30Percent";
        public const string AdLTV_OneDay_Top40Percent = "AdLTV_OneDay_Top40Percent";
        public const string AdLTV_OneDay_Top50Percent = "AdLTV_OneDay_Top50Percent";
        
        //GlacierDungeon
        public const string LavaQuest_Challenge_Start = "LavaQuest_Challenge_Start";
        public const string LavaQuest_First_Open = "LavaQuest_First_Open";
        public const string LavaQuest_Challenge_Success = "LavaQuest_Challenge_Success";
        public const string LavaQuest_Challenge_Fail = "LavaQuest_Challenge_Fail";

        //Objective
        public const string Objective_AllTime_Claim = "Objective_AllTime_Claim";
        public const string Objective_AllTime_Claim_Banner = "Objective_AllTime_Claim_Banner";
        public const string Objective_Daily_Claim = "Objective_Daily_Claim";
        public const string Objective_Daily_Claim_Banner = "Objective_Daily_Claim_Banner";
        
        // Kitchen
        public const string Kitchen_Match_Challenge_Choose_Continue = "Kitchen_Match_Challenge_Choose_Continue";
        public const string Kitchen_Match_Challenge_Get_Level_Coin = "Kitchen_Match_Challenge_Get_Level_Coin";
        public const string Kitchen_Match_Challenge_Next_Level = "Kitchen_Match_Challenge_Next_Level";
        public const string Kitchen_Match_Challenge_Retry = "Kitchen_Match_Challenge_Retry";
        public const string Kitchen_Match_Challenge_Retry_ad = "Kitchen_Match_Challenge_Retry_ad";
        public const string Kitchen_Match_Challenge_Show_Continue = "Kitchen_Match_Challenge_Show_Continue";
        public const string Kitchen_Match_Challenge_Start = "Kitchen_Match_Challenge_Start";
        public const string Kitchen_Match_Pack_Buy = "Kitchen_Match_Pack_Buy";
        public const string Kitchen_Match_Task_Stage = "Kitchen_Match_Task_Stage";
        
        //pk
        public const string PkMatch_EmptyMatch = "PkMatch_EmptyMatch";

        //Love Gift Merge
        public const string Merge_Stage_Task_Complete = "Merge_Stage_Task_Complete";
        public const string Merge_Get_Final_Box = "Merge_Get_Final_Box";
        
        //FrogJump
        public const string Frog_ad_Show = "Frog_ad_Show";
        public const string Frog_ad_AD = "Frog_ad_AD";
        public const string Frog_ad_Buy = "Frog_ad_Buy";

        //DigMerge
        public const string DigTreasure_Merge_Layer_Unlock = "DigTreasure_Merge_Layer_Unlock";
        public const string DigTreasure_Merge_Task_Unlock = "DigTreasure_Merge_Task_Unlock";
        public const string DigTreasure_Merge_Get_Coins = "DigTreasure_Merge_Get_Coins";
        public const string DigTreasure_Merge_Get_Stars = "DigTreasure_Merge_Get_Stars";
        public const string DigTreasure_Merge_Endless_Treasure_Claim = "DigTreasure_Merge_Endless_Treasure_Claim";
        public const string Merge_Open_Activity_Interface = "Merge_Open_Activity_Interface";

        //FUU率
        public const string Level_Fail_Fuu = "Level_Fail_Fuu";
        public const string Level_Fail_Fuu_Half = "Level_Fail_Fuu_50Percent";

        //宝箱打开动画播完点击领取宝箱
        public const string LevelRewardClaim = "LevelRewardClaim";
        
        public const string IAPrevenue = "IAPrevenue";
    }
}
