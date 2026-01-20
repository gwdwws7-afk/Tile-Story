using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopCoinBar : ShopBar
{
    public TextMeshProUGUI Num_Text;
    public override string BarName
    {
        get
        {
            return "ShopCoinBar";
        }
    }

    public override void OnInit(ShopPackageData shopPackageData)
    {
        var list = shopPackageData.GetItemDatas();
        Num_Text.text = list[0].num.ToString();
        base.OnInit(shopPackageData);
    }
}
