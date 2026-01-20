using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeStartMenu : PopupMenuForm
    {
        public Button m_CloseButton, m_OkButton, m_LockButton, m_InfoButton;
        public ClockBar m_ClockBar;
        public Transform lockImg;
        public TextMeshProUGUILocalize unlockText;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            bool isUnlock = GameManager.PlayerData.NowLevel >= MergeManager.PlayerData.GetActivityUnlockLevel();

            m_CloseButton.SetBtnEvent(OnCloseButtonClick);
            m_OkButton.SetBtnEvent(OnOkButtonClick);
            if (m_LockButton != null) m_LockButton.SetBtnEvent(OnLockButtonClick);
            if (m_InfoButton != null) m_InfoButton.SetBtnEvent(OnInfoButtonClick);

            m_OkButton.gameObject.SetActive(isUnlock);
            if (m_LockButton != null) m_LockButton.gameObject.SetActive(!isUnlock);

            if (m_ClockBar != null)
            {
                m_ClockBar.OnReset();
                m_ClockBar.CountdownOver += (sender, args) => { GameManager.UI.HideUIForm(this); };
                m_ClockBar.StartCountdown(Merge.MergeManager.Instance.EndTime);
            }

            if (unlockText != null)
            {
                unlockText.SetParameterValue("0", MergeManager.PlayerData.GetActivityUnlockLevel().ToString());
            }
        }

        public override void OnRelease()
        {
            m_CloseButton.onClick.RemoveAllListeners();
            m_OkButton.onClick.RemoveAllListeners();
            if (m_LockButton != null) m_LockButton.onClick.RemoveAllListeners();
            if (m_InfoButton != null) m_InfoButton.onClick.RemoveAllListeners();

            if (m_ClockBar != null)
            {
                m_ClockBar.OnReset();
            }

            base.OnRelease();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (m_ClockBar != null)
            {
                m_ClockBar.OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);
        }

        private void OnOkButtonClick()
        {
            OnCloseButtonClick();
        }

        private void OnLockButtonClick()
        {
            lockImg.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
        }

        private void OnInfoButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }
    }
}
