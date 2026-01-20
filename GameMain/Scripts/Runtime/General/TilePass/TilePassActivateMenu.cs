using MySelf.Model;
using System;
using UnityEngine;

public class TilePassActivateMenu : PopupMenuForm
{
    public DelayButton vipButton;
    public TextMeshProUGUILocalize vipPrice;
    public DelayButton superVIPButton;
    public TextMeshProUGUILocalize superVIPPrice;
    public DelayButton closeButton;

    public GameObject black;
    public GameObject superCoin;
    public Transform superGasoline;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        vipButton.OnInit(OnVIPButtonClicked);
        superVIPButton.OnInit(OnSuperVIPButtonClicked);
        closeButton.OnInit(OnClose);

        vipPrice.SetTerm(GameManager.Purchase.GetPrice(ProductNameType.VIP_Pass));
        if (!TilePassModel.Instance.IsVIP)
        {
            superVIPPrice.SetTerm(GameManager.Purchase.GetPrice(ProductNameType.Super_VIP));

            black.SetActive(false);
            superCoin.SetActive(true);
            superGasoline.localScale = Vector3.one;
            superGasoline.localPosition = Vector3.zero;
        }
        else
        {
            superVIPPrice.SetTerm(GameManager.Purchase.GetPrice(ProductNameType.Super_VIP_Discount));

            black.SetActive(true);
            superCoin.SetActive(false);
            superGasoline.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            superGasoline.localPosition = new Vector3(0, 110);
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        GameManager.UI.HideUIForm(this);
        TilePassMainMenu mainMenu = GameManager.UI.GetUIForm("TilePassMainMenu") as TilePassMainMenu;
        mainMenu.RefreshVIPState();
    }

    private void OnVIPButtonClicked()
    {
        GameManager.Purchase.BuyProduct(ProductNameType.VIP_Pass, () =>
        {
            TilePassUtil.RecordTilePassPurchase(GameManager.DataTable.GetDataTable<DTProductID>().Data.GetProductID(ProductNameType.VIP_Pass));//打点

            GameManager.UI.HideUIForm(this);

            RewardManager.Instance.SaveRewardData(TotalItemData.Coin, 1000, true);

            TilePassModel.Instance.IsVIP = true;
            GameManager.UI.ShowUIForm("TilePassVIPUnlockMenu");
        });
    }

    private void OnSuperVIPButtonClicked()
    {
        if (!TilePassModel.Instance.IsVIP)
        {
            GameManager.Purchase.BuyProduct(ProductNameType.Super_VIP, () =>
            {
                TilePassUtil.RecordTilePassPurchase(GameManager.DataTable.GetDataTable<DTProductID>().Data.GetProductID(ProductNameType.Super_VIP));//打点

                GameManager.UI.HideUIForm(this);

                RewardManager.Instance.SaveRewardData(TotalItemData.Coin, 1000, true);

                TilePassModel.Instance.IsVIP = true;
                TilePassModel.Instance.IsSuperVIP = true;
                TilePassModel.Instance.TotalTargetNum += 100;
                GameManager.UI.ShowUIForm("TilePassVIPUnlockMenu");
            });
        }
        else
        {
            GameManager.Purchase.BuyProduct(ProductNameType.Super_VIP_Discount, () =>
            {
                TilePassUtil.RecordTilePassPurchase(GameManager.DataTable.GetDataTable<DTProductID>().Data.GetProductID(ProductNameType.Super_VIP_Discount));//打点

                GameManager.UI.HideUIForm(this);

                TilePassModel.Instance.IsVIP = true;
                TilePassModel.Instance.IsSuperVIP = true;
                TilePassModel.Instance.TotalTargetNum += 100;
                GameManager.UI.ShowUIForm("TilePassVIPUnlockMenu");
            });
        }
    }
}
