using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用教程界面
/// </summary>
public sealed class FingerGuideMenu : UIForm
{
    public CommonGuideImage guideImage;
    public GuideTipBox tipBox;
    public SkeletonAnimation finger;
    public Action onCoverAction;
    public Action onRevealAction;
    public DelayButton skipButton;
    public Action onSkipAction;

    private float showTimer;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        tipBox.okButton.onClick.AddListener(OnOkButtonClick);

        skipButton.SetBtnEvent(() =>
        {
            onSkipAction?.Invoke();
            onSkipAction = null;
            GameManager.UI.HideUIForm(this);
        });
        skipButton.gameObject.SetActive(false);
        showTimer = 0;
    }

    public override void OnReset()
    {
        tipBox.okButton.onClick.RemoveAllListeners();

        AutoHide = false;
        for (int i = 0; i < extraList.Count; i++)
        {
            Destroy(extraList[i]);
        }
        extraList.Clear();

        onCoverAction = null;
        onRevealAction = null;

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

        tipBox.OnShow(localPos);
        
        finger.gameObject.SetActive(true);
        if (finger.AnimationState == null)
        {
            finger.Initialize(true);
        }

        var anim = finger.AnimationState?.SetAnimation(0, "02", true);
        if (anim != null) anim.TimeScale = 0.9f;

        if (guideImage.gameObject.activeSelf)
        {
            guideImage.DOFade(0.78f, 0.2f);
        }

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);

        tipBox.OnHide();
        
        OnClose();
        
        base.OnHide(hideSuccessAction, userData);
    }

    public bool AutoHide = false;
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (AutoHide)
        {
#if UNITY_EDITOR || UNITY_EDITOR_WIN
            if(Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)||Input.GetMouseButtonDown(2))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase==TouchPhase.Began)
#endif
            {
                GameManager.UI.HideUIForm(this);
            }
        }
        else
        {
            if (showTimer < 3)
                showTimer += elapseSeconds;
            else
                skipButton.gameObject.SetActive(true);
        }

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnCover()
    {
        base.OnCover();

        onCoverAction?.Invoke();
    }

    public override void OnReveal()
    {
        base.OnReveal();

        onRevealAction?.Invoke();
    }

    public void SetText(string content)
    {
        tipBox.SetText(content);
    }

    public void SetGuideImageVisibility(bool inputVisibility)
    {
        guideImage.gameObject.SetActive(inputVisibility);
    }

    public void OnOkButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }
    public List<GameObject> extraList = new List<GameObject>();
    public void AttachButton(DelayButton button)
    {
        var g = Instantiate(button, transform);
        g.GetComponent<DelayButton>().OnReset();
        g.GetComponent<DelayButton>().OnInit(() =>
        {
            g.GetComponent<DelayButton>().OnReset();
            button.onClick.Invoke();
        });
        extraList.Add(g.gameObject);
    }
    public void AttachPrefab(string prefabName)
    {
        UnityUtility.InstantiateAsync(prefabName, transform, g =>
        {
            extraList.Add(g);
        });
    }
}
