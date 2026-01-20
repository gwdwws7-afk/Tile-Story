using MySelf.Model;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public interface IRobotPlayerBase
{
    public void InitRobot(string name);

    public void UpdateByTime(float elapseSeconds, float realElapseSeconds);

    public void CheckTimeAfterLoad();
}


public class BalloonRiseRobotPlayer : BalloonRisePlayerBase, IRobotPlayerBase
{
    public override bool IsRobot => true;
    public override bool IsSelf => false;

    public int RobotGrade;
    public DateTime SaveTime;
    private float m_AddTimeCounter = 0;
    private float m_AddTriggerTime = Random.Range(60f, 240f);
    private float m_SubtractTimeCounter = 0;
    private float m_SubtractTriggerTime = Random.Range(60f, 120f);
    public float SubtractScore = 0;

    public void InitRobot(string name)
    {
        SerialNumber = "robot";
        Name = name;
        Avatar = Random.Range(0, 9);
        UpdateTime = DateTime.Now;
        RealScore = 0;
        RecordScore = Score;

        RobotGrade = GetRandomGrade();
        SaveTime = UpdateTime;
    }

    private int GetRandomGrade()
    {
        if (GameManager.Task.BalloonRiseManager.Stage == 1)
        {
            int random = Random.Range(1, 56);
            if (random <= 15)
                return 3;
            if (random <= 35)
                return 2;
            return 1;
        }
        if (GameManager.Task.BalloonRiseManager.Stage == 2)
        {
            int random = Random.Range(1, 71);
            if (random <= 15)
                return 4;
            if (random <= 30)
                return 3;
            if (random <= 50)
                return 2;
            return 1;
        }
        else
        {
            int random = Random.Range(1, 101);
            if (random <= 15)
                return 6;
            if (random <= 30)
                return 5;
            if (random <= 45)
                return 4;
            if (random <= 60)
                return 3;
            if (random <= 80)
                return 2;
            return 1;
        }
    }

    private float CalculateScore(float time)
    {
        // if (GameManager.Task.BalloonRiseManager.Rank == 1)
        // {
        //     if (Random.Range(0, 5) == 0) return 0;
        // }
        // else
        // {
            if (Random.Range(0, 2) == 0) return 0;
        // }

        //打关数增加随机值
        float randomNum = 1f / 20f * time / 60 + 1f / 14f;

        //机器人成长速率
        float growthRate = RobotGrade switch
        {
            1 => 0.7f,
            2 => 0.85f,
            3 => 1f,
            4 => 1.5f,
            5 => 1.8f,
            6 => 2f,
            _ => 0
        };

        float ans = randomNum + growthRate;
        return ans;
    }

    private float CalculateSubtractScore()
    {
        BalloonRiseActivityManager manager = GameManager.Task.BalloonRiseManager;
        if (manager.Rank >= 4 &&
            manager.Score != manager.PlayerDatas[0].Score)
        {
            if (Random.Range(1, 101) > 75) return SubtractScore;
        }
        else
        {
            if (Random.Range(1, 101) > 20) return SubtractScore;
        }

        //机器人成长速率
        float growthRate = RobotGrade switch
        {
            1 => 0.7f,
            2 => 0.85f,
            3 => 1f,
            4 => 1.5f,
            5 => 1.8f,
            6 => 2f,
            _ => 0
        };

        SubtractScore += growthRate;
        return SubtractScore;
    }

    public void UpdateByTime(float elapseSeconds, float realElapseSeconds)
    {
        if (Score >= GameManager.Task.BalloonRiseManager.StageTarget) return;

        m_AddTimeCounter += realElapseSeconds;
        if (m_AddTimeCounter >= m_AddTriggerTime)
        {
            int lastScore = Score;
            RealScore += CalculateScore(m_AddTriggerTime);
            SaveTime = DateTime.Now;
            m_AddTimeCounter = 0;
            m_AddTriggerTime = Random.Range(60f, 240f);

            if (Score != lastScore)
            {
                UpdateTime = SaveTime;
                GameManager.Event.Fire(this, OnRobotsScoreUpdateEventArgs.Create(RobotType.BalloonRiseRobot));
                //Debug.LogError($"Robot {Name}'s LastScore is {lastScore}, current Score is {Score}.");
                
                if (Score >= GameManager.Task.BalloonRiseManager.StageTarget)
                    GameManager.Task.BalloonRiseManager.CheckIsFinishStage = true;
            }
            BalloonRiseModel.Instance.SaveToLocal();
        }

        m_SubtractTimeCounter += realElapseSeconds;
        if (m_SubtractTimeCounter >= m_SubtractTriggerTime)
        {
            CalculateSubtractScore();
            SaveTime = DateTime.Now;
            m_SubtractTimeCounter = 0;
            m_SubtractTriggerTime = Random.Range(60f, 120f);

            if (SubtractScore > RealScore && RealScore != 0)
            {
                //Debug.LogError($"Robot {Name}'s LastRealScore is {RealScore}, Subtract {SubtractScore} to 0.");
                SubtractScore = 0;
                RealScore = 0;
                UpdateTime = SaveTime;
                GameManager.Event.Fire(this, OnRobotsScoreUpdateEventArgs.Create(RobotType.BalloonRiseRobot));
            }
            BalloonRiseModel.Instance.SaveToLocal();
        }
    }

    public void CheckTimeAfterLoad()
    {
        BalloonRiseActivityManager balloonRiseManager = GameManager.Task.BalloonRiseManager;
        DateTime nowTime = DateTime.Now < balloonRiseManager.StageEndTime ? DateTime.Now : balloonRiseManager.StageEndTime;
        int lastScore = Score;
        while (new TimeSpan(nowTime.Ticks - SaveTime.Ticks).TotalSeconds >= m_AddTriggerTime
               && Score < balloonRiseManager.StageTarget)
        {
            RealScore += CalculateScore(m_AddTriggerTime);
            SaveTime = SaveTime.AddSeconds(m_AddTriggerTime);
            m_AddTimeCounter = 0;
            m_AddTriggerTime = Random.Range(60f, 360f);
        }
        if (Score != lastScore)
        {
            UpdateTime = SaveTime;
            GameManager.Event.Fire(this, OnRobotsScoreUpdateEventArgs.Create(RobotType.BalloonRiseRobot));
            //Debug.LogError($"Robot {Name}'s LastScore is {lastScore}, current Score is {Score}.");
            
            if (Score >= GameManager.Task.BalloonRiseManager.StageTarget)
                GameManager.Task.BalloonRiseManager.CheckIsFinishStage = true;
        }
        BalloonRiseModel.Instance.SaveToLocal();
    }
}
