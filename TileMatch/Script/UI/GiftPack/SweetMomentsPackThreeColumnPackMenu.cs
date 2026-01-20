using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SweetMomentsPackThreeColumnPackMenu : ThreeColumnPackMenuBase
{
    public static DateTime GiftPackStartTime = new DateTime(2026, 1, 16, 0, 0, 0);
    public static DateTime GiftPackEndTime = new DateTime(2026, 1, 28, 0, 0, 0);

    protected override ProductNameType ProductType_1 => ProductNameType.Dessert_Small;

    protected override ProductNameType ProductType_2 => ProductNameType.Dessert_Middle;

    protected override ProductNameType ProductType_3 => ProductNameType.Dessert_Big;

    protected override DateTime StartDate => GiftPackStartTime;

    protected override DateTime EndDate => GiftPackEndTime;
}
