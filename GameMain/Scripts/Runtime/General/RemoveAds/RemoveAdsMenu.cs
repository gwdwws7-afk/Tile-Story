using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class RemoveAdsMenu : PopupMenuForm
{
    public DelayButton buyButton;
    public DelayButton closeButton;
    public TextMeshProUGUILocalize buyButtonText;
    public Material greyMaterial;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        buyButton.SetBtnEvent(OnBuyButtonClick);
        closeButton.SetBtnEvent(OnCloseButtonClick);
        SetBuyButtonNormal();

        string price = GameManager.Purchase.GetPrice(ProductNameType.Remove_Ads_New_Pro);
        if (!string.IsNullOrEmpty(price))
        {
            buyButtonText.SetTerm(price);
        }
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    public void OnBuyButtonClick()
    {
        SetBuyButtonGrey();

        GameManager.Purchase.BuyProduct(ProductNameType.Remove_Ads_New_Pro, () =>
        {
            Action hideCompleteAction = m_OnHideCompleteAction;
            m_OnHideCompleteAction = null;
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                hideCompleteAction?.Invoke();
            });
        });
    }

    public void OnCloseButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }

    private void SetBuyButtonNormal()
    {
        buyButton.interactable = true;

        try
        {
            buyButton.body.GetComponent<Image>().material = null;
            buyButtonText.SetMaterialPreset(MaterialPresetName.Btn_Green);
        }
        catch(Exception e)
        {
            Log.Error(e.Message);
        }
    }

    private void SetBuyButtonGrey()
    {
        buyButton.interactable = false;

        try
        {
            buyButton.body.GetComponent<Image>().material = greyMaterial;
            buyButtonText.SetMaterialPreset(MaterialPresetName.Btn_Grey);
        }
        catch(Exception e)
        {
            Log.Error(e.Message);
        }
    }
}
