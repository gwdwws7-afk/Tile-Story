using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelingPackMenu : OnePlusTwoPackMenuBase
{
    public static DateTime PackStartTime = new DateTime(2025, 10, 10, 0, 0, 0);
    public static DateTime PackEndTime = new DateTime(2025, 10, 17, 0, 0, 0);

    protected override string PackName => "TravelingPackMenu";
    protected override ProductNameType SmallProductType_Buy => ProductNameType.Travel_Package_Small;
    protected override ProductNameType SmallProductType_Free_1 => ProductNameType.Travel_Package_Small_1;
    protected override ProductNameType SmallProductType_Free_2 => ProductNameType.Travel_Package_Small_2;
    protected override ProductNameType BigProductType_Buy => ProductNameType.Travel_Package_Big;
    protected override ProductNameType BigProductType_Free_1 => ProductNameType.Travel_Package_Big_1;
    protected override ProductNameType BigProductType_Free_2 => ProductNameType.Travel_Package_Big_2;
    protected override DateTime StartTime => PackStartTime;
    protected override DateTime EndTime => PackEndTime;

    protected override bool SmallPackExist()
    {
        return !GameManager.Ads.IsRemovePopupAds;
    }
}
