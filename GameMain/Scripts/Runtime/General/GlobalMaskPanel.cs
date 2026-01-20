using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class GlobalMaskPanel : UIForm
{
    public Button blackBG;

    private int recordClickCount = 0;
    private float recordTime = 0;
    private float initTime = 0;
    
    public override bool IsAutoRelease => false;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        initTime = Time.time;
        blackBG.SetBtnEvent(() =>
        {
            if (Time.time - initTime > 3)
            {
                recordClickCount++;
                if (recordClickCount >= 3)
                {
                    GameManager.UI.HideUIForm(this);
                }
            }
        });
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        transform.SetAsLastSibling();

        gameObject.SetActive(true);

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnReset()
    {
        StopAllCoroutines();
        blackBG.onClick.RemoveAllListeners();

        recordClickCount = 0;
        recordTime = 0;
        base.OnReset();
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (Time.frameCount % 10 == 0)
        {
            if (recordTime == 0)recordTime = Time.time;

            if (Time.time - recordTime > 1)
            {
                recordClickCount = 0;
                recordTime = Time.time;
            }
        }

        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnClose()
    {
        GameManager.UI.HideUIForm(this);
        base.OnClose();
    }
}
