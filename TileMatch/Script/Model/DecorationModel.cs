using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using UnityEngine;
using Newtonsoft.Json;

namespace MySelf.Model
{
    public class DecorationModelData
    {
		public int DecorationAreaID = 1;//当前处在的章节ID
        public Dictionary<int, bool> areaGetRewardDic = new Dictionary<int, bool>();//(Alter前)AreaID -> 是否已获得装修奖励
        public Dictionary<int, int> decorateIDTypeDic = new Dictionary<int, int>();//DecorateID -> 装修类型(0未装修，1+已装修)
        public Dictionary<int, long> areaFinishedTimeStampDic = new Dictionary<int, long>();//(Alter前)AreaID -> 完成时的timeStamp inSeconds
        
        public int DecorationOperatingAreaID = 0;//1.11.3 版本修改了解锁区域的流程 可能出现玩家数据上已经解锁了 Area_N 但他装修界面还停留在 Area_N-1 的情况
        public bool NeedToShowDecorationViewAnim = false;

        //满足某些条件后标记下来，之后不再向Firebase请求，只从我们缓存的值中获取
        //1 获取到了exchangeArea_9_10 = true
        //2 DecorationAreaID >= 9;
        public bool areaSequenceIsPermenant = false;
        public bool exchangeArea_9_11 = false;

        public int tryToOverrideDecorationID;//用于在章节列表额外的保护 避免玩家因为动画卡住而无法进入下一章节
	}

