using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CommonRulesPanel : PopupMenuForm
    {
        public GameObject[] guides;

        private bool isAnimFinish;

        public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
        {
            base.OnInit(uiGroup, initCompleteAction, userData);
        }

        public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
        {
            for (int i = 0; i < guides.Length; i++)
            {
                guides[i].transform.localScale = Vector3.zero;
            }
            base.OnShowInit(showInitSuccessAction, userData);
        }

        public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
        {
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
                            isAnimFinish = true;
                        };
                    }
                    else
                    {
                        guides[index].transform.DOScale(1f, fadeTime);
                    }
                };
            }
        }

        public override void OnHide(Action hideSuccessAction = null, object userData = null)
        {
            gameObject.SetActive(false);
            OnReset();
            GameManager.Sound.PlayUICloseSound();
        }

        public override void OnReset()
        {
            isAnimFinish = false;
            base.OnReset();
        }

        public override void OnClose()
        {
            if (!isAnimFinish)
            {
                return;
            }
            GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.PersonRankRuleHide));
            GameManager.UI.HideUIForm(this);
            base.OnClose();
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
#if UNITY_EDITOR||UNITY_STANDALONE
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