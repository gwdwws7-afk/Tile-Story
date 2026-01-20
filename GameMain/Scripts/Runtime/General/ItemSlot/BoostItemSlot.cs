using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class BoostItemSlot : ItemSlot
{
    public Image itemImage;
    public GameObject bgEffect;
    public GameObject trailEffect;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

        bgEffect.SetActive(false);
        trailEffect.SetActive(false);

        ClearAsyncHandleList();
        string spriteKey = UnityUtility.GetRewardSpriteKey(type, num);
        AsyncOperationHandle asyncHandle = UnityUtility.LoadGeneralSpriteAsync(spriteKey, sp =>
        {
            itemImage.sprite = sp;

            OnInitCompleteAction?.Invoke();
        });
        AddAsyncHandle(asyncHandle);
    }
}
