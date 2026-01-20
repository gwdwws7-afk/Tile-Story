using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Firestore;
using NSubstitute;
using UnityEngine;
using UnityEngineInternal;

namespace MySelf.Model
{
    public enum PkGameStatus
    {
        None,
        Playing,
        Over,
    }

    public enum PkGameOverStatus
    {
        Win,
        Lose,
        Deuce,
    }

    public class PkGamePlayerData
    {
        public string PlayerName="???";
        public int PlayerHeadPortrait;
        public int ItemNum;

        private Dictionary<string, object> _dictionary;
        public Dictionary<string, object> GetDict(bool isSelf=false)
        {
            if (_dictionary == null)
                _dictionary = new Dictionary<string, object>();

            _dictionary["PlayerName"] =isSelf?GameManager.PlayerData.PlayerName:(string.IsNullOrEmpty(PlayerName)?"???":PlayerName);
            _dictionary["PlayerHeadPortrait"] =isSelf?GameManager.PlayerData.HeadPortrait:PlayerHeadPortrait;
            _dictionary["ItemNum"] = ItemNum;
            return _dictionary;
        }

        public PkGamePlayerData(bool isSelf)
        {
            PlayerName = GameManager.PlayerData.PlayerName;
            PlayerHeadPortrait = GameManager.PlayerData.HeadPortrait;
        }

        public void Clear(bool isSelf=false)
        {
            if (isSelf)
            {
                PlayerName = GameManager.PlayerData.PlayerName;
                PlayerHeadPortrait = GameManager.PlayerData.HeadPortrait;
                ItemNum = 0;
            }
            else
            {
                PlayerName = "???";
                PlayerHeadPortrait = 0;
                ItemNum = 0;
            }
        }

        public PkGamePlayerData(){}

        public PkGamePlayerData(Dictionary<string, object> dic)
        {
            PlayerName = dic.TryGetValue("PlayerName", out var value) ? (string)value : "";
            PlayerHeadPortrait = dic.TryGetValue("PlayerHeadPortrait", out value) ? (int)(long)value : 0;
            ItemNum = dic.TryGetValue("ItemNum", out value) ? (int)(long)value : 0;
        }
    }

    public class PkGameModelData
    {
        //获取是否开启
        public bool IsActivityOpen;
        //每次上线自动弹出弹框
        public bool IsHaveAutoOpenPanel = false;
        //开启活动的开始时间
        public DateTime EndDateTime = DateTime.MinValue;
        //玩家所处数据库位置编号
        public int PlayerPosNum;
        //记录玩家参与的次数
        public int EnterPkGameCount=1;
        //记录last 
        public int RecordVersionNew=0;
        //当前玩家所有数据
        public PkGamePlayerData SelfPlayerPkData = new PkGamePlayerData(true);
        public PkGamePlayerData TargetPlayerPkData=new PkGamePlayerData();
        
        //记录当前关卡编号  以及当前关卡失败次数【用来计算当前关卡胜利可获得奖励数目】
        //public int CurLevel;
        public int CurLevelFailCount;

        //旧分数 用来显示分数进度
        public int SelfOldSorce;
        public int TargetOldSorce;
        
        //是否弹出过引导
        public bool IfOpenFirstGuide = false;
        //是否弹出过升级引导
        public bool IfOpenUpgradesGuide = false;
        //记录开启游戏时间
        public DateTime LastOpenDateTime=DateTime.MinValue;
        //段位
        public int CurRankNum = 1;
        public int LastRankNum = 1;
        //上次弹出的对话编号
        public int LastTextNum = 0;
        //胜利结算文案弹出次数
        public Dictionary<int, int> RecordGameOverTextDict = new Dictionary<int, int>();

