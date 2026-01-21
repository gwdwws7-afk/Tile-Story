using DG.Tweening;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Merge
{
    public class MergeMainMenu_Christmas : MergeMainMenu
    {
        //Christmas
        public DelayButton m_ChristmasEntrance;
        public GameObject m_ChristmasTipIcon;
        public TextMeshProUGUI m_ChristmasTipIconText;
        public ParticleSystem m_ReachEffect;
        public GameObject m_ChristmasEntanceEffect;
        public CountdownTimer m_ChristmasTimer;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);
            
            GameManager.Event.Subscribe(ChristmasDecorateEndEventArgs.EventId, ChristmasDecorateEnd);
            
            RefreshChristmasEntrance();
            RefreshChristmasBubbleFullEffect();
        }

        public override void OnReset()
        {
            GameManager.Event.Unsubscribe(ChristmasDecorateEndEventArgs.EventId, ChristmasDecorateEnd);

            base.OnReset();
        }

        public override void OnPause()
        {
            if (m_ChristmasEntrance != null)
            {
                m_ChristmasEntrance.interactable = false;
            }

            base.OnPause();
        }

        public override void OnResume()
        {
            if (m_ChristmasEntrance != null)
            {
                m_ChristmasEntrance.interactable = true;
            }

            base.OnResume();
        }

        public override void OnReveal()
        {
            RefreshChristmasBubbleFullEffect();
            
            base.OnReveal();
        }

        protected override void InitializeButton()
        {
            base.InitializeButton();

            if (m_ChristmasEntrance == null)
            {
                Debug.LogError($"{nameof(MergeMainMenu_Christmas)}: {nameof(m_ChristmasEntrance)} is not assigned.", this);
                return;
            }

            m_ChristmasEntrance.OnInit(OnChristmasEntranceButtonClick);

            m_ChristmasEntrance.interactable = true;
        }

        public void RefreshChristmasEntrance()
        {
            if (m_ChristmasTipIconText == null || m_ChristmasTipIcon == null)
            {
                Debug.LogError($"{nameof(MergeMainMenu_Christmas)}: Christmas tip icon references are not assigned.", this);
                return;
            }

            int time = GetCanShowChristmasPropRank(MergeManager.PlayerData.GetCurrentMaxMergeStage() + 1) - MergeManager.PlayerData.GetChristmasDecorationStage();
            if (time > 0)
            {
                m_ChristmasTipIconText.text = time.ToString();
                m_ChristmasTipIcon.SetActive(true);
            }
            else
            {
                m_ChristmasTipIcon.SetActive(false);
            }
        }
        
        public int GetCanShowChristmasPropRank(int rank)
        {
            switch (rank)
            {
                case 4:
                    return 1;
                case 6:
                    return 2;
                case 8:
                    return 3;
                case 10:
                    return 4;
                case 11:
                    return 5;
                case 12:
                    return 6;
            }

            return 0;
        }
        
        public void RefreshChristmasBubbleFullEffect()
        {
            if (m_ChristmasEntanceEffect == null || m_ChristmasTimer == null)
            {
                Debug.LogError($"{nameof(MergeMainMenu_Christmas)}: Christmas entrance effect or timer is not assigned.", this);
                return;
            }

            if (CheckChristmasBubbleFull())
            {
                m_ChristmasEntanceEffect.SetActive(true);
            }
            else
            {
                m_ChristmasEntanceEffect.SetActive(false);
            }
        }

        private bool CheckChristmasBubbleFull()
        {
            int bubbleMaxNum = MergeManager.Instance.GetMaxBubbleNum();
            if (bubbleMaxNum <= 0)
                return false;

            DateTime fullTime = DateTime.Now;
            for (int i = 0; i < 7; i++)
            {
                DateTime time = MergeManager.PlayerData.GetChristmasBubbleGetRewardTime(i);
                if (time > fullTime && MergeManager.PlayerData.GetChristmasBubbleRewardId(i) > 0)  
                {
                    fullTime = time;
                }
            }

            if (DateTime.Now < fullTime)
            {
                m_ChristmasTimer.OnReset();
                m_ChristmasTimer.CountdownOver += (a, b) =>
                {
                    RefreshChristmasBubbleFullEffect();
                };
                m_ChristmasTimer.StartCountdown(fullTime);
            }
            else
            {
                return true;
            }

            return false;
        }

        public void ChristmasDecorateEnd(object sender, GameEventArgs e)
        {
            RefreshChristmasEntrance();
        }

        public void OnChristmasEntranceButtonClick()
        {
            GameManager.UI.ShowUIForm("MergeChristmasMenu", UIFormType.CenterUI);
        }
    }
}
