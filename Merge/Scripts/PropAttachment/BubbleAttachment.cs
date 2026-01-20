using DG.Tweening;
using GameFramework.Event;
using System;
using TMPro;
using UnityEngine;

namespace Merge
{
    public class BubbleAttachment : PropAttachment
    {
        public Canvas m_Canvas;
        public GameObject m_AdsTip;
        public TextMeshProUGUI m_TimeText;

        private void OnEnable()
        {
            GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoadComplete);
            GameManager.Event.Subscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarnedReward);
            GameManager.Event.Subscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);
        }

        private void OnDisable()
        {
            GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId, OnRewardAdLoadComplete);
            GameManager.Event.Unsubscribe(RewardAdEarnedRewardEventArgs.EventId, OnRewardAdEarnedReward);
            GameManager.Event.Unsubscribe(RefreshMergeEnergyBoxEventArgs.EventId, OnRefreshMergeEnergyBox);
        }

        public override void Initialize(PropLogic prop)
        {
            base.Initialize(prop);

            Refresh();
        }

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_Canvas.sortingLayerName = layerName;
            m_Canvas.sortingOrder = sortOrder + 1;
        }

        public override void OnSelected()
        {
            base.OnSelected();

            float m_BodyScale = 1f;
            transform.DOScale(m_BodyScale * 0.8f, 0.2f).onComplete = () =>
            {
                transform.DOScale(m_BodyScale * 1.1f, 0.15f).onComplete = () =>
                {
                    transform.DOScale(m_BodyScale * 0.95f, 0.15f).onComplete = () =>
                    {
                        transform.DOScale(m_BodyScale * 1.05f, 0.15f).onComplete = () =>
                        {
                            transform.DOScale(m_BodyScale, 0.15f);
                        };
                    };
                };
            };

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null && Prop != null) 
            {
                mainBoard.ShowBubbleOperationBox(Prop.PropId, Prop);
            }
        }

        public void Refresh()
        {
            if (CanShowBubbleAdsButton())
            {
                m_AdsTip.SetActive(true);
                m_TimeText.transform.localPosition = new Vector3(20.9f, m_TimeText.transform.localPosition.y, 0);
            }
            else
            {
                m_AdsTip.SetActive(false);
                m_TimeText.transform.localPosition = new Vector3(0, m_TimeText.transform.localPosition.y, 0);
            }
        }

        private bool CanShowBubbleAdsButton()
        {
            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null && Prop != null) 
            {
                return mainBoard.CanShowBubbleAdsButton(Prop.PropId);
            }

            return false;
        }

        private void OnRewardAdLoadComplete(object sender, GameEventArgs e)
        {
            Refresh();
        }

        private void OnRewardAdEarnedReward(object sender, GameEventArgs e)
        {
            Refresh();
        }

        private void OnRefreshMergeEnergyBox(object sender, GameEventArgs e)
        {
            Refresh();
        }
    }
}
