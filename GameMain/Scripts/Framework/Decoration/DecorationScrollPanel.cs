using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationScrollPanel : ScrollColumn
{
    private string columnName;
    private int targetId;
    private bool isSpawning;


    public DecorationScrollPanel(string columnName, int targetId,ScrollArea area, float height)
    {
        this.columnName = columnName;
        this.targetId = targetId;
        this.scrollArea = area;
        this.height = height;
    }

    public override void Spawn(Action<bool> callback)
    {
        if (instance == null)
        {
            if (!isSpawning)
            {
                isSpawning = true;
                GameManager.ObjectPool.Spawn<ColumnObject>(columnName, "DecorationPanelPool", Vector3.zero, Quaternion.identity, scrollArea.content, obj =>
                {
                    GameObject target = (GameObject)obj.Target;
                    if (!isSpawning)
                    {
                        GameManager.ObjectPool.Unspawn<ColumnObject>("DecorationPanelPool", target);
                        return;
                    }

                    instance = target;

                    callback?.Invoke(true);
                    instance.GetComponent<DecorationPanel>().InitializePanel(targetId);
                    isSpawning = false;
                });
            }
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
            //instance.GetComponent<DecorationPanel>().OnRelease();

            GameManager.ObjectPool.Unspawn<ColumnObject>("DecorationPanelPool", instance);
            instance = null;
        }
    }

    public override void Release()
    {
        isSpawning = false;

        if (instance != null)
        {
            instance.GetComponent<DecorationPanel>().OnRelease();

            GameManager.ObjectPool.Unspawn<ColumnObject>("DecorationPanelPool", instance);
            instance = null;
        }
    }

    public override float Refresh()
    {
        if (instance != null)
        {
            instance.GetComponent<DecorationPanel>().Refresh();
        }

        return 0;
    }

    public override bool CheckSpawnComplete()
    {
        return !isSpawning;
    }

}
