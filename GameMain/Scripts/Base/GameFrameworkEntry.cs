using GameFramework;
using System;
using System.Collections.Generic;

/// <summary>
/// 游戏框架入口。
/// </summary>
public static class GameFrameworkEntry
{
    private static readonly GameFrameworkLinkedList<GameFrameworkModule> gameFrameworkModules = new GameFrameworkLinkedList<GameFrameworkModule>();

    /// <summary>
    /// 所有游戏框架模块轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    public static void Update(float elapseSeconds, float realElapseSeconds)
    {
        foreach (var module in gameFrameworkModules)
        {
            module.Update(elapseSeconds, realElapseSeconds);
        }
    }

    /// <summary>
    /// 关闭并清理所有游戏框架模块。
    /// </summary>
    public static void Shutdown()
    {
        for (LinkedListNode<GameFrameworkModule> current = gameFrameworkModules.Last; current != null; current = current.Previous)
        {
            current.Value.Shutdown();
        }

        gameFrameworkModules.Clear();
        ReferencePool.ClearAll();
    }

    /// <summary>
    /// 获取游戏框架模块。
    /// </summary>
    /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
    /// <returns>要获取的游戏框架模块。</returns>
    /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
    public static T GetModule<T>() where T : class
    {
        return GetModule(typeof(T)) as T;
    }

    /// <summary>
    /// 获取游戏框架模块。
    /// </summary>
    /// <param name="moduleType">要获取的游戏框架模块类型。</param>
    /// <returns>要获取的游戏框架模块。</returns>
    /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
    public static GameFrameworkModule GetModule(Type moduleType)
    {
        foreach (var module in gameFrameworkModules)
        {
            if (module.GetType() == moduleType)
            {
                return module;
            }
        }
        return CreateModule(moduleType);
    }

    /// <summary>
    /// 创建游戏框架模块。
    /// </summary>
    /// <param name="moduleType">要创建的游戏框架模块类型。</param>
    /// <returns>要创建的游戏框架模块。</returns>
    public static GameFrameworkModule CreateModule(Type moduleType)
    {
        GameFrameworkModule module = (GameFrameworkModule)Activator.CreateInstance(moduleType);
        if (module == null)
        {
            throw new Exception(string.Format("Can not create module '{0}'.", moduleType.FullName));
        }

        LinkedListNode<GameFrameworkModule> current = gameFrameworkModules.First;
        while (current != null)
        {
            if (module.Priority > current.Value.Priority)
            {
                break;
            }
            current = current.Next;
        }

        if (current != null)
        {
            gameFrameworkModules.AddBefore(current, module);
        }
        else
        {
            gameFrameworkModules.AddLast(module);
        }
        return module;
    }
}
