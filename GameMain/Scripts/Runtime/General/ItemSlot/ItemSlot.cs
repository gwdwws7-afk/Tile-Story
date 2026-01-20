using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 放置槽
/// </summary>
public class ItemSlot : MonoBehaviour
{
    protected TotalItemData[] itemTypeArray;
    protected int[] itemNumArray;
    private List<AsyncOperationHandle> asyncHandleList;
    private bool isInit;

    public Action OnInitCompleteAction;

    /// <summary>
    /// 放置槽内物品种类数量
    /// </summary>
    public int ItemCount
    {
        get
        {
            if (itemTypeArray != null)
            {
                return itemTypeArray.Length;
            }
            return 0;
        }
    }

    /// <summary>
    /// 放置槽内单个物品种类
    /// </summary>
    public TotalItemData ItemType {
        get
        {
            if (itemTypeArray != null && itemTypeArray.Length > 0)
            {
                if (itemTypeArray.Length > 1)
                {
                    Log.Warning("Item slot has multiple items {0}", itemTypeArray.Length);
                }
                return itemTypeArray[0];
            }
            return TotalItemData.None;
        }
    }

    /// <summary>
    /// 放置槽内单个物品数量
    /// </summary>
    public int ItemNum {
        get
        {
            if (itemNumArray != null && itemNumArray.Length > 0)
            {
                return itemNumArray[0];
            }
            return 0;
        }
    }

    /// <summary>
    /// 放置槽内所有物品种类
    /// </summary>
    public TotalItemData[] ItemTypeArray { get => itemTypeArray; }

    /// <summary>
    /// 放置槽内所有物品数量
    /// </summary>
    public int[] ItemNumArray { get => itemNumArray; }

    public bool IsInit { get => isInit; }

    public virtual void OnInit(TotalItemData type, int num)
    {
        itemTypeArray = new TotalItemData[1] { type };
        itemNumArray = new int[1] { num };

        isInit = true;
    }

    public virtual void OnInit(TotalItemData[] types, int[] nums)
    {
        itemTypeArray = types;
        itemNumArray = nums;
        isInit = true;
    }

    public virtual void OnReset()
    {
        itemTypeArray = null;
        itemNumArray = null;
        OnInitCompleteAction = null;
        isInit = false;
    }

    public virtual void OnRelease()
    {
        OnReset();
        ClearAsyncHandleList();
    }

    public virtual void OnShow()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnHide()
    {
        gameObject.SetActive(false);
    }

    public void AddAsyncHandle(AsyncOperationHandle handle)
    {
        if (asyncHandleList == null)
        {
            asyncHandleList = new List<AsyncOperationHandle>();
        }

        asyncHandleList.Add(handle);
    }

    public void ClearAsyncHandleList()
    {
        if (asyncHandleList != null)
        {
            for (int i = 0; i < asyncHandleList.Count; i++)
            {
                UnityUtility.UnloadAssetAsync(asyncHandleList[i]);
            }

            asyncHandleList.Clear();
        }
    }

    public virtual bool CheckInitComplete()
    {
        if (asyncHandleList == null)
        {
            return true;
        }

        for (int i = 0; i < asyncHandleList.Count; i++)
        {
            if (asyncHandleList[i].IsValid() && !asyncHandleList[i].IsDone) 
            {
                return false;
            }
        }

        return true;
    }
}
