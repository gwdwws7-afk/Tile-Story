using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

/// <summary>
/// 延时按钮
/// </summary>
public sealed class DelayButton : Button
{
    public float delayTime = 0.05f;
    public Transform body;
    public SoundType SoundType = SoundType.None;

    public BtnAnimType BtnAnimType=BtnAnimType.Small;

    protected override void Start()
	{
        if (body == null) body = transform;

		base.Start();
	}

	public override void OnPointerDown(PointerEventData eventData)
    {
        if (interactable)
        {
            ShowButtonAnimate(true);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (interactable)
        {
            ShowButtonAnimate(false);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (interactable)
        {
            if (delayTime <= 0)
            {
                onClick?.Invoke();
                return;
            }

            interactable = false;
            int b = 1;
            DOTween.To(() => 1, (t) =>b=t , 2, delayTime).OnComplete(() =>
            {
                interactable = true;
                onClick?.Invoke();
            });
            ShowButtonAnimate(false);
        }
    }

    public void OnInit(UnityAction onClickCall)
    {
        onClick.RemoveAllListeners();
        onClick.AddListener(()=> 
        {
            if (SoundType != SoundType.None)
            {
                GameManager.Sound.PlayAudio(SoundType.ToString());
            }
            UnityUtil.EVibatorType.VeryShort.PlayerVibrator();

            onClickCall?.Invoke();
        });
    }

    public void OnReset()
    {
        StopAllCoroutines();
        onClick.RemoveAllListeners();

        if (body != null)
        {
            body.DOKill();
            body.localScale = Vector3.one;
        }
        interactable = true;
    }

    private void ShowButtonAnimate(bool hover)
    {
        body.DOKill();
        if (hover)
        {
            if (BtnAnimType == BtnAnimType.Small)
                body.DOScale(Vector3.one * 0.88f, 0.1f);
            else if (BtnAnimType == BtnAnimType.Big)
            {
                body.DOScale(Vector3.one * 1.24f, 0.1f).SetEase(Ease.OutBack);
            }
        }
        else
        {
            body.DOScale(Vector3.one, 0.1f);
        }
    }

    protected override void OnDestroy()
    {
        body.DOKill(true);
        base.OnDestroy();
    }
}