        public string GroupPath = "PkGame";
        public void Balance()
        {
            //只有展示过引导之后  才会做段位结算，【跟前版本的信息区分开】
            if (IfOpenFirstGuide)
            {
                switch (GameOverStatus)
                {
                    case PkGameOverStatus.Deuce: 
                        CurRankNum = LastRankNum;
                        break;
                    case PkGameOverStatus.Lose:
                        CurRankNum = 1;
                        break;
                    case PkGameOverStatus.Win:
                        CurRankNum = Math.Min(LastRankNum + 1, 4);
                        break;
                }
            }
  
            IsActivityOpen = false;
            EndDateTime = DateTime.MinValue;
            PlayerPosNum = 0;
            EnterPkGameCount+=1;
            SelfPlayerPkData.Clear(true);
            TargetPlayerPkData.Clear();
            //CurLevel = 0;
            CurLevelFailCount = 0;
            SelfOldSorce = 0;
            TargetOldSorce = 0;
            RecordVersionNew = 0;
            RecordGameOverTextDict.Clear();
            LastTextNum = 0;
            GroupPath = "PkGame";
            IsHaveAutoOpenPanel = false;
        }

        public PkGameOverStatus GameOverStatus
        {
            get
            {
                if (SelfPlayerPkData.ItemNum > TargetPlayerPkData.ItemNum)
                {
                    return PkGameOverStatus.Win;
                }else if (SelfPlayerPkData.ItemNum == TargetPlayerPkData.ItemNum)
                {
                    return PkGameOverStatus.Deuce;
                }
                return PkGameOverStatus.Lose;
            }
        }
    }

    public class PkGameModel : BaseModel<PkGameModel, PkGameModelData>
    {
        public static DateTime StartTime = new DateTime(2024, 1, 1);
        public PkGameStatus RecordEnterGameStatus = PkGameStatus.None;

        public int PkItemFlyNum = 0;

        //pk赛预告等级
        public const int PreviewPkGameLevel = 27;
        //pk赛开启等级
        public const int OpenPkGameLevel = 34;

        public long TimeLenghtByEachGame =
            GameManager.Firebase.GetLong(Constant.RemoteConfig.Pk_Game_Each_Time_Lenght, 15);

        //是否等级达到可激活等级
        public bool IsCanPreviewByLevel => GameManager.PlayerData.NowLevel >= PreviewPkGameLevel;
        public bool IsCanOpenByLevel => GameManager.PlayerData.NowLevel >= OpenPkGameLevel;

        private ListenerRegistration listen = null;

        public bool IsCanOpenPkGame =>
            IsForceOpenPkGame || GameManager.Firebase.GetBool(Constant.RemoteConfig.Is_Can_Open_Pk_Game, false);

