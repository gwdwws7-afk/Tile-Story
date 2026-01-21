using System;
using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public class ValentinePackMenu : TwoColumnCardPackMenuBase
{
    public static DateTime GiftPackStartTime = new DateTime(2026, 1, 28, 0, 0, 0);
    public static DateTime GiftPackEndTime = new DateTime(2026, 2, 21, 0, 0, 0);

    protected override ProductNameType Left_OneTime_ProductType => ProductNameType.Valentine_Package_Small_New;

    protected override ProductNameType Left_OneTime_OriPrice_ProductType => ProductNameType.Valentine_Package_Small_line;

    protected override ProductNameType Left_Normal_ProductType => ProductNameType.Valentine_Package_Middle_New;

    protected override ProductNameType Left_Normal_OriPrice_ProductType => ProductNameType.Valentine_Package_Middle_line;

    protected override ProductNameType Right_OneTime_ProductType => ProductNameType.Valentine_Package_Big_New;

    protected override ProductNameType Right_OneTime_OriPrice_ProductType => ProductNameType.Valentine_Package_Big_line;

    protected override ProductNameType Right_Normal_ProductType => ProductNameType.Valentine_Package_Big_New;

    protected override ProductNameType Right_Normal_OriPrice_ProductType => ProductNameType.Valentine_Package_Big_line;

    protected override ProductNameType Left_OneTime_Card_ProductType => ProductNameType.Valentine_Card_Package_Small_New;
    protected override ProductNameType Left_Normal_Card_ProductType => ProductNameType.Valentine_Card_Package_Middle_New;
    protected override ProductNameType Right_OneTime_Card_ProductType => ProductNameType.Valentine_Card_Package_Big_New;
    protected override ProductNameType Right_Normal_Card_ProductType => ProductNameType.Valentine_Card_Package_Big_New;

    protected ProductNameType Right_OneTime_Tile_ProductType => ProductNameType.Valentine_Chest_Package_Big_New;
    
    protected override DateTime StartDate => GiftPackStartTime;

    protected override DateTime EndDate => GiftPackEndTime;

    [SerializeField] private GameObject rightOneTimeTilePackBg, rightCardPackBg1, rightCardPackBg2;
    public GameObject cardText;

    private int TileId = 1004;
    
    protected override void RefreshDisplay()
    {
        base.RefreshDisplay();
        
        cardText.SetActive(CardModel.Instance.IsInCardActivity);

        if (!GameManager.PlayerData.IsOwnTileID(TileId))
        {
            rightOneTimeTilePackBg.SetActive(true);
            rightCardPackBg1.SetActive(false);
            rightCardPackBg2.SetActive(false);
            SetPrice(Right_OneTime_Tile_ProductType, rightPriceText, rightGetPriceRoot, rightNoPriceRoot);   
            
            rightBuyButton.OnInit(() =>
            {
                OnBuyButtonClicked(Right_OneTime_Tile_ProductType);
            });
        }
        else
        {
            rightOneTimeTilePackBg.SetActive(false);
        }
    }

    protected override bool OneTimePackExist()
    {
        return !GameManager.Ads.IsRemovePopupAds;
    }
}
