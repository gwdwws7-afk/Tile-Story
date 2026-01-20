using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CardSetRewardSlot : MonoBehaviour
{
    public Image rewardImg;
    public TextMeshProUGUI rewardNumText;
    public Sprite clear;

    //private string _currentRewardSpriteKey;
    private AsyncOperationHandle _assetHandle;

    public TotalItemData ItemType { get; private set; }

    public void Init(TotalItemData type, int num)
    {
        ItemType = type;
        string rewardSpriteKey = $"TotalItemAtlas[{type.TotalItemType}]";
        _assetHandle = UnityUtility.LoadAssetAsync<Sprite>(rewardSpriteKey, s =>
        {
            rewardImg.sprite = s as Sprite;
        });
        // if (rewardSpriteKey != null && rewardSpriteKey != _currentRewardSpriteKey)
        // {
        //     _currentRewardSpriteKey = rewardSpriteKey;
        //     GameManager.Resource.Release(_assetHandle);
        //     _assetHandle =
        //         GameManager.Resource.LoadAssetAsync<Sprite>(rewardSpriteKey, CallBackEvent);
        // }
        
        rewardNumText.SetItemText(num, type, true);
    }

    public void Release()
    {
        UnityUtility.UnloadAssetAsync(_assetHandle);
        _assetHandle = default;
        rewardImg.sprite = clear;
    }
}
