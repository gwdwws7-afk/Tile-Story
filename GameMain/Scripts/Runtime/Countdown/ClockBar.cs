using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ClockBar : MonoBehaviour
{
    public CountdownTimer timer;
    public GameObject body;
    public Transform pointer;
    public GameObject finishText;

    private float currentTime;
    private float moveTime = 1f;
    private bool isStart;

    public event EventHandler<CountdownOverEventArgs> CountdownOver
    {
        add
        {
            timer.CountdownOver -= value;
            timer.CountdownOver += value;
        }
        remove
        {
            timer.CountdownOver -= value;
        }
    }

    public void OnReset()
    {
        timer.OnReset();
        pointer.DOKill();
        pointer.localEulerAngles = new Vector3(0, 0, -90f);
        isStart = false;
    }

    public void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        timer.OnUpdate(elapseSeconds, realElapseSeconds);

        if (isStart)
        {
            if (currentTime >= moveTime)
            {
                currentTime = 0;
                pointer.DOBlendableLocalRotateBy(new Vector3(0, 0, -90f), 0.2f);
            }
            else
            {
                currentTime += realElapseSeconds;
            }
        }
    }

    public void StartCountdown(DateTime targetTime)
    {
        timer.timeText.gameObject.SetActive(true);
        timer.StartCountdown(targetTime);
        isStart = true;

        if (finishText != null)
        {
            finishText.SetActive(false);
        }
    }

    public void SetFinishState()
    {
        timer.timeText.gameObject.SetActive(false);
        timer.OnReset();
        isStart = false;

        if (finishText != null)
        {
            finishText.SetActive(true);
        }
    }
}