        public bool IsForceOpenPkGame
        {
            get => PlayerPrefs.GetInt("IsForceOpenPkGame", 0) == 1;
            set
            {
                PlayerPrefs.SetInt("IsForceOpenPkGame", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public int RecordVersionNewNum
        {
            get
            {
                if (Instance.Data.RecordVersionNew == 0)
                    Instance.Data.RecordVersionNew = (DateTime.Now - new DateTime(2024, 1, 1)).Days / 7;
                SaveToLocal();

                Log.Info($"RecordVersionNew:{Instance.Data.RecordVersionNew}");
                return Instance.Data.RecordVersionNew;
            }
        }

        //pk赛开启时间段
        public bool IsCanOpenByTime
        {
            get
            {
                if (!PkGameModel.Instance.IsCanOpenPkGame) return false;
                //每周五下午三点到下周一下午三点前
                var dayOfWeek = Convert.ToInt32(DateTime.Now.Date.DayOfWeek);
                var hour = DateTime.Now.Hour;
                if ((dayOfWeek == 0 || dayOfWeek == 6) || (dayOfWeek == 5 && hour >= 15) ||
                    (dayOfWeek == 1 && hour < 15))
                {
                    return true;
                }

                return false;
            }
        }

        //当前状态
        public PkGameStatus PkGameStatus
        {
            get
            {
                if (!Instance.Data.IsActivityOpen) return PkGameStatus.None;

                if (Instance.Data.IsActivityOpen && (Instance.Data.EndDateTime > DateTime.UtcNow))
                    return PkGameStatus.Playing;

                return PkGameStatus.Over;
            }
        }

        public DateTime LocalEndDateTime =>
            TimeZoneInfo.ConvertTimeFromUtc(Instance.Data.EndDateTime, TimeZoneInfo.Local);

        //当前活动是否处于激活状态
        public bool IsActivityOpen
        {
            get => Instance.Data.IsActivityOpen;
            set
            {
                Instance.Data.IsActivityOpen = value;
                if (value)
                {
                    //记录开启时间
                    Data.LastOpenDateTime=GetPkOverDateTime(DateTime.Now);
                    //保存时间信息
                    Instance.Data.EndDateTime = DateTime.UtcNow.AddMinutes(TimeLenghtByEachGame);
                }

                SaveToLocal();
            }
        }

        public DateTime CurActivityEndDateTime => GetPkOverDateTime(DateTime.Now);

        public int NeedCoinStartPkGame => MultipleByRank(Data.CurRankNum);

        public bool IsFirstWin; //是否是第一次胜利 这里存储给界面使用

        public PkGameOverStatus PkGameOverStatus => Data.GameOverStatus;

        public void SetLastOpenPkDateTime()
        {
            //不需要 重置
            if(Data.LastRankNum==1&&Data.CurRankNum==1)return;
            
            bool isChangeRank = false;
            if (Data.LastOpenDateTime == DateTime.MinValue)
            {
                //重置
                isChangeRank = true;
            }else
            {
                //如果当前时间大于上次周一下午三点，重置；
                //如果当前时间小于上次周一下午三点，并且时间差7天，同样重置
                if (DateTime.Now>Data.LastOpenDateTime||
                    (DateTime.Now<=Data.LastOpenDateTime&&(Data.LastOpenDateTime-DateTime.Now).TotalDays>7))
                {
                    isChangeRank = true;
                }
            }

            if (isChangeRank)
            {
                Data.LastRankNum = 1;
                Data.CurRankNum = 1;
                SaveToLocal();
            }
        }

        private DateTime GetPkOverDateTime(DateTime dateTime)
        {
            var dayOfWeek = Convert.ToInt32(dateTime.Date.DayOfWeek);
            int days = 0;
            if (dayOfWeek >= 5)
                days = 1 - dayOfWeek + 7;
            else
                days = 1 - dayOfWeek;
            //下周一下午三点时间
            var activityEndDateTime = DateTime.Now.AddDays(days).AddHours(15 - DateTime.Now.Hour);
            var overDateTime = new DateTime(activityEndDateTime.Year, activityEndDateTime.Month,
                activityEndDateTime.Day, 15, 0, 0);
            
            Log.Info($"CurActivityEndDateTime:{overDateTime}");
            return overDateTime;
        }

        //奖励
        public int GetRewardCoinNumByRank(PkGameOverStatus status)
        {
            int num= MultipleByRank(Data.LastRankNum);
            switch (status)
            {
                case PkGameOverStatus.Win:
                    return num * 2;
                case PkGameOverStatus.Lose:
                    return 0;
                case PkGameOverStatus.Deuce:
                    return num;
            }
            return 0;
        }

        public int MultipleByRank(int rankNum)
        {
            switch (rankNum)
            {
                case 1: return 100;
                case 2: return 200;
                case 3: return 500;
                case 4: return 1000;
                default: return 100;
            }
        }

        //结算档次活动
        public void BalanceActivity()
        {
            SetLastOpenPkDateTime();
            Data.Balance();
            RemoveListen();
            SaveToLocal();
        }

        //当前关卡胜利可以获得的奖励数量
        public int PkRewardItemNum(int levelNum)
        {
            IsFirstWin = true;
            //if (levelNum==Instance.Data.CurLevel)
            {
                switch (Instance.Data.CurLevelFailCount)
                {
                    case 0: return 10;
                    case 1:
                        IsFirstWin = false;
                        return 5;
                    case 2:
                        IsFirstWin = false;
                        return 2;
                    default :
                        IsFirstWin = false;
                        return 2;
                }
            }
            return 10;
        }

        public int GetFailCountByLevel()
        {
            return Instance.Data.CurLevelFailCount;
        }

        private string path=String.Empty;
        //获取当前数据保存的路径
        private string GetServiceDataPath(int posNum)
        {
            path = $"{Data.GroupPath}/{RecordVersionNewNum}_{Data.CurRankNum}/Group/{posNum}";
            Log.Info($"GetServiceDataPath:{path}");
            return path;
        }

        //获取当前玩家加入进游戏时的编号信息
        private string posNumPath = string.Empty;
        public void GetServicePosNum(float overTime,Action<bool> action)
        {
            if (Instance.Data.PlayerPosNum > 0)
            {
                action?.InvokeSafely(true);
                return;
            }
            
            posNumPath = $"{Data.GroupPath}/{RecordVersionNewNum}_{Data.CurRankNum}";
            var doc=FirebaseFirestore.DefaultInstance.Document(posNumPath);

            var delayEvent = GameManager.Task.AddDelayTriggerTask(overTime, () =>
            {
                action?.InvokeSafely(false);
                action = null;
            });
            int playerTotalNum = 0;
            FirebaseFirestore.DefaultInstance.RunTransactionAsync(trans =>
            {
                return trans.GetSnapshotAsync(doc).ContinueWithOnMainThread(task =>
                {
                    if (task.Result.ContainsField("PlayerTotalNum"))
                    {
                        playerTotalNum = task.Result.GetValue<int>("PlayerTotalNum");
                    }
                    playerTotalNum += 1;
                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        {"PlayerTotalNum",playerTotalNum}
                    };
                    trans.Set(doc,updates,SetOptions.MergeAll);
                });
            }).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Log.Error($"GetServicePosNum:{task.Exception}");
                    action?.InvokeSafely(false);
                    action = null;
                }
                else
                {
                    Log.Info($"GetServicePosNum：1:{playerTotalNum}");
                    //获取数据成功
                    Instance.Data.PlayerPosNum = playerTotalNum;
                    //上传当前数据
                    SaveSelfDataToService();
                    action?.InvokeSafely(true);
                    action = null;
                }
                GameManager.Task.RemoveDelayTriggerTask(delayEvent);
            });
        }

