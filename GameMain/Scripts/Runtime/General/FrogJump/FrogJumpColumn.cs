using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FrogJumpColumn : ScrollColumn
{
    private string columnName;
    private ScrollArea scrollArea;
    private bool isSpawning;
    private int index;
    
    public FrogJumpColumn(string columnName,ScrollArea scrollArea, int index, float height)
    {
        this.columnName = columnName;
        this.scrollArea = scrollArea;
        this.height = height;
        this.index = index;
    }
    public override void Spawn(Action<bool> callback)
    {
        if(isSpawning) return;
        if (instance == null)
        {
            isSpawning = true;
            GameManager.ObjectPool.SpawnByImmediately<ColumnObject>(columnName, "FrogJumpColumnPool", Vector3.zero, Quaternion.identity, scrollArea.content, obj =>
            {
                GameObject target = (GameObject)obj.Target;
                if (!isSpawning)
                {
                    GameManager.ObjectPool.Unspawn<ColumnObject>("FrogJumpColumnPool", target);
                    return;
                }

                instance = target;
                instance.GetComponent<FrogJumpScrollPanel>().Init(index);

                callback?.Invoke(true);
                isSpawning = false;
            });
        }
        else
        {
            callback?.Invoke(false);
        }
    }

    public override void Unspawn()
    {
        isSpawning = false;
        if (instance != null)
        {
            instance.transform.DOKill();
            instance.GetComponent<FrogJumpScrollPanel>().OnReset();
            GameManager.ObjectPool.Unspawn<ColumnObject>("FrogJumpColumnPool", instance);
            instance = null;
        }
    }

    public override void Release()
    {
        isSpawning = false;
        if (instance != null)
        {
            instance.transform.DOKill();
            instance.GetComponent<FrogJumpScrollPanel>().OnReset();
            GameManager.ObjectPool.Unspawn<ColumnObject>("FrogJumpColumnPool", instance);
            instance = null;
        }
    }

    public override float Refresh()
    {
        return 0;
    }
}
