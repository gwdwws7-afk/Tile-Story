using System;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeCompleteMenu_DigTreasure : PopupMenuForm
    {
        public DelayButton m_CloseButton, m_OkButton;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.OnInit(OnCloseButtonClick);
            m_OkButton.OnInit(OnOkButtonClick);
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
    }
}
