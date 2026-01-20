using System;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeEndMenu_LoveGiftBattle : PopupMenuForm
    {
        public DelayButton m_CloseButton, m_OkButton, m_TipButton;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.OnInit(OnCloseButtonClick);
            m_OkButton.OnInit(OnOkButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);

            GameManager.DataNode.SetData<bool>("ShowedMergeEndMenu", true);
        }

        public override void OnReset()
        {
            base.OnReset();
        }

        public override void OnRelease()
        {
            base.OnRelease();
        }

        protected virtual void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);
        }

        private void OnOkButtonClick()
        {
            OnCloseButtonClick();
        }

        private void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }
    }
}
