using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddItemSlot : ItemSlot
{
    public Image itemImage;
    public Image infiniteImage;
    public TextMeshProUGUI itemNumText;

    private string currentRewardSpriteKey = null;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

        string rewardSpriteKey = UnityUtility.GetRewardSpriteKey(type, num);

        if (rewardSpriteKey != currentRewardSpriteKey)
        {
            currentRewardSpriteKey = rewardSpriteKey;
            ClearAsyncHandleList();
            AsyncOperationHandle asyncHandle = UnityUtility.LoadGeneralSpriteAsync(rewardSpriteKey, sp =>
            {
                itemImage.sprite = sp;
                itemImage.SetNativeSize();
            });
            AddAsyncHandle(asyncHandle);

            if (type == TotalItemData.Star)
                itemImage.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            else
                itemImage.transform.localScale = new Vector3(0.24f, 0.24f, 1);
        }
        itemImage.gameObject.SetActive(true);
        itemNumText.SetItemAddText(num, type, true);
        itemNumText.gameObject.SetActive(true);

        if (infiniteImage != null)
        {
            infiniteImage.gameObject.SetActive(false);
        }
    }

    public override void OnRelease()
    {
        currentRewardSpriteKey = null;

        base.OnRelease();
    }
}
