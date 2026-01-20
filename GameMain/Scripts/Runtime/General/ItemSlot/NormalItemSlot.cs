using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class NormalItemSlot : ItemSlot
{
    public Image itemImage;
    public GameObject effect;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

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
