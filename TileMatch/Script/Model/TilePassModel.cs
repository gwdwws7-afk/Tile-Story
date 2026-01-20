using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Firebase.Analytics;
using Firebase.Firestore;
using MyFrameWork.Framework;
using MySelf.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace MySelf.Model
{
    public class TilePassData
    {
        public int currentIndex;
        public bool isVIP;
        public bool isSuperVIP;
        public int totalTargetNum;
        public int levelCollectTargetNum;
        public bool showedGuide;
        public bool showedStartMenu;
        public bool showedEndMenu;
        public DateTime endTime;
        public List<string> rewardGetStatus;
        public int recordUnclaimedRewardNum;        //记录未领取的奖励数量
        public int recordShowTilePassPanelCount;    //记录通行证界面的展示次数
        public int lastRecordTargetNum;
    }

    public class TilePassModel : BaseModelService<TilePassModel, TilePassData>
    {
        #region Service
        public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
        {
            return new Dictionary<string, object>()
            {
                {"TilePassModel.currentIndex", Data.currentIndex},
                {"TilePassModel.isVIP", Data.isVIP},
                {"TilePassModel.isSuperVIP", Data.isSuperVIP},
                {"TilePassModel.totalTargetNum", Data.totalTargetNum},
                {"TilePassModel.levelCollectTargetNum", Data.levelCollectTargetNum},
                {"TilePassModel.showedGuide", Data.showedGuide},
                {"TilePassModel.showedStartMenu", Data.showedStartMenu},
                {"TilePassModel.showedEndMenu", Data.showedEndMenu},
                {"TilePassModel.endTime", (Data.endTime - DateTime.MinValue).TotalMilliseconds},
                {"TilePassModel.rewardGetStatus", JsonConvert.SerializeObject(Data.rewardGetStatus)},
                {"TilePassModel.lastRecordTargetNum", Data.lastRecordTargetNum},
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
                        case "TilePassModel.currentIndex":
                            Data.currentIndex = Convert.ToInt32(item.Value);
                            break;
                        case "TilePassModel.isVIP":
                            Data.isVIP = Convert.ToBoolean(item.Value);
                            break;
                        case "TilePassModel.isSuperVIP":
                            Data.isSuperVIP = Convert.ToBoolean(item.Value);
                            break;
                        case "TilePassModel.totalTargetNum":
                            Data.totalTargetNum = Convert.ToInt32(item.Value);
                            break;
                        case "TilePassModel.levelCollectTargetNum":
                            Data.levelCollectTargetNum = Convert.ToInt32(item.Value);
                            break;
                        case "TilePassModel.showedGuide":
                            Data.showedGuide = Convert.ToBoolean(item.Value);
                            break;
                        case "TilePassModel.showedStartMenu":
                            Data.showedStartMenu = Convert.ToBoolean(item.Value);
                            break;
                        case "TilePassModel.showedEndMenu":
                            Data.showedEndMenu = Convert.ToBoolean(item.Value);
                            break;
                        case "TilePassModel.endTime":
                            Data.endTime =  DateTime.MinValue.AddMilliseconds(Convert.ToDouble(item.Value));
                            break;
                        case "TilePassModel.rewardGetStatus":
                            Data.rewardGetStatus = JsonConvert.DeserializeObject<List<string>>(Convert.ToString(item.Value));
                            break;
                        case "TilePassModel.lastRecordTargetNum":
                            Data.lastRecordTargetNum = Convert.ToInt32(item.Value);
                            break;
                    }
                }
                SaveToLocal();
            }
        }
        #endregion

        public bool IsUnlockByLevel => GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockTilePassLevel;
        public bool IsUnlockByDateTime => EndTime > DateTime.Now;
        
        public int CurrentIndex
        {
            get => Data.currentIndex;
            set
            {
                if (Data.currentIndex != value)
                {
                    Data.currentIndex = value;
                    SaveToLocal();
                }
            }
        }

        public bool IsVIP
        {
            get => Data.isVIP;
            set
            {
                if (Data.isVIP != value)
                {
                    Data.isVIP = value;
                    SaveToLocal();
                }
            }
        }

        public bool IsSuperVIP
        {
            get => Data.isSuperVIP;
            set
            {
                if (Data.isSuperVIP != value)
                {
                    Data.isSuperVIP = value;
                    SaveToLocal();
                }
            }
        }

        public int TotalTargetNum
        {
            get => Data.totalTargetNum;
            set
            {
                if (Data.totalTargetNum != value)
                {
                    Data.totalTargetNum = value;
                    SaveToLocal();
                }
            }
        }
        
        public int LastRecordTargetNum
        {
            get => Data.lastRecordTargetNum;
            set
            {
                if (Data.lastRecordTargetNum != value)
                {
                    Data.lastRecordTargetNum = value;
                    SaveToLocal();
                }
            }
        }

        public int LevelCollectTargetNum
        {
            get => Data.levelCollectTargetNum;
            set
            {
                if (Data.levelCollectTargetNum != value)
                {
                    Data.levelCollectTargetNum = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedGuide
        {
            get => Data.showedGuide;
            set
            {
                if (Data.showedGuide != value)
                {
                    Data.showedGuide = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedStartMenu
        {
            get => Data.showedStartMenu;
            set
            {
                if (Data.showedStartMenu != value)
                {
                    Data.showedStartMenu = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedEndMenu
        {
            get => Data.showedEndMenu;
            set
            {
                if (Data.showedEndMenu != value)
                {
                    Data.showedEndMenu = value;
                    SaveToLocal();
                }
            }
        }
        
        public DateTime EndTime
        {
            get => Data.endTime;
            set
            {
                if (Data.endTime != value)
                {
                    Data.endTime = value;
                    SaveToLocal();
                }
            }
        }

        public bool CheckRewardGetStatus(string key)
        {
            if (Data.rewardGetStatus == null)
            {
                Data.rewardGetStatus = new List<string>();
                SaveToLocal();
            }
            if (Data.rewardGetStatus.Contains(key))
                return true;
            else
                return false;
        }

        public void AddRewardGetStatus(string key)
        {
            if (Data.rewardGetStatus == null)
            {
                Data.rewardGetStatus = new List<string>();
            }
            if (!Data.rewardGetStatus.Contains(key))
            {
                Data.rewardGetStatus.Add(key);
            }
            SaveToLocal();
        }

        public void ResetData()
        {
            Data.currentIndex = 0;
            Data.isVIP = false;
            Data.isSuperVIP = false;
            Data.totalTargetNum = 0;
            Data.levelCollectTargetNum = 0;
            Data.showedStartMenu = false;
            Data.showedEndMenu = false;
            Data.endTime = DateTime.MinValue;
            Data.rewardGetStatus = null;
            Data.recordUnclaimedRewardNum = 0;
            Data.recordShowTilePassPanelCount = 0;
            SaveToLocal();
        }

        /// <summary>
        /// 获取当前关卡能生成的油桶数量
        /// </summary>
        /// <returns></returns>
        public int GetOilDrumNum()
        {
            //解锁
            if (IsUnlockByLevel && IsUnlockByDateTime)
            {
                DTTilePassData data = GameManager.DataTable.GetDataTable<DTTilePassData>().Data;
                //奖励拿满
                if (TotalTargetNum >= data.GetTotalTargetNum(data.CurrentTilePassDatas.Count - 1))
                    return 0;
                
                int hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());
                int levelCanCollectTargetNum = (hardIndex + 1) * (IsSuperVIP ? 2 : 1);
                return levelCanCollectTargetNum;
            }
            return 0;
        }

        /// <summary>
        /// 判断是否需要展示界面【根据上次记录的可领奖励数量>=2，并且现在可领数量大于上次记录的】
        /// </summary>
        /// <returns></returns>
        public bool IsNeedOpenTilePassPanelByUnClaimedRewardNum(int curUnClaimedRewardNum)
        {
            return curUnClaimedRewardNum >= 3 && curUnClaimedRewardNum > Data.recordUnclaimedRewardNum;
        }

        public void RecordLastUnClaimedRewardNum()
        {
            int unClaimedRewardNum = CheckUnclaimedRewards();
            if (Data.recordUnclaimedRewardNum != unClaimedRewardNum)
            {
                Data.recordUnclaimedRewardNum = unClaimedRewardNum;
                SaveToLocal();
            }
        }
        
        public int CheckUnclaimedRewards()
        {
            List<global::TilePassData> datas = GameManager.DataTable.GetDataTable<DTTilePassData>().Data.CurrentTilePassDatas;
            int count = 0;
            int currentIndex = 0;
            int totalTargetNum = TotalTargetNum;
            while (totalTargetNum >= 0 && currentIndex < datas.Count)
            {
                totalTargetNum -= datas[currentIndex].TargetNum;

                if (totalTargetNum >= 0)
                {
                    if (!CheckRewardGetStatus("TilePassFreeRewardGet" + currentIndex))
                    {
                        count++;
                    }

                    if (IsVIP && !CheckRewardGetStatus("TilePassVIPRewardGet" + currentIndex))
                    {
                        count++;
                    }
                }
                currentIndex++;
            }
            return count;
        }

        public void RecordTilePassOpen()
        {
            Data.recordShowTilePassPanelCount += 1;
            SaveToLocal();
        }

        public int GetCurPeriodsIdByEndDateTime()
        {
           return GameManager.DataTable.GetDataTable<DTTilePassScheduleData>().Data.GetCurPeriodsIdByEndDateTime(Data.endTime);
        }
    }
}

public static class TilePassUtil
{
    public static void RecordTilePassPurchase(string purchaseId)
    {
        GameManager.Firebase.RecordMessageByEvent("TilePass_Purchase",
            new Parameter("TaskNum",TilePassModel.Instance.CurrentIndex),
            new Parameter("PurchaseID",purchaseId),
            new Parameter("PeriodsID",TilePassModel.Instance.GetCurPeriodsIdByEndDateTime()));
    }
    public static void RecordTilePassCompleted()
    {
        GameManager.Firebase.RecordMessageByEvent("TilePass_Completed",
            new Parameter("TaskNum",TilePassModel.Instance.CurrentIndex),
            new Parameter("PeriodsID",TilePassModel.Instance.GetCurPeriodsIdByEndDateTime()));
    }
    public static void RecordLastChancePurchase()
    {
        GameManager.Firebase.RecordMessageByEvent("Last_Chance_Purchase",
            new Parameter("TaskNum",TilePassModel.Instance.CurrentIndex),
            new Parameter("PeriodsID",TilePassModel.Instance.GetCurPeriodsIdByEndDateTime()));
    }
    public static void RecordTilePassShow()
    {
        GameManager.Firebase.RecordMessageByEvent("TilePass_Show",
            new Parameter("TilePassOpenCount",TilePassModel.Instance.Data.recordShowTilePassPanelCount),
            new Parameter("PeriodsID",TilePassModel.Instance.GetCurPeriodsIdByEndDateTime()));
    }
    
    public static void RecordTilePassClaim()
    {
        int passLevel = 0;
        if (TilePassModel.Instance.IsVIP && !TilePassModel.Instance.IsSuperVIP)
            passLevel = 1;
        if (TilePassModel.Instance.IsSuperVIP)
            passLevel = 2;
        GameManager.Firebase.RecordMessageByEvent("Pass_ItemClaim", 
            new Parameter("Index", TilePassModel.Instance.CurrentIndex),
            new Parameter("PassLevel", passLevel));
    }
}
