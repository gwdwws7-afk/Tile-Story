using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopPackageItemSlot : ItemSlot
{
    public TotalItemData itemType;
    public TextMeshProUGUI itemNumText;

    public override void OnInit(TotalItemData type, int num)
    {
        base.OnInit(type, num);

        if (itemType == type)
        {
            if (type == TotalItemData.Coin)
            {
                string result = string.Empty;
                string numString = num.ToString();

                int count = 0;
                for (int i = numString.Length - 1; i >= 0; i--)
                {
                    if (count == 3)
                    {
                        count = 0;
                        result += " ";
                    }
                    result += numString[i];
                    count++;
                }

                string finalResult = string.Empty;
                for (int i = result.Length - 1; i >= 0; i--)
                {
                    finalResult += result[i];
                }

                itemNumText.SetText(finalResult);
            }
            else
            {
                itemNumText.SetItemText(num, type, true);
            }

            gameObject.SetActive(true);
        }
    }

    public override void OnReset()
    {
        gameObject.SetActive(false);

        base.OnReset();
    }
}
