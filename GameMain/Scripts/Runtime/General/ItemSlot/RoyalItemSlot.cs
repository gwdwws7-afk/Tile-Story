using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public sealed class RoyalItemSlot : ItemSlot
{
    public Image itemImage;
    public Image infiniteImage;
    public TextMeshProUGUI itemNumText;
    public GameObject emptyImg;
    public bool isRoyalReward;

    private string currentRewardSpriteKey = null;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

        if (type == TotalItemData.None)
        {
            gameObject.SetActive(false);
            emptyImg.SetActive(true);
            return;
        }

        gameObject.SetActive(true);
        emptyImg.SetActive(false);

        InitSingleReward(type, num);
    }

    public override void OnInit(TotalItemData[] types, int[] nums)
    {
        if (types == null || nums == null || types.Length == 0 || nums.Length == 0)
        {
            Log.Error("RoyalItemSlot init data is invalid");
            return;
        }

        base.OnInit(types, nums);

        if (types.Length == 1 && types[0] == TotalItemData.None)
        {
            gameObject.SetActive(false);
            emptyImg.SetActive(true);
            return;
        }

        gameObject.SetActive(true);
        emptyImg.SetActive(false);

        if (types.Length == 1)
        {
            InitSingleReward(types[0], nums[0]);
        }
        else
        {
            InitChestReward();
        }
    }

    public override void OnRelease()
    {
        currentRewardSpriteKey = null;

        base.OnRelease();
    }

    private void InitChestReward()
    {
        string rewardSpriteKey = "RoyalPassChest1";
        if (isRoyalReward)
        {
            rewardSpriteKey = "RoyalPassChest2";
        }

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
        }

        if (isRoyalReward)
        {
            itemImage.transform.localScale = new Vector3(0.9f, 0.9f);
            itemImage.transform.localPosition = new UnityEngine.Vector3(12.5f, 24);
        }
        else
        {
            itemImage.transform.localScale = Vector3.one;
            itemImage.transform.localPosition = new UnityEngine.Vector3(-9, 10);
        }
        itemNumText.gameObject.SetActive(false);
        infiniteImage.gameObject.SetActive(false);
    }

    private void InitSingleReward(TotalItemData type, int num)
    {
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
        }

        itemImage.transform.localScale = new Vector3(0.4f, 0.4f);

        if (type == TotalItemData.Coin)
        {
            itemNumText.fontSize = 55;
            itemNumText.characterSpacing = 0;
            itemNumText.SetText(num.ToString());
        }
        else
        {
            itemNumText.fontSize = 55;
            itemNumText.characterSpacing = 20;
            itemNumText.SetItemText(num, type, true);
        }
        itemNumText.gameObject.SetActive(true);

        if (infiniteImage != null)
        {
            infiniteImage.gameObject.SetActive(false);
        }

        if (isRoyalReward)
        {
            itemImage.transform.localPosition = new UnityEngine.Vector3(-51, 12);
        }
        else
        {
            itemImage.transform.localPosition = new UnityEngine.Vector3(-72, 12);
        }
    }
}
