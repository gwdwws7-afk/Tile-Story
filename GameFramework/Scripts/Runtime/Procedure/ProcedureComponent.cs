using GameFramework.Fsm;
using GameFramework.Procedure;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 流程组件
/// </summary>
public sealed class ProcedureComponent : GameFrameworkComponent
{
    private IProcedureManager procedureManager = null;
    private ProcedureBase entranceProcedure = null;

    /// <summary>
    /// 获取当前流程
    /// </summary>
    public ProcedureBase CurrentProcedure
    {
        get
        {
            return procedureManager.CurrentProcedure;
        }
    }

    /// <summary>
    /// 获取当前流程持续时间
    /// </summary>
    public float CurrentProcedureTime
    {
        get
        {
            return procedureManager.CurrentProcedureTime;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        procedureManager = GameFrameworkEntry.GetModule<ProcedureManager>();
        if (procedureManager == null)
        {
            Log.Fatal("Procedure manager is invalid");
            return;
        }
    }

    private IEnumerator Start()
    {
        RunAndroidJavaProxy.StartRunAndroidJavaProxy();
        
        ProcedureBase[] procedures = new ProcedureBase[]
        {
            new ProcedureLaunch(),
            new ProcedureMenu(),
            new ProcedureResourcesPreload(),
            new ProcedureMapPreload(),
            new ProcedureMap(),
            new ProcedureExecuteProcess(),
            new ProcedureGame(),
        };

        entranceProcedure = procedures[0];

        procedureManager.Initialize(GameFrameworkEntry.GetModule<FsmManager>(), procedures);

        yield return new WaitForEndOfFrame();

        procedureManager.StartProcedure(entranceProcedure.GetType());
    }

    /// <summary>
    /// 是否存在流程。
    /// </summary>
    /// <typeparam name="T">要检查的流程类型。</typeparam>
    /// <returns>是否存在流程。</returns>
    public bool HasProcedure<T>() where T : ProcedureBase
    {
        return procedureManager.HasProcedure<T>();
    }

    /// <summary>
    /// 是否存在流程。
    /// </summary>
    /// <param name="procedureType">要检查的流程类型。</param>
    /// <returns>是否存在流程。</returns>
    public bool HasProcedure(Type procedureType)
    {
        return procedureManager.HasProcedure(procedureType);
    }

    /// <summary>
    /// 获取流程。
    /// </summary>
    /// <typeparam name="T">要获取的流程类型。</typeparam>
    /// <returns>要获取的流程。</returns>
    public ProcedureBase GetProcedure<T>() where T : ProcedureBase
    {
        return procedureManager.GetProcedure<T>();
    }

    /// <summary>
    /// 获取流程。
    /// </summary>
    /// <param name="procedureType">要获取的流程类型。</param>
    /// <returns>要获取的流程。</returns>
    public ProcedureBase GetProcedure(Type procedureType)
    {
        return procedureManager.GetProcedure(procedureType);
    }

    public void ChangeProcedureBase(Type procedureType)
    {
        
    }
}
