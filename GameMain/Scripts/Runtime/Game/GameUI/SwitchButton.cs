using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ������ť
/// </summary>
public sealed class SwitchButton : Button
{
    public GameObject on;
    public GameObject off;
    public Transform body;
    public SoundType SoundType = SoundType.None;

    private bool isOn;
    public bool IsOn { get => isOn; }

    private bool isMove=false;
    
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
            onClick?.Invoke();

            if (SoundType != SoundType.None)
            {
                Log.Debug($"Play Audio:{SoundType}");
                GameManager.Sound.PlayAudio(SoundType.ToString());
            }
            ShowButtonAnimate(false);
        }
    }

    public void SetStatus(bool setOn,bool isMoveAnim=false)
    {
        this.isMove = isMoveAnim;
        on.SetActive(setOn);
        off.SetActive(!setOn);
        if (this.isMove)
        {
            if (setOn)
            {
                if (body)
                    body.SetLocalPositionX(52f);
            }
            else
            {
                if (body)
                    body.SetLocalPositionX(-52f);
            }
        }

        isOn = setOn;
    }

    public void ShowStatusChangeAnim(bool setOn,bool isMove=false)
    {
        this.isMove = isMove;
        if (isOn == setOn)
        {
            return;
        }

        interactable = false;

        if (this.isMove)
        {
            float moveX = setOn ? 52 : -52;
            if (body)
                body.DOLocalMoveX(moveX, 0.2f).onComplete = () =>
                {

                    interactable = true;
                };
        }
        else
        {
            SetStatus(setOn, this.isMove);
            interactable = true;
        }

        isOn = setOn;
    }
    
    public BtnAnimType BtnAnimType=BtnAnimType.Small;
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
}
