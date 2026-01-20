using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PromptItemSlot : ItemSlot
{
    public Image itemImage;
    public Image infiniteImage;
    public TextMeshProUGUI itemNumText;
    public GameObject bgBox;

    private string currentRewardSpriteKey = null;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

        if(type.TotalItemType == TotalItemType.Item_BgID && type.RefID == 0)
        {
            int id = type.ID;
            GameManager.Task.AddDelayTriggerTask(0, () =>
            {
                itemImage.sprite = BGSmallUtil.GetSprite(id);
                itemImage.SetNativeSize();
                itemImage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            });
        }
        else
        {
            string rewardSpriteKey = UnityUtility.GetRewardSpriteKey(type, num);
            if (rewardSpriteKey != currentRewardSpriteKey)
            {
                currentRewardSpriteKey = rewardSpriteKey;
                ClearAsyncHandleList();
                AsyncOperationHandle asyncHandle = UnityUtility.LoadGeneralSpriteAsync(rewardSpriteKey, sp =>
                {
                    itemImage.sprite = sp;
                    if (type.TotalItemType == TotalItemType.Item_BgID || type.TotalItemType == TotalItemType.Item_TileID)
                        itemImage.rectTransform.sizeDelta = new Vector2(350, 350);
                    else
                        itemImage.SetNativeSize();
                });
                AddAsyncHandle(asyncHandle);
            }

            if (type.TotalItemType == TotalItemType.Life || type.TotalItemType == TotalItemType.InfiniteLife ||
                type.TotalItemType == TotalItemType.Coin || type.TotalItemType == TotalItemType.FireworkBoost)  
            {
                itemImage.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
            else if(type.TotalItemType == TotalItemType.KitchenKey)
            {
                itemImage.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            }
            else
            {
                itemImage.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            }
        }

        if (bgBox != null)
            bgBox.SetActive(type.TotalItemType == TotalItemType.Item_BgID);

        if (infiniteImage != null)
        {
            infiniteImage.gameObject.SetActive(false);
        }

        SetItemText(num, type, true);
    }

    private void SetItemText(int num, TotalItemData type, bool tight)
    {
        //if (type == TotalItemData.Coin)
        //{
        //    itemNumText.alignment = TextAlignmentOptions.Center;
        //    itemNumText.transform.localPosition = new Vector3(0, -50);
        //}
        //else
        //{
        //    itemNumText.alignment = TextAlignmentOptions.Left;
        //    itemNumText.transform.localPosition = new Vector3(96, 0);
        //}

        if (type == TotalItemData.Coin)
        {
            itemNumText.SetText(num.ToString());
        }
        else if (type == TotalItemData.InfiniteLifeTime || type == TotalItemData.InfiniteMagnifierBoost || type == TotalItemData.InfiniteAddOneStepBoost || type == TotalItemData.InfiniteFireworkBoost)
        {
            if (num < 60)
                itemNumText.SetText(num.ToString() + "m");
            else
                itemNumText.SetText((num/60).ToString() + "h");
        }
        else
        {
            if (tight)
            {
                itemNumText.SetText("x" + num.ToString());
            }
            else
            {
                itemNumText.SetText("x " + num.ToString());
            }
        }
    }
}
