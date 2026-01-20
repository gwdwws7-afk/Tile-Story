using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace MySelf.Model
{
	public class LevelModelData
	{
		public int Level=1;
		public List<int> FirstHardOrSurpLevelSuccess = new List<int>();

        public int RecordEnterGameCount = 0;//记录进入当前关卡次数
    }
	public class LevelModel : BaseModelService<LevelModel, LevelModelData>
	{
		#region Service
		public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
		{
			return new Dictionary<string, object>()
			{
				{"LevelModel.Level",Data.Level},
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
						case "LevelModel.Level": Data.Level = Convert.ToInt32(item.Value); break;
					}
				}
				SaveToLocal();
			}
		}
		#endregion
		
        public static int MaxLevel = 1000000;
		public static int TotalLevelNum = 1900;
		public static int EachChapterLevelNum = 45;
		public static int RandomLevelMin = 501;

		public int MaxChapter => MaxLevel / EachChapterLevelNum + (MaxLevel % EachChapterLevelNum == 0 ? 0 : 1);
		public int CurChapter => Data.Level / EachChapterLevelNum + (Data.Level % EachChapterLevelNum == 0 ? 0 : 1);

		public bool IsUseRecordLevelData => GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Use_Record_Level_Data,false);
		public int RealLevel(int leveNum=0)
		{
#if UNITY_EDITOR
			if (TileMatch_LevelTypePanel.NowLevel > 0)
			{
				return TileMatch_LevelTypePanel.NowLevel;
			}
#endif
			if (leveNum == 0)
				leveNum = Data.Level;
			if (leveNum > TotalLevelNum)
			{
				UnityEngine.Random.InitState(leveNum);
				//return UnityEngine.Random.Range(RandomLevelMin, TotalLevelNum + 1);
				return DTLevelUtil.GetRandomLevel(leveNum);
			}
			return leveNum;
		}

		public void SetLevelNum(int levelNum)
		{
			Data.Level = Mathf.Max(1, levelNum);
			SaveToLocal();
		}

		public void RecordFirstLevelSuccess(int levelNum)
		{
			if (!Data.FirstHardOrSurpLevelSuccess.Contains(levelNum))
			{
				Data.FirstHardOrSurpLevelSuccess.Add(levelNum);
				SaveToLocal();
			}
		}

		public bool IsFirstLevelSuccess(int levelNum)
		{
			return !Data.FirstHardOrSurpLevelSuccess.Contains(levelNum);
		}

        public void RecordEnterLevelCount()
        {
            Data.RecordEnterGameCount += 1;
            SaveToLocal();
        }

        public void ClearEnterLevelCountByWin()
        {
            Data.RecordEnterGameCount = 0;
            SaveToLocal();
        }
    }
}

