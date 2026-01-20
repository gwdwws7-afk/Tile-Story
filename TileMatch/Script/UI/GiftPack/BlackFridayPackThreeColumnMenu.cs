using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlackFridayPackThreeColumnMenu : ThreeColumnPackMenuBase
{
    public static DateTime GiftPackStartTime = new DateTime(2025, 11, 28, 0, 0, 0);
    public static DateTime GiftPackEndTime = new DateTime(2025, 12, 06, 0, 0, 0);

    protected override ProductNameType ProductType_1 => ProductNameType.Blackfriday_Package_Small;
    protected override ProductNameType ProductType_2 => ProductNameType.Blackfriday_Package_Middle;
    protected override ProductNameType ProductType_3 => ProductNameType.Blackfriday_Package_Big;
    protected override DateTime StartDate => GiftPackStartTime;
    protected override DateTime EndDate => GiftPackEndTime;

    public DelayButton GetAllButton;
    public TextMeshProUGUILocalize GetAllPriceText;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        base.OnInit(uiGroup, initCompleteAction, userData);

        ProductNameType product = ProductNameType.Blackfriday_Package_Getall;
        
        String price = GameManager.Purchase.GetPrice(product);
        if (!string.IsNullOrEmpty(price))
            GetAllPriceText.SetTerm(price);
        
        GetAllButton.OnInit(() =>
        {
            GameManager.Purchase.BuyProduct(product, () =>
            {
                GameManager.UI.HideUIForm(this);
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null);
            });
        });
    }

    public override void OnReset()
    {
        base.OnReset();
        
        GetAllButton.OnReset();
    }
}
