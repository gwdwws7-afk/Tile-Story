using System;
using UnityEngine;

public class GoldCollectionScrollColumn : ScrollColumn
{
    private string columnName;
    private GoldCollectionStage data;
    private GoldCollectionMenu goldCollectionMenu;
    private bool isSpawning;

    public GoldCollectionScrollColumn(string columnName, GoldCollectionStage data, GoldCollectionMenu goldCollectionMenu, float height)
    {
        this.columnName = columnName;
        this.data = data;
        this.goldCollectionMenu = goldCollectionMenu;
        this.height = height;
        scrollArea = goldCollectionMenu.scrollArea;
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
                GameManager.ObjectPool.Spawn<ColumnObject>(columnName, "GoldCollectionColumnPool", Vector3.zero, Quaternion.identity, scrollArea.content, obj =>
                {
                    GameObject target = (GameObject)obj.Target;
                    if (!isSpawning)
                    {
                        GameManager.ObjectPool.Unspawn<ColumnObject>("GoldCollectionColumnPool", target);
                        return;
                    }
                    instance = target;
                    callback?.Invoke(true);
                    instance.GetComponent<GoldCollectionColumn>().OnInit(data, goldCollectionMenu);
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
            instance.GetComponent<GoldCollectionColumn>().OnRelease();
            GameManager.ObjectPool.Unspawn<ColumnObject>("GoldCollectionColumnPool", instance);
            instance = null;
        }
    }

    public override bool CheckSpawnComplete()
    {
        return !isSpawning;
    }
}
