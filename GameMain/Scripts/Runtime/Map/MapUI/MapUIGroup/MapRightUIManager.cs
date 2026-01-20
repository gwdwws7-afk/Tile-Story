using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地图界面右边UI管理器
/// </summary>
public sealed class MapRightUIManager : UIGroup
{
    public override string GroupName => "RightUI";

    public override UIGroupType GroupType => UIGroupType.Static;

    private string[] canHideEntranceNames = new string[] { "NewPlayerPackageEntrance" };

    public override void OnShow()
    {
        RectTransform rectTrans = GetComponent<RectTransform>();
        rectTrans.DOKill();
        rectTrans.DOAnchorPos(new Vector3(-60, 0), 0.15f).SetDelay(0.15f).onComplete = () =>
        {
            rectTrans.DOAnchorPos(Vector3.zero, 0.15f);
        };

        base.OnShow();
    }

    public override void OnHide(bool skipAnim)
    {
        RectTransform rectTrans = GetComponent<RectTransform>();
        rectTrans.DOKill();
        if (skipAnim)
        {
            rectTrans.anchoredPosition = new Vector3(300, 0);
        }
        else
        {
            rectTrans.DOAnchorPos(new Vector3(300, 0), 0.2f);
        }

        base.OnHide(skipAnim);
    }

    public override void RefreshLayout()
    {
        //边界栏按钮的上限是5个，超出上限的根据优先度进行隐藏
        if (uiFormInfos.Count > 5)
        {
            for (int i = 0; i < canHideEntranceNames.Length; i++)
            {
                foreach (UIFormInfo info in uiFormInfos)
                {
                    if (info.UIForm.GetType().Name == canHideEntranceNames[i])
                    {
                        GameManager.UI.HideUIForm(info.UIForm);
                        return;
                    }
                }
            }
        }

        float paddingX = -72.4f;
        float paddingY = -580;
        int delta = -229;
        float scale = 1;

        //根据UI成员数量和屏幕分辨率调整UI成员大小和位置
        if (uiFormInfos.Count > 4 && Screen.height * 1080 <= 1920 * Screen.width)
        {
            delta = -215;
            scale = 0.95f;
        }

        List<UIFormInfo> formInfoList = new List<UIFormInfo>(uiFormInfos);
        formInfoList.Sort((a, b) => a.UIForm.SerialId.CompareTo(b.UIForm.SerialId));

        for (int i = 0; i < formInfoList.Count; i++)
        {
            RectTransform uiFormTrans = formInfoList[i].UIForm.GetComponent<RectTransform>();
            uiFormTrans.anchoredPosition = new Vector2(paddingX, paddingY + i * delta);
            uiFormTrans.localScale = new Vector3(scale, scale, scale);
        }

        base.RefreshLayout();
    }
}
