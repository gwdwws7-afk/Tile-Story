using DG.Tweening;
using System;
using UnityEngine;

namespace Merge
{
    public class MergeHowToPlayMenu : UIForm
    {
        public GameObject[] guides;

        private bool m_IsAnimFinish;

        public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
        {
            base.OnInit(uiGroup, initCompleteAction, userData);

            for (int i = 0; i < guides.Length; i++)
            {
                guides[i].transform.localScale = Vector3.zero;
            }
        }

        public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
        {
            for (int i = 0; i < guides.Length; i++)
            {
                guides[i].transform.localScale = Vector3.zero;
            }

            gameObject.SetActive(true);
            GameManager.Sound.PlayUIOpenSound();
            float delayTime = -0.2f;
            for (int i = 0; i < guides.Length; i++)
            {
                var index = i;
                const float showTime = 0.25f;
                const float fadeTime = 0.25f;
                delayTime += 0.2f;
                guides[i].transform.DOScale(1.1f, showTime).SetDelay(delayTime).onComplete = () =>
                {
                    if (index == guides.Length - 1)
                    {
                        guides[index].transform.DOScale(1f, fadeTime).onComplete = () =>
                        {
                            m_IsAnimFinish = true;
                        };
                    }
                    else
                    {
                        guides[index].transform.DOScale(1f, fadeTime);
                    }
                };
            }

            m_IsAvailable = true;
            showSuccessAction?.Invoke(this);
        }

        public override void OnHide(Action hideSuccessAction = null, object userData = null)
        {
            gameObject.SetActive(false);
            OnReset();

            base.OnHide();
        }

        public override void OnReset()
        {
            m_IsAnimFinish = false;
            base.OnReset();
        }

        public override void OnClose()
        {
            if (!m_IsAnimFinish)
            {
                return;
            }

            if (m_OnHideCompleteAction != null && GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) == null)
            {
                return;
            }

            GameManager.UI.HideUIForm(this);
            base.OnClose();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonUp(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
            {
                OnClose();
            }
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }
}
