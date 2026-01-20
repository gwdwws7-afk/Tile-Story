

using UnityEngine.UI;

public static partial class Constant
{
    /// <summary>
    /// firebase远程配置
    /// </summary>
    public static class RemoteConfig
    {
        public const string Show_Banner_Start_Level = "Show_Banner_Start_Level";
        public const string Show_Interstitial_Start_Level = "Show_Interstitial_Start_Level";
        public const string If_Use_AdmobSDK = "If_Use_AdmobSDK";
        public const string ShowRateLevel = "ShowRateLevel";
        public const string Ads_Timeout = "Ads_Timeout";

        public const string Yandex_Banner_Refresh_Time = "Yandex_Banner_Refresh_Time";

        public const string Use_Max_Load_Fail_Count = "Use_Max_Load_Fail_Count";
        public const string Round_Delay_Load_Ads_Time = "Round_Delay_Load_Ads_Time";
        public const string LoadAds_DelayTime = "LoadAds_DelayTime";

        public const string Refresh_Banner_FrameCount = "Refresh_Banner_FrameCount";
        public const string Use_Level_Type_Index = "Use_Level_Type_Index";
        public const string Is_Force_Open_Rate_URL = "Is_Force_Open_Rate_URL";

        //rate 
        public const string Can_Open_Rate_MaxCount_ByVersion = "Can_Open_Rate_MaxCount_ByVersion";
        public const string If_Open_Rate_ByLevel = "If_Open_Rate_ByLevel";
        public const string If_Open_Rate_ByUnlockBG = "If_Open_Rate_ByUnlockTheme";
        public const string If_Open_Rate_ByFiveWin = "If_Open_Rate_ByFiveWin";
        
        //level
        public const string If_Use_Record_Level_Data = "If_Use_Record_Level_Data";
        
        public const string SpinAdTimesLimit = "SpinAdTimesLimit";
        
        //Level btn
        public const string If_Only_Once_Show_Ads_Continue_Btn = "If_Only_Once_Show_Ads_Continue_Btn";
        
        public const string If_Low_Device_Load_Banner= "If_Low_Device_Load_Banner";

        public const string Enable_GPRP = "Enable_GPRP";

        public const string If_Use_Tile_Furniture = "If_Use_Tile_Furniture";//两套棋子abtest
        public const string If_Show_Rate_Us_Menu = "If_Show_Rate_Us_Menu";//是否使用新的评价界面
        
        //
        public const string TaiChi_Top50Percent = "TaiChi_Top50Percent";
        public const string TaiChi_Top40Percent = "TaiChi_Top40Percent";
        public const string TaiChi_Top30Percent = "TaiChi_Top30Percent";
        public const string TaiChi_Top20Percent = "TaiChi_Top20Percent";
        public const string TaiChi_Top10Percent = "TaiChi_Top10Percent";

        //控制RV广告加载间隔
        public const string Level_RV_CD = "Level_RV_CD";

        //控制广告三选一
        public const string Is_Show_Level_PropAD = "Is_Show_Level_PropAD";
        
        //
        public const string Is_Open_Fail_Ads_Price_Change = "Is_Open_Fail_Ads_Price_Change";
        //
        public const string Is_Open_Native_Interstitial_Ads = "Is_Open_Native_Interstitial_Ads";
        
        public const string Is_Open_Native_Reward_Ads = "Is_Open_Native_Reward_Ads";

        public const string Is_Interstitial_Change_To_Native = "Is_Interstitial_Change_To_Native";
        
        public const string Is_Reward_Change_To_Native = "Is_Reward_Change_To_Native";

        //胜利广告翻倍，x2和x3需要做实验的开关
        public const string Level_Win_Rewards_RV_Multiple = "Level_Win_Rewards_RV_Multiple";

        public const string RV_Times_Total = "RV_Times_Total";        //RV广告总次数实验，限制所有RV24小时内的累计次数
        public const string RV_Times_Props = "RV_Times_Props";        //RV广告次数实验，限制游戏内三选一广告累计次数
        public const string RV_Times_Continue = "RV_Times_Continue";    //RV广告次数实验，限制游戏内接关广告累计次数
        //
        public const string Remote_Native_Interstitial_Ads_Id = "Remote_Native_Interstitial_Ads_Id";

