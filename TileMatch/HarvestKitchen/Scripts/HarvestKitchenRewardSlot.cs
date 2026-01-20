using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class HarvestKitchenRewardSlot : MonoBehaviour
{
    public Image rewardImage;
    public TextMeshProUGUI rewardText;

    private AsyncOperationHandle handle;
    
    public void Init(TotalItemData type, int num)
    {
        rewardText.SetItemText(num, type, false);
        
        string rewardImageName = UnityUtility.GetRewardSpriteKey(type, num);
        if (type == TotalItemData.Coin)
            rewardImageName = "Coin1";
        
        UnityUtility.UnloadAssetAsync(handle);
        handle = UnityUtility.LoadSpriteAsync(rewardImageName, "TotalItemAtlas", sp =>
        {
            rewardImage.sprite = sp;
        });
    }

    public void Release()
    {
        UnityUtility.UnloadAssetAsync(handle);
        handle = default(AsyncOperationHandle);
    }
}
