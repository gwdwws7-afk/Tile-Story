using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

public class KitchenStageRewardItem : ItemSlot
{
    public float lifeScale = 0.25f;
    public float otherScale = 0.18f;
    
    public Image itemImage;
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
                itemImage.SetNativeSize();
            });
            AddAsyncHandle(asyncHandle);
        }

        if (type.ID == 17 || type.ID == 18)
        {
            itemImage.transform.localScale = Vector3.one * lifeScale;
        }
        else
        {
            itemImage.transform.localScale = Vector3.one * otherScale;
        }
        
        itemNumText.SetItemText(num, type, true);
    }
    
    public override void OnRelease()
    {
        currentRewardSpriteKey = null;

        base.OnRelease();
    }
}
