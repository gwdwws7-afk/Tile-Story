using System;
using System.Globalization;
using UnityEngine;
using System.Collections.Generic;
using MySelf.Model;
using Random = System.Random;

/// <summary>
/// 玩家数据组件
/// </summary>
public sealed class PlayerDataComponent : GameFrameworkComponent
{
    private Action newDayEvent;

    private Action UpdateDayEvent
    {
        get
        {
            if (newDayEvent == null)
            {
                newDayEvent = NewDayEvent;
            }
            return newDayEvent;
        }
    }

    public void Init()
    {
        CheckNewPlayerState();
        RefreshPlayerBehavior();
        DateModel.Instance.RefreshDate(UpdateDayEvent);
    }

    private void Update()
    {
        DateModel.Instance.RefreshDate(UpdateDayEvent);
    }

    private void NewDayEvent()
    {
        RefreshPlayerBehavior();

        GameManager.Objective.ResetDailyObjectiveProgress();
    }

    #region AB 对照组

    private void CheckNewPlayerState()
    {
        //获取玩家状态
        NewPlayer = NowLevel <= 1;

        if (NewPlayer)
        {
            //如果是新玩家做新玩家处理
            //1、hint,ab组处理
            bool isTurnOffHint = GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Turn_Off_Hint,false);
            TurnOffTips = isTurnOffHint;

            if (SystemInfoManager.DeviceType <= DeviceType.Low)
            {
                GameManager.PlayerData.MusicMuted = true;
                GameManager.PlayerData.AudioMuted = true;
                GameManager.PlayerData.ShakeMuted = true;
                GameManager.Task.AddSaveDataTask();
            }
        } 
        //2、背景音乐音量大小
        int bgMusicVolume = (int)GameManager.Firebase.GetDouble(Constant.RemoteConfig.Bg_Music_Volume,30);
        BgMusicVolume = bgMusicVolume;
        
