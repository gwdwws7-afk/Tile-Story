using MySelf.Model;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BalloonRiseActivityManager : TaskManagerBase
{
    public bool CheckIsOpen => BalloonRiseModel.Instance.CheckIsOpen();
    public bool CheckIsFinishStage
    {
        get => BalloonRiseModel.Instance.CheckIsFinishStage;
        set => BalloonRiseModel.Instance.CheckIsFinishStage = value;
    }
    public DateTime EndTime => BalloonRiseModel.Instance.Data.endTime;
    public int Stage => BalloonRiseModel.Instance.Data.stage;
    public DateTime StageEndTime => BalloonRiseModel.Instance.Data.stageEndTime;
    public int StageTarget => BalloonRiseModel.Instance.Data.stageTarget;
    public int StageStartLevel => BalloonRiseModel.Instance.Data.stageStartLevel;
    public List<BalloonRiseRobotPlayer> RobotPlayers => BalloonRiseModel.Instance.Data.robotPlayerDatas;
    public BalloonRisePlayer SelfPlayer => BalloonRiseModel.Instance.LocalPlayer;
    public List<BalloonRisePlayerBase> PlayerDatas => BalloonRiseModel.Instance.BalloonRisePlayerDatas;

    public bool Upgrade
    {
        get => BalloonRiseModel.Instance.Upgrade;
        set => BalloonRiseModel.Instance.Upgrade = value;
    }

    public int Score
    {
        get => BalloonRiseModel.Instance.Score;
        set => BalloonRiseModel.Instance.Score = value;
    }

    public int WinningStreakTimes
    {
        get => BalloonRiseModel.Instance.WinningStreakTimes;
        set => BalloonRiseModel.Instance.WinningStreakTimes = value;
    }

    public bool ScoreChange
    {
        get => BalloonRiseModel.Instance.ScoreChange;
        set => BalloonRiseModel.Instance.ScoreChange = value;
    }

    public int Rank
    {
        get
        {
            List<BalloonRisePlayerBase> playerDatas = GameManager.Task.BalloonRiseManager.PlayerDatas;
            return BalloonRiseModel.Instance.Rank;
        }
    }

    public bool ShowedStartMenu
    {
        get => BalloonRiseModel.Instance.ShowedStartMenu;
        set => BalloonRiseModel.Instance.ShowedStartMenu = value;
    }

    public bool PoppedStartMenu
    {
        get => BalloonRiseModel.Instance.PoppedStartMenu;
        set => BalloonRiseModel.Instance.PoppedStartMenu = value;
    }

    public void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (DateTime.Now > StageEndTime) return;
        if (CheckIsFinishStage) return;

        if (RobotPlayers != null && RobotPlayers.Count > 0)
        {
            foreach (var robot in RobotPlayers)
            {
                if (!CheckIsFinishStage)
                    robot.UpdateByTime(elapseSeconds, realElapseSeconds);
            }
        }
    }

    public void UpdateRobotScore()
    {
        if (CheckIsFinishStage) return;

        if (RobotPlayers != null && RobotPlayers.Count > 0)
        {
            List<int> finishRobotIndex = new List<int>();
            for (int i = 0; i < RobotPlayers.Count; i++)
            {
                RobotPlayers[i].CheckTimeAfterLoad();
                if (RobotPlayers[i].Score == StageTarget)
                    finishRobotIndex.Add(i);
            }

            if (finishRobotIndex.Count > 1)
            {
                finishRobotIndex.Remove(finishRobotIndex[Random.Range(0, finishRobotIndex.Count)]);
                foreach (int index in finishRobotIndex)
                {
                    RobotPlayers[index].Score -= 1;
                }
            }
            BalloonRiseModel.Instance.SaveToLocal();
        }
    }

    public override void OnInit()
    {

    }

    public override void OnReset()
    {

    }

    public override void ConfirmTargetCollection(TaskTargetCollection collection)
    {

    }

    public override TaskTarget GetTaskTarget()
    {
        return TaskTarget.None;
    }
}