using System;

public class HarvestTwoColumnPackMenu : TwoColumnPackMenuBase
{
    public static DateTime GiftPackStartTime = new DateTime(2025, 9, 24, 0, 0, 0);
    public static DateTime GiftPackEndTime = new DateTime(2025, 10, 2, 0, 0, 0);

    protected override ProductNameType Left_OneTime_ProductType => ProductNameType.Harvest_Package_Small;

    protected override ProductNameType Left_OneTime_OriPrice_ProductType => ProductNameType.Harvest_Package_Small_line;

    protected override ProductNameType Left_Normal_ProductType => ProductNameType.Harvest_Package_Middle;

    protected override ProductNameType Left_Normal_OriPrice_ProductType => ProductNameType.Harvest_Package_Middle_line;


    protected override ProductNameType Right_OneTime_ProductType => ProductNameType.Harvest_Package_Big;

    protected override ProductNameType Right_OneTime_OriPrice_ProductType => ProductNameType.Harvest_Package_Big_line;

    protected override ProductNameType Right_Normal_ProductType => ProductNameType.Harvest_Package_Big;

    protected override ProductNameType Right_Normal_OriPrice_ProductType => ProductNameType.Harvest_Package_Big_line;

    protected override DateTime StartDate => GiftPackStartTime;
    protected override DateTime EndDate => GiftPackEndTime;

    protected override bool OneTimePackExist()
    {
        return !GameManager.Ads.IsRemovePopupAds;
    }
}
