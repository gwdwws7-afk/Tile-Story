using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopSpecialOfferBar : ShopBar
{
    public TextMeshProUGUI[] Num_Texts;
    public override string BarName=>"ShopSpecialOfferBar";

    public GameObject sideMarkRoot;
    public TextMeshProUGUILocalize sideMarkLocalize;

    public override void OnInit(ShopPackageData shopPackageData)
    {
        //var list = shopPackageData.GetItemDatas();
        //for (int i = 0; i < Num_Texts.Length; i++)
        //{
        //    if (i == 0)
        //    {
        //        Num_Texts[i].text = $"{list[i].num}";
        //    }
        //    else
        //        Num_Texts[i].text = $"{list[i].num}";
        //}

        sideMarkRoot.SetActive(!string.IsNullOrEmpty(shopPackageData.SideMarkTerm));
        sideMarkLocalize.SetTerm(shopPackageData.SideMarkTerm);

        base.OnInit(shopPackageData);
    }

    public override void OnBuyButtonClick()
    {
        GameManager.Purchase.BuyProduct(m_ProductNameType, () =>
        {
            GameManager.UI.HideUIForm("ShopMenuManager");

            if (m_ProductNameType == ProductNameType.Special_Offer)
                GameManager.PlayerData.RecordEverBoughtSpecialOfferPack(true);

            //if (productNameType == ProductNameType.Remove_ads)
            //{
            //    GameManager.UI.ShowUIForm<RemovePopupAdsBuySuccessMenu>();
            //}
            //else
            //{
            //    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.NewPlayerPackageRewardPanel, false, null);
            //}
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.NewPlayerPackageRewardPanel, false, () =>
            {
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.ShopBuyGetRewardComplete));
            });
        }, (error) =>
        {
            Log.Error($"ShopSpecialOfferBar is Error :{error}");
        });
    }
}
