using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySelf.Model
{
    public class ClimbBeanstalkData
    {
        public int currentActivityID;
        public int currentPhaseID;
        public int currentWinStreak;
        public int highestClaimRewardNum;//最高的 领取过奖励的胜利次数
        public bool everShowedFirstTimeIntro;//整个活动类型第一次开启时 直接弹出活动界面
        public bool everShowedIntro;//每个Activity的每个Phase都需要展示一次 播放一个从上到下的动画
        public DateTime endTime;
        public int lastWinStreakNum;//表现用 已表现过的 玩家到达的层数
        public bool needFlyReward;//表现用 主界面上是否需要飞入奖励 在胜利时置为true 在播放时置为false

        public List<int> finishedActivityIDList = new List<int>();//玩家历史上曾经完全完成过的 ActivityID列表
    }

    public class ClimbBeanstalkModel : BaseModel<ClimbBeanstalkModel, ClimbBeanstalkData>
    {
        public int CurrentActivityID
        {
            get => Data.currentActivityID;
            set
            {
                if (Data.currentActivityID != value)
                {
                    GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.ClimbBeanstalkInfoChanged));
                    Data.currentActivityID = value;
                    SaveToLocal();
                }
            }
        }

        public int CurrentPhaseID
        {
            get => Data.currentPhaseID;
            set
            {
                Data.currentPhaseID = value;
                SaveToLocal();
            }
        }

        public int CurrentWinStreak
        {
            get => Data.currentWinStreak;
            set
            {
                if (Data.currentWinStreak != value)
                {
                    GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.ClimbBeanstalkInfoChanged));
                    Data.currentWinStreak = value;
                    SaveToLocal();
                }
            }
        }

        public int HighestClaimRewardNum
        {
            get => Data.highestClaimRewardNum;
            set
            {
                Data.highestClaimRewardNum = value;
                SaveToLocal();
            }
        }

        public int LastWinStreakNum
        {
            get => Data.lastWinStreakNum;
            set
            {
                Data.lastWinStreakNum = value;
                SaveToLocal();
            }
        }

        public bool NeedFlyReward
        {
            get => Data.needFlyReward;
            set
            {
                Data.needFlyReward = value;
                SaveToLocal();
            }
        }

        public bool EverShowedFirstTimeIntro
        {
            get => Data.everShowedFirstTimeIntro;
            set
            {
                Data.everShowedFirstTimeIntro = value;
                SaveToLocal();
            }
        }

        public bool EverShowedIntro
        {
            get => Data.everShowedIntro;
            set
            {
                Data.everShowedIntro = value;
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

        public void Init(int inputActivityID, int inputPhaseID)
        {
            Log.Info($"ClimbBeanstalk：初始化活动 {inputActivityID}   {inputPhaseID}");
            Data.currentActivityID = inputActivityID;
            Data.currentPhaseID = inputPhaseID;
            Data.currentWinStreak = 0;
            Data.highestClaimRewardNum = 0;

            Data.everShowedIntro = false;

            Data.lastWinStreakNum = 0;
            Data.needFlyReward = false;
            SaveToLocal();

            GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.ClimbBeanstalkInfoChanged));
        }

    }
}
