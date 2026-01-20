using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public class AnimalPackMenu : TwoColumnCardPackMenuBase
{
    public static DateTime GiftPackStartTime = new DateTime(2025, 12, 28, 0, 0, 0);
    public static DateTime GiftPackEndTime = new DateTime(2026, 1, 16, 0, 0, 0);

    protected override ProductNameType Left_OneTime_ProductType => ProductNameType.Animal_Package_Small;

    protected override ProductNameType Left_OneTime_OriPrice_ProductType => ProductNameType.Animal_Package_Small_line;

    protected override ProductNameType Left_Normal_ProductType => ProductNameType.Animal_Package_Middle;

    protected override ProductNameType Left_Normal_OriPrice_ProductType => ProductNameType.Animal_Package_Middle_line;

    protected override ProductNameType Right_OneTime_ProductType => ProductNameType.Animal_Package_Big;

    protected override ProductNameType Right_OneTime_OriPrice_ProductType => ProductNameType.Animal_Package_Big_line;

    protected override ProductNameType Right_Normal_ProductType => ProductNameType.Animal_Package_Big;

    protected override ProductNameType Right_Normal_OriPrice_ProductType => ProductNameType.Animal_Package_Big_line;

    protected override ProductNameType Left_OneTime_Card_ProductType => ProductNameType.Animal_Card_Package_Small;
    protected override ProductNameType Left_Normal_Card_ProductType => ProductNameType.Animal_Card_Package_Middle;
    protected override ProductNameType Right_OneTime_Card_ProductType => ProductNameType.Animal_Card_Package_Big;
    protected override ProductNameType Right_Normal_Card_ProductType => ProductNameType.Animal_Card_Package_Big;
    
    protected override DateTime StartDate => GiftPackStartTime;

    protected override DateTime EndDate => GiftPackEndTime;

    public GameObject cardText;

    protected override void RefreshDisplay()
    {
        base.RefreshDisplay();
        
        cardText.SetActive(CardModel.Instance.IsInCardActivity);
    }

    protected override bool OneTimePackExist()
    {
        return !GameManager.Ads.IsRemovePopupAds;
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);
    }
}