        //存数据到服务器
        public void SaveSelfDataToService()
        {
            GetServicePosNum(2, (u) =>
            {
                if(Instance.Data.PlayerPosNum==0)return;
                //每次胜利时数据存储到服务器
                var doc =FirebaseFirestore.DefaultInstance.Document(GetServiceDataPath(Instance.Data.PlayerPosNum));
                doc.SetAsync(Instance.Data.SelfPlayerPkData.GetDict(true)).ContinueWithOnMainThread(task =>
                {
                    if(task.IsFaulted||task.IsCanceled)Log.Error($"Pk赛数据上传失败；原因如下:{task.Exception}");
                    else Log.Info($"Pk赛数据上传成功");
                });
            });
        }
        
        //获取服务器目标对象数据
        public void GetTargetDataByService(Action<bool> finishAction,float overTime=12f)
        {
            Action<bool> fAction = finishAction;
            var task= GameManager.Task.AddDelayTriggerTask(overTime, () =>
            {
                fAction?.InvokeSafely(false);
                fAction = null;
            });
            GetServicePosNum(3, (u) =>
            {
                Log.Info($"GetTargetDataByService获取匹配数据111");
                if(Instance.Data.PlayerPosNum==0)return;
                Log.Info($"GetTargetDataByService获取匹配数据222");
                //根据当前玩家编号 获取目标玩家编号 然后拉去服务器数据
                int targetPosNum = Instance.Data.PlayerPosNum + (Instance.Data.PlayerPosNum%2== 1?1:-1);
                string docPath = GetServiceDataPath(targetPosNum);

                int time = 3;
                Action action = null;
                action = () =>
                {
                    Log.Info($"获取对方数据");
                    GetServiceDataByPath(docPath, (data) =>
                    {
                        Log.Info($"获取匹配数据{(data!=null?"有效1111":"无效2222")}");
                        if (data != null)
                        {
                            Instance.Data.TargetPlayerPkData = data;
                            SaveToLocal();
                            GameManager.Task.RemoveDelayTriggerTask(task);
                        
                            fAction?.InvokeSafely(true);
                            fAction = null;
                            AddListen();
                        }
                        else
                        {
                            if (time > 0 && fAction != null)
                            {
                                Log.Error($"重试");
                                GameManager.Task.AddDelayTriggerTask(2f, () =>
                                {
                                    time -= 1;
                                    action?.Invoke();
                                });
                            }
                        }
                    });
                };
                action?.Invoke();
                
                AddListen((b) =>
                {
                    if (b)
                    {
                        GameManager.Task.RemoveDelayTriggerTask(task);
                        fAction?.InvokeSafely(true);
                        fAction = null;
                    }
                });
            });
        }

