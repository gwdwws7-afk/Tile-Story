using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace MySelf.Model
{
	public class PlayerDataBase
	{
		public virtual void IsRefreshDataByVersion(){}
		public virtual void IsRefreshDataByNewDay(){}
	}
	
	public class PlayerBehaviorModelData:PlayerDataBase
	{
		public string AppVersion;
		public int RecordDateByDay;

		public PlayerLevelData LevelData = new PlayerLevelData();
		public PlayerRateData RateData=new PlayerRateData();
		public PlayerGuideData GuideData = new PlayerGuideData();
		public PlayerAdsData AdsData = new PlayerAdsData();
		public PlayerDaliyGiftData DaliyGiftData = new PlayerDaliyGiftData();
		public PersonRankBehaviourData PersonRankBehaviourData = new PersonRankBehaviourData();
		public TurntableData TurntableData = new TurntableData();
		public SkillLockData SkillLockData = new SkillLockData();
		public CommonBehaviorData CommonBehaviorData = new CommonBehaviorData();
		public ActivityPackData ActivityPackData = new ActivityPackData();
		public DaliyWatchAdsData DaliyWatchAdsData = new DaliyWatchAdsData();
		public GlacierQuestDayData GlacierQuestDayData = new GlacierQuestDayData();
		public BalloonRiseBehaviourData BalloonRiseBehaviourData = new BalloonRiseBehaviourData();
        public ClimbBeanstalkDayData ClimbBeanstalkDayData = new ClimbBeanstalkDayData();
        public KitchenDayData KitchenDayData = new KitchenDayData();
        
        public FrogJumpActivityData FrogJumpActivityData = new FrogJumpActivityData();
		
		[JsonIgnore] private List<PlayerDataBase> playerDataList 
			=> new List<PlayerDataBase>()
			{
				LevelData,RateData,GuideData,AdsData,DaliyGiftData,PersonRankBehaviourData,TurntableData,SkillLockData,CommonBehaviorData,ActivityPackData,
				DaliyWatchAdsData,GlacierQuestDayData,BalloonRiseBehaviourData,ClimbBeanstalkDayData,KitchenDayData,FrogJumpActivityData
			};
			
		public override void IsRefreshDataByVersion()
		{
			foreach (PlayerDataBase playeData in playerDataList)
			{
				playeData.IsRefreshDataByVersion();
			}
		}
		
		public override void IsRefreshDataByNewDay()
		{
			foreach (PlayerDataBase playeData in playerDataList)
			{
				playeData.IsRefreshDataByNewDay();
			}
		}
		public bool IsRefreshData()
		{
			bool isSave = false;

			bool isNewVersion = Application.version != AppVersion;
			bool isNewDay = (DateTime.Now - Constant.GameConfig.DateTimeMin).Days != RecordDateByDay;

			this.AppVersion = Application.version;
			this.RecordDateByDay = (DateTime.Now - Constant.GameConfig.DateTimeMin).Days;
			
			if (isNewVersion)
			{
				IsRefreshDataByVersion();
			}

			if (isNewDay)
			{
				IsRefreshDataByNewDay();
				PlayerPrefs.SetInt("WeekendTodayShowTime", 0);
				HiddenTemple.HiddenTempleManager.PlayerData.SetTodayShowedLastChanceMenu(false);
			}
			return isNewVersion || isNewDay;
		}
	}

	public class DaliyWatchAdsData : PlayerDataBase
	{
		public bool NeedShowWatchAdsGuide=true;
	}

	public class PlayerRateData:PlayerDataBase
	{
		public List<int> OpenRateLevels = new List<int>();//有史以来记录的 level关弹出的情况

		public int RecordOpenRateCount;//这个版本打开的次数
		public bool IfHaveEffectiveRate;//当前版本是否有有效展示
		public bool IsHaveOpenRateByFiveCount;//每日胜利五次会尝试弹一次
		public int RecordWinByDaliy;//达到10关的时候开始记录

		public int TodayOpenRateCount;//今天打开的次数

		[JsonIgnore]
		public bool IsUnlockBG=false;

		public override void IsRefreshDataByVersion()
		{
			IfHaveEffectiveRate = false;
			RecordOpenRateCount = 0;
			IsHaveOpenRateByFiveCount = false;
			RecordWinByDaliy = 0;
			IsUnlockBG = false;
		}
		
		public override void IsRefreshDataByNewDay()
		{
			TodayOpenRateCount = 0;
			IsHaveOpenRateByFiveCount = false;
			RecordWinByDaliy = 0;
			IsUnlockBG = false;
		}
	}

	public class PlayerLevelData:PlayerDataBase
	{
		public int TodayWinCount;
		public int GameToMapCountByDay;
		[JsonIgnore]
		public bool WinLastGame;
		
		public override void IsRefreshDataByVersion()
		{
			TodayWinCount = 0;
		}
		
		public override void IsRefreshDataByNewDay()
		{
			TodayWinCount = 0;
			GameToMapCountByDay = 0;
		}
	}

	public class PlayerGuideData:PlayerDataBase
	{
		public bool HasShownGamePlayGuide;
		public bool HasShownHelpFirstGuide;
		public bool HasShownHelpSecondGuide;
		public bool HasShownObjectiveGuide;
		public bool HasShownEarnMoreStarGuide;
		public bool HasShownDailyWatchAdsGuide;
	}

	public class PlayerAdsData:PlayerDataBase
	{
		//DailyRest
		public bool IsWatchAdsDouble = true;
		public int WatchDaliyAdsCountByDay = 5;
		public int WatchInterstitialAdTimeToday = 0;
		public bool TodayEverShowedNoAdsPanel = false;//今天是否弹出过

		public int WatchInterstitialAdTime = 0;
		public bool IsShowWatchAdsPanel = true;
		public bool LifeTimeEverShowedNoAdsPanel = false;//生涯是否弹出过

		public int TodayWatchRewardedAdTime;//今天看奖励广告的次数
		public int TodayWatchPropsAdTime;//今天看三选一奖励广告的次数
		public int TodayWatchContinueAdTime;//今天看接关奖励广告的次数

		[JsonIgnore]
		public bool ShowRemoveAdsMenuWhenBackToMap = false;//这次是否要弹出的临时的不持久化的变量

		public override void IsRefreshDataByNewDay()
		{
			IsWatchAdsDouble = true;
			WatchDaliyAdsCountByDay = 5;
			WatchInterstitialAdTimeToday = 0;
			TodayEverShowedNoAdsPanel = false;
			TodayWatchRewardedAdTime = 0;
			TodayWatchPropsAdTime = 0;
			TodayWatchContinueAdTime = 0;
		}
	}

	public class PlayerDaliyGiftData:PlayerDataBase
	{
		public bool IsLockDaliyGift = false;
		public bool IsAutoShowDaliyGift = true;
		public bool IsShowDaliyGiftBtn = true;
		public bool NeedShowLoginGiftBtn = true;
		
		public override void IsRefreshDataByNewDay()
		{
			IsShowDaliyGiftBtn = true;
			IsAutoShowDaliyGift = true;
			NeedShowLoginGiftBtn = true;
		}
	}

	public class PersonRankBehaviourData : PlayerDataBase
	{
		public int CurrentRank;
		public int LastRank;
		public int LevelPass;
		public int Score;
		public int LastScore;
		public bool HasShownEntranceBtn;
		public DateTime EndTime;
		public bool HasShownTodayWelcomeMenu;

		public override void IsRefreshDataByNewDay()
		{
			HasShownTodayWelcomeMenu = false;
			base.IsRefreshDataByNewDay();
		}
	}

	public class BalloonRiseBehaviourData : PlayerDataBase
	{
		public override void IsRefreshDataByNewDay()
		{
			base.IsRefreshDataByNewDay();
			
			BalloonRiseModel.Instance.ResetByNewDay();
		}
	}

	public class SkillLockData : PlayerDataBase
	{
		public Dictionary<TotalItemType, bool> IsSkillUnlocks = new Dictionary<TotalItemType, bool>()
		{
			{ TotalItemType.Prop_Back ,false},
			{ TotalItemType.Prop_Absorb,false},
			{ TotalItemType.Prop_Grab ,false},
			{ TotalItemType.Prop_ChangePos ,false},
			{ TotalItemType.Prop_AddOneStep ,false}
		};
	}


	public class TurntableData : PlayerDataBase
	{
		public int NormalTurntableCoinSpinTime;
		public int NormalTurntableAdsSpinTime;
		public int NormalTurntableFreeSpinTime;
		public int NormalTurntableDailySpinTime;
		public int TurntableCoinSpinGuaranteeLevel = 1;
		public int TurntableCoinSpinCumulativeNumber;
		public DateTime TurntableNextAdsSpinReadyTime;
		public DateTime TurntableShowWarningTime;

		public bool IsFirstFreeByDaliy = true;

		public override void IsRefreshDataByNewDay()
		{
			NormalTurntableCoinSpinTime = 0;
			NormalTurntableAdsSpinTime = 0;
			NormalTurntableFreeSpinTime = 0;
			NormalTurntableDailySpinTime = 0;
			TurntableNextAdsSpinReadyTime = Constant.GameConfig.DateTimeMin;
			TurntableShowWarningTime = Constant.GameConfig.DateTimeMin;
			IsFirstFreeByDaliy = true;
			base.IsRefreshDataByNewDay();
		}
	}

	public class ActivityPackData: PlayerDataBase
    {
		public int TodayActivityPackShowTimes;
		public bool everBoughtSpecialOfferPack;
		public bool everFocusedOnEventBGItemInChangeImagePanel;
		public bool everFocusedOnFirstTileItemInChangeImagePanel;

        public override void IsRefreshDataByVersion()
        {
			everFocusedOnEventBGItemInChangeImagePanel = false;
			everFocusedOnFirstTileItemInChangeImagePanel = false;
		}

        public override void IsRefreshDataByNewDay()
        {
			TodayActivityPackShowTimes = 0;
		}
    }

	public class CommonBehaviorData : PlayerDataBase
	{
		public bool HasUsedTodayFreeChance;
		public bool HasShownCalendarChallengeGuide = false;
		public bool HasShownCalendarSwitchGuide = false;
		public bool DailyFirstStartLevel = false;

		public override void IsRefreshDataByNewDay()
		{
			HasUsedTodayFreeChance = false;
			DailyFirstStartLevel = false;
			base.IsRefreshDataByNewDay();
		}
	}
	
	public class GlacierQuestDayData : PlayerDataBase
	{
		public bool isNewDayRefresh = false;
		
		public override void IsRefreshDataByNewDay()
		{
			isNewDayRefresh = true;
			base.IsRefreshDataByNewDay();
		}
	}
	
	public class ClimbBeanstalkDayData : PlayerDataBase
	{
		public bool isFirstShowEveryday = true;
		public override void IsRefreshDataByNewDay()
		{
			isFirstShowEveryday = true;
			base.IsRefreshDataByNewDay();
		}
	}

	public class KitchenDayData : PlayerDataBase
	{
		public bool isFirstShowEveryday = true;
		public override void IsRefreshDataByNewDay()
		{
			isFirstShowEveryday = true;
			base.IsRefreshDataByNewDay();
		}
	}
	
	public class FrogJumpActivityData : PlayerDataBase
	{
		public int CurFrogJumpActivityLevel = 0;
		public bool IsFirstStartFrogJumpActivity = true;
		public DateTime NextAdReadyTime = DateTime.MinValue;

		public override void IsRefreshDataByNewDay()
		{
			CurFrogJumpActivityLevel = 0;
			base.IsRefreshDataByNewDay();
		}
	}
	
	public class PlayerBehaviorModel: BaseModelService<PlayerBehaviorModel, PlayerBehaviorModelData>
	{
		#region Service
		public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
		{
			return new Dictionary<string, object>()
			{
				{"PlayerBehaviorModel.TurntableCoinSpinGuaranteeLevel",Data.TurntableData.TurntableCoinSpinGuaranteeLevel},
				{"PlayerBehaviorModel.TurntableCoinSpinCumulativeNumber" ,Data.TurntableData.TurntableCoinSpinCumulativeNumber},
				{"PlayerBehaviorModel.IsFirstFreeByDaliy",Data.TurntableData.IsFirstFreeByDaliy},
				{"PlayerBehaviorModel.IsShowDaliyGiftBtn",Data.DaliyGiftData.IsShowDaliyGiftBtn},
			};
		}

		public override void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary)
		{
			if (dictionary != null)
			{
				foreach (var item in dictionary)
				{
					switch (item.Key)
					{
						case "PlayerBehaviorModel.TurntableCoinSpinGuaranteeLevel":
							Data.TurntableData.TurntableCoinSpinGuaranteeLevel = Convert.ToInt32(item.Value);
							break;
						case "PlayerBehaviorModel.TurntableCoinSpinCumulativeNumber":
							Data.TurntableData.TurntableCoinSpinCumulativeNumber = Convert.ToInt32(item.Value);
							break;
						case "PlayerBehaviorModel.IsFirstFreeByDaliy":
							Data.TurntableData.IsFirstFreeByDaliy = Convert.ToBoolean(item.Value);
							break;
						case "PlayerBehaviorModel.IsShowDaliyGiftBtn":
							Data.DaliyGiftData.IsShowDaliyGiftBtn = Convert.ToBoolean(item.Value);
							break;
					}
				}
				SaveToLocal();
			}
		}
		#endregion
		private int OneVersionOpenRateCount => (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Can_Open_Rate_MaxCount_ByVersion,3);
		private bool IfOpenRateByLevel=> GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Open_Rate_ByLevel, true);
		private bool IfOpenRateByUnlockBg => GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Open_Rate_ByUnlockBG, true);
		private bool IfOpenRateByFiveWin => GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Open_Rate_ByFiveWin, true);

		private int showRateLevel => (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.ShowRateLevel, 10L);

		public void Refresh()
		{
			if (Data.IsRefreshData()) SaveToLocal();
		}

        #region level
        public void RecordTodayWinCount()
		{
			Data.LevelData.TodayWinCount += 1;
			//SaveToLocal();
		}

		public void RecordGameToMapCount()
		{
			Data.LevelData.GameToMapCountByDay += 1;
			SaveToLocal();
		}
		
		public void RecordWinLastGame(bool value)
		{
			Data.LevelData.WinLastGame = value;
			//SaveToLocal();
		}
		
		public bool GetIfWinLastGame()
		{
			return Data.LevelData.WinLastGame;
		}

		#endregion

		#region rate
		public void RecordEffectiveDisplay()
		{
			Data.RateData.IfHaveEffectiveRate = true;
			SaveToLocal();
		}

		public void RecordUnlockBg(int nowLevel)
		{
			if (nowLevel >= 10)
			{
				Data.RateData.IsUnlockBG = true;
				SaveToLocal();
			}
		}

		public bool IsShowRatePanelByLevel(int curLevel,bool isSave)
		{
			return false;
			if (IsShowRate()&& IfOpenRateByLevel)
			{
				var list = Data.RateData.OpenRateLevels;

				int lastRecordLevel = (list == null || list.Count == 0) ? 0 : list[list.Count - 1];

				int curNearLevel = Math.Max(curLevel / 50 * 50, curLevel >= 10 ? 10 : 0);

				if (curNearLevel > 0 && curNearLevel > lastRecordLevel)
				{
					if (isSave)
					{
						Data.RateData.RecordOpenRateCount += 1;
						Data.RateData.TodayOpenRateCount += 1;
						Data.RateData.OpenRateLevels.Add(curNearLevel);
						SaveToLocal();
					}
					return true;
				}
			}
			return false;
		}

		public bool IsShowRatePanelByBg(bool isSave)
		{
			if (IsShowRate() && IfOpenRateByUnlockBg)
			{
				if (Data.RateData.IsUnlockBG)
				{
					if (isSave)
					{
						Data.RateData.RecordOpenRateCount += 1;
						Data.RateData.TodayOpenRateCount += 1;
						Data.RateData.IsUnlockBG = false;
						SaveToLocal();
					}
					return true;
				}
			}
			return false;
		}

		public bool IsShowRatePanelByTodayFiveWin(bool isSave)
		{
			if (IsShowRate()&& IfOpenRateByFiveWin)
			{
				if (!Data.RateData.IsHaveOpenRateByFiveCount && Data.RateData.RecordWinByDaliy >= 5)
				{
					if (isSave)
					{
						Data.RateData.IsHaveOpenRateByFiveCount = true;
						Data.RateData.RecordOpenRateCount += 1;
						Data.RateData.TodayOpenRateCount += 1;
						SaveToLocal();
					}
					return true;
				}
			}
			return false;
		}

		public void RecordWinByDaliy_Rate(int nowLevel)
		{
			if (nowLevel >= 10)
			{
				Data.RateData.RecordWinByDaliy+=1;
				SaveToLocal();
			}
		}

		private bool IsShowRateByInternet()
		{
			return Application.internetReachability != NetworkReachability.NotReachable;
		}

		private bool IsCanOpenRateCountByVersion()
		{
			return OneVersionOpenRateCount > Data.RateData.RecordOpenRateCount;
		}

		private bool IsTodayHaveOpen()
		{
			return Data.RateData.TodayOpenRateCount >= 1;
		}

		private bool IsShowRate()
		{
			if (GameManager.PlayerData.NowLevel < showRateLevel) return false;
			
			if (Data.RateData.IfHaveEffectiveRate) return false;

			if (!IsShowRateByInternet()) return false;

			if (!IsCanOpenRateCountByVersion()) return false;

			if (IsTodayHaveOpen()) return false;

			return true;
		}
		#endregion

		#region guide

		public void RecordGamePlayGuide()
		{
			Data.GuideData.HasShownGamePlayGuide = true;
			SaveToLocal();
		}
		
		public bool HasShownGamePlayGuide()
		{
			return Data.GuideData.HasShownGamePlayGuide;
		}
		
		public void RecordHelpFirstGuide()
		{
			Data.GuideData.HasShownHelpFirstGuide = true;
			SaveToLocal();
		}
		
		public bool HasShownHelpFirstGuide()
		{
			return Data.GuideData.HasShownHelpFirstGuide;
		}
		
		public void RecordHelpSecondGuide()
		{
			Data.GuideData.HasShownHelpSecondGuide = true;
			SaveToLocal();
		}
		
		public bool HasShownHelpSecondGuide()
		{
			return Data.GuideData.HasShownHelpSecondGuide;
		}
		
		public void RecordObjectiveGuide()
        {
			Data.GuideData.HasShownObjectiveGuide = true;
			SaveToLocal();
		}

		public bool HasShownObjectiveGuide()
        {
			return Data.GuideData.HasShownObjectiveGuide;
		}

		public void RecordEarnMoreStarGuide()
        {
			Data.GuideData.HasShownEarnMoreStarGuide = true;
			SaveToLocal();
        }

		public bool HasShownEarnMoreStarGuide()
        {
			return Data.GuideData.HasShownEarnMoreStarGuide;
        }

		public void RecordDailyWatchAdsGuide()
        {
			Data.GuideData.HasShownDailyWatchAdsGuide = true;
			SaveToLocal();
		}

		public bool HasShownDailyWatchAdsGuide()
        {
			return Data.GuideData.HasShownDailyWatchAdsGuide;
        }

		#endregion

		#region ads
		public void RecordWatchAdsDoubleByToday()
		{
			Data.AdsData.IsWatchAdsDouble = false;
			SaveToLocal();
		}

		public bool IsWatchAdsDoubleByToday()
		{
			return Data.AdsData.IsWatchAdsDouble;
		}

		public void RecordDaliyWatchAdsByToday()
		{
			Data.AdsData.WatchDaliyAdsCountByDay-=1;
			SaveToLocal();
		}

		public bool IsCanDaliyWatchAds()
		{
			return Data.AdsData.WatchDaliyAdsCountByDay > 0;
		}

		public bool IsShowWatchAdsPanel()
		{
			return Data.AdsData.IsShowWatchAdsPanel;
		}

		public void RecordIsShowWatchAdsPanel(bool isShow=false)
		{
			Data.AdsData.IsShowWatchAdsPanel = isShow;
			SaveToLocal();
		}

		public void RecordWatchInterstitialAdTime(int value)
		{
			Data.AdsData.WatchInterstitialAdTime = value;
			SaveToLocal();
		}

		public void RecordTodayEverShowedNoAdsPanel(bool value)
        {
			Data.AdsData.TodayEverShowedNoAdsPanel = value;
			SaveToLocal();
        }

		public void RecordWatchInterstitialAdTimeToday(int value)
        {
			Data.AdsData.WatchInterstitialAdTimeToday = value;
			SaveToLocal();
		}

		public void RecordEverShowedNoAdsPanel(bool value)
        {
			Data.AdsData.LifeTimeEverShowedNoAdsPanel = value;
			SaveToLocal();
		}

		public void RecordShowRewardedAds()
        {
			Data.AdsData.TodayWatchRewardedAdTime += 1;
			SaveToLocal();
		}

		public void RecordShowPropsRewardedAds()
		{
			Data.AdsData.TodayWatchPropsAdTime += 1;
			SaveToLocal();
		}

		public void RecordShowContinueRewardedAds()
		{
			Data.AdsData.TodayWatchContinueAdTime += 1;
			SaveToLocal();
		}

		#endregion

		#region DaliyGift

		public void RecordShowDaliyGiftByToday()
		{
			Data.DaliyGiftData.IsShowDaliyGiftBtn = false;
			Data.DaliyGiftData.IsLockDaliyGift = true;
			SaveToLocal();
		}

		public bool IsShowDaliyGiftByToday()
		{
			return Data.DaliyGiftData.IsShowDaliyGiftBtn;
		}

		public bool IsLockDaliyGift()
		{
			bool islock = Data.DaliyGiftData.IsLockDaliyGift;
			if (!islock)
			{
				Data.DaliyGiftData.IsLockDaliyGift = true;
				SaveToLocal();
			}

			return islock;
		}

		public void RecordShowLoginGiftByToday()
        {
			Data.DaliyGiftData.NeedShowLoginGiftBtn = false;
			SaveToLocal();
		}

		public bool NeedShowLoginGiftByToday()
		{
			return Data.DaliyGiftData.NeedShowLoginGiftBtn;
		}

		#endregion

		#region PersonRank

		public void RecordHasShownPersonRankEntranceBtn(bool isShow)
		{
			Data.PersonRankBehaviourData.HasShownEntranceBtn = isShow;
			SaveToLocal();
		}
		
		public bool HasShownPersonRankEntranceBtn()
		{
			return Data.PersonRankBehaviourData.HasShownEntranceBtn;
		}
		
		public void RecordLeaderBoardCurrentRank(int rank)
		{
			Data.PersonRankBehaviourData.CurrentRank = rank;
			SaveToLocal();
		}
		
		public void RecordLeaderBoardLastRank(int rank)
		{
			Data.PersonRankBehaviourData.LastRank = rank;
			SaveToLocal();
		}
		
		public void RecordLeaderBoardEndTime(DateTime time)
		{
			Data.PersonRankBehaviourData.EndTime = time;
			SaveToLocal();
		}
		
		public void RecordLeaderBoardLevelPass(int level)
		{
			Data.PersonRankBehaviourData.LevelPass = level;
			SaveToLocal();
		}
		
		public void RecordLeaderBoardScore(int score)
		{
			Data.PersonRankBehaviourData.Score = score;
			SaveToLocal();
		}
		

		public void RecordLeaderBoardLastScore(int score)
		{
			Data.PersonRankBehaviourData.LastScore = score;
			SaveToLocal();
		}
		
		public int GetLeaderBoardCurrentRank()
		{
			return Data.PersonRankBehaviourData.CurrentRank;
		}
		
		public int GetLeaderBoardLastRank()
		{
			return Data.PersonRankBehaviourData.LastRank;
		}
		
		public int GetLeaderBoardScore()
		{
			return Data.PersonRankBehaviourData.Score;
		}
		
		public int GetLeaderBoardLastScore()
		{
			return Data.PersonRankBehaviourData.LastScore;
		}

		public DateTime GetLeaderBoardEndTime()
		{
			return Data.PersonRankBehaviourData.EndTime;
		}
		
		public int GetLeaderBoardLevelPass()
		{
			return Data.PersonRankBehaviourData.LevelPass;
		}
		
		public bool HasShownTodayWelcomeMenu()
		{
			return Data.PersonRankBehaviourData.HasShownTodayWelcomeMenu;
		}
		
		public void RecordHasShownTodayWelcomeMenu(bool value)
		{
			Data.PersonRankBehaviourData.HasShownTodayWelcomeMenu = value;
			SaveToLocal();
		}
		

		#endregion
		
		#region Turntable
		public int GetTurntableNormalCoinSpinTime()
		{
			return Data.TurntableData.NormalTurntableCoinSpinTime;
		}
		
		public int GetTurntableNormalAdsSpinTime()
		{
			return Data.TurntableData.NormalTurntableAdsSpinTime;
		}
		
		public int GetTurntableNormalFreeSpinTime()
		{
			return Data.TurntableData.NormalTurntableFreeSpinTime;
		}
		
		public int GetTurntableNormalDailySpinTime()
		{
			return Data.TurntableData.NormalTurntableDailySpinTime;
		}
		
		public int GetTurntableCoinSpinGuaranteeLevel()
		{
			return Data.TurntableData.TurntableCoinSpinGuaranteeLevel;
		}
		
		public int GetTurntableCoinSpinCumulativeNumber()
		{
			return Data.TurntableData.TurntableCoinSpinCumulativeNumber;
		}
		
		public DateTime GetTurntableNextAdsSpinReadyTime()
		{
			return Data.TurntableData.TurntableNextAdsSpinReadyTime;
		}	
		
		public DateTime GetTurntableShowWarningTime()
		{
			return Data.TurntableData.TurntableShowWarningTime;
		}
		
		public void RecordTurntableNormalCoinSpinTime(int time)
		{
			Data.TurntableData.NormalTurntableCoinSpinTime = time;
			SaveToLocal();
		}
		
		public void RecordTurntableNormalAdsSpinTime(int time)
		{
			Data.TurntableData.NormalTurntableAdsSpinTime = time;
			SaveToLocal();
		}
		
		public void RecordTurntableNormalFreeSpinTime(int time)
		{
			Data.TurntableData.NormalTurntableFreeSpinTime = time;
			SaveToLocal();
		}
		
		public void RecordTurntableNormalDailySpinTime(int time)
		{
			Data.TurntableData.NormalTurntableDailySpinTime = time;
			SaveToLocal();
		}
		
		public void RecordTurntableNextAdsSpinReadyTime(DateTime time)
		{
			Data.TurntableData.TurntableNextAdsSpinReadyTime = time;
			SaveToLocal();
		}
		
		public void RecordTurntableShowWarningTime(DateTime time)
		{
			Data.TurntableData.TurntableShowWarningTime = time;
			SaveToLocal();
		}
		
		public void RecordTurntableCoinSpinGuaranteeLevel(int level)
		{
			Data.TurntableData.TurntableCoinSpinGuaranteeLevel = level;
			SaveToLocal();
		}
		
		public void RecordTurntableCoinSpinCumulativeNumber(int number)
		{
			Data.TurntableData.TurntableCoinSpinCumulativeNumber = number;
			SaveToLocal();
		}

		public bool IsFirstFreeTurnByDaliy => Data.TurntableData.IsFirstFreeByDaliy;

		public void RecordFirstFreeTrun()
		{
			if (Data.TurntableData.IsFirstFreeByDaliy)
			{
				Data.TurntableData.IsFirstFreeByDaliy = false;
				SaveToLocal();
			}
		}

		#endregion

		#region Skill

		public bool IsSkillUnlock(TotalItemType type)
		{
			if (Data.SkillLockData.IsSkillUnlocks.TryGetValue(type, out bool isUnlock))
			{
				return isUnlock;
			}

			return false;
		}

		public void RecordSkillUnlock(params TotalItemType[] types)
		{
			bool isRecord = false;
			foreach (var type in types)
			{
				if (Data.SkillLockData.IsSkillUnlocks.ContainsKey(type)&&
				    Data.SkillLockData.IsSkillUnlocks[type]!=true)
				{
					Data.SkillLockData.IsSkillUnlocks[type]=true;
					isRecord = true;
				}
			}
			if(isRecord)SaveToLocal();
		}

		#endregion

		#region CalendarChallenge

		public bool HasUsedTodayFreeChance()
		{
			return Data.CommonBehaviorData.HasUsedTodayFreeChance;
		}

		public void RecordHasUsedTodayFreeChance(bool value)
		{
			Data.CommonBehaviorData.HasUsedTodayFreeChance = value;
			SaveToLocal();
		}
		
		public bool HasShownCalendarChallengeGuide()
		{
			return Data.CommonBehaviorData.HasShownCalendarChallengeGuide;
		}
		
		public void RecordHasShownCalendarChallengeGuide(bool value)
		{
			Data.CommonBehaviorData.HasShownCalendarChallengeGuide = value;
			SaveToLocal();
		}
		
		public bool HasShownCalendarSwitchGuide()
		{
			return Data.CommonBehaviorData.HasShownCalendarSwitchGuide;
		}
		
		public void RecordHasShownCalendarSwitchGuide(bool value)
		{
			Data.CommonBehaviorData.HasShownCalendarSwitchGuide = value;
			SaveToLocal();
		}
		
		public bool DailyFirstStartLevel()
		{
			return Data.CommonBehaviorData.DailyFirstStartLevel;
		}
		
		public void RecordDailyFirstStartLevel(bool value)
		{
			Data.CommonBehaviorData.DailyFirstStartLevel = value;
			SaveToLocal();
		}
		
		
		

		#endregion

		#region ActivityPack
		public int GetTodayActivityPackShowTimes()
        {
			return Data.ActivityPackData.TodayActivityPackShowTimes;
        }

		public void RecordTodayActivityPackShowTimes(int inputShowTimes)
        {
			Data.ActivityPackData.TodayActivityPackShowTimes = inputShowTimes;
			SaveToLocal();
        }

		public bool GetEverBoughtSpecialOfferPack()
        {
			return Data.ActivityPackData.everBoughtSpecialOfferPack;
        }

		public void RecordEverBoughtSpecialOfferPack(bool value)
        {
			Data.ActivityPackData.everBoughtSpecialOfferPack = value;
			SaveToLocal();
        }

		public bool GetEverFocusedOnEventBGItemInChangeImagePanel()
		{
			return Data.ActivityPackData.everFocusedOnEventBGItemInChangeImagePanel;
		}

		public void RecordEverFocusedOnEventBGItemInChangeImagePanel(bool value)
		{
			Data.ActivityPackData.everFocusedOnEventBGItemInChangeImagePanel = value;
			SaveToLocal();
		}

		public bool GetEverFocusedOnFirstTileItemInChangeImagePanel()
		{
			return Data.ActivityPackData.everFocusedOnFirstTileItemInChangeImagePanel;
		}

		public void RecordEverFocusedOnFirstTileItemInChangeImagePanel(bool value)
		{
			Data.ActivityPackData.everFocusedOnFirstTileItemInChangeImagePanel = value;
			SaveToLocal();
		}
		#endregion
		
		#region ClimbBeanstalk

		public bool GetIsFirstShowEveryday()
		{
			return Data.ClimbBeanstalkDayData.isFirstShowEveryday;
		}
		
		public void SetIsFirstShowEveryday(bool value)
		{
			Data.ClimbBeanstalkDayData.isFirstShowEveryday = value;
			SaveToLocal();
		}

		#endregion

		#region Kitchen

		public bool GetIsFirstShowEveryday_Kitchen()
		{
			return Data.KitchenDayData.isFirstShowEveryday;
		}
		
		public void SetIsFirstShowEveryday_Kitchen(bool value)
		{
			Data.KitchenDayData.isFirstShowEveryday = value;
			SaveToLocal();
		}

		#endregion
		
		#region Frog

		public int GetCurFrogJumpActivityLevel()
		{
			return Data.FrogJumpActivityData.CurFrogJumpActivityLevel;
		}
		
		public void RecordCurFrogJumpActivityLevel(int level)
		{
			Data.FrogJumpActivityData.CurFrogJumpActivityLevel = level;
			SaveToLocal();
		}

		public DateTime GetNextAdReadyTime()
		{
			return Data.FrogJumpActivityData.NextAdReadyTime;
		}
		
		public void RecordNextAdReadyTime(DateTime time)
		{
			Data.FrogJumpActivityData.NextAdReadyTime = time;
			SaveToLocal();
		}
		
		public bool GetIsFirstStartFrogJumpActivity()
		{
			return Data.FrogJumpActivityData.IsFirstStartFrogJumpActivity;
		}
		
		public void RecordIsFirstStartFrogJumpActivity(bool value)
		{
			Data.FrogJumpActivityData.IsFirstStartFrogJumpActivity = value;
			SaveToLocal();
		}
		
		#endregion
	}
}

