using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用教程界面
/// </summary>
public sealed class CommonGuideMenu : UIForm
{
    public CommonGuideImage guideImage;
    public GuideTipBox tipBox;
    public Transform guideArrow;

    public Action onCoverAction;
    public Action onRevealAction;

    private Sequence guideArrowSequence;

    public Action autoHideAction;
    private int autoHideClickTime = 0;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        tipBox.okButton.onClick.AddListener(OnOkButtonClick);

        autoHideAction = null;
        autoHideClickTime = 0;
    }

    public override void OnReset()
    {
        tipBox.okButton.onClick.RemoveAllListeners();
        guideImage.gameObject.SetActive(true);
        guideImage.DOKill();
        guideImage.color = new Color(1, 1, 1, 0);
        guideImage.OnReset();

        if (guideArrowSequence != null)
        {
            guideArrowSequence.Kill();
        }
        guideArrow.gameObject.SetActive(false);
        guideArrow.localScale = new Vector3(1,1);

        onCoverAction = null;
        onRevealAction = null;

        autoHideAction = null;
        autoHideClickTime = 0;

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

        guideImage.DOFade(0.78f, 0.2f);

        tipBox.OnShow(localPos);

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);

        tipBox.OnHide();

        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
#if UNITY_EDITOR || UNITY_EDITOR_WIN
        if (Input.GetMouseButtonDown(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase==TouchPhase.Began)
#endif
        {
            autoHideClickTime++;

            if (autoHideClickTime > 3)
            {
                Action action = autoHideAction;
                GameManager.UI.HideUIForm(this);
                action?.Invoke();
            }
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

    public void SetArrowScale(Vector3 scale)
    {
        guideArrow.localScale = scale;
    }

    public void HideGuideImage()
    {
        guideImage.DOKill();
        guideImage.color = new Color(1, 1, 1, 0);
        guideImage.raycastAll = true;
        guideImage.raycastTarget = false;
    }

    public void ShowGuideArrow(Vector3 startPos, Vector3 endPos, PromptBoxShowDirection direction = PromptBoxShowDirection.Down)
    {
        if (guideArrowSequence != null)
        {
            guideArrowSequence.Kill();
        }

        guideArrow.transform.position = startPos;

        switch (direction)
        {
            case PromptBoxShowDirection.Up:
                guideArrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case PromptBoxShowDirection.Down:
                guideArrow.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case PromptBoxShowDirection.Left:
                guideArrow.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case PromptBoxShowDirection.Right:
                guideArrow.transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
        }

        guideArrow.gameObject.SetActive(true);

        guideArrowSequence = DOTween.Sequence();
        guideArrowSequence.Append(guideArrow.transform.DOMove(endPos, 0.6f))
            .Append(guideArrow.transform.DOMove(startPos, 0.6f))
            .SetLoops(-1);
        guideArrowSequence.OnComplete(()=>guideArrowSequence=null).OnKill(()=>guideArrowSequence=null);
    }

    public void HideGuideArrow()
    {
        if (guideArrowSequence != null)
        {
            guideArrowSequence.Kill();
        }
        guideArrow.gameObject.SetActive(false);
    }

    public void OnOkButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }
}

public enum GuideType
{
    Skill_AddOneStep,
    Skill_Back,
    Skill_ChangePos,
    Skill_Absorb,
    Skill_Grab,
}

public static class CommonGuideMenuUtil
{
    public static void ShowSkillGuide(GuideType type, Button btn, Action showFinishAction = null, Action guideFinishAction = null)
    {
        ShowStrongSkillGuide(type, btn, showFinishAction, guideFinishAction);
    }

    public static void ShowStrongSkillGuide(GuideType type, Button btn, Action showFinishAction, Action guideFinishAction)
    {
        string contentTerm = null;
        switch (type)
        {
            case GuideType.Skill_Back:
                contentTerm = "Common.Tap to undo the last tile in the rack";
                break;
            case GuideType.Skill_ChangePos:
                contentTerm = "Common.Tap to shuffle all tiles on the board";
                break;
            case GuideType.Skill_Absorb:
                contentTerm = "Common.Tap to use the magnet to make an auto-match";
                break;
            case GuideType.Skill_Grab:
                contentTerm = "Common.Tap to move 3 surface tiles away";
                break;
            case GuideType.Skill_AddOneStep:
                contentTerm = "Common.Tap to expand your rack with extra Slot";
                break;
        }

        GameManager.UI.ShowUIForm("CommonGuideMenuByFinger", form =>
        {
            form.gameObject.SetActive(false);
            var guideMenu = form.GetComponent<CommonGuideMenuByFinger>();
            var originParent = btn.transform.parent;
            btn.transform.SetParent(form.transform);
            guideMenu.tipBox.SetOkButton(false);
            guideMenu.ShowGuide(contentTerm, btn.transform.position);

            guideMenu.tipBox.transform.position = new Vector3(0, -0.3f, 0);
            guideMenu.OnShow(null, null);

            void ClickAction()
            {
                btn.transform.SetParent(originParent);
                GameManager.UI.HideUIForm(form);
                guideMenu.guideImage.onTargetAreaClick = null;
                btn.onClick.RemoveListener(ClickAction);

                guideFinishAction?.Invoke();
            }
            btn.onClick.AddListener(ClickAction);

            showFinishAction?.Invoke();
        });
    }

    public static void ShowWeakSkillGuide(GuideType type)
    {
        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null)
        {
            for (int i = 0; i < panel.skillList.Count; i++)
            {
                if (type == GuideType.Skill_Grab && panel.skillList[i].GetTotalItemType() == TotalItemData.Prop_Grab) 
                {
                    panel.skillList[i].SetUnlockEvent();
                    panel.skillList[i].PlayFingerAnim();
                    break;
                }
            }
        }
    }

    public static void HideWeakSkillGuide(GuideType type)
    {
        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null)
        {
            for (int i = 0; i < panel.skillList.Count; i++)
            {
                if (type == GuideType.Skill_Grab && panel.skillList[i].GetTotalItemType() == TotalItemData.Prop_Grab)
                {
                    panel.skillList[i].PlayFingerAnim(false);
                    break;
                }
            }
        }
    }
}
