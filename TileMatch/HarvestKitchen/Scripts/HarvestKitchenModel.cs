using System;
using System.Collections.Generic;
using UnityEngine;

namespace MySelf.Model
{
    public class HarvestKitchenData
    {
        // 第一次进入游戏
        public bool isFirstStart = true;
        // 第一次开启MainMenu界面
        public bool isFirstOpenMainMenu = true;
        // 是否显示游戏引导
        public bool isShowGameGuide = true;
        // 是否显示棋盘触顶引导
        public bool isShowTopGuide = true;
        // 是否显示选择栏将满引导
        public bool isShowChooseBarGuide = true;
        // 当前的活动ID
        public int currentActivityId = 0;
        // 当前的关卡
        public int currentLevel = 0;
        // 当前关卡的失败次数
        public int failedNum = 0;
        
        // 当前的任务ID
        public int taskId = 0;
        //当前宝箱ID
        public int chestId = 0;
        // 当前任务积累的点赞数
        public int praiseNum = 0;
        // 旧的点赞数
        public int oldPraiseNum = -1;
        // 挑战次数
        public int challengeNum = 0;
        //活动结束标识
        public int finishedActivityId = 0;
    }
    
    public class HarvestKitchenModel : BaseModel<HarvestKitchenModel, HarvestKitchenData>
    {
        public bool IsFirstStart
        {
            get => Data.isFirstStart;
            set
            {
                if (Data.isFirstStart != value)
                {
                    Data.isFirstStart = value;
                    SaveToLocal();
                }
            }
        }
        
        public bool IsFirstOpenMainMenu
        {
            get => Data.isFirstOpenMainMenu;
            set
            {
                if (Data.isFirstOpenMainMenu != value)
                {
                    Data.isFirstOpenMainMenu = value;
                    SaveToLocal();
                }
            }
        }
        
        public bool IsShowGameGuide
        {
            get => Data.isShowGameGuide;
            set
            {
                if (Data.isShowGameGuide != value)
                {
                    Data.isShowGameGuide = value;
                    SaveToLocal();
                }
            }
        }
        
        public bool IsShowTopGuide
        {
            get => Data.isShowTopGuide;
            set
            {
                if (Data.isShowTopGuide != value)
                {
                    Data.isShowTopGuide = value;
                    SaveToLocal();
                }
            }
        }
        
        public bool IsShowChooseBarGuide
        {
            get => Data.isShowChooseBarGuide;
            set
            {
                if (Data.isShowChooseBarGuide != value)
                {
                    Data.isShowChooseBarGuide = value;
                    SaveToLocal();
                }
            }
        }
        
        public int CurrentActivityID
        {
            get => Data.currentActivityId;
            set
            {
                if (Data.currentActivityId != value)
                {
                    Data.currentActivityId = value;
                    SaveToLocal();
                }
            }
        }
        
        public int CurrentLevel
        {
            get => Data.currentLevel;
            set
            {
                if (Data.currentLevel != value)
                {
                    Data.currentLevel = value;
                    SaveToLocal();
                }
            }
        }
        
        public int FailedNum
        {
            get => Data.failedNum;
            set
            {
                if (Data.failedNum != value)
                {
                    Data.failedNum = value;
                    SaveToLocal();
                }
            }
        }
        
        public int TaskId
        {
            get => Data.taskId;
            set
            {
                if (Data.taskId != value)
                {
                    Data.taskId = value;
                    SaveToLocal();
                }
            }
        }

        public int ChestId
        {
            get => Data.chestId;
            set
            {
                if (Data.chestId != value)
                {
                    Data.chestId = value;
                    SaveToLocal();
                }
            }
        }

        public int PraiseNum
        {
            get => Data.praiseNum;
            set
            {
                if (Data.praiseNum != value)
                {
                    Data.praiseNum = value;
                    SaveToLocal();
                }
            }
        }
        
        public int OldPraiseNum
        {
            get => Data.oldPraiseNum;
            set
            {
                if (Data.oldPraiseNum != value)
                {
                    Data.oldPraiseNum = value;
                    SaveToLocal();
                }
            }
        }

        public int ChallengeNum
        {
            get => Data.challengeNum;
            set
            {
                if (Data.challengeNum != value)
                {
                    Data.challengeNum = value;
                    SaveToLocal();
                }
            }
        }

        public bool EverFinishedTargetActivityID(int periodID)
        {
            return Data.finishedActivityId == periodID;
        }

        public void RecordFinishedActivityId(int periodID)
        {
            if (Data.finishedActivityId != periodID)
            {
                Data.finishedActivityId = periodID;
                SaveToLocal();
            }
        }
        
        public void Init(int inputActivityID)
        {
            Data.isFirstStart = true;
            Data.isFirstOpenMainMenu = true;
            Data.currentActivityId = inputActivityID;
            Data.currentLevel = 0;
            Data.failedNum = 0;
            Data.taskId = 0;
            Data.praiseNum = 0;
            Data.oldPraiseNum = -1;
            SaveToLocal();
        }
    }
}
