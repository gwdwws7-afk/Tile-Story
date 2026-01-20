using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MySelf.Model;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;

public class GamePlayGuide : UIForm
{
    public Transform hand;
    private Sequence sequence;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        var positions = (Vector3[])userData;
        hand.position = positions[0];
        Vector3 smallScale = new Vector3(0.8f,0.8f);
        float clickTime = 0.2f;
        sequence = DOTween.Sequence();
        sequence.SetLoops(-1,LoopType.Restart);
        sequence.Append(hand.GetComponent<Graphic>().DOFade(1, 0f));
        sequence.Append(hand.DOScale(smallScale, clickTime));
        sequence.Append(hand.DOScale(Vector3.one, clickTime));
        sequence.Append(hand.DOMove(positions[1], clickTime));
        sequence.Append(hand.DOScale(smallScale, clickTime));
        sequence.Append(hand.DOScale(Vector3.one, clickTime));
        sequence.Append(hand.DOMove(positions[2], clickTime));
        sequence.Append(hand.DOScale(smallScale, clickTime));
        sequence.Append(hand.DOScale(Vector3.one, clickTime));
        sequence.Append(hand.GetComponent<Graphic>().DOFade(0, 0.2f));
        sequence.AppendInterval(2f);
        sequence.OnKill(() => sequence = null);
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        PlayerBehaviorModel.Instance.RecordGamePlayGuide();
        gameObject.SetActive(true);
        if(sequence!=null)sequence.Play();
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnReset()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        hand.DOKill();
        base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
#if UNITY_EDITOR || UNITY_EDITOR_WIN
        if (Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)||Input.GetMouseButtonDown(2))
#else
        if (Input.touchCount > 0)
#endif
        {
            GameManager.UI.HideUIForm(this);
        }
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }
}
