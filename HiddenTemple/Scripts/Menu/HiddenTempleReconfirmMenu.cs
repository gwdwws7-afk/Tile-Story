using System;
using TMPro;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝二次确认界面
    /// </summary>
    public sealed class HiddenTempleReconfirmMenu : PopupMenuForm
    {
        public Button m_CloseButton, m_PlayButton, m_QuitButton;
        public TextMeshProUGUI m_NumText;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.SetBtnEvent(OnCloseButtonClick);
            m_PlayButton.SetBtnEvent(OnCloseButtonClick);
            m_QuitButton.SetBtnEvent(OnQuitButtonClick);

            m_QuitButton.interactable = true;

            m_NumText.text = "x" + HiddenTempleManager.PlayerData.GetPickaxeNum().ToString();
        }

        public override void OnReset()
        {
            m_CloseButton.onClick.RemoveAllListeners();
            m_PlayButton.onClick.RemoveAllListeners();
            m_QuitButton.onClick.RemoveAllListeners();

            base.OnReset();
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            GameManager.Sound.PlayAudio(SoundType.SFX_UI_close.ToString());
        }

        private void OnQuitButtonClick()
        {
            m_QuitButton.interactable = false;

            GameManager.UI.HideUIForm("HiddenTempleMainMenu");
            GameManager.UI.HideUIForm("HiddenTempleReconfirmMenu");
            HiddenTempleManager.Instance.EndActivity();

            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
        }
    }
}
