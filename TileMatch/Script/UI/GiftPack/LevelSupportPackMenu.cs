using DG.Tweening;
using MySelf.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelSupportPackMenu : PopupMenuForm
{
    [SerializeField]
    private GameObject mainBody;
    [SerializeField]
    private DelayButton buyButton, closeButton;
    [SerializeField]
    private TextMeshProUGUI priceText;
    [SerializeField]
    private TextMeshProUGUILocalize discountText;
    [SerializeField]
    private GameObject getPriceRoot, noPriceRoot;
    [SerializeField]
    private List<GameObject> props;

    private ProductNameType productType;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        buyButton.OnInit(OnBuyButtonClick);
        closeButton.OnInit(OnCloseButtonClick);

        //弹出礼包前，先检测玩家道具库存量，当前拥有数量最少的道具类型将展示在所推送的礼包中
        int curNum = int.MaxValue;
        List<TotalItemType> list = new List<TotalItemType>();
        for (int i = 0; i < 5; i++)
        {
            TotalItemType type = (TotalItemType)(i + 2);
            int num = ItemModel.Instance.GetItemTotalNum(type);
            if (num < curNum)
            {
                curNum = num;
                list.Clear();
                list.Add(type);
            }
            else if (num == curNum)
            {
                list.Add(type);
            }
        }

        TotalItemType targetType = TotalItemType.Prop_Back;
        if (list.Count > 0)
        {
            targetType = list[UnityEngine.Random.Range(0, list.Count)];
        }

        for (int i = 0; i < props.Count; i++)
        {
            if (i == (int)targetType - 2)
                props[i].SetActive(true);
            else
                props[i].SetActive(false);
        }

        switch (targetType)
        {
            case TotalItemType.Prop_Back:
                productType = ProductNameType.Hard_Level_Package_1;
                discountText.SetParameterValue("Rate", "500%");
                break;
            case TotalItemType.Prop_ChangePos:
                productType = ProductNameType.Hard_Level_Package_2;
                discountText.SetParameterValue("Rate", "600%");
                break;
            case TotalItemType.Prop_Absorb:
                productType = ProductNameType.Hard_Level_Package_3;
                discountText.SetParameterValue("Rate", "500%");
                break;
            case TotalItemType.Prop_AddOneStep:
                productType = ProductNameType.Hard_Level_Package_4;
                discountText.SetParameterValue("Rate", "600%");
                break;
            case TotalItemType.Prop_Grab:
                productType = ProductNameType.Hard_Level_Package_5;
                discountText.SetParameterValue("Rate", "600%");
                break;
            default:
                productType = ProductNameType.Hard_Level_Package_1;
                break;
        }

        SetPrice(productType, priceText);
    }

    public override void OnReset()
    {
        buyButton.OnReset();
        closeButton.OnReset();

        base.OnReset();
    }

    //public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    //{
    //    mainBody.SetActive(true);
    //    Transform trans = mainBody.transform;
    //    trans.DOKill();
    //    trans.localScale = Vector3.one;
    //    trans.DOScale(1.03f, 0.12f).onComplete = () =>
    //    {
    //        trans.DOScale(0.99f, 0.1f).onComplete = () =>
    //        {
    //            trans.DOScale(1f, 0.1f);
    //            m_IsAvailable = true;
    //        };
    //    };

    //    GameManager.Sound.PlayUIOpenSound();

    //    if (!gameObject.activeSelf)
    //        gameObject.SetActive(true);

    //    showSuccessAction?.Invoke(this);
    //}

    private void SetPrice(ProductNameType productType, TextMeshProUGUI priceText)
    {
        string price = GameManager.Purchase.GetPrice(productType);
        bool priceIsGet = false;
        if (!string.IsNullOrEmpty(price))
        {
            priceIsGet = true;
            priceText.text = price;
        }

        getPriceRoot.SetActive(priceIsGet);
        noPriceRoot.SetActive(!priceIsGet);
    }

    private void OnBuyButtonClick()
    {
        GameManager.Purchase.BuyProduct(productType, () =>
        {
            Action hideCompleteAction = m_OnHideCompleteAction;
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                hideCompleteAction?.Invoke();
            });

            PlayerPrefs.SetInt("LevelSupportPackBuyedLevel", GameManager.PlayerData.NowLevel);
        });
    }

    private void OnCloseButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }
}
