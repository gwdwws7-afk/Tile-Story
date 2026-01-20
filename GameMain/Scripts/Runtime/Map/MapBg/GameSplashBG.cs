using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSplashBG : UIForm
{
    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);
        gameObject.SetActive(false);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        gameObject.SetActive(true);
    }
}
