using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Firestore;
using MyFrameWork.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MySelf.Model
{
    public class PersonRankModel : BaseModel<PersonRankModel, PersonRankRecordedData>, IRankModelBase
    {
        public RankType RankType => RankType.PersonRank;

        public int RankTerm { get; set; }

        private FirebaseFirestore _db;

        public string SerialNum
        {
            get => Data.SerialNum;
            private set
            {
                if (Data.SerialNum != value)
                {
                    Data.SerialNum = value;
                    SaveToLocal();
                }
            }
        }

        public PersonRankLevel RankLevel
        {
            get => Data.RankLevel;
            private set
            {
                if (Data.RankLevel != value)
                {
                    Data.RankLevel = value;
                    SaveToLocal();
                }
            }
        }

        public PersonRankState TaskState
        {
            get => Data.TaskState;
            set
            {
                if (Data.TaskState != value)
                {
                    Data.TaskState = value;
                    SaveToLocal();
                }
            }
        }

        public int Score
        {
            get => Data.Score;
            set
            {
                if (Data.Score != value)
                {
                    Data.Score = value;
                    SaveToLocal();
                }
            }
        }

        public int ScoreInGame
        {
            get => Data.ScoreInGame;
            set
            {
                if (Data.ScoreInGame != value)
                {
                    Data.ScoreInGame = value;
                    SaveToLocal();
                }
            }
        }

        public int LastScore
        {
            get => Data.LastScore;
            set
            {
                if (Data.LastScore != value)
                {
                    Data.LastScore = value;
                    SaveToLocal();
                }
            }
        }

        public DateTime EndTime
        {
            get => Data.EndTime;
            set
            {
                Data.EndTime = value;
                SaveToLocal();
            }
        }

        public DateTime StartTime
        {
            get => Data.StartTime;
            set
            {
                Data.StartTime = value;
                SaveToLocal();
            }
        }
        
        public float LastUpdateTime
        {
            get => Data.LastUpdateTime;
            set
            {
                Data.LastUpdateTime = value;
                SaveToLocal();
            }
        }

        public int ContinuousWinTime
        {
            get => Data.ContinuousWinTime;
            private set
            {
                if (Data.ContinuousWinTime != value)
                {
                    Data.ContinuousWinTime = value;
                    SaveToLocal();
                }
            }
        }

        public int LastContinuousWinTime
        {
            get => Data.LastContinuousWinTime;
            private set
            {
                if (Data.LastContinuousWinTime != value)
                {
                    Data.LastContinuousWinTime = value;
                    SaveToLocal();
                }
            }
        }

        public int LevelPass
        {
            get => Data.LevelPass;
            set
            {
                Data.LevelPass = value;
                SaveToLocal();
            }
        }

        public int Rank
        {
            get
            {
                // return 5;
                if (TaskState == PersonRankState.None || TaskState == PersonRankState.End)
                {
                    return 0;
                }

                var data = RankDatas;
                if (_rankDatas == null && TaskState == PersonRankState.Playing)
                {
                    _rankDatas = new List<PersonRankData> { LocalData };
                    OrderPlayerRankDatas(ref _rankDatas);
                }

                return Data.Rank;
            }
            private set
            {
                if (Data.Rank != value)
                {
                    Data.Rank = value;
                    SaveToLocal();
                }
            }
        }

        public int LastRank
        {
            get => Data.LastRank;
            set
            {
                if (Data.LastRank != value)
                {
                    Data.LastRank = value;
                    SaveToLocal();
                }
            }
        }

        public int LastUpRank
        {
            get => Data.LastUpRank;
            // get => 11;
            set
            {
                if (Data.LastUpRank != value)
                {
                    Data.LastUpRank = value;
                    SaveToLocal();
                }
            }
        }

        public bool HasShownWelcome
        {
            get => Data.HasShownWelcome;
            set
            {
                Data.HasShownWelcome = value;
                SaveToLocal();
            }
        }

        public bool NeedToFlyMedal
        {
            get => Data.NeedToFlyMedal;
            set
            {
                Data.NeedToFlyMedal = value;
                SaveToLocal();
            }
        }

        private List<PersonRankData> _rankDatas;

        public List<PersonRankData> RankDatas
        {
            get
            {
                _rankDatas = Data.RankDatas ?? new List<PersonRankData> { LocalData };

                if (!string.IsNullOrEmpty(LocalData.Name))
                {
                    _rankDatas.Merge(LocalData, a => a.IsSelf());
                }

                OrderPlayerRankDatas(ref _rankDatas);
                return _rankDatas;
            }
            set
            {
                Data.RankDatas = value;
                _rankDatas = Data.RankDatas;
                SaveToLocal();
            }
        }

        private void OrderPlayerRankDatas(ref List<PersonRankData> playerRankDatas)
        {
            if (playerRankDatas == null) return;

            foreach (var child in playerRankDatas)
            {
                if (!child.IsSelf() || LocalData.Score < 0) continue;
                child.Score = LocalData.Score;
                break;
            }

            playerRankDatas = playerRankDatas.Where(obj => obj != null).ToList();

            if (playerRankDatas == null || playerRankDatas.Count <= 0) return;

            //排序
            playerRankDatas.Sort((left, right) =>
            {
                if (left.Score > right.Score)
                    return -1;
                if (left.Score == right.Score && right.IsSelf())
                    return -1;
                return 1;
            });

            //设Rank
            var rank = 0;
            if (playerRankDatas == null) return;
            foreach (var data in playerRankDatas)
            {
                if (data == null || data.Name == "" && !data.IsSelf() || data.Score < 0) continue;

                data.Rank = rank + 1;
                rank++;
                if (data.IsSelf())
                {
                    Rank = rank;
                }
            }
        }

        public PersonRankData LocalData =>
            new PersonRankData
            {
                HeadId = GameManager.PlayerData.HeadPortrait,
                Name = string.IsNullOrEmpty(GameManager.PlayerData.PlayerName)
                    ? $"User{Random.Range(1, 1000):D3}"
                    : GameManager.PlayerData.PlayerName,
                Score = Data.Score,
                SerialNum = Data.SerialNum
            };

        public bool HasShownPersonRankGuide
        {
            get => Data.HasShownPersonRankGuide;
            set
            {
                Data.HasShownPersonRankGuide = value;
                SaveToLocal();
            }
        }


        public void Init()
        {
            _db = FirebaseFirestore.DefaultInstance;
            Data = GetLocalModel();
            _rankDatas = Data.RankDatas;
            if (DateTime.Now > EndTime && TaskState == PersonRankState.None)
            {
                Data.ResetData();
                SaveToLocal();
            }

            InitTimer();
        }

        private DateTime startDate;

        private void InitTimer()
        {
            startDate = new DateTime(2023, 11, 10, 15, 0, 0);
            SetCurrentTerm(!string.IsNullOrEmpty(SerialNum));
        }

        private void SetCurrentTerm(bool hasKey)
        {
            if (hasKey)
            {
                RankTerm = GetTermGroupIndexBySerialNum(SerialNum).Item1;
            }
            else
            {
                var nowTime = DateTime.Now;
                var secondSpan = new TimeSpan(nowTime.Ticks - startDate.Ticks);
                var totalSec = (int)secondSpan.TotalSeconds;
                // var interDate = 7;
                var totalDays = totalSec / 24 / 60 / 60;
                if (totalDays % 7 > 2)
                {
                    RankTerm = 2 * (totalDays / 7) + 2;
                }
                else
                {
                    RankTerm = 2 * (totalDays / 7) + 1;
                }
            }

            EndTime = startDate.AddDays(7 * (RankTerm / 2) + 3 * (RankTerm % 2));
            StartTime = EndTime.AddDays(RankTerm % 2 == 0 ? -4 : -3);
        }


        private int[] multipleNum = { 1, 2, 4, 6, 10 };

        public void GameWin()
        {
            ScoreInGame = 1;
            ScoreInGame *= GetMultipleNum(ContinuousWinTime);
            LastScore = Score;
            Score += ScoreInGame;
            LevelPass += 1;
            LastContinuousWinTime = ContinuousWinTime;
            ContinuousWinTime = Mathf.Min(4, ContinuousWinTime + 1);
            NeedToFlyMedal = true;
            SyncData(task =>
            {
                Log.Info("SyncData " + task);
                GetPersonRankDataFromServer(null);
            });
        }

        public int GetMultipleNum(int winTime)
        {
            return multipleNum[winTime];
        }

        public void GameLose()
        {
            LastScore = Score;
            LastContinuousWinTime = ContinuousWinTime;
            ContinuousWinTime = 0;
            ScoreInGame = 0;
        }

        private void SyncData(Action<bool> callback)
        {
            if (!GameManager.Task.PersonRankManager.CheckIsOpen())
            {
                Log.Info("PersonRank is not open");
                return;
            }

            if (LastScore == Score)
            {
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(SerialNum))
            {
                FirstAddToServer(flag =>
                {
                    if (flag)
                        GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Playing);
                    callback?.Invoke(flag);
                });
            }
            else
            {
                var (term, group, index) = GetTermGroupIndexBySerialNum(SerialNum);
                if(group==0||index==0)
                {
                    FirstAddToServer(flag =>
                    {
                        if (flag)
                            GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Playing);
                        callback?.Invoke(flag);
                    });
                }
                else
                {
                    SavePersonRankDataToServer(callback);
                }
            }
        }


        private void FirstAddToServer(Action<bool> callback)
        {
            var termDoc = _db.Collection("PersonRank").Document($"{RankTerm:D8}");
            var num = 0L;
            ClearFirestore();
            _db.RunTransactionAsync(transaction =>
            {
                return transaction.GetSnapshotAsync(termDoc).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        callback?.Invoke(false);
                        Log.Error("Get termDoc fail，Exception：" + task.Exception);
                        return;
                    }

                    var result = task.Result;
                    var termData = result.ToDictionary();
                    if (!ReferenceEquals(termData, null) && termData.TryGetValue(RankLevel.ToString(), out var obj))
                    {
                        num = (long)obj;
                        num += 1;
                        termData[RankLevel.ToString()] = num;
                    }
                    else if (ReferenceEquals(termData, null))
                    {
                        num = 1L;
                        termData = new Dictionary<string, object>();
                        termData.Add(RankLevel.ToString(), 1);
                    }
                    else
                    {
                        num = 1L;
                        termData.Add(RankLevel.ToString(), 1);
                    }

                    transaction.Set(termDoc, termData, SetOptions.MergeAll);
                });
            }).ContinueWithOnMainThread(transactionTask =>
            {
                if (transactionTask.IsCanceled || transactionTask.IsFaulted)
                {
                    callback?.Invoke(false);
                    Log.Error("FirstAddToServer fail，Exception：" + transactionTask.Exception);

                    return;
                }

                SerialNum = GetRankSerialNumber(RankTerm, num);
                _writeCount = int.MaxValue;
                SavePersonRankDataToServer(callback,true);
            });
        }

        private void ClearFirestore()
        {
            // if(_db.App.Dispose())
            // _db.TerminateAsync();
            // _db.ClearPersistenceAsync();
        }

        private int _writeCount;
        private void SavePersonRankDataToServer(Action<bool> callback,bool isFirst=false)
        {
            if (_writeCount < 3)
            {
                _writeCount++;
                callback?.Invoke(false);
                return;
            }

            _writeCount = 0;
            ClearFirestore();
            var (term, group, index) = GetTermGroupIndexBySerialNum(SerialNum);
            var groupStr = $"Group{group:D8}";
            var myRankDoc = _db.Collection($"PersonRank/{term:D8}/{RankLevel.ToString()}").Document(groupStr);
            
            //第一次进组时分数大于100 则不在组内显示
            if (isFirst)
            {
                SetIsShowIntGroupByFirst();
            }

            var child = new Dictionary<string, object>
            {
                { "HeadId", LocalData.HeadId },
                { "Name", LocalData.Name },
                { "Score", LocalData.Score },
                { "SerialNum", LocalData.SerialNum },
                { "IsNoShowInGroup", LocalData.IsNoShowInGroup },
            };

            var updateChild = new Dictionary<string, object>
            {
                { index.ToString(), child }
            };
            myRankDoc.SetAsync(updateChild, SetOptions.MergeAll).ContinueWithOnMainThread(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                {
                    callback?.Invoke(false);
                    _writeCount = int.MaxValue;
                    Log.Error("SavePersonRankDataToServer fail, Exception: " + t.Exception);
                    return;
                }

                callback?.Invoke(true);
            });
        }

        private void SetIsShowIntGroupByFirst()
        {
            LocalData.IsNoShowInGroup=LocalData.Score>100;
        }

        private bool _isGettingRankDataFromServer;

        public void GetPersonRankDataFromServer(Action<bool> callback)
        {
            if (_isGettingRankDataFromServer)
            {
                Log.Warning("GetPersonRankDataFromServer is running");
                callback?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(SerialNum))
            {
                Log.Warning("SerialNum is null");
                callback?.Invoke(false);
                return;
            }

            ClearFirestore();

            //Debug.Log($"SerialNum:{SerialNum}");
            _isGettingRankDataFromServer = true;
            var (term, group, index) = GetTermGroupIndexBySerialNum(SerialNum);
            var groupStr = $"Group{group:D8}";
            var myRankDoc = _db.Collection($"PersonRank/{term:D8}/{RankLevel.ToString()}").Document(groupStr);
            myRankDoc.GetSnapshotAsync().ContinueWithOnMainThread(t =>
            {
                _isGettingRankDataFromServer = false;
                if (t.IsCanceled || t.IsFaulted)
                {
                    callback?.Invoke(false);
                    Log.Error("GetPersonRankDataFromServer fail, Exception: " + t.Exception);
                    return;
                }
                var result = t.Result;
                var data = result.ToDictionary();
                if (data.Count == 0)
                {
                    Log.Error("GetPersonRankDataFromServer fail, Exception: list.Count==0!" );
                    callback?.Invoke(false);
                    return;
                }
                var list = new List<PersonRankData>();
                foreach (var kv in data)
                {
                    var child = (Dictionary<string, object>)kv.Value;
                    list.Add(new PersonRankData(child));
                }

                RankDatas = FilterPlayersByScoreNum(list);//筛选
                LastUpdateTime = Time.realtimeSinceStartup;
                callback?.Invoke(true);
            });
        }
        
        //剔除不显示对象
        internal List<PersonRankData> FilterPlayersByScoreNum(List<PersonRankData> list)
        {
            int i = 0;
            while (i<list.Count)
            {
                //不是自己 并且分数大于一百
                if (!list[i].IsSelf() &&
                    list[i].IsNoShowInGroup)
                {
                    list.RemoveAt(i);
                }
                else i=i+1;
            }
            return list;
        }

        internal void SendRewards(Action callback)
        {
            if (TaskState != PersonRankState.Finished)
            {
                callback?.Invoke();
                return;
            }

            // GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.SendingReward);

            var rewards = GameManager.Task.PersonRankManager.TaskData.GetRewardsByLevel(RankLevel, Rank);
            foreach (var reward in rewards)
            {
                RewardManager.Instance.AddNeedGetReward(reward.Key, reward.Value);
            }

            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false,
                () =>
                {
                    //GameManager.Process.EndProcess(ProcessType.PersonRankFinish);
                    InitTimer();
                    var topPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                    topPanel.SetLeaderBoardBtnState();

                    callback?.Invoke();
                });
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.PersonRank_Reward_Claim,
                new Parameter("RankLevel", RankLevel.ToString()), new Parameter("Rank", Rank));
            if (Rank > 0 && Rank <= 3) 
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.PersonRank_TrophiesNumbuer_Get,
    new Parameter("Stage", RankLevel.ToString()), new Parameter("Rank", Rank), new Parameter("TrophiesNum", Score));

            if (GameManager.Task.PersonRankManager.WillLevelUp())
            {
                RankLevel += 1;
            }

            GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.End);
            Data.ResetData();
            SaveToLocal();
        }

        public void RestartGame()
        {
            GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.End);
            Data.ResetData();
            SaveToLocal();
            InitTimer();
            var topPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            topPanel.SetLeaderBoardBtnState();
        }


        private string GetRankSerialNumber(int term, long teamNumber)
        {
            var (groupNum, indexNum) = GetGroupAndIndexByTeamNumber(teamNumber);
            return $"{term:D4}_{groupNum:D4}_{indexNum:D2}";
        }

        private (int, int) GetGroupAndIndexByTeamNumber(long playerSerialNumber)
        {
            if (playerSerialNumber <= 0) return (0, 0);

            var groupNum = playerSerialNumber / 50 + (playerSerialNumber % 50 == 0 ? 0 : 1);
            var lastGroupNum = playerSerialNumber - groupNum * 50 + 50;

            return ((int)groupNum, (int)lastGroupNum);
        }

        private (int, int, int) GetTermGroupIndexBySerialNum(string serialNum)
        {
            var serialNumArray = serialNum.Split('_');
            if (serialNumArray.Length != 3) return (0, 0, 0);
            var term = int.Parse(serialNumArray[0]);
            var group = int.Parse(serialNumArray[1]);
            var index = int.Parse(serialNumArray[2]);
            return (term, group, index);
        }
    }

    public class PersonRankRecordedData
    {
        public string SerialNum;
        public int Score;
        public PersonRankLevel RankLevel;
        public int LastScore;
        public DateTime EndTime;
        public DateTime StartTime;
        public int ScoreInGame;
        public PersonRankState TaskState;
        public int LevelPass;
        public List<PersonRankData> RankDatas;
        public int ContinuousWinTime;
        public int LastContinuousWinTime;
        public int Rank;
        public int LastRank;
        public int LastUpRank;
        public bool HasShownWelcome;
        public bool NeedToFlyMedal;
        public bool HasShownPersonRankGuide;
        public float LastUpdateTime;

        public void ResetData()
        {
            ContinuousWinTime = 0;
            LastContinuousWinTime = 0;
            ScoreInGame = 0;
            LastScore = 0;
            Score = 0;
            Rank = 0;
            LastRank = 0;
            LevelPass = 0;
            LastUpRank = 0;
            RankDatas = null;
            HasShownWelcome = false;
            SerialNum = "";
            NeedToFlyMedal = false;
            LastUpdateTime = 0;
        }
    }
}