using System;
using DG.Tweening;
using UnityEngine;

namespace MySelf.Tools.AnimUtil
{
    public class AnimUtil
    {
        public static void ShowLastTileDestroyAnim(Transform[] lastThreeTileTrans,Action finishAction)
        {
            float y0 = lastThreeTileTrans[0].transform.localPosition.y;
            float x0 = lastThreeTileTrans[0].transform.localPosition.x;
            float x1 = lastThreeTileTrans[1].transform.localPosition.x;
            float x2 = lastThreeTileTrans[2].transform.localPosition.x;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(lastThreeTileTrans[0].DOLocalMoveY(y0+300, 0.3f).SetEase(Ease.OutSine));
            sequence.Join(lastThreeTileTrans[0].DOLocalRotate(Vector3.forward*180, 0.3f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            sequence.Join(lastThreeTileTrans[1].DOLocalMoveX(x1+90,0.3f).SetEase(Ease.OutSine));
            sequence.Join(lastThreeTileTrans[2].DOLocalMoveX(x2+120, 0.3f).SetEase(Ease.OutSine));
            sequence.Join(lastThreeTileTrans[1].DOScaleX(1.02f, 0.1f).SetDelay(0.1f));
            sequence.Join(lastThreeTileTrans[2].DOScaleX(1.02f, 0.1f).SetDelay(0.1f));
            sequence.Append(lastThreeTileTrans[0].DOLocalMoveY(y0, 0.3f).SetEase(Ease.InSine));
            sequence.Join(lastThreeTileTrans[0].DOLocalRotate(Vector3.forward*360, 0.25f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            sequence.Join(lastThreeTileTrans[1].DOLocalMoveX(x0,0.3f).SetEase(Ease.InSine));
            sequence.Join(lastThreeTileTrans[2].DOLocalMoveX(x0, 0.3f).SetEase(Ease.InSine));
            sequence.Join(lastThreeTileTrans[1].DOScaleX(1f, 0.1f).SetDelay(0.1f));
            sequence.Join(lastThreeTileTrans[2].DOScaleX(1f, 0.1f).SetDelay(0.1f));
            sequence.AppendCallback(() => finishAction?.Invoke());
        }
    }
}