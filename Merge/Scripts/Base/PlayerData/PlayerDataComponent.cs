using GameFramework;
using MySelf.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 合成玩家数据模块
    /// </summary>
    public sealed class PlayerDataComponent : MonoBehaviour
    {
        private const string ActivityPrefix = "Merge_";

        /// <summary>
        /// 获取活动预告开启关卡
        /// </summary>
        public int GetActivityPreviewLevel()
        {
            return 12;
        }

        /// <summary>
        /// 获取活动解锁关卡
        /// </summary>
        public int GetActivityUnlockLevel()
        {
            return (int)GameManager.Firebase.GetDouble(Constant.RemoteConfig.Event_Merge_UnlockLevel, 21);
        }

        #region 活动配置

        public int GetSavedPeriod()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "SavedPeriod", 0);
        }

        public void SetSavedPeriod(int period)
        {
            PlayerPrefs.SetInt(ActivityPrefix + "SavedPeriod", period);
        }

        #endregion

        /// <summary>
        /// 获取开启活动的次数
        /// </summary>
        public int GetOpenActivityTime()
        {
            return PlayerPrefs.GetInt(ActivityPrefix + "OpenActivityTime", 0);
        }

        /// <summary>
        /// 增加开启活动的次数
        /// </summary>
        public void AddOpenActivityTime()
        {
            PlayerPrefs.SetInt(ActivityPrefix + "OpenActivityTime", GetOpenActivityTime() + 1);
        }

        /// <summary>
        /// 获取道具布局数据字段
        /// </summary>
        public string GetSavedPropDistributedMap()
        {
            return MergeModel.Instance.SavedPropDistributedMap;
        }

        /// <summary>
        /// 设置道具布局数据字段
        /// </summary>
        public void SetSavedPropDistributedMap(string newMap)
        {
            MergeModel.Instance.SavedPropDistributedMap = newMap;
        }

        /// <summary>
        /// 获取当前合成的最大等级道具
        /// </summary>
        public int GetCurrentMaxMergeStage()
        {
            return MergeModel.Instance.CurrentMaxMergeStage;
        }

        /// <summary>
        /// 设置当前合成的最大等级道具
        /// </summary>
        public void SetCurrentMaxMergeStage(int stage)
        {
            MergeModel.Instance.CurrentMaxMergeStage = stage;
        }

        public void SetPropIsUnlock(int propId)
        {
            PlayerPrefs.SetInt("PropIsUnlock" + propId.ToString(), 1);
        }

        public bool GetPropIsUnlock(int propId)
        {
            return PlayerPrefs.GetInt("PropIsUnlock" + propId.ToString(), 0) == 1;
        }

        public void ClearPropIsUnlock()
        {
            //将所有道具设为未解锁
            IDataTable<DRProp> propDataTable = MergeManager.DataTable.GetDataTable<DRProp>(MergeManager.Instance.GetMergeDataTableName());
            foreach (var item in propDataTable)
            {
                PlayerPrefs.SetInt("PropIsUnlock" + item.Id.ToString(), 0);
            }
        }

        /// <summary>
        /// 获取所有的存储道具编号
        /// </summary>
        /// <returns>所有的存储道具编号集合</returns>
        public List<StoredProp> GetAllStorePropIds()
        {
            List<StoredProp> result = new List<StoredProp>();
            string str = MergeModel.Instance.StorePropIds;
            if (!string.IsNullOrEmpty(str))
            {
                string[] splitedStr = str.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < splitedStr.Length; i++)
                {
                    string[] savedPropsSplits = splitedStr[i].Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.TryParse(savedPropsSplits[0], out int id))
                    {
                        if (savedPropsSplits.Length > 1)
                        {
                            var data = ReferencePool.Acquire<PropSavedData>();
                            data.Load(savedPropsSplits[1]);
                            result.Add(new StoredProp(id, data));
                        }
                        else
                        {
                            result.Add(new StoredProp(id));
                        }
                    }
                    else
                    {
                        Log.Error("Get Store Prop Id Error.{0}", splitedStr[i]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 增加存储道具
        /// </summary>
        /// <param name="propId">存储道具的编号</param>
        /// <param name="propNum">存储道具数量</param>
        /// <param name="data">道具数据</param>
        public void AddStorePropId(int propId, int propNum, PropSavedData data = null)
        {
            string str = MergeModel.Instance.StorePropIds;
            for (int i = 0; i < propNum; i++)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str = str + ":" + propId.ToString();
                }
                else
                {
                    str = propId.ToString();
                }

                if (data != null)
                {
                    string savedString = data.Save();
                    if (savedString.Contains("#") || savedString.Contains("$"))
                    {
                        Log.Error("SavePropDistributedMap Fail - saved string {0} is invalid", savedString);
                    }
                    else
                    {
                        str = str + "#" + savedString;
                    }
                }
            }

            MergeModel.Instance.StorePropIds = str;
        }

        /// <summary>
        /// 移除最后一个存储道具
        /// </summary>
        public void RemoveLastStorePropId()
        {
            string str = MergeModel.Instance.StorePropIds;
            int index = str.LastIndexOf(":");
            if (index < 0)
            {
                MergeModel.Instance.StorePropIds = null;
            }
            else
            {
                str = str.Substring(0, index);
                MergeModel.Instance.StorePropIds = str;
            }
        }

        /// <summary>
        /// 获取合成体力数
        /// </summary>
        public int GetMergeEnergyBoxNum()
        {
            //return MergeModel.Instance.MergeEnergyBoxNum;
            return GameManager.PlayerData.GetCurItemNum(TotalItemData.MergeEnergyBox);
        }

        public void AddMergeEnergyBoxNum(int addNum)
        {
            //MergeModel.Instance.MergeEnergyBoxNum = GetMergeEnergyBoxNum() + addNum;
            GameManager.PlayerData.AddItemNum(TotalItemData.MergeEnergyBox, addNum);
        }

        public void SubtractMergeEnergyBoxNum(int subNum)
        {
            //MergeModel.Instance.MergeEnergyBoxNum = GetMergeEnergyBoxNum() - subNum;
            GameManager.PlayerData.UseItem(TotalItemData.MergeEnergyBox, subNum, false);
        }

        /// <summary>
        /// 获取合成体力关卡收集数
        /// </summary>
        public int GetMergeEnergyBoxLevelCollectNum()
        {
            return MergeModel.Instance.MergeEnergyBoxLevelCollectNum;
        }

        public void AddMergeEnergyBoxLevelCollectNum(int addNum)
        {
            MergeModel.Instance.MergeEnergyBoxLevelCollectNum = GetMergeEnergyBoxLevelCollectNum() + addNum;
        }

        public void ClearMergeEnergyBoxLevelCollectNum()
        {
            MergeModel.Instance.MergeEnergyBoxLevelCollectNum = 0;
        }

        /// <summary>
        /// 获取打开最终奖励宝箱的次数
        /// </summary>
        public int GetFinalRewardTime()
        {
            return MergeModel.Instance.GetFinalRewardTime;
        }

        public void AddGetFinalRewardTime()
        {
            MergeModel.Instance.GetFinalRewardTime = GetFinalRewardTime() + 1;
        }

        /// <summary>
        /// 获取当前无尽宝箱领取等级
        /// </summary>
        public int GetCurMergeOfferLevel()
        {
            return MergeModel.Instance.CurMergeOfferLevel;
        }

        public void SetCurMergeOfferLevel(int level)
        {
            MergeModel.Instance.CurMergeOfferLevel = level;
        }

        /// <summary>
        /// 看广告获取气泡道具的次数
        /// </summary>
        public int GetShowAdsGetBubbleTime()
        {
            return MergeModel.Instance.ShowAdsGetBubbleTime;
        }

        public void AddShowAdsGetBubbleTime()
        {
            MergeModel.Instance.ShowAdsGetBubbleTime++;
        }

        public void ClearShowAdsGetBubbleTime()
        {
            MergeModel.Instance.ShowAdsGetBubbleTime = 0;
        }

        public DateTime GetBubbleBreakNextAdsReadyTime()
        {
            return MergeModel.Instance.BubbleBreakNextAdsReadyTime;
        }

        public void SetBubbleBreakNextAdsReadyTime(DateTime time)
        {
            MergeModel.Instance.BubbleBreakNextAdsReadyTime = time;
        }

        public DateTime GetGetBoxNextAdsReadyTime()
        {
            return MergeModel.Instance.GetBoxNextAdsReadyTime;
        }

        public void SetGetBoxNextAdsReadyTime(DateTime time)
        {
            MergeModel.Instance.GetBoxNextAdsReadyTime = time;
        }

        #region LoveGiftBattle

        public int GetLoveGiftRewardStage()
        {
            return PlayerPrefs.GetInt("MergeLoveGiftRewardStage", 1);
        }

        public void SetLoveGiftRewardStage(int stage)
        {
            PlayerPrefs.SetInt("MergeLoveGiftRewardStage", stage);
        }

        public int GetCurLoveGiftRewardProgress()
        {
            return PlayerPrefs.GetInt("CurLoveGiftRewardProgress", 0);
        }

        public void SetCurLoveGiftRewardProgress(int progress)
        {
            PlayerPrefs.SetInt("CurLoveGiftRewardProgress", progress);
        }

        #endregion

        #region DigTreasure

        public int GetDigTreasureRewardStage()
        {
            return PlayerPrefs.GetInt("DigTreasureRewardStage", 1);
        }

        public void SetDigTreasureRewardStage(int stage)
        {
            PlayerPrefs.SetInt("DigTreasureRewardStage", stage);
        }

        public int GetDigTreasureRewardProgress()
        {
            return PlayerPrefs.GetInt("DigTreasureRewardProgress", 0);
        }

        public void SetDigTreasureRewardProgress(int progress)
        {
            PlayerPrefs.SetInt("DigTreasureRewardProgress", progress);
        }

        public int GetDigTreasureCurDepth()
        {
            return PlayerPrefs.GetInt("DigTreasureCurDepth", 0);
        }

        public void SetDigTreasureCurDepth(int depth)
        {
            PlayerPrefs.SetInt("DigTreasureCurDepth", depth);
        }

        public int GetDigTreasureStageDepth()
        {
            return PlayerPrefs.GetInt("DigTreasureStageDepth", 0);
        }

        public void SetDigTreasureStageDepth(int depth)
        {
            PlayerPrefs.SetInt("DigTreasureStageDepth", depth);
        }

        #endregion

        #region Christmas
        
        public int GetChristmasDecorationStage()
        {
            return PlayerPrefs.GetInt("ChristmasDecorationStage2024", 0);
        }

        public void SetChristmasDecorationStage(int stage)
        {
            PlayerPrefs.SetInt("ChristmasDecorationStage2024", stage);
        }

        public int GetChristmasBubbleRewardId(int index)
        {
            return PlayerPrefs.GetInt("ChristmasBubbleRewardId_" + index.ToString(), 0);
        }

        public void SetChristmasBubbleRewardId(int index, int id)
        {
            PlayerPrefs.SetInt("ChristmasBubbleRewardId_" + index.ToString(), id);
        }

        public void ClearAllChristmasBubbleRewardId()
        {
            for (int i = 0; i < 7; i++)
            {
                SetChristmasBubbleRewardId(i, 0);
            }
        }

        public DateTime GetChristmasBubbleGetRewardTime(int index)
        {
            var timeString = PlayerPrefs.GetString("ChristmasBubbleGetRewardTime_" + index.ToString(), string.Empty);

            if (!string.IsNullOrEmpty(timeString))
            {
                try
                {
                    return DateTime.ParseExact(timeString, Constant.GameConfig.DefaultDateTimeFormet,
                        System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    PlayerPrefs.DeleteKey("ChristmasBubbleGetRewardTime_" + index.ToString());
                    return Constant.GameConfig.DateTimeMin;
                }
            }
            else
            {
                return Constant.GameConfig.DateTimeMin;
            }
        }

        public void SetChristmasBubbleGetRewardTime(int index, DateTime time)
        {
            PlayerPrefs.SetString("ChristmasBubbleGetRewardTime_" + index.ToString(),
                time.ToString(Constant.GameConfig.DefaultDateTimeFormet,
                    System.Globalization.CultureInfo.InvariantCulture));
        }

        public void ClearChristmasBubbleGetRewardTime(int index)
        {
            PlayerPrefs.DeleteKey("ChristmasBubbleGetRewardTime_" + index.ToString());
        }

        public void ClearChristmasBubbleGetRewardTime()
        {
            for (int i = 0; i < 7; i++)
            {
                PlayerPrefs.DeleteKey("ChristmasBubbleGetRewardTime_" + i.ToString());
            }

            PlayerPrefs.DeleteKey("ChristmasBubbleGetRewardRVTime");
        }
        
        public DateTime GetChristmasBubbleGetRewardRVTime()
        {
            var timeString = PlayerPrefs.GetString("ChristmasBubbleGetRewardRVTime", string.Empty);

            if (!string.IsNullOrEmpty(timeString))
            {
                try
                {
                    return DateTime.ParseExact(timeString, Constant.GameConfig.DefaultDateTimeFormet,
                        System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    PlayerPrefs.DeleteKey("ChristmasBubbleGetRewardRVTime");
                    return Constant.GameConfig.DateTimeMin;
                }
            }
            else
            {
                return Constant.GameConfig.DateTimeMin;
            }
        }

        public void SetChristmasBubbleGetRewardRVTime(DateTime time)
        {
            PlayerPrefs.SetString("ChristmasBubbleGetRewardRVTime",
                time.ToString(Constant.GameConfig.DefaultDateTimeFormet,
                    System.Globalization.CultureInfo.InvariantCulture));
        }

        #endregion

        #region Dog

        public int GetDogDecorationStage()
        {
            return PlayerPrefs.GetInt("DogDecorationStage2024", 0);
        }

        public void SetDogDecorationStage(int stage)
        {
            PlayerPrefs.SetInt("DogDecorationStage2024", stage);
        }

        public int GetDogBubbleRewardId(int index)
        {
            return PlayerPrefs.GetInt("DogBubbleRewardId_" + index.ToString(), 0);
        }

        public void SetDogBubbleRewardId(int index, int id)
        {
            PlayerPrefs.SetInt("DogBubbleRewardId_" + index.ToString(), id);
        }

        public void ClearAllDogBubbleRewardId()
        {
            for (int i = 0; i < 7; i++)
            {
                SetDogBubbleRewardId(i, 0);
            }
        }

        public DateTime GetDogBubbleGetRewardTime(int index)
        {
            var timeString = PlayerPrefs.GetString("DogBubbleGetRewardTime_" + index.ToString(), string.Empty);

            if (!string.IsNullOrEmpty(timeString))
            {
                try
                {
                    return DateTime.ParseExact(timeString, Constant.GameConfig.DefaultDateTimeFormet,
                        System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    PlayerPrefs.DeleteKey("DogBubbleGetRewardTime_" + index.ToString());
                    return Constant.GameConfig.DateTimeMin;
                }
            }
            else
            {
                return Constant.GameConfig.DateTimeMin;
            }
        }

        public void SetDogBubbleGetRewardTime(int index, DateTime time)
        {
            PlayerPrefs.SetString("DogBubbleGetRewardTime_" + index.ToString(),
                time.ToString(Constant.GameConfig.DefaultDateTimeFormet,
                    System.Globalization.CultureInfo.InvariantCulture));
        }

        public void ClearDogBubbleGetRewardTime(int index)
        {
            PlayerPrefs.DeleteKey("DogBubbleGetRewardTime_" + index.ToString());
        }

        public void ClearDogBubbleGetRewardTime()
        {
            for (int i = 0; i < 7; i++)
            {
                PlayerPrefs.DeleteKey("DogBubbleGetRewardTime_" + i.ToString());
            }

            PlayerPrefs.DeleteKey("DogBubbleGetRewardRVTime");
        }

        public DateTime GetDogBubbleGetRewardRVTime()
        {
            var timeString = PlayerPrefs.GetString("DogBubbleGetRewardRVTime", string.Empty);

            if (!string.IsNullOrEmpty(timeString))
            {
                try
                {
                    return DateTime.ParseExact(timeString, Constant.GameConfig.DefaultDateTimeFormet,
                        System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    PlayerPrefs.DeleteKey("DogBubbleGetRewardRVTime");
                    return Constant.GameConfig.DateTimeMin;
                }
            }
            else
            {
                return Constant.GameConfig.DateTimeMin;
            }
        }

        public void SetDogBubbleGetRewardRVTime(DateTime time)
        {
            PlayerPrefs.SetString("DogBubbleGetRewardRVTime",
                time.ToString(Constant.GameConfig.DefaultDateTimeFormet,
                    System.Globalization.CultureInfo.InvariantCulture));
        }

        #endregion

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void ClearAllData()
        {
            MergeModel.Instance.ResetData();
            SubtractMergeEnergyBoxNum(GetMergeEnergyBoxNum());

            SetLoveGiftRewardStage(1);
            SetCurLoveGiftRewardProgress(0);

            SetDigTreasureRewardStage(1);
            SetDigTreasureRewardProgress(0);
            SetDigTreasureCurDepth(0);
            SetDigTreasureStageDepth(0);
            
            //Christmas
            SetChristmasDecorationStage(0);
            ClearAllChristmasBubbleRewardId();
            ClearChristmasBubbleGetRewardTime();

            SetDogDecorationStage(0);
            ClearAllDogBubbleRewardId();
            ClearDogBubbleGetRewardTime();
        }
    }
}
