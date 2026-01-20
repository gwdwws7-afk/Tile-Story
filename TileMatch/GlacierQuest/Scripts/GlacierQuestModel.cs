using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum GlacierQuestState
{
    Wait,   //等待开启
    Open,   //开启中
    Clear,  //通关
    ClearTime,//通关冷却中
    Time,   //冷却中
    Close,  //不在开放时间
}

namespace MySelf.Model
{
    public class GlacierQuestData
    {
        // 活动状态
        public GlacierQuestState activityState = GlacierQuestState.Wait;
        // 是否时第一次开启活动
        public bool isFirstOpen = true;
        // 是否是本期首次参与活动
        public bool isFirstStart = true;
        // 是否是第一次弹出
        public bool isFirstPop = true;
        // 是否展示活动引导
        public bool isShowGuide = true;
        // 是否存在可领取的活动奖励
        public bool isCanClaimedReward = false;
        // 是否显示入口按钮
        public bool isShowEntrance = true;
        // 是否是重新开始本次挑战
        public bool isRechallenge = false;
        // 本期挑战是否需要弹出开启弹窗
        public bool needPopStart = true;
        // 当期活动的ID    
        public int currentActivityID = -1;
        // 当天活动开放的次数，只在新的一天刷新
        public int todayOpenCount = 0;
        // 活动进度
        public int curLevel = 0;
        // 老的活动进度
        public int oldLevel = 0;
        // 剩余人数
        public int curPeople = 100;
        // 老的剩余人数
        public int oldPeople = 0;
        // 当前的掉队曲线
        public int reducePeopleCurveIndex = -1;
        // 活动胜利时平分奖励的人数
        public int successPeopleNum = 0;
        // 活动重开时间
        public DateTime restartTime;
        // 当期活动结束时间
        public DateTime endTime;
        // 整体活动的结束时间
        public DateTime activityEndTime = DateTime.MinValue;

        // 参与活动的100位用户头像
        public int[] headIdArray = new int[30];
        // 活动胜利时平分奖励的人的头像
        public int[] successHeadIdArray = new int[6];

        [JsonIgnore]
        // 机器人掉队曲线的出现比例
        public List<float> curveIndexPossibilityList = new List<float>() { 0.25f, 0.25f, 0.20f, 0.20f, 0.10f };
        [JsonIgnore]
        // 机器人掉队曲线
        public int[][] curveArray = new int[5][]
        {
        new int[]{90, 70, 69, 50, 49, 40, 39, 31, 30, 26, 25, 20, 11, 11} ,
        new int[]{88, 68, 67, 48, 47, 38, 37, 29, 28, 24, 23, 18, 10, 10} ,
        new int[]{86, 66, 65, 46, 45, 36, 35, 27, 26, 22, 21, 16, 9, 9},
        new int[]{84, 64, 63, 44, 43, 34, 33, 25, 24, 20, 19, 14, 8, 8},
        new int[]{80, 60, 59, 40, 39, 30, 29, 21, 20, 16, 15, 10, 6, 6},
        };

        public List<int> finishedActivityIDList = new List<int>();
    }

    public class GlacierQuestModel : BaseModel<GlacierQuestModel, GlacierQuestData>
    {
        public GlacierQuestState ActivityState
        {
            get => Data.activityState;
            set
            {
                if (Data.activityState != GlacierQuestState.Wait && value == GlacierQuestState.Wait)
                {
                    //刷新活动
                    RestartActivity();
                }
                Data.activityState = value;
                SaveToLocal();
            }
        }

        public bool IsFirstOpen
        {
            get => Data.isFirstOpen;
            set
            {
                Data.isFirstOpen = value;
                SaveToLocal();
            }
        }

        public bool IsFirstStart
        {
            get => Data.isFirstStart;
            set
            {
                Data.isFirstStart = value;
                SaveToLocal();
            }
        }

        public bool IsFirstPop
        {
            get => Data.isFirstPop;
            set
            {
                if (Data.isFirstPop == value) return;
                Data.isFirstPop = value;
                SaveToLocal();
            }
        }

        public bool IsShowGuide
        {
            get => Data.isShowGuide;
            set
            {
                Data.isShowGuide = value;
                SaveToLocal();
            }
        }

        public bool IsCanClaimedReward
        {
            get => Data.isCanClaimedReward;
            set
            {
                Data.isCanClaimedReward = value;
                SaveToLocal();
            }
        }

        public bool IsShowEntrance
        {
            get => Data.isShowEntrance;
            set
            {
                Data.isShowEntrance = value;
                SaveToLocal();
            }
        }

        public bool IsRechallenge
        {
            get => Data.isRechallenge;
            set
            {
                Data.isRechallenge = value;
                SaveToLocal();
            }
        }

        public bool NeedPopStart
        {
            get => Data.needPopStart;
            set
            {
                Data.needPopStart = value;
                SaveToLocal();
            }
        }

        /// <summary>
        /// 活动ID，不能在重置活动时清除数据
        /// </summary>
        public int CurrentActivityID
        {
            get => Data.currentActivityID;
            set
            {
                Data.currentActivityID = value;
                SaveToLocal();
            }
        }

