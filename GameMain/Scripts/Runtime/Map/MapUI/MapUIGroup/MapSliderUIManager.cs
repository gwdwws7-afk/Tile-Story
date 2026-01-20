using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSliderUIManager : UIGroup
{
    public override string GroupName => "SliderUI";

    public override UIGroupType GroupType => UIGroupType.Static;

    public override void OnHide(bool skipAnim)
    {
        var rectTrans = GetComponent<RectTransform>();
        rectTrans.DOKill();
        rectTrans.DOAnchorPos(new Vector3(0, 600f), 0.2f);
        base.OnHide(skipAnim);
    }

    public override void OnShow()
    {
        var rectTrans = GetComponent<RectTransform>();
        rectTrans.DOKill();
        rectTrans.DOAnchorPos(new Vector3(0, 980f), 0.2f).onComplete = () =>
        {
            rectTrans.DOAnchorPos(new Vector3(0, 960f), 0.2f);
        };
        base.OnShow();
    }
}