    public class DecorationModel : BaseModelService<DecorationModel, DecorationModelData>
    {
        #region SERVICE
        public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
        {
            return new Dictionary<string, object>()
            {
                {"DecorationModel.DecorationAreaID",Data.DecorationAreaID},
                {"DecorationModel.areaGetRewardDic",JsonConvert.SerializeObject(Data.areaGetRewardDic)},
                {"DecorationModel.decorateIDTypeDic",JsonConvert.SerializeObject(Data.decorateIDTypeDic)},
                {"DecorationModel.DecorationOperatingAreaID",Data.DecorationOperatingAreaID},
                {"DecorationModel.NeedToShowDecorationViewAnim",Data.NeedToShowDecorationViewAnim},
                {"DecorationModel.areaSequenceIsPermenant",Data.areaSequenceIsPermenant},
                {"DecorationModel.exchangeArea_9_11",Data.exchangeArea_9_11},
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
                        case "DecorationModel.DecorationAreaID": Data.DecorationAreaID = Convert.ToInt32(item.Value); break;
                        case "DecorationModel.areaGetRewardDic": Data.areaGetRewardDic = JsonConvert.DeserializeObject<Dictionary<int, bool>>((string)item.Value); break;
                        case "DecorationModel.decorateIDTypeDic": Data.decorateIDTypeDic = JsonConvert.DeserializeObject<Dictionary<int, int>>((string)item.Value); break;
                        case "DecorationModel.DecorationOperatingAreaID": Data.DecorationOperatingAreaID = Convert.ToInt32(item.Value); break;
                        case "DecorationModel.NeedToShowDecorationViewAnim": Data.NeedToShowDecorationViewAnim = Convert.ToBoolean(item.Value); break;
                        case "DecorationModel.areaSequenceIsPermenant": Data.areaSequenceIsPermenant = Convert.ToBoolean(item.Value); break;
                        case "DecorationModel.exchangeArea_9_11": Data.exchangeArea_9_11 = Convert.ToBoolean(item.Value); break;
                    }
                }
                SaveToLocal();
            }
        }
        #endregion

        public bool inAreaCompleteAnim = false;
        private int tempDecoratingOperatingAreaID = 0;//这个提供给DecorationViewPanel上临时点出来想查看某Area时使用 只记内存不持久化！
        private bool everGetRemoteConfigForAreaSequence = false;//临时标记位，希望 RemoteConfig.Decoration_ExchangeArea_9_10 的获取，在一次启动时只进行一次。这万一游戏跑到一半值变了可受不了
        public int TempDecoratingOperatingAreaID => tempDecoratingOperatingAreaID;

        #region Set
        public void SetDecorationAreaID(int inputDecorationAreaID)
		{
            inputDecorationAreaID = Mathf.Min(Constant.GameConfig.MaxDecorationArea, inputDecorationAreaID);
			if (Data.DecorationAreaID == inputDecorationAreaID)
				return;

            if (inputDecorationAreaID >= 9)
                Data.areaSequenceIsPermenant = true;

			Data.DecorationAreaID = inputDecorationAreaID;
            //GameManager.Event.Fire(this, ChangeDecorationAreaEventArgs.Create(inputDecorationAreaID));
            SaveToLocal();
		}


        public void SetDecorationType(int areaID, int index, int value)
        {
            DecorateItem decorateItem = GetTargetDecorationItem(areaID, index);
            if (Data.decorateIDTypeDic.ContainsKey(decorateItem.ID))
            {
                Data.decorateIDTypeDic[decorateItem.ID] = value;
            }
            else
            {
                Data.decorateIDTypeDic.Add(decorateItem.ID, value);
            }
            SaveToLocal();
        }

        //传入Alternative前的ID
        public void SetTargetAreaGetReward(int areaID, bool value)
        {
            if (!Data.areaGetRewardDic.ContainsKey(areaID))
                Data.areaGetRewardDic.Add(areaID, value);
            else
                Data.areaGetRewardDic[areaID] = value;

            long timeStampInSeconds = GetRecentTimeStampInSecounds();
            if (!Data.areaFinishedTimeStampDic.ContainsKey(areaID))
                Data.areaFinishedTimeStampDic.Add(areaID, timeStampInSeconds);
            else
                Data.areaFinishedTimeStampDic[areaID] = timeStampInSeconds;
            SaveToLocal();
        }

        public bool SetDecorationOperatingAreaID(int inputID)
        {
            inputID = Mathf.Min(Constant.GameConfig.MaxDecorationArea, inputID);
            if (Data.DecorationOperatingAreaID == inputID)
                return false;

            Data.DecorationOperatingAreaID = inputID;
            GameManager.Event.Fire(this, ChangeDecorationAreaEventArgs.Create(inputID));
            SaveToLocal();

            return true;
        }

        public void SetTryToOverrideDecorationAreaID(int inputID)
        {
            Data.tryToOverrideDecorationID = inputID;
            SaveToLocal();
        }
        #endregion

        public bool NeedToShowDecorationViewAnim
        {
            get => Data.NeedToShowDecorationViewAnim;
            set
            {
                Data.NeedToShowDecorationViewAnim = value;
                SaveToLocal();
            }
        }

        public static string GetAreaNameById(int id)
        {
            return $"DecorationArea_{id}";
        }

        public string GetNowAreaResourceName()
        {
            int result = GetDecorationOperatingAreaID();
            if (result <= 0) result = 1;
            return $"DecorationArea_{result}";
        }

        public List<string> GetNeedDownloadByMenu()
        {
            List<string> list = new List<string>();
            int result = Data.DecorationAreaID;
            result = Mathf.Min(result, Constant.GameConfig.MaxDecorationArea);
            if (result <= 0) result = 1;

            //转换一次
            int newResult = GetAlteredDecorationAreaID(result);
            list.Add(GetAreaNameById(newResult));
            if (result < Constant.GameConfig.MaxDecorationArea)
            {
                newResult = GetAlteredDecorationAreaID(result + 1);
                list.Add(GetAreaNameById(newResult));
            }
            return list;
        }

        public List<string> GetNeedDownNameList(bool isContainNext, int addIndex)
        {
            List<string> list = new List<string>();
            int result = GetDecorationOperatingAreaIDByNoAlter();//Data.DecorationAreaID;

            for (int i = 1; i <= result + (isContainNext ? addIndex : 0); i++)
            {
                if (i <= Constant.GameConfig.MaxDecorationArea)
                {
                    //做一次转换
                    int resId = GetAlteredDecorationAreaID(i);
                    if (resId < Constant.GameConfig.StartNeedDownloadArea)
                        continue;
                    list.Add(GetAreaNameById(resId));
                }
            }
            return list;
        }

        /// <summary>
        /// 获取当前 正在操作的AreaID
        /// 如有 tempDecoratingOperatingAreaID，则使用它
        /// 否则如有 Data.DecorationOperatingAreaID, 则使用它
        /// 再否则 就使用 Data.DecorationAreaID
        /// 最后 响应一些ABTest调整整体章节顺序的配置 再做一次变换
        /// </summary>
        /// <returns></returns>
        public int GetDecorationOperatingAreaID()
        {
            int returnValue = GetDecorationOperatingAreaIDByNoAlter();
            returnValue = GetAlteredDecorationAreaID(returnValue);
            return returnValue;
        }

        public int GetDecorationOperatingAreaIDByNoAlter()
        {
            int returnValue = 0;
            if (tempDecoratingOperatingAreaID > 0)
                returnValue = Mathf.Min(tempDecoratingOperatingAreaID, Data.DecorationAreaID);
            else if (Data.DecorationOperatingAreaID > 0)
                returnValue = Mathf.Min(Data.DecorationOperatingAreaID, Data.DecorationAreaID);
            else
            {
                Data.DecorationOperatingAreaID = Data.DecorationAreaID;
                returnValue = Data.DecorationAreaID;
            }
            return returnValue;
        }

        public int GetTargetDecorationType(int areaId, int index)
        {
            DecorateItem decorateItem = GetTargetDecorationItem(areaId, index);
            if (decorateItem == null)
            {
                //Log.Error($"Unexpected areaId = {areaId} index = {index}");
                return -1;
            }
            if (Data.decorateIDTypeDic.ContainsKey(decorateItem.ID))
            {
                return Data.decorateIDTypeDic[decorateItem.ID];
            }
            else
            {
                Data.decorateIDTypeDic.Add(decorateItem.ID, 0);
                return 0;
            }
        }

        public int GetTargetDecorationType(int decorateItemID)
        {
            DecorateItem decorateItem = GetTargetDecorationItem(decorateItemID);
            return GetTargetDecorationType(decorateItem);
        }

        private int GetTargetDecorationType(DecorateItem decorateItem)
        {
            if (Data.decorateIDTypeDic.ContainsKey(decorateItem.ID))
            {
                return Data.decorateIDTypeDic[decorateItem.ID];
            }
            else
            {
                Data.decorateIDTypeDic.Add(decorateItem.ID, 0);
                return 0;
            }
        }

        public bool GetTargetDecorationItemIsUnlocked(DecorateItem targetItem)
        {
            //如果自己已装修 不认为是Unlock状态
            int currentType = GetTargetDecorationType(targetItem);
            if (currentType > 0)
                return false;

            //所有前置Require都已装修 或根本没有前置要求
            if (targetItem.Require == null)
                return true;

            for (int i = 0; i < targetItem.Require.Length; ++i)
            {
                int decorationType = GetTargetDecorationType(targetItem.Require[i]);
                if (decorationType <= 0)
                    return false;
            }
            return true;
        }

        public DecorateItem GetTargetDecorationItem(int areaId, int position)
        {
            //传入的需要是Alter后的
            List<DecorateItem> itemList = GameManager.DataTable.GetDataTable<DTDecorateItem>().Data.GetDecorateItems(areaId);
            if (position >= itemList.Count)
                return null;
            return itemList[position];
        }

        public DecorateItem GetTargetDecorationItem(int decorateItemID)
        {
            return GameManager.DataTable.GetDataTable<DTDecorateItem>().Data.GetDecorateItem(decorateItemID);
        }

        //传入Alter前的ID
        public bool CheckTargetAreaIsComplete(int areaID)
        {
            areaID = GetAlteredDecorationAreaID(areaID);
            var items = GameManager.DataTable.GetDataTable<DTDecorateItem>().Data.GetDecorateItems(areaID);
            for (int i = 0; i < items.Count; i++)
            {
                int result;
                Data.decorateIDTypeDic.TryGetValue(items[i].ID, out result);
                if (result == 0)
                {
                    return false;
                }
            }
            return true;
        }

        //传入Alter前的ID
        public bool GetTargetAreaGetReward(int targetAreaID)
        {
            bool returnValue = false;
            Data.areaGetRewardDic.TryGetValue(targetAreaID, out returnValue);
            return returnValue;
        }

        //传入Alter前的ID
        public long GetTargetAreaGetRewardTime(int targetAreaID)
        {
            long returnValue = -1;
            long recentTimeStamp = GetRecentTimeStampInSecounds();
            if (GetTargetAreaGetReward(targetAreaID))
            {
                if(!Data.areaFinishedTimeStampDic.ContainsKey(targetAreaID))
                    UpdateAllAreaFinishedTimeStamp();

                return Data.areaFinishedTimeStampDic[targetAreaID];
            }
            return returnValue;
        }

        /// <summary>
        /// 对于加入这个机制之前就建立的旧账号 发现问题时把所有 已完成但未记录完成时间的章节 都更新成当前时间
        /// </summary>
        private void UpdateAllAreaFinishedTimeStamp()
        {
            long recentTimeStamp = GetRecentTimeStampInSecounds();
            foreach (KeyValuePair<int,bool> pair in Data.areaGetRewardDic)
            {
                if(!Data.areaFinishedTimeStampDic.ContainsKey(pair.Key))
                {
                    Data.areaFinishedTimeStampDic.Add(pair.Key, recentTimeStamp);
                }
            }
            SaveToLocal();
        }

        //传入Alter后ID
        public int GetTargetAreaMaxDecorationCount(int targetAreaID)
        {
            //需要是Alter后ID
            List<DecorateItem> itemList = GameManager.DataTable.GetDataTable<DTDecorateItem>().Data.GetDecorateItems(targetAreaID);
            return itemList.Count;
        }

        //传入Alter后ID
        public int GetTargetAreaFinishedDecorationCount(int targetAreaID)
        {
            //需要是Alter后ID
            List<DecorateItem> itemList = GameManager.DataTable.GetDataTable<DTDecorateItem>().Data.GetDecorateItems(targetAreaID);

            int maxCount = itemList.Count;
            int currentCount = 0;
            for (int i = 0; i < maxCount; ++i)
            {
                if (GetTargetDecorationType(itemList[i].ID) > 0)
                {
                    currentCount++;
                }
            }
            return currentCount;
        }

        public int GetOperatingAreaCanDecorateObjectNum()
        {
            int curStarNum = ItemModel.Instance.GetItemTotalNum(TotalItemData.Star.TotalItemType);
            int totalCanDecorateObjectNum = 0;
            List<DecorateItem> itemList = GameManager.DataTable.GetDataTable<DTDecorateItem>().Data.GetDecorateItems(GetDecorationOperatingAreaID());
            List<int> costList = new List<int>();
            for (int i = 0; i < itemList.Count; ++i)
            {
                if (GetTargetDecorationItemIsUnlocked(itemList[i]))
                {
                    costList.Add(itemList[i].Cost);
                }
            }
            costList.Sort();

            for (int i = 0; i < costList.Count; ++i)
            {
                if (curStarNum >= costList[i])
                {
                    curStarNum -= costList[i];
                    totalCanDecorateObjectNum++;
                }
            }

            return totalCanDecorateObjectNum;
        }

        /// <summary>
        /// 返回最大的已装修完毕的区域ID
        /// </summary>
        /// <returns></returns>
        public int GetHighestFinishedDecorationAreaID()
        {
            //以areaGetRewardDic (是否领奖) 为准
            for (int i = Constant.GameConfig.MaxDecorationArea; i > 0; --i)
            {
                bool isFinished = GetTargetAreaGetReward(i);
                if (isFinished)
                    return i;
            }
            return 0;
        }

        /// <summary>
        /// 返回最小的标记为ComingSoon的区域ID 暂时认为就是Constant.GameConfig.MaxDecorationArea + 1
        /// </summary>
        /// <returns></returns>
        public int GetMinCommingSoonAreaID()
        {
            return Constant.GameConfig.MaxDecorationArea + 1;
        }

        /// <summary>
        /// 标记当前区域为已领奖 （实际领奖 及 弹出领奖界面）
        /// </summary>
        public void CompleteNowArea(int starNum)
        {
            if (starNum != 0) 
                GameManager.Firebase.RecordMessageByEvent($"{Constant.AnalyticsEvent.Decoration_Complete_}{Data.DecorationAreaID}", new Parameter("StarNum", starNum));
            GameManager.Firebase.RecordMessageByEvent
                (Constant.AnalyticsEvent.Decoration_Reward, 
                new Parameter("ChapterNum",Data.DecorationAreaID));

            DecorateArea data = GameManager.DataTable.GetDataTable<DTDecorateArea>().Data.GetDecorateArea((area) => area.ID == Data.DecorationAreaID);

            foreach (var rewardData in data.Reward)
            {
                RewardManager.Instance.AddNeedGetReward(TotalItemData.FromInt(rewardData.itemId), rewardData.number);
                //GameManager.Firebase.RecordLevelToolsGet("Decorate", (TotalItemType)rewardData.itemId, rewardData.number);
            }

            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DecorationChestRewardPanel, false, () =>
            {
                inAreaCompleteAnim = false;
                GameManager.Process.EndProcess(ProcessType.CheckDecorationAreaCompleteReward);

                if (GameManager.UI.HasUIForm("DecorationOperationPanel"))
                {
                    DecorationOperationPanel decorationOperationPanel = (DecorationOperationPanel)GameManager.UI.GetUIForm("DecorationOperationPanel");
                    if (decorationOperationPanel != null)
                        decorationOperationPanel.UnregisterInAnimReason(DecorationOperationPanel.InAnimReason.AreaComplete);
                }
            });
        }

        public int GetOperatingAreaFinishDecortionCount()
        {
            return GetTargetAreaFinishedDecorationCount(GetDecorationOperatingAreaID());
        }


        #region TempDecoratingOperatingAreaID
        /// <summary>
        /// 给浏览旧装修区域使用 记录临时的tempDecoratingOperatingAreaID 不持久化
        /// </summary>
        /// <param name="inputID"></param>
        public void SetTempDecoratingOperatingAreaID(int inputID)
        {
            inputID = Mathf.Min(Constant.GameConfig.MaxDecorationArea, inputID);
            if (tempDecoratingOperatingAreaID == inputID)
                return;

            tempDecoratingOperatingAreaID = inputID;
            GameManager.Event.Fire(this, ChangeDecorationAreaEventArgs.Create(GetDecorationOperatingAreaID()));
        }
        public void ClearTempDecoratingOperatingAreaID()
        {
            tempDecoratingOperatingAreaID = 0;
            GameManager.Event.Fire(this, ChangeDecorationAreaEventArgs.Create(GetDecorationOperatingAreaID()));
        }
        #endregion

        public int GetAlteredDecorationAreaID(int inputID)
        {
            return inputID;
        }

        public int GetTryToOverrideDecorationAreaID()
        {
            return Data.tryToOverrideDecorationID;
        }

        private long GetRecentTimeStampInSecounds()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }
}
