using DG.Tweening;
using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

/// <summary>
/// 通用教程界面
/// </summary>
public sealed class CommonGuideMenuByFinger : UIForm
{
    public CommonGuideImage guideImage;
    public GuideTipBox tipBox;
    public SkeletonGraphic guideFinger;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        tipBox.okButton.onClick.AddListener(OnOkButtonClick);
    }

    public override void OnReset()
    {
        tipBox.okButton.onClick.RemoveAllListeners();
        guideImage.gameObject.SetActive(true);
        guideImage.DOKill();
        guideImage.color = new Color(1, 1, 1, 0);
        guideImage.OnReset();

        AutoHide = false;

        base.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        Vector3 localPos = tipBox.transform.localPosition;
        if (userData != null)
        {
            localPos = (Vector3)userData;
        }

        gameObject.SetActive(true);

        guideImage.DOFade(0.78f, 0.2f);

        tipBox.OnShow(localPos);

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);

        tipBox.OnHide();

        base.OnHide(hideSuccessAction, userData);
    }

    public bool AutoHide = false;
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (AutoHide)
        {
#if UNITY_EDITOR || UNITY_EDITOR_WIN
            if(Input.GetMouseButtonDown(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase==TouchPhase.Began)
#endif
            {
                GameManager.UI.HideUIForm(this);
            }
        }

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public void ShowGuide(string term,Vector3 targetPos, PromptBoxShowDirection direction = PromptBoxShowDirection.Down)
    {
        tipBox.SetText(term);
        guideFinger.transform.position = targetPos;
        guideFinger.gameObject.SetActive(true);
        guideFinger.transform.SetAsLastSibling();
    }

    public void OnOkButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }
}
