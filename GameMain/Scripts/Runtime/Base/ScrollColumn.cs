using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 滚动栏基类
/// </summary>
public abstract class ScrollColumn
{
    protected string id;
    protected ScrollArea scrollArea;
    protected GameObject instance;
    protected float height;
    public int index;

    public string ID { get => id; }
    public virtual string ColumnName { get; }
    public GameObject Instance { get => instance;}
    public float Height { get => height; }

    public abstract void Spawn(Action<bool> callback);

    public abstract void Unspawn();

    public abstract void Release();

    public abstract float Refresh();

    public virtual bool CheckSpawnComplete()
    {
        return true;
    }

    public virtual void Unlock()
    {

    }

    public virtual void RefreshRewardStatus()
    {

    }

    public virtual void ClaimAll()
    {

    }
}
