using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Merge
{
    public class MergeDigCelebrateMenu : MonoBehaviour
    {
        public TextMeshProUGUILocalize m_CelebrationText;
        public TextMeshProUGUILocalize m_ReachedText;
        public SkeletonAnimation m_CelebrateEffect;
        public CanvasGroup m_CanvasGroup;

        private bool m_CanSkipAnim;
        private int m_HideEventId;

        public Action m_HideAction;

        public void Show(int depth)
        {
            m_CanSkipAnim = false;

            string celebrateKey = "DigMerge.Congratulation";
            if (depth >= 1000 && depth < 2000)
                celebrateKey = "DigMerge.Congratulation1";
            else if (depth >= 2000 && depth < 3000)
                celebrateKey = "DigMerge.Congratulation2";
            else if (depth >= 3000 && depth < 4000)
                celebrateKey = "DigMerge.Congratulation3";
            else if (depth >= 4000 && depth < 5000)
                celebrateKey = "DigMerge.Congratulation4";
            else
                celebrateKey += UnityEngine.Random.Range(5, 15).ToString();

            MergeManager.PlayerData.SetDigTreasureStageDepth(depth / 1000 * 1000);

            m_CelebrationText.SetTerm(celebrateKey);
            m_ReachedText.SetParameterValue("depth", depth.ToString());
            m_CelebrateEffect.Initialize(false);

            m_CelebrationText.transform.localScale = Vector3.zero;
            m_ReachedText.transform.localScale = Vector3.zero;
            m_CanvasGroup.alpha = 1;
            gameObject.SetActive(true);

            GameManager.Task.AddDelayTriggerTask(0.7f, () =>
            {
                m_CanSkipAnim = true;
            });

            m_HideEventId = GameManager.Task.AddDelayTriggerTask(2.3f, Hide);

            m_CelebrationText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).onComplete = () =>
            {
                m_CelebrateEffect.AnimationState.SetAnimation(0, "active", false);
                m_ReachedText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).onComplete = () =>
                {
                    m_CanvasGroup.DOFade(0, 0.2f).SetDelay(1.3f);
                };
            };

            GameManager.Sound.PlayAudio(SoundType.SFX_DigTreasure_Progress_Celebration.ToString());
        }

        private void Hide()
        {
            gameObject.SetActive(false);

            m_HideAction?.Invoke();
            m_HideAction = null;
        }

        private void Update()
        {
            if (m_CanSkipAnim)
            {
                if((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
                {
                    m_CanSkipAnim = false;
                    GameManager.Task.RemoveDelayTriggerTask(m_HideEventId);
                    m_CelebrationText.DOKill();
                    m_ReachedText.DOKill();
                    m_CanvasGroup.DOKill();
                    Hide();
                }
            }
        }
    }
}
