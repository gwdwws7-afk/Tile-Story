using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

/// <summary>
/// 地图场景开始流程
/// </summary>
public sealed class ProcedureMap : ProcedureBase
{
    public override string ProcedureName => "ProcedureMap";

    private bool isToGame = false;
    
    public override void OnEnter(IFsm<ProcedureManager> fsm)
    {
        base.OnEnter(fsm);

        //ATT授权
#if UNITY_IOS && !UNITY_EDITOR
        if (ATTAuth.CanGetTrackingAuthorization)
        {
            Debug.Log($"Current ATTStatus is: {ATTAuth.GetAppTrackingAuthorizationStatus()}");
            ATTAuth.RequestTrackingAuthorizationWithCompletionHandler();
        }
#endif
        
        MapTopPanelManager topPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
        if (topPanel != null) topPanel.OnResume();

        GameManager.Event.Subscribe(CommonEventArgs.EventId, GoToGame);

        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);

        if (GameManager.DataNode.GetData("IsNewPlayer", false)) 
        {
            if (GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Use_Tile_Furniture, false))
                MySelf.Model.BGModel.Instance.SetTileIconID(5);
            GameManager.DataNode.SetData("IsNewPlayer", false);
        }
    }

    public override void OnLeave(IFsm<ProcedureManager> fsm, bool isShutdown)
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, GoToGame);

        isToGame = false;

        base.OnLeave(fsm, isShutdown);
    }

    public override void OnUpate(IFsm<ProcedureManager> fsm, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpate(fsm, elapseSeconds, realElapseSeconds);

        if (isToGame)
        {
            ChangeState<ProcedureGame>(fsm);
            return;
        }

        ProcedureSkipType type = fsm.GetData<ProcedureSkipType>("ProcedureSkipType");
        if (type != ProcedureSkipType.None)
        {
            ChangeState<ProcedureExecuteProcess>(fsm);
        }
    }

    private void GoToGame(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;

        if (ne.Type == CommonEventType.MapToGame)
        {
            isToGame = true;
        }
    }
}
