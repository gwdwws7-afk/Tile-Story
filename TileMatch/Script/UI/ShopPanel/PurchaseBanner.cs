using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;
using MySelf.Model;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PurchaseBanner : MonoBehaviour
{
    [SerializeField] private GameObject PurchaseBannerText1,PurchaseBannerText2,PurchaseBannerText3;
    [SerializeField] private DelayButton BuyBannerBtn;
    [SerializeField] private TextMeshProUGUILocalize BuyBtnText;
    [SerializeField] private TextMeshProUGUILocalize OtherText;
    [SerializeField] private Image FreeItemImage;

    private AsyncOperationHandle asyncOperationHandle;
    private TotalItemType itemType = 0;
    public void Init(TotalItemType itemType,Action buySuccessAction,Action getRewardSuccessAction)
    {
        this.itemType = itemType;
        ShowPurchaseBanner(itemType,buySuccessAction,getRewardSuccessAction);
    }

    private void OnEnable()
    {
        GameManager.Ads.HideBanner("PurchaseBanner");
    }

    private void OnDisable()
    {
        GameManager.Ads.ShowBanner("PurchaseBanner");
    }

    private void OnDestroy()
    {
        if (asyncOperationHandle.IsValid())
        {
            Addressables.Release(asyncOperationHandle);
            asyncOperationHandle = default;
        }
    }

    public void PauseButton()
    {
        BuyBannerBtn.interactable = false;
    }

    private List<ProductNameType> list;

    private List<ProductNameType> productList
    {
        get
        {
            if (list == null)
            {
                //接关
                if (itemType ==0)
                {
                    list = new List<ProductNameType>()
                    {
                        ProductNameType.LevelContinueFavor1,
                        ProductNameType.LevelContinueFavor2,
                        ProductNameType.LevelContinueFavor3,
                    };
                }
                else
                {
                    list = new List<ProductNameType>()
                    {
                        ProductNameType.PropFavor1,
                        ProductNameType.PropFavor2,
                        ProductNameType.PropFavor3,
                    };
                }
            }
            return list;
        }
    }
    
    private void ShowPurchaseBanner(TotalItemType itemType,Action buySuccessAction,Action getRewardSuccessAction)
    {
        if(this.itemType!=TotalItemType.None)
        asyncOperationHandle=UnityUtility.LoadSpriteAsync(itemType.ToString(), "TotalItemAtlas", s =>
        {
            FreeItemImage.sprite = s;
        });
        
        GameManager.Ads.HideBanner("PurchaseBanner");//需要隐藏banner位
        
        //取当前 posid 对应的礼包 购买的个数
        int haveBuyCount = PurchaseModel.Instance.GetBuyProductCountByPos(itemType);

        ProductNameType productType = productList[Math.Min(haveBuyCount,productList.Count-1)];

        PurchaseBannerText1.gameObject.SetActive(haveBuyCount==0);
        PurchaseBannerText2.gameObject.SetActive(haveBuyCount==1);
        PurchaseBannerText3.gameObject.SetActive(haveBuyCount>=2);

        string price = GameManager.Purchase.GetPrice(productType);
        if (!string.IsNullOrEmpty(price))BuyBtnText.SetTerm(price);

        BuyBannerBtn.SetBtnEvent(() =>
        {
            GameManager.Firebase.RecordMessageByEvent("LevelPurchase_Start",
                new Parameter("Source",(int)itemType),
                new Parameter("PurchaseId",productType.ToString()));
            //执行购买
            //下发奖励
            //然后执行继续 continue
            GameManager.Purchase.BuyProduct(productType, () =>
            {
                PurchaseModel.Instance.RecordProductByPos(itemType,productType);
                
                GameManager.Firebase.RecordMessageByEvent("LevelPurchase_Finish",
                    new Parameter("Source",(int)itemType),
                    new Parameter("PurchaseId",productType.ToString()));
                
                buySuccessAction?.Invoke();
                
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel,true, () =>
                {
                    getRewardSuccessAction?.Invoke();
                });
            });
        });
        BuyBannerBtn.interactable = true;
    }
}
