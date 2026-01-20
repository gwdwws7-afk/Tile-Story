using GameFramework.Event;

public enum CommonEventType
{
    None,
    IsRemovePopupAds,
    WatchAdsToAddThreeBack,
    UseCoinToAddThreeBack,
    ChangBGImageID,
    ChangeTileIconID,

    UseBGSmall,

    ChangeHeadPortrait,
    
    MapToGame,
    GameToMap,
    GameToGame,
    Objective,
    
    DecorationScaleUp,
    DecorationScaleDown,
    PersonRankRuleHide,
    
    ShopMenuClose,
    ShowPersonRankRewardPromptBox,
    ShowPersonRankTextPromptBox,
    PersonRankChanged,
    ClimbBeanstalkInfoChanged,
    BuyTileID,
    
    CalendarChallengeDaySelected,
    RewardAdsShownEvent,
    ShowCalendarChallengeRewardPromptBox,
    
    RefreshUIBySyncData,
	TilePosFill,
    TileMatch,
    EnterTileMatchPanel,
    GameWin,
    ShopBuyGetRewardComplete,
    GetLoginGift,
    LevelTimeLimit,
    PauseLevelTime,
    ContinueLevelTime,
    BuyPackageBannerToContinueLevel,
    FreePlayPropSkill,
    AutoUsePropSkill,

    KitchenLoseContinue,
    KitchenInfoChanged,
    KitchenContinue,
    KitchenEntranceUpdate,
    KitchenBuyPackageComplete,
    
    BoostNumChange,
    
    RefreshPiggyBank,
    
    EndlessOver,
    EndlssGetFreeReward,
    EndlessBalance,
    
    ChristmasBubbleGetRewardRVTime,
    
    PkListenData,
    PkGameOver,
    PkGameStart,
    
    FrogLeafCreated,
    FrogLeafDestroy,
    FrogAdStateChange,

    SetMainBgBlack,
    SetMainBgNormal,
}

public class CommonEventUserData
{
    public CommonEventType Type;
    public object[] UserDatas;

    public CommonEventUserData(CommonEventType type,params object[] datas)
    {
        this.Type = type;
        this.UserDatas = datas;
    }

    public void Clear()
    {
        Type = CommonEventType.None;
        UserDatas = null;
    }
}

public sealed class CommonEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(CommonEventArgs).GetHashCode();

    public CommonEventArgs()
    {
        Clear();
    }

    public override int Id
    {
        get
        {
            return EventId;
        }
    }
    public CommonEventType Type
    {
        get;
        private set;
    }

    /// <summary>
    /// 用户自定义数据
    /// </summary>
    public object[] UserDatas
    {
        get;
        private set;
    }

    public static CommonEventArgs Create(CommonEventType type,params object[] objs)
    {
        CommonEventArgs args = GameFramework.ReferencePool.Acquire<CommonEventArgs>();
        args.Type = type;
        args.UserDatas = objs;
        return args;
    }

    public override void Clear()
    {
        Type = CommonEventType.None;
        UserDatas = null;
    }
}
