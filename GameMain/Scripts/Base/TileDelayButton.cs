using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public enum BtnAnimType
{
    Small,
    Big,

    None,
}

public sealed class TileDelayButton : Button
{
    public float delayTime = 0.05f;
    public Transform body;
    public SoundType SoundType = SoundType.None;

    public BtnAnimType BtnAnimType = BtnAnimType.Small;

    public bool IsRecordSiblingIndex = false;
    public bool DisableAnim = false;
    public bool DisableButtonAnim = false;

    public AnimationCurve HoverAnimationCurve = null;
    public AnimationCurve HoverAnimationCurve1 = null;
    public AnimationCurve NoHoverAnimationCurve = null;

    protected override void Start()
    {
        if (body == null) body = transform;

        base.Start();
    }

    private int curSiblingIndex = 0;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (interactable)
        {
            if (!DisableButtonAnim)
            {
                ShowButtonAnimate(true);
            }

            if (IsRecordSiblingIndex && !DisableAnim) 
            {
                curSiblingIndex = transform.GetSiblingIndex();
                transform.SetAsLastSibling();
            }
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (interactable)
        {
            if (!DisableButtonAnim)
            {
                ShowButtonAnimate(false);
            }

            if (IsRecordSiblingIndex && !DisableAnim)
            {
                transform.SetSiblingIndex(curSiblingIndex);
            }
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (interactable)
        {
            if (!DisableButtonAnim)
            {
                ShowButtonAnimate(false, true);
            }

            if (delayTime <= 0)
            {
                onClick?.Invoke();
                return;
            }
            StartCoroutine(DelayClick());
        }
    }

    public void OnInit(UnityAction onClickCall)
    {
        onClick.RemoveAllListeners();
        onClick.AddListener(() =>
        {
            if (SoundType != SoundType.None && !DisableAnim) 
            {
                GameManager.Sound.PlayAudio(SoundType.ToString());
            }

            try
            {
                UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
            }
            catch
            {
            }

            onClickCall?.Invoke();
        });
    }

    public void OnReset()
    {
        StopAllCoroutines();
        onClick.RemoveAllListeners();

        if (body != null)
        {
            body.DOKill();
            body.localScale = Vector3.one;
        }
        interactable = true;
    }

    IEnumerator DelayClick()
    {
        interactable = false;
        yield return new WaitForSecondsRealtime(delayTime);
        interactable = true;
        onClick?.Invoke();
    }

    static float[] AnimTimes = new float[2] { 0.12f, 0.36f };
    Vector3 recordVect = Vector3.zero;

    private void ShowButtonAnimate(bool hover, bool isClick = false)
    {
        if (DisableAnim)
            return;

        body.DOKill();
        if (hover)
        {
            if (BtnAnimType == BtnAnimType.Small)
                body.DOScale(Vector3.one * 0.88f, 0.1f);
            else if (BtnAnimType == BtnAnimType.Big)
            {
                recordVect = body.localPosition;
                body.DOScale(Vector3.one * 1.4f, 0.2f).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    body.DOBlendableScaleBy(Vector3.one * (1.3f - body.transform.localScale.x), 0.05f).SetEase(Ease.OutSine).OnComplete(() =>
                    {
                        body.DOBlendableLocalMoveBy(Vector3.up * 24, 4).SetLoops(-1, LoopType.Restart).SetEase(HoverAnimationCurve1);
                        body.DOBlendableLocalRotateBy(Vector3.forward * 6, 4).SetLoops(-1, LoopType.Restart).SetEase(HoverAnimationCurve);
                    });
                });
            }
        }
        else
        {
            float xTime = Random.Range(AnimTimes[0], AnimTimes[1]);
            float yTime = Random.Range(AnimTimes[0], AnimTimes[1]);
            if (BtnAnimType == BtnAnimType.Big)
            {
                body.localPosition = recordVect;
                body.DOBlendableScaleBy(Vector3.right * (1f - body.transform.localScale.x), xTime).SetEase(NoHoverAnimationCurve);
                body.DOBlendableScaleBy(Vector3.up * (1f - body.transform.localScale.y), yTime).SetEase(NoHoverAnimationCurve);
            }
            else
            {
                body.DOBlendableScaleBy(Vector3.right * (1f - body.transform.localScale.x), xTime).SetEase(Ease.OutBack);
                body.DOBlendableScaleBy(Vector3.up * (1f - body.transform.localScale.y), yTime).SetEase(Ease.OutBack);
            }

            if (!isClick && BtnAnimType == BtnAnimType.Big)
                body.DORotate(Vector3.zero, 0.1f);
        }
    }

    protected override void OnDestroy()
    {
        body.DOKill(true);
        base.OnDestroy();
    }
}
