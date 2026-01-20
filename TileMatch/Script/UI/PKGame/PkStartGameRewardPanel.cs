using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MySelf.Model;
using TMPro;
using UnityEngine;

public class PkStartGameRewardPanel : PopupMenuForm
{
   [SerializeField] private TextMeshProUGUILocalize[] TittleTexts;
   [SerializeField] private TextMeshProUGUI NumText;
   [SerializeField] private RectTransform RootTransform;

   public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
   {
      foreach (var text in TittleTexts)
      {
         text.gameObject.SetActive(false);
      }
      int rewardNum = PkGameModel.Instance.PkRewardItemNum(GameManager.PlayerData.NowLevel);
      int failCount = PkGameModel.Instance.GetFailCountByLevel();
      switch (failCount)
      {
         case 0:
            TittleTexts[0].gameObject.SetActive(true);
            break;
         case 1:
            TittleTexts[1].gameObject.SetActive(true);
             break;
         case 2:
            TittleTexts[2].gameObject.SetActive(true);
            break;
         default:
            TittleTexts[3].gameObject.SetActive(true);
            string levelStr = (failCount+1).ToString();
            TittleTexts[3].SetParameterValue("{1}",levelStr);
            break;
      }
      NumText.text = rewardNum.ToString();
      RootTransform.anchoredPosition = Vector2.zero;
      Anim();
      base.OnInit(uiGroup, completeAction, userData);
   }

   private void Anim()
   {
      var seq = DOTween.Sequence();
      seq.Append(RootTransform.DOAnchorPosY(-520, 0.4f).SetEase(Ease.OutQuart));
      seq.Append(RootTransform.DOAnchorPosY(0, 0.4f).SetDelay(2f));
      seq.AppendCallback(() =>
      {
         GameManager.UI.HideUIForm(this);
      });
   }
}
