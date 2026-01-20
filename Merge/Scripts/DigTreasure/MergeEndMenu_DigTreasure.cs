using System;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeEndMenu_DigTreasure : PopupMenuForm
    {
        public DelayButton m_CloseButton, m_OkButton;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.OnInit(OnCloseButtonClick);
            m_OkButton.OnInit(OnOkButtonClick);

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

        protected void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            if (MergeManager.PlayerData.GetMergeEnergyBoxNum() > 0)
            {
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeLastChanceMenu"));
            }
            else
            {
                MergeManager.Instance.EndActivity();
            }
        }

        private void OnOkButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"), form =>
            {
                GameManager.UI.HideUIForm(this);
            });
        }
    }
}
