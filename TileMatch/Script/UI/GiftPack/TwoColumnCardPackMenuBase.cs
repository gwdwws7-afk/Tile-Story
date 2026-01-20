using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public abstract class TwoColumnCardPackMenuBase : TwoColumnPackMenuBase
{
    [SerializeField]
    private GameObject leftOneTimeCardPackBg, leftNormalCardPackBg, rightOneTimeCardPackBg, rightNormalCardPackBg;
    
    protected abstract ProductNameType Left_OneTime_Card_ProductType { get; }
    protected abstract ProductNameType Left_Normal_Card_ProductType { get; }

    protected abstract ProductNameType Right_OneTime_Card_ProductType { get; }
    protected abstract ProductNameType Right_Normal_Card_ProductType { get; }

    protected override void RefreshDisplay()
    {
        bool isInCardActivity = CardModel.Instance.IsInCardActivity;
        if (OneTimePackExist())
        {
            leftOneTimePackBg.SetActive(!isInCardActivity);
            leftNormalPackBg.SetActive(false);
            //rightNormalPackBg.SetActive(false);
            rightOneTimePackBg.SetActive(!isInCardActivity);
            leftOneTimeCardPackBg.SetActive(isInCardActivity);
            leftNormalCardPackBg.SetActive(false);
            //rightOneTimeCardPackBg.SetActive(false);
            rightNormalCardPackBg.SetActive(isInCardActivity);

            ProductNameType leftProductType =
                isInCardActivity ? Left_OneTime_Card_ProductType : Left_OneTime_ProductType;
            ProductNameType rightProductType =
                isInCardActivity ? Right_OneTime_Card_ProductType : Right_OneTime_ProductType;
            
            SetPrice(leftProductType, leftPriceText, leftGetPriceRoot, leftNoPriceRoot);
            SetPrice(Left_OneTime_OriPrice_ProductType, leftOriginalPriceText, leftGetPriceRoot, leftNoPriceRoot);
            SetPrice(rightProductType, rightPriceText, rightGetPriceRoot, rightNoPriceRoot);
            SetPrice(Right_OneTime_OriPrice_ProductType, rightOriginalPriceText, rightGetPriceRoot, rightNoPriceRoot);

            leftBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(leftProductType);
            });
            rightBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(rightProductType);
            });
        }
        else
        {
            leftOneTimePackBg.SetActive(false);
            leftNormalPackBg.SetActive(!isInCardActivity);
            //rightOneTimePackBg.SetActive(false);
            rightNormalPackBg.SetActive(!isInCardActivity);
            leftOneTimeCardPackBg.SetActive(false);
            leftNormalCardPackBg.SetActive(isInCardActivity);
            //rightOneTimeCardPackBg.SetActive(false);
            rightNormalCardPackBg.SetActive(isInCardActivity);

            ProductNameType leftProductType =
                isInCardActivity ? Left_Normal_Card_ProductType : Left_Normal_ProductType;
            ProductNameType rightProductType =
                isInCardActivity ? Right_Normal_Card_ProductType : Right_Normal_ProductType;
            
            SetPrice(leftProductType, leftPriceText, leftGetPriceRoot, leftNoPriceRoot);
            SetPrice(Left_Normal_OriPrice_ProductType, leftOriginalPriceText, leftGetPriceRoot, leftNoPriceRoot);
            SetPrice(rightProductType, rightPriceText, rightGetPriceRoot, rightNoPriceRoot);
            SetPrice(Right_Normal_OriPrice_ProductType, rightOriginalPriceText, rightGetPriceRoot, rightNoPriceRoot);

            leftBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(leftProductType);
            });
            rightBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(rightProductType);
            });
        }
    }
}
