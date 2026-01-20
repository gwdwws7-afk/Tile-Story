using System;
using DG.Tweening;
using GameFramework.Event;
using MySelf.Model;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class PiggyBankEntrance : UIForm
{
   [SerializeField] private DelayButton EnterBtn;
   [SerializeField] private TextMeshProUGUI CoinNumText;
   [SerializeField] private GameObject RedPointObj,FullTextObj;
   [SerializeField] private SkeletonGraphic SkeletonGraphic;
   
   [SerializeField] private Transform MainImageTransform;
   [SerializeField] private ParticleSystem ReachEffect;


   public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
   {
      bool IsCanShow = PiggyBankModel.Instance.IsCanShow;
      gameObject.SetActive(IsCanShow);
      if (!IsCanShow) return;

      GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonHandle);
      
      base.OnInit(uiGroup, completeAction, userData);

      ShowIconImage();
      Init();
      SetBtnEvent();
   }

   public override void OnRelease()
   {
      GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonHandle);
      base.OnRelease();
   }

   private void Init()
   {
      bool isFull = PiggyBankModel.Instance.IsPiggyBankFull;
      FullTextObj.gameObject.SetActive(isFull);
      CoinNumText.gameObject.SetActive(!isFull);
      CoinNumText.text =PiggyBankModel.Instance.Data.PigTotalCoins.ToString();
      RedPointObj.gameObject.SetActive(isFull);
   }

   private void ShowIconImage()
   {
      //计算出当前所处状态
      int status = PiggyBankModel.Instance.GetBankStatus();
      string animName = $"idle{status}";
      
      SkeletonGraphic.AnimationState.SetAnimation(0,animName,true);
      // Sprite lastSprite = IconImage.sprite;
      //
      // if(lastSprite!=null&&lastSprite.name==imageName)return;
      // if(lastSprite==null)IconImage.color = new Color(1, 1, 1, 0);
      // LoadAssetAsync<Sprite>(imageName, s =>
      // {
      //    IconImage.DOFade(0, 0.1f).onComplete += () =>
      //    {
      //       IconImage.sprite=s as Sprite;
      //       IconImage.DOFade(1, 0.1f);
      //    };
      // });
   }

   private void SetBtnEvent()
   {
      EnterBtn.SetBtnEvent(() =>
      {
         //点击展示界面
         GameManager.UI.ShowUIForm("PiggyBankMenu");
      });
   }
   
   public void CommonHandle(object sender, GameEventArgs e)
   {
      CommonEventArgs ne = (CommonEventArgs)e;
      switch (ne.Type)
      {
         case CommonEventType.RefreshPiggyBank:
            ShowIconImage();
            Init();
            break;
      }
   }
   
   public void ShowGetFlyReward()
   {
      if (PiggyBankModel.Instance.IsPiggyBankFull) return;
      int num = PiggyBankModel.Instance.Data.AddCoinByCurLevel;
      string key = "Coin";
      EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent($"TotalItemAtlas[{key}]", num, 21, new Vector3(-250f, 0), Vector3.zero, gameObject,
         () =>
         {
            PiggyBankModel.Instance.RecordAddCoinToTotalCoin();
         }, () =>
         {
            MainImageTransform.DOScale(0.85f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
            {
               ShowIconImage();
               Init();
               
               MainImageTransform.DOScale(1f, 0.15f);
               EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
            };
          
            if (ReachEffect != null)
            {
               ReachEffect.Play();
            }
         }, false);
   }
}
