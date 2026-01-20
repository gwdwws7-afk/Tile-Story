using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    /// <summary>
    /// ½ø¶ÈÃæ°å
    /// </summary>
    public class ProgressBoard : MonoBehaviour
    {
        public ScrollRect m_ScrollRect;
        public Slider m_Slider;
        public SkeletonGraphic m_ShineSpine;
        public Transform[] m_Stages;

        private int m_CurMaxStage = 0;
        private bool m_IsInitialized = false;

        public void Initialize()
        {
            if (m_IsInitialized)
                return;
            m_IsInitialized = true;
        }

        public void RefreshProgress(bool skipAnim)
        {
            if (!m_IsInitialized)
                Initialize();

            int totalNum = m_Stages.Length;
            int curMaxStage = MergeManager.PlayerData.GetCurrentMaxMergeStage();
            float endValue = curMaxStage / (float)(totalNum - 1);
            float scrollValue = (curMaxStage - 1) / (float)(totalNum - 1);

            int lastMaxStage = m_CurMaxStage;
            m_CurMaxStage = curMaxStage;
            if (lastMaxStage < curMaxStage && !skipAnim)
            {
                m_Slider.DOValue(endValue, 0.5f).onComplete = () =>
                {
                    for (int i = lastMaxStage + 1; i <= curMaxStage; i++)
                    {
                        int index = i;
                        Vector3 originalScale = m_Stages[index].localScale;
                        m_Stages[index].localScale = Vector3.zero;
                        m_Stages[index].DOScale(0f, 0.09f).SetEase(Ease.InQuad).onComplete = () =>
                        {
                            m_Stages[index].GetChild(0).gameObject.SetActive(false);
                            m_Stages[index].DOScale(1.15f * originalScale, 0.11f).onComplete = () =>
                             {
                                 m_Stages[index].DOScale(originalScale, 0.1f).SetEase(Ease.InQuad);
                             };
                        };

                        m_ShineSpine.transform.position = m_Stages[index].transform.position;
                        m_ShineSpine.gameObject.SetActive(true);
                        m_ShineSpine.AnimationState.SetAnimation(0, "01", false).Complete += t =>
                        {
                            m_ShineSpine.gameObject.SetActive(false);
                        };
                    }
                };
                RefreshScrollPosition(scrollValue, true);
            }
            else
            {
                for (int i = 0; i < m_Stages.Length; i++)
                {
                    if (i <= curMaxStage)
                    {
                        m_Stages[i].GetChild(0).gameObject.SetActive(false);
                    }
                    else
                    {
                        m_Stages[i].GetChild(0).gameObject.SetActive(true);
                    }
                }

                m_Slider.value = endValue;
                RefreshScrollPosition(scrollValue, false);
            }
        }

        private void RefreshScrollPosition(float endValue, bool isLerp)
        {
            if (endValue <= 0.2f)
                endValue = 0f;
            if (endValue >= 0.7f)
                endValue = 1f;

            if (isLerp)
            {
                if (endValue - m_ScrollRect.horizontalNormalizedPosition > 0.1f || endValue - m_ScrollRect.horizontalNormalizedPosition < -0.1f || endValue == 1)
                    m_ScrollRect.horizontalNormalizedPosition = endValue;
            }
            else
            {
                m_ScrollRect.horizontalNormalizedPosition = endValue;
            }
        }

        public Vector3 GetStagePos(int stage)
        {
            return stage < m_Stages.Length ? m_Stages[stage].transform.position : m_Stages[m_Stages.Length - 1].transform.position;
        }
    }
}
