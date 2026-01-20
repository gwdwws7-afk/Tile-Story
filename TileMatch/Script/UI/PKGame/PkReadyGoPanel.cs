using System;
using DG.Tweening;
using UnityEngine;

public class PkReadyGoPanel : PopupMenuForm
{
    [SerializeField] private PkSorce PkSorce;
    [SerializeField] private Transform SelfTrans, TargetTrans;
    [SerializeField] private CanvasGroup VsGroup;
    [SerializeField] private CanvasGroup Bg_Image;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        PkSorce.Init(isAutoProgressAnim:false);
        ShowAnim();
        base.OnInit(uiGroup, completeAction, userData);
    }

    private void ShowAnim()
    {
        //初始位置
        SelfTrans.localPosition = new Vector3(-1500f,-418,0);
        TargetTrans.localPosition = new Vector3(1500f,422f,0);
        VsGroup.gameObject.SetActive(false);
        VsGroup.alpha = 0;

        GameManager.Sound.PlayAudio("SFX_Level_Start_PK_Show");
        var seq = DOTween.Sequence();
        // seq.SetDelay(0.4f);
        seq.Append(SelfTrans.DOLocalMove(new Vector3(-335f, -418f, 0), 0.6f).SetEase(Ease.OutBack));
        seq.Join(TargetTrans.DOLocalMove(new Vector3(308f, 422f, 0), 0.6f).SetEase(Ease.OutBack));
        seq.Join(DOTween.To(() => 1, (t) => t = t, 1, 0.2f).OnComplete(() =>
        {
            VsGroup.alpha = 1;
            VsGroup.gameObject.SetActive(true);
        }));
        seq.AppendInterval(1f);
        seq.AppendCallback(() => { Bg_Image.gameObject.SetActive(false); });
        seq.Append(SelfTrans.DOLocalMove(new Vector3(-335f, -1800f, 0), 0.6f).SetEase(Ease.InQuart));
        seq.Join(TargetTrans.DOLocalMove(new Vector3(308f,1800f,0), 0.6f).SetEase(Ease.InQuart));
        seq.Join(Bg_Image.transform.DOScale(0f, 0.01f));
        seq.Join(VsGroup.transform.DOScale(0, 0.5f));
        seq.AppendCallback(() => { GameManager.UI.HideUIForm(this); });
    }
}
