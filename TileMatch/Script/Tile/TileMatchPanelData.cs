using System.Collections.Generic;
using UnityEngine;
using MySelf.Model;
using Firebase.Analytics;

public class TileMatchPanelData
{
	public int LevelNum;
	public int TileCellNum =7;
	public bool IsUseAddSkill = false;
	public bool IsHaveRecord = false;
	public bool IsWatchLevelAds = false;

	public bool IsOldLevelData = false;
	public bool IsWatchDaliyAds = false;

	public bool IsHaveContinue = false;
    
	public List<TotalItemData> SkillItemList = new List<TotalItemData> {TotalItemData.Prop_Back,TotalItemData.Prop_ChangePos, TotalItemData.Prop_Absorb,TotalItemData.Prop_Grab };

	public TileMatch_LevelData RealLevelData;	
	public TileMatch_LevelData TileMatchData = new TileMatch_LevelData();
	public Dictionary<int, Dictionary<int, TileMoveDirectionType>> RecordRandomMoveDict = new Dictionary<int, Dictionary<int, TileMoveDirectionType>>();

	//comb
	public float LastInputTime;
	public int ClickTileCount;
	public int CombCount;

	public int RecordStartGameTime;//��¼��ʼ��Ϸ��ʱ��

	public static TileMatchPanelData InitData()
	{
		var panelData = new TileMatchPanelData();
		bool isCalendarChallenge = GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge;
		if (isCalendarChallenge)
		{
			panelData.LevelNum = GameManager.Task.CalendarChallengeManager.GetRandomLevel();
			panelData.RealLevelData = TileMatch_LevelData.GetTileMatchCalendarChallengeLevelTypeData(panelData.LevelNum);

			switch (GameManager.Task.CalendarChallengeManager.ChallengeStartType)
			{
				case 1:
					GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Watch_AD_Start, new Parameter("LevelNum", panelData.LevelNum));
					break;
				case 2:
					GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Spend_Coin_Start, new Parameter("LevelNum", panelData.LevelNum));
					break;

			}

			GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DailyChallenge_Level_Start
				, new Parameter("DailyLevel", panelData.LevelNum));
		}
		else
		{
			int realLevelNum = GameManager.PlayerData.RealLevel();
            //TileMatch_LevelData levelData = GameManager.DataNode.GetData<TileMatch_LevelData>("levelData" + realLevelNum.ToString(), null);

			panelData.LevelNum = GameManager.PlayerData.NowLevel;

			panelData.RealLevelData = TileMatch_LevelData.GetTileMatchLevelData(GameManager.PlayerData.RealLevel());
			//if (levelData == null)
			//	panelData.RealLevelData = TileMatch_LevelData.GetTileMatchLevelData(GameManager.PlayerData.RealLevel());
			//else
			//	panelData.RealLevelData = levelData;

			//放过机制
			LevelModel.Instance.RecordEnterLevelCount();
			panelData.RealLevelData = panelData.RealLevelData.GetTileMatchLevelDataByEnterCount(realLevelNum, LevelModel.Instance.Data.RecordEnterGameCount);
		}
		
		panelData.TileMatchData = TileMatchUtil.GetNewData(panelData.LevelNum, panelData.RealLevelData, ref panelData.RecordRandomMoveDict);
		if (!isCalendarChallenge)
		{
			//添加油桶
			panelData.TileMatchData.GasolineCount = TilePassModel.Instance.GetOilDrumNum();
			// panelData.TileMatchData = panelData.TileMatchData.AddOilDrumAttachItem(panelData.TileMatchData.GasolineCount);
		}
		
		panelData.LastInputTime = Time.realtimeSinceStartup;
		panelData.RecordStartGameTime = (int)Time.realtimeSinceStartup;
		return panelData;
	}

	public void SetIsHaveContinue()
	{
		IsHaveContinue = true;
	}

	public static int GetNowLevelTileTotalCount()
    {
		int level = GameManager.PlayerData.RealLevel();
		//var realLevelData = TileMatch_LevelData.GetTileMatchLevelData(level);

		// int abType = (int)GameManager.Firebase.GetLong(Constant.RemoteConfig.Use_Level_Type_Index, 0);
		// switch (abType)
		// {
		// 	case 1:
		// 		if (GeneratedTotalCounts.LevelADict.TryGetValue(level,out int resA))
		// 			return resA;
		// 		break;
		// 	case 2:
		// 		if (GeneratedTotalCounts.LevelBDict.TryGetValue(level,out int resB))
		// 			return resB;
		// 		break;
		// }
		
		if (level <= GeneratedTotalCounts.Counts.Length) 
			return GeneratedTotalCounts.Counts[level - 1];
		else
			return 0;
	}
}