        public const string Remote_Native_Reward_Ads_Id = "Remote_Native_Reward_Ads_Id";

        public const string Win_Direct_Next_End_Level = "Win_Direct_Next_End_Level";
        
          
        //pk比赛 
        public const string Pk_Game_Each_Time_Lenght = "Pk_Game_Each_Time_Lenght";

        public const string Is_Can_Open_Pk_Game = "Is_Can_Open_Pk_Game";
        
        //是否关闭hint提示
        public const string Is_Turn_Off_Hint = "Is_Turn_Off_Hint";
        
        //背景音乐音量大小
        public const string Bg_Music_Volume = "Bg_Music_Volume";

        //是否开启断网限制
        public const string Enable_Offline_Restrictions = "Enable_Offline_Restrictions";
        
        public const string If_Use_B_Level_Table_Data = "If_Use_B_Level_Table_Data";//是否level表使用b组数据

        public const string Winpanel_Chest_IncreaseRewards = "Winpanel_Chest_IncreaseRewards";//胜利界面进度宝箱数值

        public const string Event_Merge_UnlockLevel = "Event_Merge_UnlockLevel";//配置merge活动开启的关卡ID（达到XX关解锁Merge活动）

        public const string First_Level_Reduce_Type_Num_By_Day = "First_Level_Reduce_Type_Num_By_Day";//每天第一关，默认减少花色数量

        public const string Ads_Init_Level = "Ads_Init_Level";//广告初始化等级

        public const string Is_Use_KV_Ads = "Is_Use_KV_Ads";//是否使用KV

        //特殊机型策略
        public const string Is_Use_Special_Device_Strage = "Is_Use_Special_Device_Strage";//是否使用特殊机型策略，总开关
        //public const string Is_Use_Special_Device_Strage_Forbid_Banner = "Is_Use_Special_Device_Strage_Forbid_Banner";
        public const string Is_Use_Special_Device_Strage_Use_Low_Interstitial = "Is_Use_Special_Device_Strage_Use_Low_Interstitial";
        public const string Is_Use_Special_Device_Strage_Use_Low_RV = "Is_Use_Special_Device_Strage_Use_Low_RV";
        public const string Is_Use_Special_Device_Strage_Turn_Off_Particle_Effect = "Is_Use_Special_Device_Strage_Turn_Off_Particle_Effect";
        public const string Is_Use_Special_Device_Strage_Close_Low_Priority_Pop = "Is_Use_Special_Device_Strage_Close_Low_Priority_Pop";
        public const string Is_Use_Special_Device_Strage_Optimize_Heavy_Work = "Is_Use_Special_Device_Strage_Optimize_Heavy_Work";
        public const string Is_Use_Special_Device_Strage_Use_Native_Ads = "Is_Use_Special_Device_Strage_Use_Native_Ads";

        public const string If_Have_Thumb = "If_Have_Thumb";//是否展示点赞大拇指【默认展示】

        public const string If_Use_Level_Start_FirstTry = "If_Use_Level_Start_FirstTry";

        public const string Is_Use_KVGroup = "Is_Use_KVGroup";
        public const string Remote_Int_KVGroup_Json = "Remote_Int_KVGroup_Json";
        public const string Remote_RV_KVGroup_Json = "Remote_RV_KVGroup_Json";
        public const string Remote_KVGroup_DecayX = "Remote_KVGroup_DecayX";
        public const string Remote_KVGroup_DecayY = "Remote_KVGroup_DecayY";
        
        public const string ItemFunction_Change_Scale = "ItemFunction_Change_Scale";//开启放大关卡块实验，变更道具功能，使得关卡棋盘区域可以放大，且看广告三选一礼包改为倒计时RV

        public const string BG_Change_1to5 = "BG_Change_1to5";//替换前5张背景图片
    }
}
