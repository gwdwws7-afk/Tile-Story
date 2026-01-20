using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 弹窗
/// </summary>
public abstract class PopupMenuForm : UIForm
{
    [SerializeField]
    private Transform cachedTransform;

#if UNITY_EDITOR
	new private void OnValidate()
	{
        if (cachedTransform == null) cachedTransform = transform.Find("Root");

        base.OnValidate();
    }
#endif

    protected override void Awake()
	{
        base.UIFormType = UIFormType.PopupUI;
        base.Awake();
	}

    public override void OnReset()
    {
        if (cachedTransform) cachedTransform.DOKill();

        base.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        if (m_IsAvailable)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            base.OnShow(showSuccessAction, userData);
            return;
        }

        if (cachedTransform)
        {
            cachedTransform.DOKill();
            cachedTransform.localScale = Vector3.one;
            cachedTransform.DOScale(1.03f, 0.12f).onComplete = () =>
            {
                cachedTransform.DOScale(0.99f, 0.1f).onComplete = () =>
                {
                    cachedTransform.DOScale(1f, 0.1f);
                    m_IsAvailable = true;
                };
            };
        }
        
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        GameManager.Sound.PlayUIOpenSound();

        showSuccessAction?.Invoke(this);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        OnReset();
        cachedTransform.DOKill();

        if(cachedTransform) cachedTransform.localScale = Vector3.one;

        if (gameObject.activeSelf)
            gameObject.SetActive(false);

        if (UIGroup.UIFormCount == 0)
        {
            GameManager.Sound.PlayUIOpenSound();
        }

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnCover()
    {
		if (GameManager.UI.GetUIGroup(UIFormType.PopupUI).CurrentUIForm != null
			&& GameManager.UI.GetUIGroup(UIFormType.PopupUI).CurrentUIForm.GetType().Name != "LoadingMenu")
		{
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
		}

		base.OnCover();
    }

    public override void OnReveal()
    {
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
