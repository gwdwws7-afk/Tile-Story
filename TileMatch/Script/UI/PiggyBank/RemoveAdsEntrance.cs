using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class RemoveAdsEntrance : UIForm
{
   [SerializeField] private CountdownTimer CountdownTimer;
   [SerializeField] private Button RemoveAds_btn;
   [SerializeField] private SkeletonGraphic AnimSpine;

   public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
   {
      base.OnInit(uiGroup, completeAction, userData);

      Init();

      if(!gameObject.activeSelf)return;
      GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonHandle);

        if (SystemInfoManager.IsSuperLowMemorySize)
        {
            var track = AnimSpine.AnimationState.SetAnimation(0, "idle", false);
            track.AnimationStart = 1f;
            AnimSpine.freeze = true;
        }
   }

   public override void OnRelease()
   {
      GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonHandle);
      base.OnRelease();
   }

   public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
   {
      
   }

   private void Init()
   {
      bool isRemoveAds = AdsModel.Instance.Data.IsRemoveAds;
      DateTime endDateTime = AdsModel.Instance.EndDateTime;

      bool isShow = !isRemoveAds && endDateTime > DateTime.Now;
      gameObject.SetActive(isShow);
      if (isShow)
      {
         CountdownTimer.OnReset();
         //CountdownTimer.timeText.gameObject.SetActive(true);
         CountdownTimer.StartCountdown(endDateTime);
         CountdownTimer.CountdownOver += (a,b) =>
         {
            Init();
         };
      }else return;

      PreloadGiftPackEntranceUI();
      RemoveAds_btn.SetBtnEvent(() =>
      {
         if (GameManager.Ads.IsRemovePopupAds)
          {
              RemoveAds_btn.gameObject.SetActive(false);
              return;
          }

          RemoveAds_btn.interactable = false;
         //直接拉起支付界面
         GameManager.Purchase.BuyProduct(ProductNameType.Remove_Ads_New_Pro, () =>
         {
            GameManager.UI.ShowUIForm("RemovePopupAdsBuySuccessMenu",uiForm =>
            {
            });
         }, reason =>
         {
            if (!GameManager.Ads.IsRemovePopupAds)
               RemoveAds_btn.gameObject.SetActive(true);
             RemoveAds_btn.interactable = true;
         });
      });
        RemoveAds_btn.interactable = true;
   }
   
   private void PreloadGiftPackEntranceUI()
   {
      if (GameManager.Ads.IsRemovePopupAds)
      {
         RemoveAds_btn.gameObject.SetActive(false);
         return;
      }

      if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockRemoveAdsButtonLevel)
      {
         RemoveAds_btn.gameObject.SetActive(true);
         return;
      }

      RemoveAds_btn.gameObject.SetActive(false);
   }
   
   public void CommonHandle(object sender, GameEventArgs e)
   {
      if (GameManager.Ads.IsRemovePopupAds)
      {
         RemoveAds_btn.gameObject.SetActive(false);
      }
   }
}
