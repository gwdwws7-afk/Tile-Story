using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ItemPrefab : MonoBehaviour
{
    [SerializeField] private Image ItemImage;
    [SerializeField] private TextMeshProUGUI ItemText;

    private AsyncOperationHandle _assetHandle;
    public void Init(TotalItemData item,int itemNum)
    {
        UnityUtility.UnloadAssetAsync(_assetHandle);
        _assetHandle= UnityUtility.LoadAssetAsync<Sprite>(GetRewardImageName(item), (s) =>
        {
            ItemImage.sprite=s as Sprite;
        });
        // ItemText.text =item.TotalItemType == TotalItemType.Coin?$"{itemNum}" :$"x {itemNum}";
        ItemText.SetItemText(itemNum, item, false);
    }

    private void OnDestroy()
    {
        //释放资源
        OnRelease();
    }

    private void OnRelease()
    {
        UnityUtility.UnloadAssetAsync(_assetHandle);
    }

    private string GetRewardImageName(TotalItemData data)
    {
        if (data.TotalItemType == TotalItemType.Coin)
        {
            return "TotalItemAtlas[Coin1]";
        }
        return $"TotalItemAtlas[{data.TotalItemType.ToString()}]";
    }

    public void ChangePos(int num)
    {
        switch (num)
        {
            case 1:
                ItemText.rectTransform.anchoredPosition =new Vector2(0,-65);
                return;
            case 2:
                ItemText.rectTransform.anchoredPosition =new Vector2(0, -76);
                return;
            case 3:
                ItemText.rectTransform.anchoredPosition =new Vector2(0, -95);
                return;
        }
       
    }
}
