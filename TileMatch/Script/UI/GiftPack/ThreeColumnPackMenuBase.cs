using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 三排礼包基类
/// </summary>
public abstract class ThreeColumnPackMenuBase : PopupMenuForm
{
    [SerializeField]
    private TextMeshProUGUILocalize priceText_1, priceText_2, priceText_3;
    [SerializeField]
    private DelayButton buyButton_1, buyButton_2, buyButton_3, closeButton;
    [SerializeField]
    private ClockBar clockBar;

    protected abstract ProductNameType ProductType_1 { get; }
    protected abstract ProductNameType ProductType_2 { get; }
    protected abstract ProductNameType ProductType_3 { get; }
    protected abstract DateTime StartDate { get; }// => GiftPackStartTime
    protected abstract DateTime EndDate { get; }// => GiftPackEndTime

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        base.OnInit(uiGroup, initCompleteAction, userData);

        SetPrice(ProductType_1, priceText_1);
        SetPrice(ProductType_2, priceText_2);
        SetPrice(ProductType_3, priceText_3);

        buyButton_1.OnInit(() =>
        {
            OnBuyButtonClicked(ProductType_1);
        });

        buyButton_2.OnInit(() =>
        {
            OnBuyButtonClicked(ProductType_2);
        });

        buyButton_3.OnInit(() =>
        {
            OnBuyButtonClicked(ProductType_3);
        });

        closeButton.OnInit(OnCloseButtonClick);

        if (DateTime.Now < EndDate && DateTime.Now > StartDate)
        {
            clockBar.gameObject.SetActive(true);
            clockBar.OnReset();
            //clockBar.CountdownOver += OnCountdownOver;
            clockBar.StartCountdown(EndDate);
        }
        else
        {
            clockBar.gameObject.SetActive(false);
        }
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnReset()
    {
        buyButton_1.OnReset();
        buyButton_2.OnReset();
        buyButton_3.OnReset();
        closeButton.OnReset();
        clockBar.OnReset();

        base.OnReset();
    }

    private void SetPrice(ProductNameType productType, TextMeshProUGUILocalize priceText)
    {
        string price = GameManager.Purchase.GetPrice(productType);
        if (!string.IsNullOrEmpty(price))
        {
            priceText.SetTerm(price);
        }
    }

    protected void OnBuyButtonClicked(ProductNameType targetProductType)
    {
        GameManager.Purchase.BuyProduct(targetProductType, () =>
        {
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null);
        });
    }

    private void OnCloseButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }
}
