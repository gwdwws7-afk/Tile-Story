

using GameFramework.Fsm;
using GameFramework.Procedure;
/// <summary>
/// 主菜单流程
/// </summary>
public sealed class ProcedureMenu : ProcedureBase
{
    private bool loadingShowComplete;
    private SplashLoadingMenuManager loadingMenuManager;
    private bool startShowSplash;

    public override string ProcedureName => "ProcedureMenu";

    public override void OnEnter(IFsm<ProcedureManager> fsm)
    {
        base.OnEnter(fsm);

        //播放背景音乐
        //GameManager.Sound.PlayMusic(GameManager.PlayerData.HappyBgMusicName);

        GameManager.Scene.SetSceneType(SceneType.Menu);
    }

    public override void OnUpate(IFsm<ProcedureManager> fsm, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpate(fsm, elapseSeconds, realElapseSeconds);

        if (!startShowSplash)
        {
            startShowSplash = true;

            //生成进度条
            GameManager.UI.ShowUIForm("SplashLoadingMenuManager",UIFormType.BgUI, obj =>
             {
                 loadingMenuManager = obj as SplashLoadingMenuManager;
                 loadingShowComplete = true;
             });
        }

        if (!loadingShowComplete || !loadingMenuManager.LogoShowAnimComplete) 
        {
            return;
        }

        ChangeState<ProcedureResourcesPreload>(fsm);
    }

    public override void OnLeave(IFsm<ProcedureManager> fsm, bool isShutdown)
    {
        loadingShowComplete = false;

        base.OnLeave(fsm, isShutdown);
    }
}
