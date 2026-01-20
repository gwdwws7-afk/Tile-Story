using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public sealed class MergeWeakHintMenu : UIForm
    {
        public RectTransform root;
        public TextMeshProUGUI weakHintText;
        public CanvasGroup canvasGroup;

        private Vector3 newStartPos = Vector3.zero;

        private RawImage rawImage;

        private RawImage RawImage
        {
            get
            {
                if (rawImage == null)
                    rawImage = root.GetComponent<RawImage>();
                return rawImage;
            }
        }

        public override void OnReset()
        {
            RawImage.enabled = true;
            root.gameObject.SetActive(false);
            canvasGroup.DOKill();
            root.anchoredPosition = newStartPos;

            base.OnReset();
        }

        public void SetHintText(string content, Vector3 startPos, params string[] args)
        {
            TextMeshProUGUILocalize textMeshProUGUILocalize = weakHintText.GetComponent<TextMeshProUGUILocalize>();
            textMeshProUGUILocalize.SetTerm(content);

            newStartPos = new Vector3(0, startPos.y);
            root.anchoredPosition = startPos;
            canvasGroup.DOKill();

            for (int i = 0; i < args.Length; i++)
            {
                textMeshProUGUILocalize.SetParameterValue("{" + i + "}", args[i]);
            }
        }

        public override void OnRelease()
        {
            base.OnRelease();
        }

        public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
        {
            OnReset();

            canvasGroup.alpha = 0;
            root.gameObject.SetActive(true);

            canvasGroup.DOFade(1, 0.3f).onComplete = () =>
            {
                canvasGroup.DOFade(0, 0.3f).SetDelay(0.5f).onComplete = () =>
                {
                    GameManager.UI.HideUIForm(this);
                };
            };

            base.OnShow(showSuccessAction, userData);
        }

        public override void OnHide(Action hideSuccessAction = null, object userData = null)
        {
            OnReset();
            root.gameObject.SetActive(false);

            base.OnHide(hideSuccessAction, userData);
        }

        public void DisableBg()
        {
            RawImage.enabled = false;
        }
    }
}
