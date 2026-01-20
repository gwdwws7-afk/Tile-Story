using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummerTwoColumnPackMenu : TwoColumnPackMenuBase
{
    public static DateTime GiftPackStartTime = new DateTime(2025, 7, 30, 0, 0, 0);
    public static DateTime GiftPackEndTime = new DateTime(2025, 8, 7, 0, 0, 0);

    protected override ProductNameType Left_OneTime_ProductType => ProductNameType.Island_Package_Small;

    protected override ProductNameType Left_OneTime_OriPrice_ProductType => ProductNameType.Father_Package_Small_line;

    protected override ProductNameType Left_Normal_ProductType => ProductNameType.Island_Package_Middle;

    protected override ProductNameType Left_Normal_OriPrice_ProductType => ProductNameType.Father_Package_Middle_line;


    protected override ProductNameType Right_OneTime_ProductType => ProductNameType.Island_Package_Big;

    protected override ProductNameType Right_OneTime_OriPrice_ProductType => ProductNameType.Father_Package_Big_line;

    protected override ProductNameType Right_Normal_ProductType => ProductNameType.Island_Package_Big;

    protected override ProductNameType Right_Normal_OriPrice_ProductType => ProductNameType.Father_Package_Big_line;

    protected override DateTime StartDate => GiftPackStartTime;
    protected override DateTime EndDate => GiftPackEndTime;

    protected override bool OneTimePackExist()
    {
        return !GameManager.Ads.IsRemovePopupAds;
    }
}