        public int TodayOpenCount
        {
            get => Data.todayOpenCount;
            set
            {
                Data.todayOpenCount = value;
                SaveToLocal();
            }
        }

        public int CurLevel
        {
            get => Data.curLevel;
            set
            {
                Data.curLevel = value;
                SaveToLocal();
            }
        }

        public int OldLevel
        {
            get => Data.oldLevel;
            set
            {
                Data.oldLevel = value;
                SaveToLocal();
            }
        }

        public int CurPeople
        {
            get => Data.curPeople;
            set
            {
                Data.curPeople = value;
                SaveToLocal();
            }
        }

        public int OldPeople
        {
            get => Data.oldPeople;
            set
            {
                Data.oldPeople = value;
                SaveToLocal();
            }
        }

        public int SuccessPeopleNum
        {
            get => Data.successPeopleNum;
            set
            {
                Data.successPeopleNum = value;
                SaveToLocal();
            }
        }

        public DateTime RestartTime
        {
            get => Data.restartTime;
            set
            {
                Data.restartTime = value;
                SaveToLocal();
            }
        }

        public DateTime EndTime
        {
            get => Data.endTime;
            set
            {
                Data.endTime = value;
                SaveToLocal();
            }
        }

        public DateTime ActivityEndTime
        {
            get
            {
                if (Data.activityEndTime == DateTime.MinValue)
                    ActivityEndTime = GameManager.DataTable.GetDataTable<DTGlacierQuestScheduleData>().Data.GetNowActiveActivityEndTime();
                return Data.activityEndTime;
            }
            set
            {
                Data.activityEndTime = value;
                SaveToLocal();
            }
        }

        public int[] HeadIdArray
        {
            get
            {
                if (Data.headIdArray.Length > 0)
                {
                    Data.headIdArray[0] = GameManager.PlayerData.HeadPortrait;
                }
                return Data.headIdArray;
            }
            set => Data.headIdArray = value;
        }

        public int[] SuccessHeadIdArray
        {
            get
            {
                if (Data.successHeadIdArray.Length > 0)
                {
                    Data.successHeadIdArray[0] = GameManager.PlayerData.HeadPortrait;
                }
                return Data.successHeadIdArray;
            }
            set
            {
                for (int i = 1; i < Data.successHeadIdArray.Length; i++)
                {
                    int id = 1;
                    if (i > value.Length)
                        id = UnityEngine.Random.Range(1, 10);
                    else
                        id = value[i];
                    Data.successHeadIdArray[i] = id;
                }
                SaveToLocal();
            }
        }

        public int ReducePeopleCurveIndex
        {
            get => Data.reducePeopleCurveIndex;
            set
            {
                Data.reducePeopleCurveIndex = value;
                SaveToLocal();
            }
        }

        public List<float> CurveIndexPossibilityList
        {
            get => Data.curveIndexPossibilityList;
        }

        public int[] CurveArray
        {
            get => Data.curveArray[ReducePeopleCurveIndex];
        }

        public List<int> FinishedActivityIDList
        {
            get => Data.finishedActivityIDList;
        }

        public void RecordFinishActivityId(int id)
        {
            Data.finishedActivityIDList.Add(id);
            SaveToLocal();
        }

        public void RestartActivity()
        {
            Log.Info("GlacierQuest：重置活动设置");
            // Data.isCanClaimedReward = false;
            Data.isShowEntrance = true;
            Data.isRechallenge = false;
            Data.needPopStart = true;
            Data.curLevel = 0;
            Data.oldLevel = 0;
            Data.curPeople = 100;
            Data.oldPeople = 0;

            float totalPossibility = 0;
            for (int i = 0; i < CurveIndexPossibilityList.Count; i++)
            {
                totalPossibility += CurveIndexPossibilityList[i];
            }
            float randomResult = UnityEngine.Random.Range(0, totalPossibility);
            Log.Info($"GlacierQuest：随机数{randomResult}");
            for (int i = 0; i < CurveIndexPossibilityList.Count; ++i)
            {
                if (randomResult < CurveIndexPossibilityList[i])
                {
                    Data.reducePeopleCurveIndex = i;
                    Log.Info($"GlacierQuest：设置曲线序列{i}   {CurveIndexPossibilityList[i]}");
                    if (i >= 5)
                    {
                        Debug.LogError($"GlacierQuest：值获取错误 {CurveIndexPossibilityList.Count}");
                        string att = "";
                        for (int j = 0; j < CurveIndexPossibilityList.Count; j++)
                        {
                            att += $"{CurveIndexPossibilityList[j]}, ";
                        }
                        Log.Error(att);
                    }
                    break;
                }
                randomResult -= CurveIndexPossibilityList[i];
            }

            // 初始化头像ID
            Data.headIdArray[0] = GameManager.PlayerData.HeadPortrait;
            for (int i = 1; i < 30; i++)
            {
                Data.headIdArray[i] = UnityEngine.Random.Range(1, 10);
            }
            SaveToLocal();
        }
    }
}
