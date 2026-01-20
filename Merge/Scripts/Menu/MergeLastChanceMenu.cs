using System;
using TMPro;

namespace Merge
{
    public class MergeLastChanceMenu : PopupMenuForm
    {
        public DelayButton m_MergeButton, m_CloseButton, m_TipButton;
        public TextMeshProUGUI m_NumText;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_MergeButton.OnInit(OnMergeButtonClick);
            m_CloseButton.OnInit(OnCloseButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);

            m_NumText.text = "x " + MergeManager.PlayerData.GetMergeEnergyBoxNum().ToString();

            //GameManager.DataNode.SetData<bool>("ShowedMergeLastChanceMenu", true);
        }

        public override void OnReset()
        {
            m_MergeButton.onClick.RemoveAllListeners();
            m_CloseButton.onClick.RemoveAllListeners();

            base.OnReset();
        }

        private void OnMergeButtonClick()
        {
            GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeGiveUpMenu"));
        }

        private void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }
    }
}
