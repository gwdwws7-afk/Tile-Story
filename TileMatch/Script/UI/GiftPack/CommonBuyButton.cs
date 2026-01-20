using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class CommonBuyButton : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUILocalize PriceText;
   [SerializeField] private DelayButton BuyBtn;

   public void Init(ProductNameType type,Action buySuccessAction,Action<PurchaseFailureReason> buyFailAction)
   {
      SetText(type,buySuccessAction,buyFailAction);
   }
   
   private void SetText(ProductNameType productType,Action buySuccessAction,Action<PurchaseFailureReason> buyFailAction)
   {
        string price = GameManager.Purchase.GetPrice(productType);
        if (!string.IsNullOrEmpty(price))
        {
            PriceText.SetTerm(price);

            BuyBtn.SetBtnEvent(() =>
            {
                GameManager.Purchase.BuyProduct(productType, buySuccessAction, buyFailAction);
            });
        }
        else
        {
            PriceText.SetTerm("Shop.Buy");

            BuyBtn.SetBtnEvent(() =>
            {
                GameManager.Purchase.BuyProduct(productType, buySuccessAction, buyFailAction);
            });
        }
    }
}
