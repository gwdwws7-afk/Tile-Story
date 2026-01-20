using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WeakHintMenu : UIForm
{
    public RectTransform root;
    public TextMeshProUGUI weakHintText;

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
        weakHintText.DOKill();
        root.DOKill();
        weakHintText.color = Color.white;
        root.anchoredPosition = newStartPos;

        base.OnReset();
    }

    public void SetHintText(string content, Vector3 startPos, params string[] args)
    {
        TextMeshProUGUILocalize textMeshProUGUILocalize = weakHintText.GetComponent<TextMeshProUGUILocalize>();
        textMeshProUGUILocalize.SetTerm(content);

        newStartPos = new Vector3(0, startPos.y);
        root.anchoredPosition = startPos;

        for (int i = 0; i < args.Length; i++)
        {
            textMeshProUGUILocalize.SetParameterValue("{" + i + "}", args[i]);
        }
    }

    public override void OnRelease()
    {
        root.DOKill(true);
        base.OnRelease();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        OnReset();
        root.localScale = Vector3.zero;

        root.gameObject.SetActive(true);

        root.DOScale(1.1f, 0.15f).onComplete = () =>
        {
            root.DOScale(1f, 0.15f);

            var targetPos = root.anchoredPosition;
            root.DOAnchorPos(targetPos + new Vector2(0, 80f), 1.2f).onComplete = () =>
            {
                GameManager.UI.HideUIForm(this);
            };
            weakHintText.DOFade(0, 0.5f).SetDelay(0.9f);
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
