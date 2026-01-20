using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopRemoveAdsBar : ShopBar
{
    public override string BarName => "ShopRemoveAdsBar";

    //public Material greyMaterial;

    public override void OnInit(ShopPackageData shopPackageData)
    {
        base.OnInit(shopPackageData);

        SetBuyButtonNormal();
    }

    public override void OnBuyButtonClick()
    {
        SetBuyButtonGrey();

        base.OnBuyButtonClick();
    }

    private void SetBuyButtonNormal()
    {
        buyButton.interactable = true;

        //try
        //{
        //    buyButton.body.GetComponent<Image>().material = null;
        //    buyText.SetMaterialPreset(MaterialPresetName.Btn_Green);
        //}
        //catch (Exception e)
        //{
        //    Log.Error(e.Message);
        //}
    }

    private void SetBuyButtonGrey()
    {
        buyButton.interactable = false;

        //try
        //{
        //    buyButton.body.GetComponent<Image>().material = greyMaterial;
        //    buyText.SetMaterialPreset(MaterialPresetName.Btn_Grey);
        //}
        //catch (Exception e)
        //{
        //    Log.Error(e.Message);
        //}
    }
}
