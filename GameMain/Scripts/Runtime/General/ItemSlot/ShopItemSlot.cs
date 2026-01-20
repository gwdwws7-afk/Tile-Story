using System;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// 商品放置槽
/// </summary>
public sealed class ShopItemSlot : ItemSlot
{
    public Image itemImage;
    public Image infiniteImage;
    public TextMeshProUGUI itemNumText;

    private string currentRewardSpriteKey = null;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

        string rewardSpriteKey;
        string atlasKey;
        rewardSpriteKey = UnityUtility.GetRewardSpriteKey(type, num);
        atlasKey = "TotalItemAtlas";

        if (rewardSpriteKey != currentRewardSpriteKey)
        {
            currentRewardSpriteKey = rewardSpriteKey;
            ClearAsyncHandleList();
            AsyncOperationHandle asyncHandle = UnityUtility.LoadSpriteAsync(rewardSpriteKey, atlasKey, sp =>
            {
                itemImage.sprite = sp;
                // itemImage.SetNativeSize();
            });
            AddAsyncHandle(asyncHandle);
        }

        if (type == TotalItemData.Coin)
        {
            string result = string.Empty;
            string numString = num.ToString();

            int count = 0;
            for (int i = numString.Length - 1; i >= 0; i--)
            {
                if (count == 3)
                {
                    count = 0;
                    result += " ";
                }

                result += numString[i];
                count++;
            }

            string finalResult = string.Empty;
            for (int i = result.Length - 1; i >= 0; i--)
            {
                finalResult += result[i];
            }

            itemNumText.SetText(finalResult);
        }
        else if (type == TotalItemData.RemoveAds)
        {
            itemNumText.fontSize = 50;
            itemNumText.alignment = TextAlignmentOptions.Center;
            itemNumText.GetComponent<TextMeshProUGUILocalize>().enabled = true;
            itemNumText.GetComponent<TextMeshProUGUILocalize>().SetTerm("Shop.Remove Pop-up Ads");
        }
        else
        {
            itemNumText.SetItemText(num, type, true);
        }

        var cacheTransform = itemImage.transform;
        if (type == TotalItemData.Prop_Absorb ||
            type == TotalItemData.Prop_Back ||
            type == TotalItemData.Prop_Grab ||
            type == TotalItemData.Prop_ChangePos)
        {
            cacheTransform.localRotation = Quaternion.Euler(0, 0, 22.5f);
            cacheTransform.localPosition = new Vector3(-4, -3);
            cacheTransform.localScale = new Vector3(0.44f, 0.44f);
        }
        else if (type == TotalItemData.Prop_AddOneStep)
        {
            cacheTransform.localRotation = Quaternion.Euler(0, 0, 0);
            cacheTransform.localPosition = new Vector3(0, -5);
            cacheTransform.localScale = new Vector3(0.4f, 0.4f);
        }
        else if(type == TotalItemData.None ||
            type == TotalItemData.Coin ||
            type == TotalItemData.Star ||
            type == TotalItemData.RemoveAds)
        {
            cacheTransform.localRotation = Quaternion.Euler(0, 0, 0);
            cacheTransform.localPosition = new Vector3(0, -5);
            cacheTransform.localScale = new Vector3(0.44f, 0.44f);
        }
        else
        {
            cacheTransform.localRotation = Quaternion.Euler(0, 0, 0);
            cacheTransform.localPosition = new Vector3(0, -5);
            cacheTransform.localScale = new Vector3(0.44f, 0.44f);
        }

        if (infiniteImage != null)
        {
            infiniteImage.gameObject.SetActive(false);
        }
    }

    public override void OnReset()
    {
        if (ItemType == TotalItemData.RemoveAds)
        {
            itemNumText.fontSize = 65;
            itemNumText.alignment = TextAlignmentOptions.Left;
            itemNumText.GetComponent<TextMeshProUGUILocalize>().enabled = false;
        }

        base.OnReset();
    }

    public override void OnRelease()
    {
        currentRewardSpriteKey = null;

        base.OnRelease();
    }
}