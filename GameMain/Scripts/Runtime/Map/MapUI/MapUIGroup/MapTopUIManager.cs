using DG.Tweening;
using UnityEngine;

/// <summary>
/// 地图界面顶部UI管理器
/// </summary>
public sealed class MapTopUIManager : UIGroup
{
    public override string GroupName => "TopUI";

    public override UIGroupType GroupType => UIGroupType.Static;

    public override void OnShow()
    {
        var rectTrans = GetComponent<RectTransform>();
        rectTrans.DOKill();
        rectTrans.DOAnchorPos(new Vector3(0, -1010f), 0.2f).onComplete = () =>
        {
            rectTrans.DOAnchorPos(new Vector3(0, -960), 0.2f);
        };

        base.OnShow();
    }

    public override void OnHide(bool skipAnim)
    {
        var rectTrans = GetComponent<RectTransform>();
        rectTrans.DOKill();
        if (skipAnim)
        {
            rectTrans.anchoredPosition = new Vector3(0, -460f);
        }
        else
        {
            rectTrans.DOAnchorPos(new Vector3(0, -460f), 0.1f);
        }

        base.OnHide(skipAnim);
    }
}
