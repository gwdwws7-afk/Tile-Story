using DG.Tweening;
using GameFramework.Event;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Merge
{
    public class MergeMainMenu_Dog : MergeMainMenu
    {
        //Dog
        public DelayButton m_DogEntrance;
        public GameObject m_DogTipIcon;
        public TextMeshProUGUI m_DogTipIconText;
        public ParticleSystem m_ReachEffect;
        public GameObject m_DogEntanceEffect;
        public CountdownTimer m_DogTimer;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);
            
            GameManager.Event.Subscribe(DogDecorateEndEventArgs.EventId, DogDecorateEnd);

            RefreshDogEntrance();
            RefreshDogBubbleFullEffect();
        }

        public override void OnReset()
        {
            GameManager.Event.Unsubscribe(DogDecorateEndEventArgs.EventId, DogDecorateEnd);

            base.OnReset();
        }

        public override void OnPause()
        {
            m_DogEntrance.interactable = false;

            base.OnPause();
        }

        public override void OnResume()
        {
            m_DogEntrance.interactable = true;

            base.OnResume();
        }

        public override void OnReveal()
        {
            RefreshDogBubbleFullEffect();
            
            base.OnReveal();
        }

        protected override void InitializeButton()
        {
            base.InitializeButton();

            m_DogEntrance.OnInit(OnDogEntranceButtonClick);

            m_DogEntrance.interactable = true;
        }

        public void RefreshDogEntrance()
        {
            int time = GetCanShowDogPropRank(MergeManager.PlayerData.GetCurrentMaxMergeStage() + 1) - MergeManager.PlayerData.GetDogDecorationStage();
            if (time > 0)
            {
                m_DogTipIconText.text = time.ToString();
                m_DogTipIcon.SetActive(true);
            }
            else
            {
                m_DogTipIcon.SetActive(false);
            }
        }
        
        public int GetCanShowDogPropRank(int rank)
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
        
        public void RefreshDogBubbleFullEffect()
        {
            if (CheckDogBubbleFull())
            {
                m_DogEntanceEffect.SetActive(true);
            }
            else
            {
                m_DogEntanceEffect.SetActive(false);
            }
        }

        private bool CheckDogBubbleFull()
        {
            int bubbleMaxNum = MergeManager.Instance.GetMaxBubbleNum();
            if (bubbleMaxNum <= 0)
                return false;

            DateTime fullTime = DateTime.Now;
            for (int i = 0; i < 7; i++)
            {
                DateTime time = MergeManager.PlayerData.GetDogBubbleGetRewardTime(i);
                if (time > fullTime && MergeManager.PlayerData.GetDogBubbleRewardId(i) > 0)  
                {
                    fullTime = time;
                }
            }

            if (DateTime.Now < fullTime)
            {
                m_DogTimer.OnReset();
                m_DogTimer.CountdownOver += (a, b) =>
                {
                    RefreshDogBubbleFullEffect();
                };
                m_DogTimer.StartCountdown(fullTime);
            }
            else
            {
                return true;
            }

            return false;
        }

        public void DogDecorateEnd(object sender, GameEventArgs e)
        {
            RefreshDogEntrance();
        }

        public void OnDogEntranceButtonClick()
        {
            GameManager.UI.ShowUIForm("MergeDogMenu", UIFormType.CenterUI);
        }
    }
}
