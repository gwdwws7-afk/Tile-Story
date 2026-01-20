using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 弹窗
/// </summary>
public abstract class CenterForm : UIForm
{
    protected override void Awake()
    {
        base.UIFormType = UIFormType.CenterUI;
        base.Awake();
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        OnReset();
        gameObject.SetActive(false);

        if (UIGroup!=null&&UIGroup.UIFormCount == 0)
        {
            GameManager.Sound.PlayUIOpenSound();
        }

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnCover()
    {
		if (GameManager.UI.GetUIGroup(UIFormType.CenterUI).CurrentUIForm != null)
		{
            gameObject.SetActive(false);
            OnHide();
        }

		base.OnCover();
    }

    public override void OnReveal()
    {
        gameObject.SetActive(true);
        OnShow();

        base.OnReveal();
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
        if (depthInUIGroup == uiGroupDepth)
        {
            transform.SetAsLastSibling();
        }

        base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
    }
}
