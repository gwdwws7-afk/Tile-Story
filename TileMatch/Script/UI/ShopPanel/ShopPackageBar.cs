using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPackageBar : ShopBar
{
    public TextMeshProUGUILocalize packNameLocalize;

    public TextMeshProUGUI coinRewardNumText;
    public GameObject[] rewardItemRoots;
    public TextMeshProUGUI[] rewardNumTexts;
    public GridLayoutGroup layoutGroup;

    public GameObject sideMarkRoot;
    public TextMeshProUGUILocalize sideMarkLocalize;

    public int ID;
    public override string BarName
    {
        get
        {
            return "ShopPackageBar";
        }
    }

    public override void OnInit(ShopPackageData shopPackageData)
    {
        ID = shopPackageData.ID;
        packNameLocalize.SetTerm(shopPackageData.ProductNameTerm);

        List<ItemData> list = shopPackageData.GetItemDatas();

        for (int i = 0; i < rewardItemRoots.Length; ++i)
        {
            rewardItemRoots[i].SetActive(false);
            coinRewardNumText.text = string.Empty;
            rewardItemRoots[i].transform.GetChild(0).localPosition = new Vector3(10.6f, 0, 0);
        }

        if (list.Count == 6)
        {
            layoutGroup.spacing = new Vector2(50, 15);
            layoutGroup.transform.localPosition = new Vector3(-144.5f, -24f, 0);
            rewardItemRoots[rewardItemRoots.Length - 1].transform.GetChild(0).localPosition = new Vector3(167, 0, 0);
        }
        else if (list.Count == 5)
        {
            layoutGroup.spacing = new Vector2(50, 35);
            layoutGroup.transform.localPosition = new Vector3(-144.5f, -68f, 0);
        }
        else if (list.Count == 4)
        {
            layoutGroup.spacing = new Vector2(50, 35);
            layoutGroup.transform.localPosition = new Vector3(-144.5f, -68f, 0);
        }
        else
        {
            Log.Warning("Unexpected rewardItemNum");
        }

        for (int i = 0; i < list.Count; ++i)
        {
            if(list[i].type.TotalItemTypeInt <= 1)
            {
                coinRewardNumText.text = list[i].num.ToString();
            }
            else if(list[i].type.TotalItemTypeInt == 18)
            {
                rewardItemRoots[0].SetActive(true);
                if (list[i].num < 60)
                    rewardNumTexts[0].text = list[i].num + "min";
                else
                    rewardNumTexts[0].text = list[i].num/60 + "h";
            }
            else if(list[i].type.TotalItemTypeInt <= 6)
            {
                int index = list[i].type.TotalItemTypeInt - 1;
                rewardItemRoots[index].SetActive(true);
                rewardNumTexts[index].text = "x " + list[i].num.ToString();
            }
            else
            {
                Log.Error($"Unexpected list[i].type.TotalItemTypeInt = {list[i].type.TotalItemTypeInt}");
            }
        }


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
