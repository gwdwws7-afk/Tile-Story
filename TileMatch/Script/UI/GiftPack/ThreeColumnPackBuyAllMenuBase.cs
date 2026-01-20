using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ThreeColumnPackBuyAllMenuBase : ThreeColumnPackMenuBase
{
    [SerializeField]
    private TextMeshProUGUILocalize buyAllPriceText, buyAllText;
    [SerializeField]
    private DelayButton buyAllButton;

    protected abstract ProductNameType ProductType_BuyAll { get; }

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        base.OnInit(uiGroup, initCompleteAction, userData);

        string price = GameManager.Purchase.GetPrice(ProductType_BuyAll);
        if (!string.IsNullOrEmpty(price))
        {
            buyAllPriceText.SetTerm(price);
            buyAllPriceText.gameObject.SetActive(true);
            buyAllText.gameObject.SetActive(false);
        }
        else
        {
            buyAllPriceText.gameObject.SetActive(false);
            buyAllText.gameObject.SetActive(true);
        }

        buyAllButton.OnInit(() =>
        {
            OnBuyButtonClicked(ProductType_BuyAll);
        });
    }

    public override void OnReset()
    {
        base.OnReset();

        buyAllButton.OnReset();
    }
}
