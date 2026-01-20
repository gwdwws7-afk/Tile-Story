using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MySelf.Model;
using UnityEngine;
using UnityEngine.UI;

public class PkGameShowPkScorePanel : PopupMenuForm
{
    [SerializeField] private PkSorce PkSorce;
    [SerializeField] private CanvasGroup GuideCanvasGroup;

    [SerializeField] private GameObject[] AnimObjs;
    [SerializeField] private Image HeadImage;
    [SerializeField] private CanvasGroup TextBgGroup;
    [SerializeField] private Transform GuideTextsParent;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        //先将动画部分隐藏
        foreach (var obj in AnimObjs)
        {
            obj.gameObject.SetActive(false);
        }

        int textCode =(int)userData;
        bool isShow = textCode != 0;
        //先将对方数据刷新成最新数据
        PkGameModel.Instance.RecordOldTargetSorce();
        //然后展示敌我数据
        PkSorce.Init(isAutoProgressAnim:false);
        //展示头像

        if (isShow)
        {
            GameManager.Task.AddDelayTriggerTask(1.8f, () =>
            {
                //先做动画
                HeadImage.transform.localScale = Vector3.one * 0.6f;
                HeadImage.gameObject.SetActive(true);
                HeadImage.transform.DOScale(1, 0.15f).SetEase(Ease.OutBack).onComplete += () =>
                {
                    TextBgGroup.alpha =0;
                    TextBgGroup.gameObject.SetActive(true);
                    TextBgGroup.transform.localEulerAngles = new Vector3(0,0,30);
                    TextBgGroup.transform.DOLocalRotate(Vector3.zero, 0.15f).SetEase(Ease.OutBack);
                    TextBgGroup.DOFade(1, 0.1f);
                };
            });
            //Guide_Text.SetTerm(GetTextTrem(textCode));
            // SetGuideText(textCode);
            ShowGuideText(textCode);
        }

        GameManager.Task.AddDelayTriggerTask(isShow?6f:5f, () =>
        {
            GameManager.UI.HideUIForm(this);
        });
        base.OnInit(uiGroup, completeAction, userData);
    }

    // private void ShowAnim(int textCode)
    // {
    //     Guide_Text.SetTerm(GetTextTrem(textCode));
    //     Guide_Text.Target.maxVisibleCharacters = 0;
    //     int finalCharacterCount = Guide_Text.Target.textInfo.characterCount;
    //     DOTween.To(() => 0, t => Guide_Text.Target.maxVisibleCharacters = t, finalCharacterCount, 3);
    // }

    // private void SetGuideText(int code)
    // {
    //    var content = GameManager.Localization.GetString(GetTextTrem(code));
    //    Guide_Text.text = content;
    // }

    private void ShowGuideText(int code)
    {
        GuideTextsParent.SetChildActive(false,code.ToString());
    }

    private string GetTextTrem(int code)
    {
        switch (code)
        {
            case 1:
                return "Pk.Nice try! But the win belongs to me!";
            case 2:
                return "Pk.Hey, stay humble! I lost on purpose!";
            case 4:
                return "Pk.Seriously? Losing doesn't suit my vibe!";
            case 5:
                return "Pk.Oops! Looks like someone's having a bumpy ride!";
            case 6:
                return "Pk.Impressive! Yet, I remain unbeaten!";
            default:
                return "Pk.Nice try! But the win belongs to me!";
        }
    }
}
