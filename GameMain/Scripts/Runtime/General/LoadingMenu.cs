using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoadingMenu : UIForm
{
    public Transform loadingTip;
    public Button blackBG;

    private bool canClose = false;

    private float delayTime = 3f;

    private Action timeOutAction;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        if (userData != null)
        {
            var delyTIme = (float)userData;

            delayTime = Mathf.Max(delyTIme, 3f);
        }
        else
            delayTime = 3f;

        blackBG.SetBtnEvent(() =>
        {
            if (canClose)
            {
                GameManager.UI.HideUIForm(this);
            }
        });
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        transform.SetAsLastSibling();

        loadingTip.DOBlendableLocalRotateBy(new Vector3(0, 0, -360f), 2f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);

        gameObject.SetActive(true);

        StartCoroutine(DelayClose());

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnReset()
    {
        StopAllCoroutines();
        blackBG.onClick.RemoveAllListeners();
        canClose = false;
        timeOutAction = null;
        base.OnReset();
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        if (loadingTip != null)
        {
            loadingTip.DOKill();
        }

        gameObject.SetActive(false);

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
        base.OnClose();
    }

    IEnumerator DelayClose()
    {
        yield return new WaitForSeconds(delayTime);
        canClose = true;

        Action callback = timeOutAction;
        GameManager.UI.HideUIForm(this);
        callback?.InvokeSafely();
    }

    public void SetTimeOutAction(Action timeOutAction)
    {
        this.timeOutAction = timeOutAction;
    }
}
