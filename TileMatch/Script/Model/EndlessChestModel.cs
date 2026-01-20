using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;

namespace MySelf.Model
{
	public class EndlessChestData
	{
		public bool IsAutoOpenPanel = false;//当期是否自动弹出过
		public int ActivityId=0;//活动id
		public int CurChestId = 1;//当前操作的奖励Id
		public int CurBuyChestId=0;//当前购买过的无尽宝箱id
		
		public bool IsPurchase => CurBuyChestId>0;//是否购买过无尽宝箱


		public List<int> RecordOverActivityIds = new List<int>();
		public int LastActivityId=0;
		public Dictionary<int, int> RecordFreeRewardDict;//每次领取之后记录一下可以免费领取的奖励，用来发放[不用totalItemData是因为序列化问题]
		public Dictionary<int, int> CardFreeRewardDict;
	}

	public class EndlessChestModel:BaseModel<EndlessChestModel,EndlessChestData>
	{
		public bool IsEndlessUnlock => GameManager.PlayerData.NowLevel >= 49;

		public bool IsHaveOverById(int id) => Data.RecordOverActivityIds.Contains(id);

		public Dictionary<TotalItemData, int> GetRcordFreeRewards()
		{
			var dict=(CardModel.Instance.IsInCardActivity && Data.CardFreeRewardDict!=null)? Data.CardFreeRewardDict:Data.RecordFreeRewardDict;
			if (dict != null)
			{
				return dict.ToDictionary(obj => TotalItemData.FromInt(obj.Key), obj => obj.Value);
			}
			return null;
		}

		//结算
		public void Balance()
		{
			if(Data.LastActivityId!=0)
				Data.RecordOverActivityIds.Add(Data.LastActivityId);
			else if(Data.LastActivityId==0&&Data.ActivityId>0)
			{
				Data.RecordOverActivityIds.Add(Data.ActivityId);
			}
			Data.IsAutoOpenPanel = false;
			Data.ActivityId = 0;
			Data.CurChestId = 1;
			Data.CurBuyChestId = 0;
			Data.RecordFreeRewardDict=null;
			Data.CardFreeRewardDict=null;
			Data.LastActivityId = 0;
			
			SaveToLocal();
			GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.EndlessBalance));
		}
		
		//记录购买过的chestid
		public void RecordBuyChestId(int buyChestId)
		{
			Data.CurBuyChestId = buyChestId;
			SaveToLocal();
		}

		//记录领取过的chestid
		public void RecordGetRewardChestId(int layerIndex)
		{
			Data.CurChestId = layerIndex+1;
			GameManager.Firebase.RecordMessageByEvent("Endless_Treasure_Claim",new Parameter("stage",layerIndex));
			SaveToLocal();
		}

		//记录可以领取的奖励
		public void RecordCanGetRewards(Dictionary<TotalItemData,int> freeRewardDict, Dictionary<TotalItemData,int> cardFreeRewardDict)
		{
			//广播消息更新了 待领取奖励信息
			Data.RecordFreeRewardDict = GetDictBy(freeRewardDict);
			Data.CardFreeRewardDict = CardModel.Instance.IsInCardActivity ? GetDictBy(cardFreeRewardDict) : null;

			GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.EndlssGetFreeReward,null));
			SaveToLocal();
		}

		public void RecordActivityId(int activityId)
		{
			if (Data.ActivityId != activityId)
			{
				Data.LastActivityId = Data.ActivityId;
				Data.ActivityId = activityId;
				SaveToLocal();
			}
		}

		public void RecordAutoOpen()
		{
			Data.IsAutoOpenPanel = true;
			SaveToLocal();
		}

		public Dictionary<int, int> GetDictBy(Dictionary<TotalItemData,int> dict)
		{
			if (dict == null) return null;
			return dict.ToDictionary(obj=>obj.Key.ID,obj=>obj.Value);
		}
	}
}

