using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonGiftMenu : PopupMenuForm
{
   [SerializeField] private ProductNameType[] ProductNameTypes;
   [SerializeField] private CommonBuyButton[] BuyButtons;
   [SerializeField] private DelayButton CloseBtn;
   [SerializeField] private ClockBar ClockBar;

   private DateTime endDateTime=DateTime.MaxValue;
   public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
   {
      if (ProductNameTypes == null || BuyButtons == null || ProductNameTypes.Length != BuyButtons.Length)
      {
         Log.Error($"商品信息不全，或者购买商品按钮信息不全！");
         return;
      }

      if (userData!=null)
      {
         endDateTime = (DateTime)userData;
      }

      BtnEvent();
      SetClockBar();
      base.OnInit(uiGroup, completeAction, userData);
   }

   public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
   {
      if(ClockBar)ClockBar.OnUpdate(elapseSeconds,realElapseSeconds);
      base.OnUpdate(elapseSeconds, realElapseSeconds);
   }

   private void BtnEvent()
   {
      CloseBtn.SetBtnEvent(()=>GameManager.UI.HideUIForm(this));
      for (int i = 0; i < BuyButtons.Length; i++)
      {
         BuyButtons[i].Init(ProductNameTypes[i], () =>
         {
            GameManager.UI.HideUIForm(this);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, null);
         }, (fail) =>
         {
            Log.Error($"Buy Fail:{fail.ToString()}");
         });
      }
   }

   private void SetClockBar()
   {
        if (ClockBar && endDateTime != DateTime.MaxValue)
        {
            ClockBar.StartCountdown(endDateTime);
            ClockBar.CountdownOver -= SetCountdownOver;
            ClockBar.CountdownOver += SetCountdownOver;
        }
        else
        {
            if (ClockBar) ClockBar.gameObject.SetActive(false);
        }
    }

    private void SetCountdownOver(object a, CountdownOverEventArgs b)
    {
        GameManager.UI.HideUIForm(this);
    }
}
