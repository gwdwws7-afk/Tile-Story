using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSlider : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI sliderText;
    public float minValue = 0;

    private float currentValue;
    private float currentNum;
    private float totalNum;
    private bool allowExceedTotalNum = false;

    private TweenerCore<float, float, FloatOptions> anim;

    public float Value
    {
        get
        {
            return currentValue;
        }
        set
        {
            InternalSetSliderValue(value);
        }
    }

    public float CurrentNum
    {
        get
        {
            return currentNum;
        }
        set
        {
            currentNum = value;

            Refresh();
        }
    }

    public float TotalNum
    {
        get
        {
            return totalNum;
        }
        set
        {
            totalNum = value;
        }
    }

    public bool AllowExceedTotalNum
    {
        get
        {
            return allowExceedTotalNum;
        }
        set
        {
            allowExceedTotalNum = value;
        }
    }

    public void OnReset()
    {
        if (anim != null)
        {
            anim.Kill();
            anim = null;
        }
    }

    public void Refresh()
    {
        if (totalNum > 0)
        {
            currentValue = currentNum / totalNum;
        }
        else
        {
            currentValue = 1;
        }

        if (currentValue > 0 && currentValue < minValue)
        {
            slider.value = minValue;
        }
        else
        {
            slider.value = currentValue;
        }

        int showValue = (int)currentNum;

        if (showValue > totalNum && !allowExceedTotalNum) 
        {
            showValue = (int)totalNum;
        }

        sliderText.text = $"{showValue} / {totalNum}";
    }

    public TweenerCore<float, float, FloatOptions> DOValue(float sliderValue, float duration, Action completeAction = null)
    {
        OnReset();
        anim = DOTween.To(() => currentValue, x => InternalSetSliderValue(x), sliderValue, duration);
        anim.onComplete = () =>
        {
            Refresh();

            completeAction?.Invoke();
        };
        return anim;
    }

    public TweenerCore<float, float, FloatOptions> DOValue(float targetNum, float totalNum, float duration, Action completeAction = null)
    {
        this.totalNum = totalNum;
        float sliderValue = targetNum / totalNum;
        sliderValue = sliderValue > 1 ? 1 : sliderValue;

        OnReset();
        anim = DOTween.To(() => currentValue, x => InternalSetSliderValue(x), sliderValue, duration);
        anim.onComplete = () =>
        {
            currentNum = targetNum;
            Refresh();

            completeAction?.Invoke();
        };
        return anim;
    }

    private void InternalSetSliderValue(float sliderValue)
    {
        currentValue = sliderValue;

        if (sliderValue > 0 && sliderValue < minValue) 
        {
            slider.value = minValue;
        }
        else
        {
            slider.value = sliderValue;
        }

        currentNum = currentValue * totalNum;
        int showValue = (int)currentNum;

        if (showValue > totalNum)
        {
            showValue = (int)totalNum;
        }

        sliderText.text = $"{showValue} / {totalNum}";
    }
}
