using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class PackageDisplayArea : MonoBehaviour
{
    public List<ItemSlot> itemSlots;

    public void OnInit(List<ItemData> itemDatas)
    {
        RefreshDisplayArea(itemDatas);
    }

    public void OnReset()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            itemSlots[i].OnReset();
        }
    }

    public void OnRelease()
    {

    }

    private void RefreshDisplayArea(List<ItemData> itemDatas)
    {
        for (int i = 0; i < itemDatas.Count; i++)
        {
            for (int j = 0; j < itemSlots.Count; j++)
            {
                itemSlots[j].OnInit(itemDatas[i].type, itemDatas[i].num);
            }
        }
    }

    public void RefreshLayout()
    {

    }

    public ItemSlot GetItemSlot(TotalItemData itemType)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].ItemType == itemType)
            {
                return itemSlots[i];
            }
        }

        return null;
    }

    public void SetFontPresetMaterial(MaterialPresetName materialPresetName, string fontName)
    {
        GameManager.Localization.GetPresetMaterialAsync(materialPresetName.ToString(), fontName, mat =>
        {
            for (int i = 0; i < itemSlots.Count; i++)
            {
                var textList = itemSlots[i].GetComponentsInChildren<TextMeshProUGUI>();
                for (int j = 0; j < textList.Length; j++)
                {
                    textList[j].fontSharedMaterial = mat;
                }
            }
        });
    }
}