        //将玩家设置为老玩家状态
        NewPlayer = false;
    }

    #endregion

    #region PlayerLogin
    public string UserID
    {
        get => PlayerLoginModel.Instance.Data.UserID;
        set => PlayerLoginModel.Instance.SetUserID(value);
    }

    public string UserName
    {
        get => PlayerLoginModel.Instance.Data.UserName;
        set => PlayerLoginModel.Instance.SetUserName(value);
    }

    public LoginType LoginSdkName
    {
        get => PlayerLoginModel.Instance.Data.LoginSdkNameType;
        set
        {
            PlayerLoginModel.Instance.Data.LoginSdkNameType = value;
            PlayerLoginModel.Instance.SaveToLocal();
        }
    }

    public void RecordSaveDataTime() => PlayerLoginModel.Instance.RecordSaveDataTime();

    public string GetLastSaveDataTime
    {
        get
        {
            if (string.IsNullOrEmpty(PlayerLoginModel.Instance.Data.RecordLastSaveDataTime))
            {
                return System.DateTime.Now.ToString();
            }
            return PlayerLoginModel.Instance.Data.RecordLastSaveDataTime;
        }
    }

    public bool IsShowLoginGuide
    {
        get => PlayerLoginModel.Instance.Data.IsHaveShowLoginGuide;
        set
        {
            PlayerLoginModel.Instance.Data.IsHaveShowLoginGuide = value;
            PlayerLoginModel.Instance.SaveToLocal();
        }
    }
    
    public bool IsHaveShowSaveDataGuide
    {
        get => PlayerLoginModel.Instance.Data.IsHaveShowSaveDataGuide;
        set
        {
            PlayerLoginModel.Instance.Data.IsHaveShowSaveDataGuide = value;
            PlayerLoginModel.Instance.SaveToLocal();
        }
    }

    public bool IsCanShowGuideByLevel()
    {
        PlayerLoginModel.Instance.Data.NeedLevelShowGuide--;
        PlayerLoginModel.Instance.SaveToLocal();
        return PlayerLoginModel.Instance.Data.NeedLevelShowGuide <= 0;
    }

    #endregion
    
    #region Common
    /// <summary>
    /// 是否新玩家
    /// </summary>
    public bool NewPlayer
    {
        get => CommonModel.Instance.Data.NewPlayer;
        set => CommonModel.Instance.SetNewPlayer(value);
    }

    public bool HasOpenDebug
    {
        get => CommonModel.Instance.Data.HasOpenDebug;
        set => CommonModel.Instance.SetOpenDebug(value);
    }

    public bool MusicMuted
    {
        get => CommonModel.Instance.Data.IsMusicMuted;
        set => CommonModel.Instance.SetMusicMuted(value);
    }

    public bool AudioMuted
    {
        get => CommonModel.Instance.Data.IsAudioMuted;
        set => CommonModel.Instance.SetAudioMuted(value);
    }

    public bool ShakeMuted
    {
        get => CommonModel.Instance.Data.IsShakeMuted;
        set => CommonModel.Instance.SetShakeMuted(value);
    }

    public bool TurnOffTips
    {
        get => CommonModel.Instance.Data.IsTurnOffTips;
        set => CommonModel.Instance.SetTurnOffTips(value);
    }

    public bool KidMode
    {
        get => CommonModel.Instance.Data.IsKidMode;
        set => CommonModel.Instance.SetKidMuted(value);
    }

    public string BGMusicName => "Lonely House LOOP";
    public string HappyBgMusicName => "BGM_Levels";

    public int Music
    {
        get => CommonModel.Instance.Data.Music;
        set => CommonModel.Instance.SetMusicIndex(value);
    }

    public int AccumulatedLoginDays
    {
        get => CommonModel.Instance.Data.AccumulatedLoginDays;
        set => CommonModel.Instance.SetAccumulatedLoginDays(value);
    }

    public string PlayerName
    {
        get
        {
            if (string.IsNullOrEmpty(CommonModel.Instance.Data.PlayerName))
            {
                PlayerName = $"User{UnityEngine.Random.Range(0, 1000):D3}";
            }
            return CommonModel.Instance.Data.PlayerName;
        }

        set => CommonModel.Instance.SetPlayerName(value);
    }

    public void SetPlayerNameByLogin(string displayName)
    {
        if (!CommonModel.Instance.Data.IsSetPlayerName)
        {
            CommonModel.Instance.Data.PlayerName = displayName;
            CommonModel.Instance.RecordPlayerNameInput();
        }
    }

    public bool RecordSetPlayerName
    {
        get => CommonModel.Instance.Data.IsSetPlayerName;
        set => CommonModel.Instance.RecordPlayerNameInput();
    }

    public int HeadPortrait
    {
        get
        {
            if (CommonModel.Instance.Data.HeadPortrait == 0)
            {
                HeadPortrait = UnityEngine.Random.Range(1,10);
            }
            return CommonModel.Instance.Data.HeadPortrait;
        }
        set
        {
            if (CommonModel.Instance.Data.HeadPortrait != value)
            {
                CommonModel.Instance.SetHeadPortrait(value);
                GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.ChangeHeadPortrait));
            }
        }
    }

    public string Language
    {
        get => CommonModel.Instance.Data.Language;
        set => CommonModel.Instance.SetLanguage(value);
    }

    public bool IsShowSettingRedPoint
    {
        get => CommonModel.Instance.Data.IsShowSettingRedPoint;
        set => CommonModel.Instance.SetIsShowSettingRedPoint(value);
    }

    public bool IsShowHeadPortraitRedPoint
    {
        get => CommonModel.Instance.Data.IsShowHeadPortraitRedPoint;
        set => CommonModel.Instance.SetIsShowHeadPortraitRedPoint(value);
    }

    public bool IsShowChangeImageRedPoint
    {
        get => CommonModel.Instance.Data.IsShowChangeImageRedPoint;
        set => CommonModel.Instance.SetIsShowChangeImageRedPoint(value);
    }

    public bool IsShowChangeImageToggleRedPoint
    {
        get => CommonModel.Instance.Data.IsShowChangeImageToggleRedPoint;
        set => CommonModel.Instance.SetIsShowChangeImageToggleRedPoint(value);
    }

    public string OnePlusOnePackType
    {
        get => CommonModel.Instance.Data.OnePlusOnePackType;
        set => CommonModel.Instance.SetOnePlusOnePackType(value);
    }

    public int OnePlusTwoPackGetRewardTime
    {
        get => CommonModel.Instance.Data.OnePlusTwoPackGetRewardTime;
        set => CommonModel.Instance.SetOnePlusTwoPackGetRewardTime(value);
    }

    public int ChainPackStage
    {
        get => CommonModel.Instance.Data.ChainPackStage;
        set => CommonModel.Instance.SetChainPackStage(value);
    }

    public int BgMusicVolume
    {
        get => CommonModel.Instance.Data.BgMusicVolume;
        set => CommonModel.Instance.SetBgMusicVolume(value);
    }
    #endregion

    #region Ads
    public bool IsRemoveAds
    {
        get => AdsModel.Instance.Data.IsRemoveAds;
        set => AdsModel.Instance.SetRemoveAds(value);
    }

    public bool IsUseMytarget
    {
        get => AdsModel.Instance.Data.IsUseMytarget;
        set => AdsModel.Instance.SetUseMytarget(value);
    }
    #endregion

    #region BG
    public int MapBGImageIndex
    {
        get => BGModel.Instance.Data.MapBGImage;
    }

    /// <summary>
    /// 背景编号
    /// </summary>
    public int BGImageIndex
    {
        get => BGModel.Instance.Data.BGImageID;
        set
        {
            if (BGModel.Instance.SetBGImageID(value))
            {
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ChangBGImageID, value));
            }
        }
    }
    /// <summary>
    /// 小图标 编号
    /// </summary>
    public int TileImageIndex
    {
        get => BGModel.Instance.GetTileId();
        set
        {
            if (BGModel.Instance.SetTileIconID(value))
            {
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ChangeTileIconID));
            }
        }
    }

    public bool IsOwnTileID(int id) => BGModel.Instance.IsOwnTileID(id);
    public bool IsOwnBGID(int id) => BGModel.Instance.IsOwnBGID(id);
    public bool IsOwnBGID(int[] ids) => BGModel.Instance.IsOwnBGID(ids);
    public void BuyBGID(int id) => BGModel.Instance.BuyBGID(id);
    public void BuyTileID(int id)
    {
        GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.BuyTileID));
        BGModel.Instance.BuyTileID(id);
    }

    public bool IsShowBGRedPoint => BGModel.Instance.Data.IsShowBGRedPoint;
    public bool IsShowBGRedPointById(int id) => BGModel.Instance.Data.ShowRedPointBGIds.Contains(id);
    public void RecordShowBGRedPoint(int id) => BGModel.Instance.SetBGRedPointStatus(id);
    public void RemoveShowBGRedPoint(int id) => BGModel.Instance.RemoveBGIDRedPoint(id);
    public void RemoveShowBGRedPoint() => BGModel.Instance.RemoveBGRedPointStatus();
