using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemPromptBox : PromptBox
{
    public string itemSlotAddressableName = "PromptItemSlot";
    public float spaceX;
    public float spaceY;
    public float itemOffsetX;
    public float itemOffsetY;
    public bool balancedArrange;

    private List<ItemSlot> itemSlots = new List<ItemSlot>();
    private List<AsyncOperationHandle<GameObject>> asyncHandlesList = new List<AsyncOperationHandle<GameObject>>();
    private int rewardCount;
    private bool isRelease;

    public void Init(TotalItemData[] types, int[] nums)
    {
        isRelease = false;

        rewardCount = types.Length;

        InstantiateItemSlotAsync(types.Length, () =>
        {
            for (int i = 0; i < types.Length; i++)
            {
                itemSlots[i].OnInit(types[i], nums[i]);
                itemSlots[i].gameObject.SetActive(true);
            }

            for (int i = types.Length; i < itemSlots.Count; i++)
            {
                itemSlots[i].gameObject.SetActive(false);
            }
        });
    }

    public void Init(List<TotalItemData> types, List<int> nums)
    {
        isRelease = false;

        rewardCount = types.Count;

        InstantiateItemSlotAsync(types.Count, () =>
        {
            for (int i = 0; i < types.Count; i++)
            {
                itemSlots[i].OnInit(types[i], nums[i]);
                itemSlots[i].gameObject.SetActive(true);
            }

            for (int i = types.Count; i < itemSlots.Count; i++)
            {
                itemSlots[i].gameObject.SetActive(false);
            }
        });
    }

    public void Init(List<ItemData> datas)
    {
        isRelease = false;
        rewardCount = datas.Count;

        InstantiateItemSlotAsync(datas.Count, () =>
        {
            for (int i = 0; i < datas.Count; i++)
            {
                itemSlots[i].OnInit(datas[i].type, datas[i].num);
                itemSlots[i].gameObject.SetActive(true);
            }

            for (int i = datas.Count; i < itemSlots.Count; i++)
            {
                itemSlots[i].gameObject.SetActive(false);
            }
        });
    }

    private void InstantiateItemSlotAsync(int targetCount, Action callback)
    {
        if (isRelease)
        {
            return;
        }

        if (itemSlots.Count < targetCount)
        {
            AsyncOperationHandle<GameObject> asyncHandle = UnityUtility.InstantiateAsync(itemSlotAddressableName, box.transform, obj =>
             {
                 if (isRelease)
                 {
                     return;
                 }

                 var slot = obj.GetComponent<ItemSlot>();
                 itemSlots.Add(slot);
                 InstantiateItemSlotAsync(targetCount, callback);
             });

            asyncHandlesList.Add(asyncHandle);
        }
        else
        {
            callback?.Invoke();
        }
    }

    public override void ShowPromptBox(PromptBoxShowDirection direction, Vector3 position, float autoHideTime = 0)
    {
        HidePromptBox();

        if (forbidShow)
        {
            return;
        }

        showDirection = direction;

        Transform cachedTransform = transform;
        cachedTransform.DOKill();
        cachedTransform.position = position;
        cachedTransform.localScale = Vector3.zero;
        gameObject.SetActive(true);

        StartCoroutine(ShowPromptBoxCor());

        if (autoHideTime > 0)
        {
            audoHideEventId = GameManager.Task.AddDelayTriggerTask(autoHideTime, HidePromptBox);
        }
    }

    public override void HidePromptBox()
    {
        StopAllCoroutines();

        base.HidePromptBox();
    }

    IEnumerator ShowPromptBoxCor()
    {
        yield return null;

        while (!CheckInitAllComplete())
        {
            yield return null;
        }

        Refresh();

        transform.DOScale(1.1f, 0.15f).onComplete = () =>
        {
            transform.DOScale(1, 0.15f);
        };
    }

    public override void Refresh()
    {
        if (itemSlots.Count <= 0 || itemSlots.Count < rewardCount) 
        {
            base.Refresh();

            return;
        }

        float slotWidth = itemSlots[0].GetComponent<RectTransform>().sizeDelta.x;
        float slotHeight = itemSlots[0].GetComponent<RectTransform>().sizeDelta.y;

        int maxColumnNum = (int)(boxMaxWidth / (float)slotWidth);
        int columnNum = rewardCount < maxColumnNum ? rewardCount : maxColumnNum;
        int rowNum = Mathf.CeilToInt(rewardCount / (float)columnNum);

        if (balancedArrange && rowNum > 1) 
        {
            columnNum = Mathf.CeilToInt(rewardCount / (float)rowNum);
            rowNum = Mathf.CeilToInt(rewardCount / (float)columnNum);
        }

        boxPreferredWidth = columnNum * (slotWidth + spaceX) + boxHorizontalPadding - spaceX;

        rowNum = rowNum <= 0 ? 1 : rowNum;
        boxPreferredHeight = rowNum * (slotHeight + spaceY) + boxVerticalPadding;

        box.sizeDelta = new Vector2(boxPreferredWidth, boxPreferredHeight);

        for (int i = 0; i < rewardCount; i++)
        {
            int index = i;
            float height = 0;

            if (rowNum > 1)
            {
                if (rowNum % 2 == 0)
                {
                    height = slotHeight / 2f + slotHeight * ((rowNum - 2) / 2f);
                }
                else
                {
                    height = slotHeight * ((rowNum - 1) / 2f);
                }
            }

            for (int j = rowNum; j >= 1; j--) 
            {
                if (i >= columnNum * j) 
                {
                    index = i - columnNum * j;
                    height -= j * slotHeight;
                    break;
                }
            }
            itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector3((index + 0.5f) * slotWidth + index * spaceX - (boxPreferredWidth - boxHorizontalPadding) / 2f + itemOffsetX, height + itemOffsetY);
        }

        base.Refresh();
    }

    public override void OnRelease()
    {
        isRelease = true;

        rewardCount = 0;

        for (int i = 0; i < itemSlots.Count; i++)
        {
            itemSlots[i].OnRelease();
        }
        itemSlots.Clear();

        for (int i = 0; i < asyncHandlesList.Count; i++)
        {
            UnityUtility.UnloadInstance(asyncHandlesList[i]);
        }
        asyncHandlesList.Clear();

        base.OnRelease();
    }

    private bool CheckInitAllComplete()
    {
        if (itemSlots.Count < rewardCount)
        {
            return false;
        }

        //for (int i = 0; i < asyncHandlesList.Count; i++)
        //{
        //    if (asyncHandlesList[i].IsValid() && !asyncHandlesList[i].IsDone)
        //    {
        //        return false;
        //    }
        //}

        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].gameObject.activeSelf && !itemSlots[i].CheckInitComplete())
            {
                return false;
            }
        }

        return true;
    }
}
