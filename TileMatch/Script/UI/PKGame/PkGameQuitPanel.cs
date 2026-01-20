using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PkGameQuitPanel : PopupMenuForm
{
   [SerializeField] private DelayButton CloseBtn,QuitBtn,ContinuBtn;
   [SerializeField] private TextMeshProUGUI NumText;

   [SerializeField] private Image BgImage;
   [SerializeField]
   private TextMeshProUGUILocalize Title_Text;
   [SerializeField] private Sprite[] BgSprites;
   [SerializeField] private PkSorce PkSorce;
   private Action quitAction;
   public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
   {
      PkSorce.Init();
      SetBtnEvent();
      SetBgImage();
      base.OnInit(uiGroup, completeAction, userData);
   }

   public void SetData(int num,Action quitAction)
   {
      this.quitAction = quitAction;
      NumText.text = num.ToString();
   }

   private void SetBtnEvent()
   {
      CloseBtn.SetBtnEvent(() =>
      {
         GameManager.UI.HideUIForm(this);
      });
      QuitBtn.SetBtnEvent(() =>
      {
         quitAction?.InvokeSafely();
         GameManager.UI.HideUIForm(this);
      });
      ContinuBtn.SetBtnEvent(() =>
      {
         GameManager.UI.HideUIForm(this);
      });
   }
   
   private List<MaterialPresetName> quitTextMaterials = new List<MaterialPresetName>()
   {
      MaterialPresetName.LevelNormal,
      MaterialPresetName.LevelHard,
      MaterialPresetName.LevelSurpHard,
   };
    
   private void SetBgImage()
   {
      var hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel());
      GameManager.Task.AddDelayTriggerTask(0.1f, () =>
      {
         Title_Text.SetMaterialPreset(quitTextMaterials[hardIndex]);
      });
      BgImage.sprite = BgSprites[hardIndex];
   }
}