        private void GetServiceDataByPath(string docPath,Action<PkGamePlayerData> action)
        {
            var doc =FirebaseFirestore.DefaultInstance.Document(docPath);
            doc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Log.Error($"GetTargetDataByService:{task.Exception}");
                }
                else
                {
                    //数据获取之后保存
                    PkGamePlayerData data=null;
                    try
                    {
                        var dict = task.Result.ToDictionary();
                        data= new PkGamePlayerData(dict);
                        action?.InvokeSafely(data);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"GetServiceDataByPath:{e.Message}");
                        action?.InvokeSafely(null);
                    }
                }
            });
        }

        public void AddListen(Action<bool> finishAction=null)
        {
            if(!Instance.Data.IsActivityOpen)return;
            if(Instance.Data.PlayerPosNum==0)return;
            if(listen!=null)return;
            Log.Info($"添加监听数据");
            
            int targetPosNum = Instance.Data.PlayerPosNum + (Instance.Data.PlayerPosNum %2== 1?1:-1);
            string docPath = GetServiceDataPath(targetPosNum);
            DocumentReference docRef = FirebaseFirestore.DefaultInstance.Document(docPath);
            listen= docRef.Listen(MetadataChanges.Include,snapshot => 
            {
                Log.Info($"111监听数据触发:{snapshot.Id}");
                try
                {
                    if (snapshot != null)
                    {
                        Log.Info($"监听数据触发:{snapshot.Id}");
                        Dictionary<string, object> dict = snapshot.ToDictionary();
                        if (dict != null)
                        {
                            var newData=new PkGamePlayerData(dict);

                            bool isItemChange = newData.ItemNum != Instance.Data.TargetPlayerPkData.ItemNum;
                            if (newData.PlayerName != Instance.Data.TargetPlayerPkData.PlayerName ||
                                newData.ItemNum != Instance.Data.TargetPlayerPkData.ItemNum ||
                                newData.PlayerHeadPortrait != Instance.Data.TargetPlayerPkData.PlayerHeadPortrait)
                            {
                                
                                Instance.Data.TargetOldSorce = Instance.Data.TargetPlayerPkData.ItemNum;
                                Instance.Data.TargetPlayerPkData = newData;
                                
                                finishAction?.InvokeSafely(true);
                                finishAction = null;
                                SaveToLocal();
                                //发送监听消息
                                if(isItemChange)
                                    GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.PkListenData));
                            }
                            
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"监听数据异常:{e.Message}");
                }
            });
            listen.ListenerTask.ContinueWithOnMainThread(t =>
            {
                Log.Error($"Listen failed: {t.Exception}");
                listen.Stop();
                listen.Dispose();
                listen = null;
            });
        }

        public void RemoveListen()
        {
            if (listen != null)
            {
                listen.Stop();
                listen.Dispose();
                listen = null;
            }
        }

        public void Lose()
        {
            if (Instance.IsActivityOpen)
            {
                //Instance.Data.CurLevel = GameManager.PlayerData.NowLevel;
                Instance.Data.CurLevelFailCount += 1;
                SaveToLocal();
            }
        }

        public void Win(out int addItemNum)
        {
            addItemNum = 0;
            if (Instance.IsActivityOpen)
            {
                addItemNum = PkRewardItemNum(GameManager.PlayerData.NowLevel);
                PkItemFlyNum = addItemNum;
                //Instance.Data.CurLevel = 0;
                Instance.Data.CurLevelFailCount = 0;
                Instance.Data.SelfPlayerPkData.ItemNum += addItemNum;
                //上传数据
                SaveSelfDataToService();
                SaveToLocal();
            }
        }

        /// <summary>
        /// 记录旧分数
        /// </summary>
        /// <param name="isSelf"></param>
        public void RecordOldSorce()
        {
            Data.SelfOldSorce = Data.SelfPlayerPkData.ItemNum;
            Data.TargetOldSorce = Data.TargetPlayerPkData.ItemNum;
            SaveToLocal();
        }

        /// <summary>
        /// 刷新记录对方数据
        /// </summary>
        public void RecordOldTargetSorce()
        {
            Data.TargetOldSorce = Data.TargetPlayerPkData.ItemNum;
            SaveToLocal();
        }

        //分组
        public void SetGroupName()
        {
           Data.GroupPath = GameManager.PlayerData.NowLevel >= 100 ? "PkGame":"PkGameLessThan100";
           SaveToLocal();
        }

        public void SetFirstGuide()
        {
            if (!Data.IfOpenFirstGuide)
            {
                Data.IfOpenFirstGuide = true;
                SaveToLocal();
            }
        }

        public void SetSecondGuide()
        {
            if (!Data.IfOpenUpgradesGuide)
            {
                Data.IfOpenUpgradesGuide = true;
                SaveToLocal();
            }
        }

        public void SetRank()
        {
            if (Data.LastRankNum != Data.CurRankNum)
            {
                Data.LastRankNum = Data.CurRankNum;
                SaveToLocal();
            }
        }

        public bool IsShowGameOverGuideText(int rewardNum,out int textCode)
        {
            int num = 1;
            //先计算出之前的状态
            bool isOldBig= (Data.SelfPlayerPkData.ItemNum - rewardNum)>=Data.TargetPlayerPkData.ItemNum;
            if (isOldBig  && (Data.SelfPlayerPkData.ItemNum<Data.TargetPlayerPkData.ItemNum))
            {
                num = 1;
            }else if (isOldBig && (Data.SelfPlayerPkData.ItemNum > Data.TargetPlayerPkData.ItemNum))
            {
                num = 2;
            }
            else if (isOldBig && (Data.SelfPlayerPkData.ItemNum == Data.TargetPlayerPkData.ItemNum))
            {
                num = 3;
            }
            else if (!isOldBig && (Data.SelfPlayerPkData.ItemNum > Data.TargetPlayerPkData.ItemNum))
            {
                num = 4;
            }
            else if (!isOldBig && (Data.SelfPlayerPkData.ItemNum < Data.TargetPlayerPkData.ItemNum))
            {
                num = 5;
            }
            else if (!isOldBig && (Data.SelfPlayerPkData.ItemNum == Data.TargetPlayerPkData.ItemNum))
            {
                num = 6;
            }

            //如果跟上次弹出的序号一样，则不弹出对话
            if (num == Data.LastTextNum)
            {
                textCode = 0;
                return false;
            }

            textCode = num;
            if (textCode == 3)
            {
                textCode = 0;
                return false;
            }

            if (!Data.RecordGameOverTextDict.TryGetValue(num, out int textNum))
            {
                Data.RecordGameOverTextDict.Add(num,1);
                Data.LastTextNum = num;
            }
            else
            {
                if (textNum >= 3)
                {
                    textCode = 0;
                    return false;
                }
                else
                {
                    Data.RecordGameOverTextDict[num] += 1;
                    Data.LastTextNum = num;
                }
            }

            return true;
        }

        public bool IsNoNetwork()
        {
            return Data.TargetPlayerPkData.ItemNum <= 0 && 
                   Data.TargetPlayerPkData.PlayerName.Equals("???")&&
                   Data.TargetPlayerPkData.PlayerHeadPortrait==0;
        }
    }
}