using System;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using MySelf.Model;

/// <summary>
/// 地图场景开始流程
/// </summary>
public sealed class ProcedureGame : ProcedureBase
{
    public override string ProcedureName => "ProcedureGame";

    private bool isToMap = false;
    private bool showTileMatchPanel = false;
    private bool StartUnloadingUnusedAssets = false;
    private int waitFrame = 5;
    private bool isGameToGame = false;
    public override void OnEnter(IFsm<ProcedureManager> fsm)
    {
        PkGameModel.Instance.RecordEnterGameStatus = PkGameModel.Instance.PkGameStatus;
        
        GameManager.Scene.SetSceneType(SceneType.Game);
        EntranceFlyObjectManager.Instance.Clear();
        RewardManager.Instance.OnRelease();
        GameManager.Process.UnregisterAll();
        GameManager.Task.ClearAllDelayTriggerTasks();

        GameManager.DataNode.SetData("NowLevel", GameManager.PlayerData.NowLevel);
        
        GameManager.Event.Subscribe(CommonEventArgs.EventId,GoToMap);

        waitFrame = 5;
        showTileMatchPanel = false;
        StartUnloadingUnusedAssets = false;
        GameManager.UI.AutoReleaseInterval = 0;
        ShowPkGameAnim((u1) =>
        {
            GameManager.UI.HideAllUIForm(new string[] { "PkReadyGoPanel" , "MapMainBGPanel", "TileMatchPanel" });
        },() =>
        {
            GameManager.UI.ShowUIForm("MapMainBGPanel",UIFormType.BgUI,(u2) =>
            {
                showTileMatchPanel = true;
                GameManager.UI.HideAllUIForm(new string[] { "MapMainBGPanel", "TileMatchPanel" });
            });
        });
        
        GameManager.PlayerData.WinLastGame = false;
        
        //收集关卡相关未完成的任务Id
       var noCompleteObjectiveIds= GameManager.Objective.GetObjectiveStatusIdByLevel(false);
       GameManager.DataNode.SetData("NoCompleteObjectiveIds",noCompleteObjectiveIds);
        
        base.OnEnter(fsm);
    }

    public override void OnUpate(IFsm<ProcedureManager> fsm, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpate(fsm, elapseSeconds, realElapseSeconds);

        if (showTileMatchPanel)
        {
            if(waitFrame>0)
            {
                waitFrame--;
                return;
            }

            showTileMatchPanel = false;
            StartUnloadingUnusedAssets = true;
            waitFrame = SystemInfoManager.CheckIsSpecialDeviceOptimizeHeavyWork() ? 12 : 3;
            GameManager.UI.AutoReleaseInterval = 5;
            GameManager.UI.StartUnloadingUnusedAssets();
        }

        if (StartUnloadingUnusedAssets && !GameManager.UI.IsUnloadingUnusedAssets) 
        {
            waitFrame--;
            if (waitFrame > 0)
            {
                return;
            }
            StartUnloadingUnusedAssets = false;
            GameManager.UI.ShowUIForm("TileMatchPanel",UIFormType.CenterUI);
            return;
        }

        if (isToMap)
        {
            fsm.SetData("ProcedureSkipType", ProcedureSkipType.GameToMap);
            ChangeState<ProcedureMapPreload>(fsm);
        }

        if (isGameToGame)
        {
            isGameToGame = false;
            ChangeState<ProcedureGame>(fsm);
        }
    }

    public override void OnLeave(IFsm<ProcedureManager> fsm, bool isShutdown)
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId,GoToMap);

        //在回到主界面时清除tile图集资源
        TileMatchUtil.ClearAllTileSprite();

        isToMap = false;
        isGameToGame = false;
        showTileMatchPanel = false;
        base.OnLeave(fsm, isShutdown);
    }

    private void GoToMap(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        if (ne.Type == CommonEventType.GameToMap)
        {
            isToMap = true;
        }else if (ne.Type == CommonEventType.GameToGame)
        {
            isGameToGame = true;
        }
    }
    
    public void ShowPkGameAnim(Action<UIForm> startAction,Action finishAction)
    {
        //处于活动中
        if (!GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge
            &&PkGameModel.Instance.RecordEnterGameStatus==PkGameStatus.Playing)
        {
            GameManager.UI.ShowUIForm("PkReadyGoPanel",UIFormType.PopupUI, (u) =>
            {
                startAction?.InvokeSafely(u);
                u.SetHideAction(() =>
                {
                    finishAction?.InvokeSafely();
                });
            });
        }
        else
        {
            finishAction?.InvokeSafely();
        }
    }
}
