using System;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 遗迹寻宝最后机会界面
    /// </summary>
    public sealed class HiddenTempleLastChanceMenu : PopupMenuForm
    {
        public Button m_CloseButton, m_PlayButton;
        public ClockBar m_ClockBar;

        private bool isClickPlay;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            isClickPlay = false;

            m_CloseButton.SetBtnEvent(OnCloseButtonClick);
            m_PlayButton.SetBtnEvent(OnPlayButtonClick);

            m_ClockBar.OnReset();
            if (HiddenTempleManager.Instance.CheckActivityHasStarted())
            {
                m_ClockBar.CountdownOver += OnCountdownOver;
                m_ClockBar.StartCountdown(GameManager.Activity.GetCurActivityEndTime());
            }
            else
            {
                m_ClockBar.SetFinishState();
            }
        }

        public override void OnReset()
        {
            m_CloseButton.onClick.RemoveAllListeners();
            m_PlayButton.onClick.RemoveAllListeners();

            m_ClockBar.OnReset();

            base.OnReset();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_ClockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        public override void OnHide(Action hideSuccessAction = null, object userData = null)
        {
            base.OnHide(hideSuccessAction, userData);

            if(!isClickPlay)
                GameManager.Process.EndProcess(ProcessType.ShowHiddenTempleLastChance);
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);
        }

        private void OnPlayButtonClick()
        {
            isClickPlay = true;

            GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm("HiddenTempleMainMenu");
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            m_ClockBar.SetFinishState();
        }
    }
}
