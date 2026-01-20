using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeStartMenu_LoveGiftBattle : PopupMenuForm
    {
        public DelayButton m_CloseButton, m_TipButton, m_StartButton, m_PreviewButton;
        public SkeletonGraphic m_LockSpine;
        public TextMeshProUGUILocalize m_UnlockText, m_StartText, m_PreviewText;
        public ClockBar m_ClockBar;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            bool isPreview = GameManager.PlayerData.NowLevel < MergeManager.PlayerData.GetActivityUnlockLevel();

            m_StartButton.gameObject.SetActive(!isPreview);
            m_PreviewButton.gameObject.SetActive(isPreview);
            m_StartText.gameObject.SetActive(!isPreview);
            m_PreviewText.gameObject.SetActive(isPreview);

            m_CloseButton.OnInit(OnCloseButtonClick);
            m_TipButton.OnInit(OnTipButtonClick);
            m_StartButton.OnInit(OnStartButtonClick);
            m_PreviewButton.OnInit(OnPreviewButtonClick);
            m_StartButton.interactable = true;
            m_CloseButton.interactable = true;
            m_TipButton.interactable = true;

            m_ClockBar.OnReset();
            if (MergeManager.Instance.CheckActivityHasStarted())
            {
                m_ClockBar.CountdownOver += OnCountdownOver;
                m_ClockBar.StartCountdown(MergeManager.Instance.EndTime);
            }
            else
            {
                m_ClockBar.CountdownOver += OnCountdownOver;
                m_ClockBar.StartCountdown(MergeManager.Instance.EndTime);
            }

            if (isPreview)
            {
                m_UnlockText.SetParameterValue("level", MergeManager.PlayerData.GetActivityUnlockLevel().ToString());
            }
        }

        public override void OnReset()
        {
            m_CloseButton.onClick.RemoveAllListeners();
            m_TipButton.onClick.RemoveAllListeners();
            m_StartButton.onClick.RemoveAllListeners();
            m_PreviewButton.onClick.RemoveAllListeners();

            m_ClockBar.OnReset();

            base.OnReset();
        }

        public override void OnRelease()
        {
            base.OnRelease();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            m_ClockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        private void OnCloseButtonClick()
        {
            if (GameManager.PlayerData.NowLevel >= MergeManager.PlayerData.GetActivityUnlockLevel())
                OnStartButtonClick();
            else
                GameManager.UI.HideUIForm(this);
        }

        private void OnTipButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeHowToPlayMenu"));
        }

        private void OnStartButtonClick()
        {
            GameManager.UI.HideUIForm(this);
        }

        private void OnPreviewButtonClick()
        {
            m_LockSpine.freeze = false;
            m_LockSpine.Initialize(false);
            m_LockSpine.AnimationState.SetAnimation(0, "shake_lock", false);
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            m_ClockBar.SetFinishState();
        }
    }
}
