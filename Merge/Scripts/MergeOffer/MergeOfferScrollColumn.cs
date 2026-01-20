using System;
using UnityEngine;

namespace Merge
{
    public class MergeOfferScrollColumn : ScrollColumn
    {
        private string columnName;
        private DRMergeOffer data;
        private MergeOfferMenu menu;
        private bool isSpawning;

        public override string ColumnName => columnName;

        public MergeOfferScrollColumn(string columnName, DRMergeOffer data, float height, MergeOfferMenu menu)
        {
            this.columnName = columnName;
            this.data = data;
            isSpawning = false;
            instance = null;
            this.height = height;
            this.menu = menu;
            this.scrollArea = menu.m_ScrollArea;
        }

        public override void Spawn(Action<bool> callback)
        {
            if (isSpawning)
                return;

            if (instance == null)
            {
                isSpawning = true;
                GameManager.ObjectPool.Spawn<ColumnObject>(columnName, "MergeColumnPool", Vector3.zero, Quaternion.identity, scrollArea.content, obj =>
                {
                    GameObject target = (GameObject)obj.Target;
                    if (!isSpawning)
                    {
                        GameManager.ObjectPool.Unspawn<ColumnObject>("MergeColumnPool", target);
                        return;
                    }

                    instance = target;

                    instance.GetComponent<MergeOfferColumn>().Initialize(data, menu);

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
                instance.GetComponent<MergeOfferColumn>().Release();
                GameManager.ObjectPool.Unspawn<ColumnObject>("MergeColumnPool", instance);
                instance = null;
            }
        }

        public override void Release()
        {
            isSpawning = false;

            if (instance != null)
            {
                instance.GetComponent<MergeOfferColumn>().Release();
                GameManager.ObjectPool.Unspawn<ColumnObject>("MergeColumnPool", instance);
                instance = null;
            }
        }

        public override float Refresh()
        {
            if (instance != null)
            {
                instance.GetComponent<MergeOfferColumn>().Refresh(true);
            }

            return 0;
        }

        public override bool CheckSpawnComplete()
        {
            return !isSpawning;
        }
    }
}
