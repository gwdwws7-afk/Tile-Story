using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardSetGuideMenu : UIForm
{
    public CommonGuideImage guideImage;
    public Transform guideArrow;

    public override void OnRelease()
    {
        guideImage.DOKill();
        guideImage.color = new Color(1, 1, 1, 0);
        guideImage.OnReset();
        base.OnRelease();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        guideArrow.gameObject.SetActive(false);
        gameObject.SetActive(true);
        guideImage.DOFade(1f, 0.25f);
        
        base.OnShow(showSuccessAction, userData);
    }

    public void ShowArrow(Vector3 pos)
    {
        guideArrow.position = pos;
        guideArrow.gameObject.SetActive(true);
    }
}
