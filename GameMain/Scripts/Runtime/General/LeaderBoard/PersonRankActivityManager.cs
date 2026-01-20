using MySelf.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonRankActivityManager : TaskManagerBase
{
    public RankType RankType => RankType.PersonRank;
    public int RankTerm => PersonRankModel.Instance.RankTerm;
    public string SerialNum => PersonRankModel.Instance.SerialNum;
    public int Score => PersonRankModel.Instance.Score;

    public int ScoreInGame
    {
        get => PersonRankModel.Instance.ScoreInGame;
        set => PersonRankModel.Instance.ScoreInGame = value;
    }

    public int LastScore => PersonRankModel.Instance.LastScore;
    public PersonRankLevel RankLevel => PersonRankModel.Instance.RankLevel;
    public List<PersonRankData> RankDatas => PersonRankModel.Instance.RankDatas;
    public PersonRankData LocalData => PersonRankModel.Instance.LocalData;
    public int ContinuousWinTime => PersonRankModel.Instance.ContinuousWinTime;
    public int LastContinuousWinTime => PersonRankModel.Instance.LastContinuousWinTime;

    public float LastUpdateTime
    {
        get => PersonRankModel.Instance.LastUpdateTime;
        set => PersonRankModel.Instance.LastUpdateTime = value;
    }

    public int Rank => PersonRankModel.Instance.Rank;
    public int LastRank
    {
        get => PersonRankModel.Instance.LastRank;
        set => PersonRankModel.Instance.LastRank = value;
    }

    public int LastUpRank
    {
        get => PersonRankModel.Instance.LastUpRank;
        set => PersonRankModel.Instance.LastUpRank = value;
    }

    public bool NeedToFlyMedal
    {
        get => PersonRankModel.Instance.NeedToFlyMedal;
        set => PersonRankModel.Instance.NeedToFlyMedal = value;
    }


    public bool HasShownWelcome
    {
        get => PersonRankModel.Instance.HasShownWelcome;
        set => PersonRankModel.Instance.HasShownWelcome = value;
    }

    public bool HasShownPersonRankGuide
    {
        get => PersonRankModel.Instance.HasShownPersonRankGuide;
        set => PersonRankModel.Instance.HasShownPersonRankGuide = value;
    }

    public bool HasShownTodayWelcome
    {
        get => PlayerBehaviorModel.Instance.HasShownTodayWelcomeMenu();
        set => PlayerBehaviorModel.Instance.RecordHasShownTodayWelcomeMenu(value);
    }

    public bool HasShownPersonRankEntranceBtn
    {
        get => PlayerBehaviorModel.Instance.HasShownPersonRankEntranceBtn();
        set => PlayerBehaviorModel.Instance.RecordHasShownPersonRankEntranceBtn(value);
    }


    public PersonRankState TaskState
    {
        get => PersonRankModel.Instance.TaskState;
        private set => PersonRankModel.Instance.TaskState = value;
    }

    public DateTime EndTime => PersonRankModel.Instance.EndTime;
    public DateTime StartTime => PersonRankModel.Instance.StartTime;

    public DTPersonRankTaskData TaskData
    {
        get;
        private set;
    }

    public int UpRange
    {
        get
        {
            if (TaskData == null)
                return 0;
            return TaskData.GetPersonRankTaskDataByLevel(RankLevel).UpRange;
        }
    }

    public bool CheckIsOpen()
    {
        CheckActivityTime();
        var flag1 = GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockPersonRankGameLevel;
        var flag2 = TaskState != PersonRankState.End;
        return flag1 && flag2 && HasShownWelcome;
    }

    private void CheckActivityTime()
    {
        if (TaskState == PersonRankState.SendingReward)
            SetTaskState(PersonRankState.Finished);
        if (TaskState == PersonRankState.End && StartTime < DateTime.Now)
            SetTaskState(PersonRankState.None);
        if (TaskState == PersonRankState.None && StartTime > DateTime.Now)
            SetTaskState(PersonRankState.End);
        if (TaskState == PersonRankState.Playing && EndTime <= DateTime.Now)
            SetTaskState(PersonRankState.Finished);

    }

    private int _pullCount = 0;
    public void GetPersonRankDataFromServer(Action<bool> callback = null, bool force = false)
    {
        if (_pullCount < 6 && !force)
        {
            _pullCount++;
            callback?.Invoke(false);
            return;
        }
        _pullCount = 0;
        PersonRankModel.Instance.GetPersonRankDataFromServer(flag => { callback?.Invoke(flag); });
    }


    public void SetTaskState(PersonRankState state)
    {
        if (state == PersonRankState.Finished && TaskState == PersonRankState.None)
        {
            PersonRankModel.Instance.RestartGame();
            return;
        }
        TaskState = state;
    }

    public override void OnInit()
    {
        TaskData = GameManager.DataTable.GetDataTable<DTPersonRankTaskData>().Data;
        PersonRankModel.Instance.Init();
    }

    public override void OnReset()
    {
    }

    public override void ConfirmTargetCollection(TaskTargetCollection collection)
    {
        if (!CheckIsOpen() || TaskState == PersonRankState.Finished)
        {
            return;
        }

        var targetNum = collection.GetTargetCollectNum(GetTaskTarget());
        if (targetNum <= 0 || GameManager.Network.CheckInternetIsNotReachable()) return;
        PersonRankModel.Instance.GameWin();
    }

    public override TaskTarget GetTaskTarget()
    {
        return TaskTarget.LevelPass;
    }

    public void SendReward(Action callback)
    {
        PersonRankModel.Instance.SendRewards(callback);
    }

    public bool WillLevelUp()
    {
        if (RankLevel == PersonRankLevel.Supreme)
        {
            return false;
        }
        var upRank = TaskData.GetPersonRankTaskDataByLevel(RankLevel).UpRange;
        return Rank <= upRank;
    }

    public int GetMultipleNum(int winTime)
    {
        return PersonRankModel.Instance.GetMultipleNum(winTime);
    }
}


public class PersonRankData : BaseRankModelData
{
    public int Rank;

    public override bool IsSelf()
    {
        return SerialNum == PersonRankModel.Instance.SerialNum;
    }

    public PersonRankData()
    {
    }

    public PersonRankData(Dictionary<string, object> dic)
    {
        HeadId = dic.TryGetValue("HeadId", out var value) ? (int)(long)value : 0;
        Name = dic.TryGetValue("Name", out value) ? (string)value : "";
        Score = dic.TryGetValue("Score", out value) ? (int)(long)value : 0;
        SerialNum = dic.TryGetValue("SerialNum", out value) ? (string)value : "";
        IsNoShowInGroup = dic.TryGetValue("IsNoShowInGroup", out value) ? Convert.ToBoolean(value) : false;
    }
}

public enum PersonRankLevel
{
    Bronze,
    Silver,
    Gold,
    Supreme,
}

public enum PersonRankState
{
    None,
    Playing,
    Finished,
    SendingReward,
    End,
}