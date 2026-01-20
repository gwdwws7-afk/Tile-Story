using System;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// “≈º£—∞±¶Ω· ¯ΩÁ√Ê
    /// </summary>
    public sealed class HiddenTempleEndMenu : PopupMenuForm
    {
        public Button m_CloseButton;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            m_CloseButton.SetBtnEvent(OnCloseButtonClick);
        }

        public override void OnReset()
        {
            m_CloseButton.onClick.RemoveAllListeners();

            base.OnReset();
        }

        public override void OnHide(Action hideSuccessAction = null, object userData = null)
        {
            GameManager.Task.AddDelayTriggerTask(0, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowHiddenTempleEndProcess);
            });

            base.OnHide(hideSuccessAction, userData);
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            GameManager.Sound.PlayAudio(SoundType.SFX_UI_close.ToString());
        }
    }
}
