using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySelf.Model
{
    public class MergeData : PlayerDataBase
    {
        public int period;//期数
        public bool isActivityStart;//活动是否开始
        public string savedPropDistributedMap;//道具布局数据字段
        public int currentMaxMergeStage;//当前合成的最大道具等级
        public string storePropIds;//所有的存储道具编号
        //public int mergeEnergyBoxNum;//合成体力数
        public int mergeEnergyBoxLevelCollectNum;//合成体力关卡收集数
        public int getFinalRewardTime;//打开最终奖励宝箱的次数
        public int curMergeOfferLevel = 1;//当前无尽宝箱领取等级
        public int showAdsGetBubbleTime = 0;//看广告获取气泡道具的次数
        public DateTime bubbleBreakNextAdsReadyTime;//看广告开气泡的倒计时
        public DateTime getBoxNextAdsReadyTime;//看广告获取体力箱子的倒计时

        public bool IsHaveEnterActivity;//是否已经进入活动

        public override void IsRefreshDataByNewDay()
        {
            showAdsGetBubbleTime = 0;
            bubbleBreakNextAdsReadyTime = Constant.GameConfig.DateTimeMin;
            getBoxNextAdsReadyTime = Constant.GameConfig.DateTimeMin;
            base.IsRefreshDataByNewDay();
        }
    }

    public class MergeModel : BaseModelService<MergeModel, MergeData>
    {
        #region Service

        public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
        {
            return new Dictionary<string, object>()
            {
                {"MergeModel.period",Data.period},
                {"MergeModel.isActivityStart",Data.isActivityStart},
                {"MergeModel.savedPropDistributedMap",Data.savedPropDistributedMap},
                {"MergeModel.currentMaxMergeStage",Data.currentMaxMergeStage},
                {"MergeModel.storePropIds",Data.storePropIds},
                //{"MergeModel.mergeEnergyBoxNum",Data.mergeEnergyBoxNum},
                {"MergeModel.mergeEnergyBoxLevelCollectNum",Data.mergeEnergyBoxLevelCollectNum},
                {"MergeModel.getFinalRewardTime",Data.getFinalRewardTime},
                {"MergeModel.curMergeOfferLevel",Data.curMergeOfferLevel},
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
                        case "MergeModel.period":
                            Data.period = Convert.ToInt32(item.Value);
                            break;
                        case "MergeModel.isActivityStart":
                            Data.isActivityStart = Convert.ToBoolean(item.Value);
                            break;
                        case "MergeModel.savedPropDistributedMap":
                            Data.savedPropDistributedMap = Convert.ToString(item.Value);
                            break;
                        case "MergeModel.currentMaxMergeStage":
                            Data.currentMaxMergeStage = Convert.ToInt32(item.Value);
                            break;
                        case "MergeModel.storePropIds":
                            Data.storePropIds = Convert.ToString(item.Value);
                            break;
                        //case "MergeModel.mergeEnergyBoxNum":
                        //    Data.mergeEnergyBoxNum = Convert.ToInt32(item.Value);
                        //    break;
                        case "MergeModel.mergeEnergyBoxLevelCollectNum":
                            Data.mergeEnergyBoxLevelCollectNum = Convert.ToInt32(item.Value);
                            break;
                        case "MergeModel.getFinalRewardTime":
                            Data.getFinalRewardTime = Convert.ToInt32(item.Value);
                            break;
                        case "MergeModel.curMergeOfferLevel":
                            Data.curMergeOfferLevel = Convert.ToInt32(item.Value);
                            break;
                    }
                }
                SaveToLocal();
            }
        }

        #endregion

        public int Period
        {
            get => Data.period;
            set
            {
                if (Data.period != value)
                {
                    Data.period = value;
                    SaveToLocal();
                }
            }
        }

        public bool IsActivityStart
        {
            get => Data.isActivityStart;
            set
            {
                if (Data.isActivityStart != value)
                {
                    Data.isActivityStart = value;
                    SaveToLocal();
                }
            }
        }

        public string SavedPropDistributedMap
        {
            get => Data.savedPropDistributedMap;
            set
            {
                if (Data.savedPropDistributedMap != value)
                {
                    Data.savedPropDistributedMap = value;
                    SaveToLocal();
                }
            }
        }

        public int CurrentMaxMergeStage
        {
            get => Data.currentMaxMergeStage;
            set
            {
                if (Data.currentMaxMergeStage != value)
                {
                    Data.currentMaxMergeStage = value;
                    SaveToLocal();
                }
            }
        }

        public string StorePropIds
        {
            get => Data.storePropIds;
            set
            {
                if (Data.storePropIds != value)
                {
                    Data.storePropIds = value;
                    SaveToLocal();
                }
            }
        }

        //public int MergeEnergyBoxNum
        //{
        //    get => Data.mergeEnergyBoxNum;
        //    set
        //    {
        //        if (Data.mergeEnergyBoxNum != value)
        //        {
        //            Data.mergeEnergyBoxNum = value;
        //            SaveToLocal();
        //        }
        //    }
        //}

        public int MergeEnergyBoxLevelCollectNum
        {
            get => Data.mergeEnergyBoxLevelCollectNum;
            set
            {
                if (Data.mergeEnergyBoxLevelCollectNum != value)
                {
                    Data.mergeEnergyBoxLevelCollectNum = value;
                    SaveToLocal();
                }
            }
        }

        public int GetFinalRewardTime
        {
            get => Data.getFinalRewardTime;
            set
            {
                if (Data.getFinalRewardTime != value)
                {
                    Data.getFinalRewardTime = value;
                    SaveToLocal();
                }
            }
        }

        public int CurMergeOfferLevel
        {
            get => Data.curMergeOfferLevel;
            set
            {
                if (Data.curMergeOfferLevel != value)
                {
                    Data.curMergeOfferLevel = value;
                    SaveToLocal();
                }
            }
        }

        public int ShowAdsGetBubbleTime
        {
            get => Data.showAdsGetBubbleTime;
            set
            {
                if (Data.showAdsGetBubbleTime != value)
                {
                    Data.showAdsGetBubbleTime = value;
                    SaveToLocal();
                }
            }
        }

        public DateTime BubbleBreakNextAdsReadyTime
        {
            get => Data.bubbleBreakNextAdsReadyTime;
            set
            {
                if (Data.bubbleBreakNextAdsReadyTime != value)
                {
                    Data.bubbleBreakNextAdsReadyTime = value;
                    SaveToLocal();
                }
            }
        }

        public DateTime GetBoxNextAdsReadyTime
        {
            get => Data.getBoxNextAdsReadyTime;
            set
            {
                if (Data.getBoxNextAdsReadyTime != value)
                {
                    Data.getBoxNextAdsReadyTime = value;
                    SaveToLocal();
                }
            }
        }

        public void ResetData()
        {
            Data.isActivityStart = false;
            Data.savedPropDistributedMap = null;
            Data.currentMaxMergeStage = 0;
            Data.storePropIds = null;
            //Data.mergeEnergyBoxNum = 0;
            Data.getFinalRewardTime = 0;
            Data.curMergeOfferLevel = 1;
            Data.showAdsGetBubbleTime = 0;

            SaveToLocal();
        }

    }
}