#endregion

    #region Item
    public int StarNum => GetItemNum(TotalItemData.Star);

    public void SyncAllItemData() => ItemModel.Instance.SyncAllItemData();

    public int GetCurItemNum(TotalItemData type) => ItemModel.Instance.GetItem(type.TotalItemType);
    public int CoinNum => GetItemNum(TotalItemData.Coin);

    public void AddAdditionalItem(TotalItemData type, int num)
    {
        if (type == TotalItemData.InfiniteLifeTime)
        {
            AddInfiniteLifeTime(num);
            return;
        }
        else if (type == TotalItemData.InfiniteMagnifierBoost || type == TotalItemData.InfiniteAddOneStepBoost || type == TotalItemData.InfiniteFireworkBoost) 
        {
            AddInfiniteBoostTime(type.TotalItemType, num);
            return;
        }

        ItemModel.Instance.AddAdditionalItem(type.TotalItemType, num);
    }

    public int GetItemNum(TotalItemData type) => ItemModel.Instance.GetItemTotalNum(type.TotalItemType);

    public void AddItemNum(TotalItemData type, int num)
    {
        if (type == TotalItemData.InfiniteLifeTime)
        {
            AddInfiniteLifeTime(num);
            return;
        }
        else if (type == TotalItemData.InfiniteMagnifierBoost || type == TotalItemData.InfiniteAddOneStepBoost || type == TotalItemData.InfiniteFireworkBoost)
        {
            AddInfiniteBoostTime(type.TotalItemType, num);
            return;
        }

        ItemModel.Instance.AddItem(type.TotalItemType, num);
    }

    public bool CheckNum(TotalItemData type) => ItemModel.Instance.CheckNum(type.TotalItemType);

    public bool UseItem(TotalItemData type, int num, bool isPlayCoinAudio = true)
    {
        if (type == TotalItemData.Life) 
        {
            int lifeNum = LifeNum;
            if (lifeNum >= FullLifeNum && lifeNum - num < FullLifeNum)
            {
                StartRecoverLifeTime = DateTime.Now.ToString(Constant.GameConfig.DefaultDateTimeFormet);
            }
        }

        if (ItemModel.Instance.UseItem(type.TotalItemType, num))
        {
            if (type == TotalItemData.Coin)
            {
                if (num > 0)
                {
                    if (isPlayCoinAudio)
                        GameManager.Sound.PlayAudio(SoundType.SFX_SpendCoin.ToString());
                }
                GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(-num, null));
                PlayerPrefs.SetInt("LevelUseCoin", 1);
            }
            else if (type == TotalItemData.Star)
            {
                GameManager.Event.Fire(this, StarNumRefreshEventArgs.Create());
            }
            else
            {
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.None, null));
            }
            return true;
        }
        return false;
    }

    public void RecordPlayerItemDataByLevel() => ItemModel.Instance.RecordItemByFirebase();
    #endregion

    #region Life

    private int fullLifeNum = 5;

    public int FullLifeNum => fullLifeNum;

    public int LifeNum => GetItemNum(TotalItemData.Life);

    /// <summary>
    /// 开始生命恢复的时间
    /// </summary>
    public string StartRecoverLifeTime { get => PlayerPrefs.GetString(Constant.PlayerData.StartRecoverLifeTime); set => PlayerPrefs.SetString(Constant.PlayerData.StartRecoverLifeTime, value); }

    /// <summary>
    /// 获取倒计时恢复生命的时间（分钟）
    /// </summary>
    public float GetRecoverLifeTime()
    {
        int lifeNum = LifeNum;
        if (lifeNum >= FullLifeNum)
        {
            return 0;
        }

        string startRecoverLifeTimeString = StartRecoverLifeTime;
        if (string.IsNullOrEmpty(startRecoverLifeTimeString))
        {
            startRecoverLifeTimeString = DateTime.Now.ToString(Constant.GameConfig.DefaultDateTimeFormet);
            StartRecoverLifeTime = startRecoverLifeTimeString;
        }

        if (DateTime.TryParse(startRecoverLifeTimeString, out DateTime startRecoverLifeTime))
        {
            int passedSeconds = (int)DateTime.Now.Subtract(startRecoverLifeTime).TotalSeconds;
            int addLifeNum = passedSeconds / Constant.GameConfig.RecoverLifeInterval;
            if (lifeNum + addLifeNum >= FullLifeNum)
            {
                StartRecoverLifeTime = string.Empty;
                AddItemNum(TotalItemData.Life, FullLifeNum - lifeNum);
                return 0;
            }

            if (addLifeNum > 0)
            {
                AddItemNum(TotalItemData.Life, addLifeNum);
                StartRecoverLifeTime = startRecoverLifeTime.AddSeconds(addLifeNum * Constant.GameConfig.RecoverLifeInterval).ToString(Constant.GameConfig.DefaultDateTimeFormet);
            }
            return (Constant.GameConfig.RecoverLifeInterval - passedSeconds % Constant.GameConfig.RecoverLifeInterval) / 60f;
        }
        else
        {
            AddItemNum(TotalItemData.Life, FullLifeNum - lifeNum);
            StartRecoverLifeTime = DateTime.Now.ToString(Constant.GameConfig.DefaultDateTimeFormet);
            Log.Error("Convert to DateTime fail {0}", startRecoverLifeTimeString);
            return 0;
        }
    }

    /// <summary>
    /// 获取无限生命剩余时间
    /// </summary>
    /// <returns>分钟</returns>
    public float GetInfiniteLifeTime()
    {
        string infiniteLifeEndTime = PlayerPrefs.GetString(Constant.PlayerData.InfiniteLifeEndTime);
        if (string.IsNullOrEmpty(infiniteLifeEndTime))
        {
            return 0;
        }

        float time = 0;
        try
        {
            time = Mathf.Clamp((float)Convert.ToDateTime(infiniteLifeEndTime).Subtract(DateTime.Now).TotalSeconds, 0, float.MaxValue);
        }
        catch
        {
            PlayerPrefs.SetString(Constant.PlayerData.InfiniteLifeEndTime, DateTime.Now.ToString(Constant.GameConfig.DefaultDateTimeFormet));
            GameManager.Firebase.RecordMessageByEventSelectContent("Life_Time_Covert_Error", infiniteLifeEndTime);
        }

        //最大无限生命时间99小时
        if (time > 356400)
        {
            time = 356400;
            PlayerPrefs.SetString(Constant.PlayerData.InfiniteLifeEndTime, DateTime.Now.AddMinutes(5940).ToString(Constant.GameConfig.DefaultDateTimeFormet));
        }

        return time / 60f;
    }

    /// <summary>
    /// 增加无限生命剩余时间
    /// </summary>
    /// <param name="minutes">分钟</param>
    public void AddInfiniteLifeTime(int minutes)
    {
        float time = GetInfiniteLifeTime();
        if (time < 0)
        {
            time = 0;
        }

        float addTime = time + minutes;
        //最大无限生命时间99小时
        if (addTime > 5940)
        {
            addTime = 5940;
        }
        PlayerPrefs.SetString(Constant.PlayerData.InfiniteLifeEndTime, DateTime.Now.AddMinutes(addTime).ToString(Constant.GameConfig.DefaultDateTimeFormet));
    }

    /// <summary>
    /// 获取无限增益剩余时间
    /// </summary>
    /// <returns>分钟</returns>
    public float GetInfiniteBoostTime(TotalItemType boostType)
    {
        if (boostType == TotalItemType.MagnifierBoost)
            boostType = TotalItemType.InfiniteMagnifierBoost;
        if (boostType == TotalItemType.Prop_AddOneStep)
            boostType = TotalItemType.InfiniteAddOneStepBoost;
        if (boostType == TotalItemType.FireworkBoost)
            boostType = TotalItemType.InfiniteFireworkBoost;

        string endTime = PlayerPrefs.GetString(Constant.PlayerData.InfiniteBoostEndTimePrefix + boostType.ToString());
        if (string.IsNullOrEmpty(endTime))
        {
            return 0;
        }

        float time = 0;
        try
        {
            time = Mathf.Clamp((float)Convert.ToDateTime(endTime).Subtract(DateTime.Now).TotalSeconds, 0, float.MaxValue);
        }
        catch
        {
            PlayerPrefs.SetString(Constant.PlayerData.InfiniteBoostEndTimePrefix + boostType.ToString(), DateTime.Now.ToString(Constant.GameConfig.DefaultDateTimeFormet));
        }

        //最大无限时间99小时
        if (time > 356400)
        {
            time = 356400;
            PlayerPrefs.SetString(Constant.PlayerData.InfiniteBoostEndTimePrefix + boostType.ToString(), DateTime.Now.AddMinutes(5940).ToString(Constant.GameConfig.DefaultDateTimeFormet));
        }

        return time / 60f;
    }

    /// <summary>
    /// 增加无限增益剩余时间
    /// </summary>
    /// <param name="minutes">分钟</param>
    public void AddInfiniteBoostTime(TotalItemType boostType, int minutes)
    {
        if (boostType == TotalItemType.MagnifierBoost)
            boostType = TotalItemType.InfiniteMagnifierBoost;
        if (boostType == TotalItemType.Prop_AddOneStep)
            boostType = TotalItemType.InfiniteAddOneStepBoost;
        if (boostType == TotalItemType.FireworkBoost)
            boostType = TotalItemType.InfiniteFireworkBoost;

        float time = GetInfiniteBoostTime(boostType);
        if (time < 0)
        {
            time = 0;
        }

        float addTime = time + minutes;
        //最大无限生命时间99小时
        if (addTime > 5940)
        {
            addTime = 5940;
        }
        PlayerPrefs.SetString(Constant.PlayerData.InfiniteBoostEndTimePrefix + boostType.ToString(), DateTime.Now.AddMinutes(addTime).ToString(Constant.GameConfig.DefaultDateTimeFormet));
    }

    #endregion

    #region Level Chapter
    public int NowLevel
    {
        get => LevelModel.Instance.Data.Level;
        set
        {
            LevelModel.Instance.SetLevelNum(value);
        }
    }
    public int RealLevel(int levelNum=0) => LevelModel.Instance.RealLevel(levelNum);
    
    public int MaxLevel => LevelModel.MaxLevel;

    public int EachChapterLevelNum
    {
        get => LevelModel.EachChapterLevelNum;
        set => LevelModel.EachChapterLevelNum = value;
    }

    public void RecordFirstLevelSuccess(int levelNum) => LevelModel.Instance.RecordFirstLevelSuccess(levelNum);

    public bool IsFirstLevelSuccess(int levelNum) => LevelModel.Instance.IsFirstLevelSuccess(levelNum);
    public bool WinLastGame
    {
        get => PlayerBehaviorModel.Instance.GetIfWinLastGame();
        set => PlayerBehaviorModel.Instance.RecordWinLastGame(value);

    }

    public int CurChapter => NowLevel / EachChapterLevelNum + (NowLevel % EachChapterLevelNum == 0 ? 0 : 1);

    public int MaxChapter => LevelModel.Instance.MaxChapter;

    public void RecordPlayerLevelWinBehavior() => PlayerBehaviorModel.Instance.RecordTodayWinCount();
    #endregion

    #region Rate
    public void RefreshPlayerBehavior() => PlayerBehaviorModel.Instance.Refresh();
    public bool IsShowRateByLevel(int level,bool isSave) => PlayerBehaviorModel.Instance.IsShowRatePanelByLevel(level,isSave);

    public bool IsShowRateByWinCount(bool isSave) => PlayerBehaviorModel.Instance.IsShowRatePanelByTodayFiveWin(isSave);

    public bool IsShowRateByUnlockBG(bool isSave) => PlayerBehaviorModel.Instance.IsShowRatePanelByBg(isSave);

    public bool IsEffectiveDisplayRate() => PlayerBehaviorModel.Instance.Data.RateData.IfHaveEffectiveRate;

    public void RecordUnlockBG(int nowLevel) => PlayerBehaviorModel.Instance.RecordUnlockBg(nowLevel);

    public void RecordEffectiveRate() => PlayerBehaviorModel.Instance.RecordEffectiveDisplay();

    public void RecordPlayerLevelWinByDaliy_Rate(int nowLevel) => PlayerBehaviorModel.Instance.RecordWinByDaliy_Rate(nowLevel);

    public void RecordGameToMapCountByDay() => PlayerBehaviorModel.Instance.RecordGameToMapCount();

    public bool IsFirstGameToMapByDay() => PlayerBehaviorModel.Instance.Data.LevelData.GameToMapCountByDay == 1;

    public int GetGameToMapCountByDay() => PlayerBehaviorModel.Instance.Data.LevelData.GameToMapCountByDay;
    #endregion

    #region Ads
    public void RecordWatchAdsDoubleByToday() => PlayerBehaviorModel.Instance.RecordWatchAdsDoubleByToday();
    public bool IsWatchAdsDoubleByToday => PlayerBehaviorModel.Instance.IsWatchAdsDoubleByToday();

    public int DaliyWatchAdsCountByToday => PlayerBehaviorModel.Instance.Data.AdsData.WatchDaliyAdsCountByDay;
    public void RecordDaliyWatchAdsByToday() => PlayerBehaviorModel.Instance.RecordDaliyWatchAdsByToday();
    public bool IsCanDaliyWatchAds => PlayerBehaviorModel.Instance.IsCanDaliyWatchAds();

    public bool IsShowWatchAdsPanel => PlayerBehaviorModel.Instance.IsShowWatchAdsPanel();

    public void RecordIsShowWatchAdsPanel()=>PlayerBehaviorModel.Instance.RecordIsShowWatchAdsPanel();

    public bool TodayEverShowedNoAdsPanel
    {
        get => PlayerBehaviorModel.Instance.Data.AdsData.TodayEverShowedNoAdsPanel;
        set => PlayerBehaviorModel.Instance.RecordTodayEverShowedNoAdsPanel(value);
    }

    public int WatchInterstitialAdTimeToday
    {
        get => PlayerBehaviorModel.Instance.Data.AdsData.WatchInterstitialAdTimeToday;
        set => PlayerBehaviorModel.Instance.RecordWatchInterstitialAdTimeToday(value);
    }

    public int WatchInterstitialAdTime
    {
        get => PlayerBehaviorModel.Instance.Data.AdsData.WatchInterstitialAdTime;
        set => PlayerBehaviorModel.Instance.RecordWatchInterstitialAdTime(value);
    }

    public bool LifeTimeEverShowedNoAdsPanel
    {
        get => PlayerBehaviorModel.Instance.Data.AdsData.LifeTimeEverShowedNoAdsPanel;
        set => PlayerBehaviorModel.Instance.RecordEverShowedNoAdsPanel(value);
    }

    public bool ShowRemoveAdsMenuWhenBackToMap
    {
        get => PlayerBehaviorModel.Instance.Data.AdsData.ShowRemoveAdsMenuWhenBackToMap;
        set => PlayerBehaviorModel.Instance.Data.AdsData.ShowRemoveAdsMenuWhenBackToMap = value;
        //确实不需要存储
    }

    public int TodayWatchRewardedAdTime
    {
        get => PlayerBehaviorModel.Instance.Data.AdsData.TodayWatchRewardedAdTime;
        set => PlayerBehaviorModel.Instance.RecordShowRewardedAds();
    }

    public int TodayWatchPropsAdTime
    {
        get => PlayerBehaviorModel.Instance.Data.AdsData.TodayWatchPropsAdTime;
        set => PlayerBehaviorModel.Instance.RecordShowPropsRewardedAds();
    }

    public int TodayWatchContinueAdTime
    {
        get => PlayerBehaviorModel.Instance.Data.AdsData.TodayWatchContinueAdTime;
        set => PlayerBehaviorModel.Instance.RecordShowContinueRewardedAds();
    }

    #endregion

    #region DaliyGift

    public bool IsShowDaliyGiftByToday() => PlayerBehaviorModel.Instance.IsShowDaliyGiftByToday();

    public void RecordShowDaliyGiftByToday() => PlayerBehaviorModel.Instance.RecordShowDaliyGiftByToday();

    public bool IsLockDaliyGift() => PlayerBehaviorModel.Instance.IsLockDaliyGift();

    public bool NeedShowLoginGiftByToday() => PlayerBehaviorModel.Instance.NeedShowLoginGiftByToday();

    public void RecordShowLoginGiftByToday() => PlayerBehaviorModel.Instance.RecordShowLoginGiftByToday();

    #endregion

    #region Decoration
    public List<string> GetNeedDownNameList(bool isContainNext, int addIndex) => DecorationModel.Instance.GetNeedDownNameList(isContainNext, addIndex);
    public List<string> GetNeedDownloadByMenu() => DecorationModel.Instance.GetNeedDownloadByMenu();

    public int GetHighestFinishedDecorationAreaID() => DecorationModel.Instance.GetHighestFinishedDecorationAreaID();

    public int GetMinCommingSoonAreaID() => DecorationModel.Instance.GetMinCommingSoonAreaID();

    public int DecorationAreaID => DecorationModel.Instance.Data.DecorationAreaID;

    public int OperatingAreaFinishDecorationCount => DecorationModel.Instance.GetOperatingAreaFinishDecortionCount();

    public bool CheckTargetAreaIsComplete(int areaID) => DecorationModel.Instance.CheckTargetAreaIsComplete(areaID);

    public bool GetTargetAreaGetReward(int targetAreaID) => DecorationModel.Instance.GetTargetAreaGetReward(targetAreaID);

    public bool inAreaCompleteAnim
    {
        get => DecorationModel.Instance.inAreaCompleteAnim;
        set { DecorationModel.Instance.inAreaCompleteAnim = value; }
    }
    #endregion
    
    #region Date

    public DateTime FirstDateTime => DateModel.Instance.Data.FirstLoginDate;

    #endregion

    #region story

    public void RecordShowStoryData(int chapterId, int buildSchedule) =>
        StoryModel.Instance.RecordShowStoryData(chapterId, buildSchedule);

    public bool IsHaveShowStory(int chapterId, int buildSchedule) =>
        StoryModel.Instance.IsHaveShowStory(chapterId, buildSchedule);

    #endregion

    #region skillUnlock
    
    public bool IsSkillUnlock(TotalItemType type)
    {
        return PlayerBehaviorModel.Instance.IsSkillUnlock(type);
    }

    public void RecordSkillUnlock(TotalItemType type)
    {
        PlayerBehaviorModel.Instance.RecordSkillUnlock(type);
    }
    
    #endregion

    #region Notification
    public bool NotificationForbidden
    {
        get => NotificationModel.Instance.Data.IsNotificationForbidden;
        set => NotificationModel.Instance.SetNotificationMuted(value);
    }

    public DateTime NotificationScheduledWeek
    {
        get => NotificationModel.Instance.Data.NotificationScheduledWeek;
        set => NotificationModel.Instance.SetNotificationScheduledWeek(value);
    }

    public int LastNotificationRandomNum
    {
        get => NotificationModel.Instance.Data.LastNotificationRandomNum;
        set => NotificationModel.Instance.SetLastNotificationRandomNum(value);
    }
    #endregion

    #region TurnTable

    public int NormalTurntableCoinSpinTime
    {
        get => PlayerBehaviorModel.Instance.GetTurntableNormalCoinSpinTime();
        set => PlayerBehaviorModel.Instance.RecordTurntableNormalCoinSpinTime(value);
    }

    public int NormalTurntableAdsSpinTime
    {
        get => PlayerBehaviorModel.Instance.GetTurntableNormalAdsSpinTime();
        set => PlayerBehaviorModel.Instance.RecordTurntableNormalAdsSpinTime(value);
    }
    
    public int NormalTurntableFreeSpinTime
    {
        get => PlayerBehaviorModel.Instance.GetTurntableNormalFreeSpinTime();
        set => PlayerBehaviorModel.Instance.RecordTurntableNormalFreeSpinTime(value);
    }
    
    public int NormalTurntableDailySpinTime
    {
        get => PlayerBehaviorModel.Instance.GetTurntableNormalDailySpinTime();
        set => PlayerBehaviorModel.Instance.RecordTurntableNormalDailySpinTime(value);
    }
    
    public int TurntableCoinSpinGuaranteeLevel
    {
        get => PlayerBehaviorModel.Instance.GetTurntableCoinSpinGuaranteeLevel();
        set => PlayerBehaviorModel.Instance.RecordTurntableCoinSpinGuaranteeLevel(value);
    }

    public int TurntableCoinSpinCumulativeNumber
    {
        get => PlayerBehaviorModel.Instance.GetTurntableCoinSpinCumulativeNumber();
        set => PlayerBehaviorModel.Instance.RecordTurntableCoinSpinCumulativeNumber(value); 
    }
    
    public DateTime TurntableNextAdsSpinReadyTime
    {
        get => PlayerBehaviorModel.Instance.GetTurntableNextAdsSpinReadyTime();
        set => PlayerBehaviorModel.Instance.RecordTurntableNextAdsSpinReadyTime(value);
    }

    public DateTime TurntableShowWarningTime
    {
        get => PlayerBehaviorModel.Instance.GetTurntableShowWarningTime();
        set => PlayerBehaviorModel.Instance.RecordTurntableShowWarningTime(value);
    }

    public bool IsFirstFreeTurnByDaliy
    {
        get => PlayerBehaviorModel.Instance.IsFirstFreeTurnByDaliy;
        set => PlayerBehaviorModel.Instance.RecordFirstFreeTrun();
    }

    public bool NeedShowDaliyWatchAdsGuide => PlayerBehaviorModel.Instance.Data.DaliyWatchAdsData.NeedShowWatchAdsGuide;

    public void RecordShowDaliyWatchAdsGuide()
    {
        PlayerBehaviorModel.Instance.Data.DaliyWatchAdsData.NeedShowWatchAdsGuide = false;
        PlayerBehaviorModel.Instance.SaveToLocal();
    }

    #endregion

    #region ActivityPack
    public void RecordTodayActivityPackShowTimes(int inputTimes) => PlayerBehaviorModel.Instance.RecordTodayActivityPackShowTimes(inputTimes);

    public int GetTodayActivityPackShowTimes() => PlayerBehaviorModel.Instance.GetTodayActivityPackShowTimes();

    public void RecordEverBoughtSpecialOfferPack(bool inputValue) => PlayerBehaviorModel.Instance.RecordEverBoughtSpecialOfferPack(inputValue);

    public bool GetEverBoughtSpecialOfferPack() => PlayerBehaviorModel.Instance.GetEverBoughtSpecialOfferPack();

    public void RecordEverFocusedOnEventBGItemInChangeImagePanel(bool inputValue) => PlayerBehaviorModel.Instance.RecordEverFocusedOnEventBGItemInChangeImagePanel(inputValue);

    public bool GetEverFocusedOnEventBGItemInChangeImagePanel() => PlayerBehaviorModel.Instance.GetEverFocusedOnEventBGItemInChangeImagePanel();

    public void RecordEverFocusedOnFirstTileItemInChangeImagePanel(bool inputValue) => PlayerBehaviorModel.Instance.RecordEverFocusedOnFirstTileItemInChangeImagePanel(inputValue);

    public bool GetEverFocusedOnFirstTileItemInChangeImagePanel() => PlayerBehaviorModel.Instance.GetEverFocusedOnFirstTileItemInChangeImagePanel();

    #endregion

    #region ClimbBeanstalk

    public bool IsFirstShowEveryday
    {
        get => PlayerBehaviorModel.Instance.GetIsFirstShowEveryday();
        set => PlayerBehaviorModel.Instance.SetIsFirstShowEveryday(value);
    }

    #endregion

    #region Kitchen

    public bool IsFirstShowEveryDay_Kitchen
    {
        get => PlayerBehaviorModel.Instance.GetIsFirstShowEveryday_Kitchen();
        set => PlayerBehaviorModel.Instance.SetIsFirstShowEveryday_Kitchen(value);
    }

    #endregion
    
    #region AppsFlyer Record

    public int AFPassLevel
    {
        get => PlayerPrefs.GetInt("AF_PassLevel_1", 0);
        set => PlayerPrefs.SetInt("AF_PassLevel_1", value);
    }

    public int AFShowInterstitial
    {
        get => PlayerPrefs.GetInt("AF_ShowInterstitial_1", 0);
        set => PlayerPrefs.SetInt("AF_ShowInterstitial_1", value);
    }

    #endregion
    
    #region FrogJump

    public int CurFrogJumpLevel
    {
        get => PlayerBehaviorModel.Instance.GetCurFrogJumpActivityLevel();
        set => PlayerBehaviorModel.Instance.RecordCurFrogJumpActivityLevel(value);
    }

    public DTFrogJumpData FrogJumpData => GameManager.DataTable.GetDataTable<DTFrogJumpData>().Data;

    public DateTime NextFrogAdReadyTime
    {
        get => PlayerBehaviorModel.Instance.GetNextAdReadyTime();
        set => PlayerBehaviorModel.Instance.RecordNextAdReadyTime(value);
    }
    
    public int MaxFrogJumpLevel => FrogJumpData.MaxLevel();

    public bool IsFirstStartFrogJumpActivity
    {
        get => PlayerBehaviorModel.Instance.GetIsFirstStartFrogJumpActivity();
        set => PlayerBehaviorModel.Instance.RecordIsFirstStartFrogJumpActivity(value);
    }
    #endregion
}

public static class PlayDataUtil
{

    public static string GetBigBgImageName(this int bgIndex)
    {
        return $"BG_{GetNewBgImageIndex(bgIndex)}";
    }

    public static int GetNewBgImageIndex(this int bgIndex)
    {
        //替换前5张背景图片
        if (bgIndex > 0 && bgIndex <= 5 && GameManager.Firebase.GetBool(Constant.RemoteConfig.BG_Change_1to5, false))
        {
            return 10000 + bgIndex;
        }
        return bgIndex;
    }
}