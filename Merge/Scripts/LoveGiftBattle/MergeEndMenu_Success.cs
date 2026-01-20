using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeEndMenu_Success : MergeEndMenu_LoveGiftBattle
    {
        public TextMeshProUGUILocalize m_TopText;
        public TextMeshProUGUILocalize m_BottomText;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            //m_TopText.SetTerm("DateMerge.SuccessInfo1");
            //m_BottomText.SetTerm("DateMerge.SuccessInfo2");
        }

        protected override void OnCloseButtonClick()
        {
            base.OnCloseButtonClick();

            //if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
            //{
            //    GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeLastChanceMenu"));
            //}
            //else
            //{
            //    MergeManager.Instance.EndActivity();
            //}

            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
        }
    }
}
