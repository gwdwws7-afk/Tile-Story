using System;

namespace Merge
{
    public class MergeGiveUpMenu : PopupMenuForm
    {
        public DelayButton m_GiveUpButton, m_MergeButton, m_CloseButton, m_TipButton;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_GiveUpButton.OnInit(OnCloseButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);
            m_MergeButton.OnInit(OnMergeButtonClick);
            m_CloseButton.OnInit(OnCloseButtonClick);
        }

        public override void OnReset()
        {
            m_GiveUpButton.onClick.RemoveAllListeners();
            m_MergeButton.onClick.RemoveAllListeners();
            m_CloseButton.onClick.RemoveAllListeners();

            base.OnReset();
        }

        private void OnMergeButtonClick()
        {
            GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
        }

        protected virtual void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
            GameManager.UI.HideUIForm(this);
            MergeManager.Instance.EndActivity();
        }

        private void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }
    }
}
