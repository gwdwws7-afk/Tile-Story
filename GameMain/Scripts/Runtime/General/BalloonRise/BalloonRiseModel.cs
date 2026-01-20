using Firebase.Analytics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MySelf.Model
{
    public class BalloonRiseData
    {
        public DateTime endTime;
        public bool upgrade;
        public int stage;
        public DateTime stageEndTime;
        public int stageTarget;
        public int stageStartLevel;
        public List<BalloonRiseRobotPlayer> robotPlayerDatas;
        public string serialNumber;
        public int score;
        public int recordScore;
        public DateTime updateTime;
        public int rank;
        public int winningStreakTimes = 0;
        public bool scoreChange = false;
        public bool showedStartMenu;
        public bool popedStartMenu;
        public bool checkIsFinishStage;
    }

    public class BalloonRiseModel : BaseModel<BalloonRiseModel, BalloonRiseData>
    {
        private List<BalloonRisePlayerBase> _playerDatas = new List<BalloonRisePlayerBase>();
        public List<BalloonRisePlayerBase> BalloonRisePlayerDatas
        {
            get
            {
                if (Data.stageEndTime == DateTime.MinValue)
                    return new List<BalloonRisePlayerBase>();
                if (Data.robotPlayerDatas.Count > 4)
                {
                    Data.robotPlayerDatas.Clear();
                    AddRobot(4);
                }
                else if (Data.robotPlayerDatas.Count < 4)
                {
                    AddRobot(4 - Data.robotPlayerDatas.Count);
                }

                _playerDatas.Clear();
                foreach (var robot in Data.robotPlayerDatas)
                {
                    _playerDatas.Add(robot);
                }
                _playerDatas.Add(LocalPlayer);
                OrderPlayerDatas(ref _playerDatas);
                return _playerDatas;
            }
        }

        private void OrderPlayerDatas(ref List<BalloonRisePlayerBase> playerDatas)
        {
            // for (int i = playerDatas.Count - 1; i > 0; i--)
            // {
            //     int randomIndex = Random.Range(0, i);
            //     (playerDatas[randomIndex], playerDatas[i]) = (playerDatas[i], playerDatas[randomIndex]);
            // }

            playerDatas.Sort((left, right) =>
            {
                if (left.Score > right.Score)
                    return -1;
                if (left.Score == right.Score && left.UpdateTime < right.UpdateTime)
                    return -1;
                return 1;
            });

            int rank = 0;
            foreach (var child in playerDatas)
            {
                if (child == null || string.IsNullOrEmpty(child.Name) || child.Score < 0) continue;
                rank++;
                child.Rank = rank;
                if (child.IsSelf)
                    Rank = rank;
            }
        }

        public BalloonRisePlayer LocalPlayer
        {
            get => new BalloonRisePlayer
            {
                SerialNumber = Data.serialNumber,
                Name = string.IsNullOrEmpty(GameManager.PlayerData.PlayerName)
                    ? $"User{Random.Range(1, 1000):D3}"
                    : GameManager.PlayerData.PlayerName,
                Avatar = GameManager.PlayerData.HeadPortrait,
                Score = Data.score,
                RecordScore = Data.recordScore,
                UpdateTime = Data.updateTime,
                Rank = Data.rank
            };
        }

        public void AddRobot(int robotNum = 1)
        {
            for (int i = 0; i < robotNum; i++)
            {
                GetRobotName(name =>
                {
                    BalloonRiseRobotPlayer robot = new BalloonRiseRobotPlayer();
                    robot.InitRobot(name);
                    Data.robotPlayerDatas.Add(robot);
                });
            }
            SaveToLocal();
        }

        private RandomNamesConfig _robotNames;
        public void GetRobotName(Action<string> robotName)
        {
            if (Random.Range(0, 10f) <= 0.7f)
            {
                robotName?.Invoke("User" + Random.Range(0, 1000));
                return;
            }
            if (_robotNames == null)
            {
                LoadRandomNames((flag) =>
                {
                    if (flag)
                    {
                        robotName?.Invoke(_robotNames.GetRandomName());
                    }
                    else
                    {
                        robotName?.Invoke("User" + Random.Range(0, 1000));
                    }
                });
            }
            else
            {
                robotName?.Invoke(_robotNames.GetRandomName());
            }
        }

        public void LoadRandomNames(Action<bool> callback = null)
        {
            TextAsset asset = AddressableUtils.LoadAsset<TextAsset>("RandomNamesConfig");
            _robotNames = JsonConvert.DeserializeObject<RandomNamesConfig>(asset.text);
            AddressableUtils.ReleaseAsset(asset);
            callback?.Invoke(true);

            // Addressables.LoadAssetAsync<TextAsset>("RandomNamesConfig").Completed += (obj) =>
            // {
            //     if (obj.Status == AsyncOperationStatus.Succeeded)
            //     {
            //         _robotNames = JsonConvert.DeserializeObject<RandomNamesConfig>(obj.Result.text);
            //
            //         Addressables.Release(obj);
            //         callback?.Invoke(true);
            //     }
            //     else
            //     {
            //         Log.Error("load {0} dataTable fail", "RandomNamesConfig");
            //         callback?.Invoke(false);
            //     }
            // };
        }


        public bool Upgrade
        {
            get => Data.upgrade;
            set
            {
                if (Data.upgrade != value)
                {
                    Data.upgrade = value;
                    SaveToLocal();
                }
            }
        }

        public int Score
        {
            get => Data.score;
            set
            {
                if (Data.score != value)
                {
                    Data.score = value;
                    Data.updateTime = DateTime.Now;
                    SaveToLocal();
                }
            }
        }

        public int RecordScore
        {
            get => Data.recordScore;
            set
            {
                if (Data.recordScore != value)
                {
                    Data.recordScore = value;
                    SaveToLocal();
                }
            }
        }

        public int Rank
        {
            get => Data.rank;
            set
            {
                if (Data.rank != value)
                {
                    Data.rank = value;
                    SaveToLocal();
                }
            }
        }

        public int WinningStreakTimes
        {
            get => Data.winningStreakTimes;
            set
            {
                if (Data.winningStreakTimes != value)
                {
                    Data.winningStreakTimes = value;
                    SaveToLocal();
                }
            }
        }

        public bool ScoreChange
        {
            get => Data.scoreChange;
            set
            {
                if (Data.scoreChange != value)
                {
                    Data.scoreChange = value;
                    SaveToLocal();
                }
            }
        }


        public bool CheckIsOpen()
        {
            bool isUnlockByLevel = GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockBalloonRiseLevel;
            if (isUnlockByLevel && Data.endTime < DateTime.Now && Data.stageEndTime == DateTime.MinValue)
            {
                var newEndTime=GameManager.DataTable.GetDataTable<DTBalloonRiseScheduleData>().Data.GetNowActiveActivityEndTime();
                if (Data.endTime != newEndTime)
                {
                    Data.endTime = newEndTime;
                    ResetByNewDay();
                }
            }
            bool isUnlockByTime = DateTime.Now < Data.endTime;
            bool isFinish = Data.stage == 3 && Data.upgrade;
            return isUnlockByLevel && isUnlockByTime && !isFinish || Data.stageEndTime != DateTime.MinValue;
        }

        public bool CheckIsFinishStage
        {
            get => Data.checkIsFinishStage;
            set
            {
                if (Data.checkIsFinishStage != value)
                {
                    Data.checkIsFinishStage = value;
                    SaveToLocal();
                }
            }
        }

        public void InitStage()
        {
            if (Upgrade)
            {
                Data.stage += 1;
                Upgrade = false;
            }
            Data.stage = Math.Clamp(Data.stage, 1, 3);

            switch (Data.stage)
            {
                case 1:
                    Data.stageEndTime = DateTime.Now.AddMinutes(15);
                    Data.stageTarget = 3;
                    break;
                case 2:
                    Data.stageEndTime = DateTime.Now.AddMinutes(25);
                    Data.stageTarget = 5;
                    break;
                case 3:
                    Data.stageEndTime = DateTime.Now.AddMinutes(40);
                    Data.stageTarget = 7;
                    break;
            }
            Data.stageStartLevel = GameManager.PlayerData.NowLevel;
            AddRobot(4);
            GameManager.Task.AddDelayTriggerTask(0.01f, () =>
            {
                Data.updateTime = Data.robotPlayerDatas[Random.Range(0, Data.robotPlayerDatas.Count)].UpdateTime;
                SaveToLocal();
            });
            GameManager.Firebase.RecordMessageByEvent("Balloon_Start", new Parameter("Stage", Data.stage));
        }

        public void ResetStage()
        {
            Data.stageEndTime = DateTime.MinValue;
            Data.robotPlayerDatas = new List<BalloonRiseRobotPlayer>();
            Score = 0;
            RecordScore = 0;
            Rank = 0;
            WinningStreakTimes = 0;
            ScoreChange = false;
            CheckIsFinishStage = false;
            SaveToLocal();
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

        public bool PoppedStartMenu
        {
            get => Data.popedStartMenu;
            set
            {
                if (Data.popedStartMenu != value)
                {
                    Data.popedStartMenu = value;
                    SaveToLocal();
                }
            }
        }

        public void ResetByNewDay()
        {
            Data.stage = 0;
            ResetStage();
            ShowedStartMenu = false;
            PoppedStartMenu = false;
        }

        public void SimulateRobotTimeAdd(int index, float seconds)
        {
            Data.robotPlayerDatas[index].UpdateByTime(seconds, seconds);
        }

        public void GameLose()
        {
            WinningStreakTimes = 0;
            Score = WinningStreakTimes;
        }
    }
}
