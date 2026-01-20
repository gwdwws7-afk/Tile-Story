using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CeremonyPackMenu : PopupMenuForm
{
    [SerializeField]
    private TextMeshProUGUILocalize titleText;
    [SerializeField]
    private DelayButton buyButton, closeButton;
    [SerializeField]
    private TextMeshProUGUI priceText, lineText;
    [SerializeField]
    private GameObject grade1, grade2;

    private int grade;
    private ProductNameType productType;
    private ProductNameType lineType;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        buyButton.OnInit(OnBuyButtonClick);
        closeButton.OnInit(OnCloseButtonClick);

        titleText.SetParameterValue("level", (GameManager.PlayerData.NowLevel - 1).ToString());

        grade = PlayerPrefs.GetInt("CeremonyPackGrade", 0);
        if(grade == 0)
        {
            productType = ProductNameType.Ceremony_Level_package_1;
            lineType = ProductNameType.Ceremony_Level_package_1_line;
            grade1.SetActive(true);
            grade2.SetActive(false);
        }
        else
        {
            productType = ProductNameType.Ceremony_Level_package_2;
            lineType = ProductNameType.Ceremony_Level_package_2_line;
            grade1.SetActive(false);
            grade2.SetActive(true);
        }

        SetPrice(productType, priceText);
        SetPrice(lineType, lineText);
    }

    public override void OnReset()
    {
        base.OnReset();
    }

    private void SetPrice(ProductNameType productType, TextMeshProUGUI priceText)
    {
        string price = GameManager.Purchase.GetPrice(productType);
        if (!string.IsNullOrEmpty(price))
        {
            priceText.text = price;
            priceText.gameObject.SetActive(true);
        }
        else
        {
            priceText.gameObject.SetActive(false);
        }
    }

    private void OnBuyButtonClick()
    {
        GameManager.Purchase.BuyProduct(productType, () =>
        {
            if (grade < 1)
                PlayerPrefs.SetInt("CeremonyPackGrade", grade + 1);

            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null);
        });
    }

    private void OnCloseButtonClick()
    {
        if (grade >= 1) 
            PlayerPrefs.SetInt("CeremonyPackGrade", grade - 1);

        GameManager.UI.HideUIForm(this);
    }
}
