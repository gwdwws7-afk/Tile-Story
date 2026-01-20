using System;
using UnityEngine;

public class HarvestKitchenRewardScrollColumn : ScrollColumn
{
    private string columnName;
    private DTHarvestKitchenTaskDatas data;
    private HarvestKitchenRewardListMenu menu;
    private bool isSpawning;

    public HarvestKitchenRewardScrollColumn(string columnName, DTHarvestKitchenTaskDatas data, HarvestKitchenRewardListMenu menu, float height)
    {
        this.columnName = columnName;
        this.data = data;
        this.height = height;
        this.menu = menu;
        scrollArea = menu.scrollArea;
    }

    public override float Refresh()
    {
        return 0;
    }

    public override void Release()
    {
        Unspawn();
    }

    public override void Spawn(Action<bool> callback)
    {
        if (instance == null)
        {
            if (!isSpawning)
            {
                isSpawning = true;
                GameManager.ObjectPool.Spawn<ColumnObject>(columnName, "HarvestKitchenRewardColumnPool", Vector3.zero, Quaternion.identity, scrollArea.content, obj =>
                {
                    GameObject target = obj.Target as GameObject;
                    if (!isSpawning)
                    {
                        GameManager.ObjectPool.Unspawn<ColumnObject>("HarvestKitchenRewardColumnPool", target);
                        return;
                    }
                    instance = target;
                    callback?.Invoke(true);
                    instance.GetComponent<HarvestKitchenRewardColumn>().OnInit(data, menu);
                    isSpawning = false;
                });
            }
            else
            {
                callback?.Invoke(false);
            }
        }
    }

    public override void Unspawn()
    {
        isSpawning = false;
        if (instance != null)
        {
            instance.GetComponent<HarvestKitchenRewardColumn>().OnRelease();
            GameManager.ObjectPool.Unspawn<ColumnObject>("HarvestKitchenRewardColumnPool", instance);
            instance = null;
        }
    }

    public override bool CheckSpawnComplete()
    {
        return !isSpawning;
    }
}
