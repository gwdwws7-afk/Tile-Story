using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 飞到按钮上增加进度的物品
/// </summary>
public class EntranceFlyObject : MonoBehaviour
{
    public Transform body;

    /// <summary>
    /// 物品飞到目标位置时的事件
    /// </summary>
    public Action onFlyObjectReach;

    /// <summary>
    /// 所有动画完毕时的事件
    /// </summary>
    public Action onAllAnimComplete;

    public virtual void OnInit(string assetName, int num)
    {

    }

    public virtual void OnReset()
    {
        body.DOKill();
        onFlyObjectReach = null;
        onAllAnimComplete = null;
    }

    public virtual void OnRelease()
    {
        body.DOKill();
        onFlyObjectReach = null;
        onAllAnimComplete = null;
    }

    /// <summary>
    /// 物体飞向目标的动画
    /// </summary>
    /// <param name="startPos">开始位置（本地坐标）</param>
    /// <param name="targetPos">目标位置（本地坐标）</param>
    /// <param name="target">目标物体</param>
    public virtual void ShowFlyToTargetAnim(Vector3 startPos, Vector3 targetPos, GameObject target, float delayTime = 0)
    {
        FadeFlyObject(0, 0);
        body.localPosition = startPos;
        body.gameObject.SetActive(true);

        FadeFlyObject(1, 0.4f);

        body.DOScale(new Vector3(1.07f, 0.93f), 0.13f).SetDelay(delayTime).onComplete = () =>
        {
            body.DOScale(new Vector3(0.93f, 1.07f), 0.1f).onComplete = () =>
            {
                body.DOScale(Vector3.one, 0.1f).onComplete = () =>
                {
                    body.DOScale(new Vector3(1.1f, 0.9f), 0.15f).SetDelay(0.1f).SetEase(Ease.InQuad).onComplete = () =>
                    {
                        body.DOScale(new Vector3(0.9f, 1.1f), 0.1f).SetEase(Ease.OutQuad);

                        body.DOLocalJump(targetPos, 110f, 1, 0.3f).SetEase(Ease.InOutQuad).onComplete = () =>
                        {
                            body.gameObject.SetActive(false);

                            ShowFlyObjectReachAnim(target);
                        };
                    };
                };
            };
        };
    }

    /// <summary>
    /// 物体飞向目标的动画并且缩小
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="targetPos"></param>
    /// <param name="target"></param>
    /// <param name="delayTime"></param>
    public virtual void ShowFlyToTargetAnimAndNarrow(Vector3 startPos, Vector3 targetPos, GameObject target, float delayTime = 0)
    {
        FadeFlyObject(0, 0);
        body.localPosition = startPos;
        body.gameObject.SetActive(true);

        FadeFlyObject(1, 0.4f);

        body.DOScale(new Vector3(1.07f, 0.93f), 0.13f).SetDelay(delayTime).onComplete = () =>
        {
            body.DOScale(new Vector3(0.93f, 1.07f), 0.1f).onComplete = () =>
            {
                body.DOScale(Vector3.one, 0.1f).onComplete = () =>
                {
                    body.DOScale(new Vector3(1.1f, 0.9f), 0.15f).SetDelay(0.1f).SetEase(Ease.InQuad).onComplete = () =>
                    {
                        body.DOScale(new Vector3(0.9f, 1.1f), 0.1f).SetEase(Ease.OutQuad).onComplete += () =>
                        {
                            body.DOScale(new Vector3(0.4f, 0.4f), 0.2f).SetEase(Ease.OutQuad);
                        };

                        body.DOLocalJump(targetPos, 110f, 1, 0.3f).SetEase(Ease.InOutQuad).onComplete = () =>
                        {
                            body.gameObject.SetActive(false);

                            ShowFlyObjectReachAnim(target);
                        };
                    };
                };
            };
        };
    }

    protected virtual void ShowFlyObjectReachAnim(GameObject target)
    {
        if (target != null)
        {
            Transform cachedTransform = target.transform;
            cachedTransform.DOScale(new Vector3(1.1f, 1.1f), 0.1f).onComplete = () =>
            {
                cachedTransform.DOScale(new Vector3(0.92f, 0.92f), 0.12f).onComplete = () =>
                {
                    cachedTransform.DOScale(Vector3.one, 0.12f).onComplete = () =>
                    {
                        onAllAnimComplete?.Invoke();

                        OnReset();
                    };
                };
            };
        }
        else
        {
            onAllAnimComplete?.Invoke();
        }

        onFlyObjectReach?.Invoke();
    }

    protected virtual void FadeFlyObject(int value, float time)
    {
    }
}
