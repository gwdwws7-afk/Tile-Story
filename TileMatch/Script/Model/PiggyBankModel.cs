using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MySelf.Model
{
	//小猪银行
	public class PiggyBankModelData
	{
		public int PigLevel=1;//储蓄罐档位
		public int PigTotalCoins;//当前储蓄罐积累金币数
		
		public int AddCoinByCurLevel;//当前关要加上的金币数
		[JsonIgnore]
		public bool IsMapShowPigPanel;//是否需要回到map界面展示panel

		public float RecordLastBankSliderValue=0;//记录上一次进度条进度比例

		public int GameToMapCountWithFullStatus = 0;//满状态之后从game回到home界面次数

		public DateTime DateTimeUTC=DateTime.MinValue;//记录上次首次进入界面展示小猪银行时间
	}

	public class PiggyBankModel : BaseModel<PiggyBankModel, PiggyBankModelData>
	{
		public ProductNameType PiggyBankProductNameType => (ProductNameType)Enum.Parse(typeof(ProductNameType), $"Coffer_Bank_{Data.PigLevel}");
		
		//储蓄罐 等级金币相关信息
		public static Dictionary<int, List<int>> PiggyBankContentDict = new Dictionary<int, List<int>>()
		{
			{1,new List<int>(){0,1000,3000}},
			{2,new List<int>(){0,3000,5000}},
		};

		public bool IsCanShow =>GameManager.PlayerData.NowLevel>=12
		                        && AdsModel.Instance.IsCanShowPiggyBank;//12级解锁小猪银行

		public List<int> PiggyBankDataByCurLevel => PiggyBankContentDict[Data.PigLevel];
		
		public float CurSliderNum=>Data.PigTotalCoins/(float)PiggyBankDataByCurLevel.LastOrDefault();//进度

		public float ActiveBuyBtnSliderValue => PiggyBankDataByCurLevel[1] / (float)PiggyBankDataByCurLevel[2];

		public bool IsPiggyBankFull => Data.PigTotalCoins >= PiggyBankDataByCurLevel.LastOrDefault();

		public bool IsCanBuy => Data.PigTotalCoins >= PiggyBankDataByCurLevel.FirstOrDefault();

		public bool IsShowAutoOpenPanelByGameToMap =>
			IsCanShow
			&&((Data.GameToMapCountWithFullStatus > 0 && Data.GameToMapCountWithFullStatus % 5 == 0)||Data.IsMapShowPigPanel);

		public bool IsShowAutoOpenPanelByMenuToMap =>
			IsCanShow
			&&IsPiggyBankFull
			&&!IsHaveRecordToDay;

		private bool IsHaveRecordToDay
		{
			get
			{
				var localDateTime = Data.DateTimeUTC.ToLocalTime();
				return
					new DateTime(localDateTime.Year,localDateTime.Month, localDateTime.Day)>= DateTime.Today;
			}
		}

		public int GetBankStatusBySliderValue(float sliderValue)
		{
			int status = 1;
			if (sliderValue > 0)
			{
				status = 2;
			}

			if (sliderValue >= 1)
			{
				status = 3;
			}
			return status;
		}

		public int GetBankStatus()
		{
			int status = 1;
			if (Data.PigTotalCoins > 0)
			{
				status = 2;
			}
			if (Data.PigTotalCoins >= PiggyBankDataByCurLevel.LastOrDefault())
			{
				status = 3;
			}
			return status;
		}


		//记录金币
		public void RecordAddCoinToTotalCoin()
		{
			Data.PigTotalCoins =Math.Min(Data.PigTotalCoins+Data.AddCoinByCurLevel,PiggyBankDataByCurLevel.LastOrDefault());
			Data.AddCoinByCurLevel = 0;
			SaveToLocal();
		}

		//胜利时记录 可获取的金币数量
		public void RecordCoinByLevelWin(int levelNum)
		{
			if(!IsCanShow)return;
			
			int hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel(levelNum));
			int coinNum = 100+100*hardIndex;

			Log.Info($"PiggyBank: RecordCoinByLevelWin||CoinNum:{coinNum}");
			Data.AddCoinByCurLevel = coinNum;

			//记录储蓄罐满的状态
			int fullCoinNum = PiggyBankDataByCurLevel.LastOrDefault();
			if (Data.PigTotalCoins<fullCoinNum
			    &&Data.PigTotalCoins + Data.AddCoinByCurLevel >= PiggyBankDataByCurLevel.LastOrDefault())
			{
				//标记回到map展示弹框
				Data.IsMapShowPigPanel = true;
				Data.DateTimeUTC = DateTime.Now;
			}
			SaveToLocal();
		}

		//购买之后升级 储蓄罐等级
		public void UpgradePigLevelByBuySuccess()
		{
			Data.PigLevel = Math.Min(Data.PigLevel + 1, PiggyBankContentDict.Count);
			Data.PigTotalCoins = 0;
			Data.AddCoinByCurLevel = 0;
			Data.RecordLastBankSliderValue = 0;
			Data.GameToMapCountWithFullStatus=0;
			Data.DateTimeUTC = DateTime.MinValue;
			Data.IsMapShowPigPanel = false;
			SaveToLocal();
		}

		//记录上一次 进度条进度
		public void RecordLastSliderValue()
		{
			Data.RecordLastBankSliderValue = CurSliderNum;
			SaveToLocal();
		}

		public void RecordGameToMapCountWithFullStatus()
		{
			if(!IsCanShow)return;
			
			if (IsPiggyBankFull)
			{
				Data.GameToMapCountWithFullStatus++;
				SaveToLocal();
			}
		}

		public void AddCoinPigTotalCoins1000()
		{
			Data.PigTotalCoins =Math.Min(1000+Data.PigTotalCoins,PiggyBankDataByCurLevel.LastOrDefault());
			SaveToLocal();
		}
		
		public void AddCoinPigTotalCoins100()
		{
			Data.PigTotalCoins =Math.Min(100+Data.PigTotalCoins,PiggyBankDataByCurLevel.LastOrDefault());
			SaveToLocal();
		}
	}
}

