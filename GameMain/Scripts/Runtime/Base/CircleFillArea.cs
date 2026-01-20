using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 环形填充区域
/// </summary>
public sealed class CircleFillArea : MonoBehaviour
{
    public Image fillImage;
    public Transform head;
    public float delta;
    public Action onCompleteAction;

    private float targetValue;
    private float currentValue;
    private float updateValue;
    private float fillDuration;

    public float TargetValue { get => targetValue; }
    public float CurrentValue { get => currentValue; }

    public void OnInit()
    {
    }

    public void OnReset()
    {
        ClearTargetValue();
        fillImage.fillAmount = 0;
        head.rotation = Quaternion.Euler(0, 0, -90);
        targetValue = 0;
        currentValue = 0;
        updateValue = 0;
        fillDuration = 0;
        onCompleteAction = null;
    }

    public void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (targetValue > currentValue) 
        {
            currentValue += elapseSeconds * updateValue;
            if (currentValue > targetValue)
            {
                currentValue = targetValue;
            }

            if (targetValue < 0.5f)
            {
                fillImage.fillAmount = currentValue * 0.8f + 0.08f;
            }
            else
            {
                fillImage.fillAmount = currentValue * 0.8f + 0.105f;
            }
            head.rotation = Quaternion.Euler(0, 0, -(fillImage.fillAmount * 360 + delta));

            if (currentValue == targetValue)
            {
                onCompleteAction?.Invoke();
                onCompleteAction = null;
            }
        }
    }

    public void SetValue(float value, float duration)
    {
        if (targetValue > value)
        {
            Log.Warning("CircleFillArea target value is bigger than value");
            return;
        }

        value = value > 1 ? 1 : value;
        targetValue = value;

        if (duration <= 0) 
        {
            currentValue = targetValue;
            fillImage.fillAmount = currentValue;
            head.rotation = Quaternion.Euler(0, 0, -(currentValue * 360 + delta));

            onCompleteAction?.Invoke();
            onCompleteAction = null;
        }
        else
        {
            fillDuration = duration;
            updateValue = (targetValue - currentValue) / fillDuration;
        }
    }

    public void ClearTargetValue()
    {
        targetValue = 0;
        currentValue = 0;
    }
}
