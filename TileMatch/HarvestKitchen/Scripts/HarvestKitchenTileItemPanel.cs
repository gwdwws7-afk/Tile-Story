using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class HarvestKitchenTileItemPanel : MonoBehaviour
{
    public int itemsCount = 0;
    // 当前行的前一行，不存在则为空
    public HarvestKitchenTileItemPanel upTileItemPanel;
    // 棋子的预设体
    public GameObject KitchenTileItemPrefab;
    public Transform[] positions;
    [SerializeField] private HarvestKitchenTileItem[] items = new HarvestKitchenTileItem[7];
    private Action<HarvestKitchenTileItemPanel> removeAllItemAction;
    
    public void Init(HarvestKitchenTileItemPanel upTileItemPanel, float coinProbabilty, List<int> tileTypeList,
        List<int> tileTypeProbabilityList, List<int> neededFoodsId, Action<HarvestKitchenTileItemPanel> recoveryAction,
        Action<HarvestKitchenTileItem> itemClickAction, ref List<HarvestKitchenTileItem> tileItemPool)
    {
        int sum = tileTypeProbabilityList.Sum();
        List<int> itemIDList = new List<int>();
        for (int i = 0; i < positions.Length; i++)
        {
            int itemID = 43;
            float rangeId = Random.Range(0, 100f);
            if (rangeId >= coinProbabilty)
            {
                int num = Random.Range(0, sum) + 1;
                for (int j = 0; j < tileTypeProbabilityList.Count; j++)
                {
                    num -= tileTypeProbabilityList[j];
                    if (num <= 0 || j == tileTypeProbabilityList.Count - 1)
                    {
                        itemID = tileTypeList[j];
                        break;
                    }
                }
            }

            itemIDList.Add(itemID);
        }

        //每行保底出现一个当前客户需要的食物
        if (neededFoodsId != null && neededFoodsId.Count > 0)
        {
            List<int> missingId = new List<int>();
            foreach (var neededId in neededFoodsId)
            {
                if (!itemIDList.Contains(neededId))
                {
                    missingId.Add(neededId);
                }
            }

            int index = 0;
            foreach (var id in missingId)
            {
                for (; index < itemIDList.Count; index++)
                {
                    if (!neededFoodsId.Contains(itemIDList[index]))
                    {
                        itemIDList[index] = id;
                        break;
                    }
                }
            }

            itemIDList = itemIDList.Shuffle<int>();
        }

        for (int i = 0; i < positions.Length; i++)
        {
            HarvestKitchenTileItem item = null;
            if (items[i] == null)
            {
                if (tileItemPool.Count == 0) 
                {
                    item = Instantiate(KitchenTileItemPrefab, positions[i].transform).GetComponent<HarvestKitchenTileItem>();
                }
                else
                {
                    item = tileItemPool[0];
                    tileItemPool.RemoveAt(0);
                }
                items[i] = item;
            }
            else
            {
                item = items[i];
            }

            item.Init(i, itemIDList[i], positions[i].transform, RemoveItem, itemClickAction);
        }

        itemsCount = items.Length;

        this.upTileItemPanel = upTileItemPanel;
        removeAllItemAction = recoveryAction;
        
        // Log.Error("Kitchen:检测棋子位置是否正确");
        // if(upTileItemPanel != null)
        //     upTileItemPanel.CheckItemPos();
        // CheckItemPos();
    }

    public void RemoveItem(int index)
    {
        if (upTileItemPanel)
        {
            HarvestKitchenTileItem item = upTileItemPanel.GetTileItemByIndex(index, positions[index]);
            if (item && item.gameObject.activeSelf)
            {
                items[index] = item; 
                item.RefreshClickAction1(RemoveItem);
                item.transform.DOKill();
                float duration = item.transform.localPosition.y / 134f * 0.25f;
                // isFall[index] = true;
                item.transform.DOLocalMoveY(0f, duration).SetEase(Ease.InCubic);
                //     .onComplete += () => isFall[index] = false;
                return;
            }
        }
        itemsCount--;
        items[index] = null;
        if (itemsCount <= 0)
        {
            gameObject.SetActive(false);
            removeAllItemAction?.Invoke(this);
        }
    }

    public HarvestKitchenTileItem GetTileItemByIndex(int index, Transform parent)
    {
        if (itemsCount > 0 && items[index] != null)
        {
            HarvestKitchenTileItem temp = items[index];
            temp.transform.SetParent(parent);
            items[index] = null;
            RemoveItem(index);
            return temp;
        }

        return null;
    }

    public void OnRelease()
    {
        upTileItemPanel = null;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i])
            {
                items[i].gameObject.SetActive(false);
                Destroy(items[i].gameObject);
            }
        }

        if(gameObject)
            Destroy(gameObject);
    }

    // public void CheckItemPos()
    // {
    //     for (int i = 0; i < items.Length; i++)
    //     {
    //         if(items[i] == null) continue;
    //         if (items[i].transform.localPosition != Vector3.zero && !isFall[i])
    //         {
    //             Debug.LogError($"Kitchen:{transform.name}行的棋子{i}位置错误{items[i].transform.localPosition}");
    //             items[i].transform.localPosition = Vector3.zero;
    //         }
    //     }
    // }
}
