using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeStageRewardScrollColumn : ScrollColumn
    {
        private string columnName;
        private DRMergeStageReward data;
        private Transform parent;
        private bool isSpawning;

        public MergeStageRewardScrollColumn(string columnName, DRMergeStageReward data,Transform parent, ScrollArea area, float height)
        {
            this.columnName = columnName;
            this.data = data;
            this.parent = parent;
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
                    GameManager.ObjectPool.SpawnByImmediately<ColumnObject>(columnName, "MergeStageRewardPool", Vector3.zero, Quaternion.identity, parent, obj =>
                    {
                        GameObject target = (GameObject)obj.Target;
                        if (!isSpawning)
                        {
                            GameManager.ObjectPool.Unspawn<ColumnObject>("MergeStageRewardPool", target);
                            return;
                        }

                        instance = target;

                        callback?.Invoke(true);
                        instance.GetComponent<MergeStageRewardColumn>().Initialize(data);
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
                GameManager.ObjectPool.Unspawn<ColumnObject>("MergeStageRewardPool", instance);
                instance = null;
            }
        }

        public override void Release()
        {
            isSpawning = false;

            if (instance != null)
            {
                instance.GetComponent<MergeStageRewardColumn>().Release();

                GameManager.ObjectPool.Unspawn<ColumnObject>("MergeStageRewardPool", instance);
                instance = null;
            }
        }

        public override float Refresh()
        {
            if (instance != null)
            {
                instance.GetComponent<MergeStageRewardColumn>().Refresh();
            }

            return 0;
        }

        public override bool CheckSpawnComplete()
        {
            return !isSpawning;
        }
    }
}
