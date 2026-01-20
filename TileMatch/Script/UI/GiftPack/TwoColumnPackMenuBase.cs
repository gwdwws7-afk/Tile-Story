using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TwoColumnPackMenuBase : PopupMenuForm
{
    [SerializeField]
    protected GameObject leftOneTimePackBg, leftNormalPackBg, rightOneTimePackBg, rightNormalPackBg;
    [SerializeField]
    protected TextMeshProUGUILocalize leftPriceText, leftOriginalPriceText, rightPriceText, rightOriginalPriceText;
    [SerializeField]
    protected DelayButton leftBuyButton, rightBuyButton;
    [SerializeField]
    protected GameObject leftNoPriceRoot, leftGetPriceRoot, rightNoPriceRoot, rightGetPriceRoot;
    [SerializeField]
    private DelayButton closeButton;
    [SerializeField]
    private ClockBar clockBar;
    [SerializeField]
    private RawImage[] rawImagesToRelease;

    protected abstract ProductNameType Left_OneTime_ProductType { get; }//ProductNameType.Christmas_Package_Small
    protected abstract ProductNameType Left_OneTime_OriPrice_ProductType { get; }//ProductNameType.Christmas_Package_Small_Off
    protected abstract ProductNameType Left_Normal_ProductType { get; }//ProductNameType.Christmas_Package_Middle
    protected abstract ProductNameType Left_Normal_OriPrice_ProductType { get; }//ProductNameType.Christmas_Package_Middle_Off

    protected abstract ProductNameType Right_OneTime_ProductType { get; }//ProductNameType.
    protected abstract ProductNameType Right_OneTime_OriPrice_ProductType { get; }//ProductNameType.
    protected abstract ProductNameType Right_Normal_ProductType { get; }//ProductNameType.Christmas_Package_Big
    protected abstract ProductNameType Right_Normal_OriPrice_ProductType { get; }//ProductNameType.Christmas_Package_Big_Off

    protected abstract DateTime StartDate { get; }// => GiftPackStartTime

    protected abstract DateTime EndDate { get; }// => GiftPackEndTime

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        RefreshDisplay();
        
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

        closeButton.OnInit(OnCloseButtonClick);
        
        base.OnInit(uiGroup, initCompleteAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnReset()
    {
        leftBuyButton.OnReset();
        rightBuyButton.OnReset();
        clockBar.OnReset();
        closeButton.OnReset();

        base.OnReset();
    }

    protected virtual void RefreshDisplay()
    {
        if (OneTimePackExist())
        {
            leftOneTimePackBg.SetActive(true);
            leftNormalPackBg.SetActive(false);
            rightNormalPackBg.SetActive(false);
            rightOneTimePackBg.SetActive(true);

            SetPrice(Left_OneTime_ProductType, leftPriceText, leftGetPriceRoot, leftNoPriceRoot);
            SetPrice(Left_OneTime_OriPrice_ProductType, leftOriginalPriceText, leftGetPriceRoot, leftNoPriceRoot);
            SetPrice(Right_OneTime_ProductType, rightPriceText, rightGetPriceRoot, rightNoPriceRoot);
            SetPrice(Right_OneTime_OriPrice_ProductType, rightOriginalPriceText, rightGetPriceRoot, rightNoPriceRoot);


            leftBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(Left_OneTime_ProductType);
            });
            rightBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(Right_OneTime_ProductType);
            });
        }
        else
        {
            leftOneTimePackBg.SetActive(false);
            leftNormalPackBg.SetActive(true);
            rightOneTimePackBg.SetActive(false);
            rightNormalPackBg.SetActive(true);

            SetPrice(Left_Normal_ProductType, leftPriceText, leftGetPriceRoot, leftNoPriceRoot);
            SetPrice(Left_Normal_OriPrice_ProductType, leftOriginalPriceText, leftGetPriceRoot, leftNoPriceRoot);
            SetPrice(Right_Normal_ProductType, rightPriceText, rightGetPriceRoot, rightNoPriceRoot);
            SetPrice(Right_Normal_OriPrice_ProductType, rightOriginalPriceText, rightGetPriceRoot, rightNoPriceRoot);

            leftBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(Left_Normal_ProductType);
            });
            rightBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(Right_Normal_ProductType);
            });
        }
    }

    protected abstract bool OneTimePackExist();

    protected void SetPrice(ProductNameType productType, TextMeshProUGUILocalize priceText, GameObject getPriceRoot, GameObject noPriceRoot)
    {
        string price = GameManager.Purchase.GetPrice(productType);
        bool priceIsGet = false;
        if (!string.IsNullOrEmpty(price))
        {
            priceIsGet = true;
            priceText.SetTerm(price);
        }

        getPriceRoot.SetActive(priceIsGet);
        noPriceRoot.SetActive(!priceIsGet);

    }

    protected void OnBuyButtonClicked(ProductNameType targetProductType)
    {
        GameManager.Purchase.BuyProduct(targetProductType, () =>
        {
            RefreshDisplay();
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null);
        });
    }

    private void OnCloseButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }
}
