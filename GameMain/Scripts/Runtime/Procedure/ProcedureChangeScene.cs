

using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
/// <summary>
/// 切换场景的流程
/// </summary>
public sealed class ProcedureChangeScene : ProcedureBase
{
    private string nextSceneName;
    private bool isChangeSceneStart;
    private bool isChangeSceneComplete;

    public override string ProcedureName => "ProcedureChangeScene";

    public override void OnEnter(IFsm<ProcedureManager> fsm)
    {
        base.OnEnter(fsm);

        //停止所有声音
        GameManager.Sound.StopAllLoadingSounds();
        GameManager.Sound.StopAllLoadedSound();

        //注销所有进程
        GameManager.Process.OnReset();

		//关闭所有对象池
		GameManager.ObjectPool.DestroyAllObjectPool();
    }

    public override void OnUpate(IFsm<ProcedureManager> fsm, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpate(fsm, elapseSeconds, realElapseSeconds);

        if (!isChangeSceneStart)
        {
            isChangeSceneStart = true;

            nextSceneName = fsm.GetData<string>("NextSceneName");
            GameManager.Scene.LoadSceneAsync(nextSceneName, operation =>
            {
                isChangeSceneComplete = true;
            });
        }

        if (!isChangeSceneComplete)
        {
            return;
        }

        if (nextSceneName.Equals("map", StringComparison.Ordinal))
        {
            ChangeState<ProcedureMap>(fsm);
        }
    }

    public override void OnLeave(IFsm<ProcedureManager> fsm, bool isShutdown)
    {
        nextSceneName = null;
        isChangeSceneStart = false;
        isChangeSceneComplete = false;

        base.OnLeave(fsm, isShutdown);
    }
}
