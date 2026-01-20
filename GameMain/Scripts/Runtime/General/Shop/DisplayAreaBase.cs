using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayAreaBase : MonoBehaviour
{
    public List<ShopItemSlot> itemSlots;
    public TextMeshProUGUI timeAreaText;
    protected bool isLooseMode;

    public void SetItemNum(TotalItemData itemType, int num)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (!itemSlots[i].IsInit)
            {
                itemSlots[i].OnInit(itemType, num);
                break;
            }
        }
    }

    public virtual void SetTimeAreaText(int minute)
    {
        if (minute < 60)
        {
            timeAreaText.SetText(minute.ToString() + "min");
        }
        else
        {
            int hour = minute / 60;
            timeAreaText.SetText(hour.ToString() + "h");
        }
    }

    public virtual void RefreshLayout(bool isLoose)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].ItemNum == 0)
            {
                itemSlots[i].OnHide();
            }
            else
            {
                itemSlots[i].OnShow();
            }
        }
    }

    public virtual void OnReset()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            itemSlots[i].OnReset();
        }
    }

    public virtual void OnRelease()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            itemSlots[i].OnRelease();
        }
    }
}
