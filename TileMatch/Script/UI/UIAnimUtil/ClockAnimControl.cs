using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ClockAnimControl : MonoBehaviour
{
    [SerializeField] private GameObject ClockPointer;

    private void OnEnable()
    {
        ClockPointer.transform.DOLocalRotate(
            ClockPointer.transform.localEulerAngles - Vector3.forward * 90,
            1f).SetDelay(1f).SetEase(Ease.InOutSine).SetLoops(-1,LoopType.Incremental);
    }

    private void OnDisable()
    {
        ClockPointer.transform.DOKill();
    }
}
