using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;

public static class AnimationExtension
{
    public static void SetToEnd(this SkeletonGraphic graphic)//设置画面至结束状态
    {
        if (graphic.AnimationState == null)
            graphic.Initialize(true);//避免动画混合
        graphic.AnimationState.SetAnimation(0, GetActiveSpineName(graphic), false);//进行激活动画的获取
        graphic.AnimationState.GetCurrent(0).TrackTime = graphic.AnimationState.GetCurrent(0).AnimationEnd;
    }

    public static string GetActiveSpineName(SkeletonGraphic graphic)
    {
        string result = "";
        foreach (var anim in graphic.SkeletonDataAsset.GetSkeletonData(false).Animations)
        {
            if (anim.Name.Equals("active")) return "active";
            result = anim.Name;
        }
        return result;
    }

    public static bool IsSpineAnimNameExist(this SkeletonGraphic graphic,string animName)
    {
        foreach (var anim in graphic.SkeletonDataAsset.GetSkeletonData(false).Animations)
        {
            if (anim.Name.Equals(animName)) return true;
        }
        return false;
    }

    public static void SetToFirst(this SkeletonGraphic graphic)//设置画面至开始状态
    {
        if (graphic.AnimationState == null)
            graphic.Initialize(true);
        graphic.AnimationState.SetAnimation(0, GetActiveSpineName(graphic), false);
        //graphic.AnimationState.GetCurrent(0).TrackTime = graphic.AnimationState.GetCurrent(0).AnimationStart;
    }

    /// <summary>
    /// 设置画面至中间状态
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="time">时间</param>
    public static void SetToMid(this SkeletonGraphic graphic,float time)
    {
        if (graphic.AnimationState == null)
            graphic.Initialize(true);
        graphic.AnimationState.SetAnimation(0, GetActiveSpineName(graphic), false);
        graphic.AnimationState.GetCurrent(0).TrackTime = time;
    }
    public static void SilderFillTo(this Slider slider, float startValue, float endValue, int startInt, int endInt, Action completeAction = null, Action<int> numChangeAction = null)//设置画面至结束状态
    {
        GameManager.Task.StartCoroutine(SliderFill(slider, startValue, endValue, startInt, endInt, completeAction, numChangeAction));
    }

    static IEnumerator SliderFill(Slider slider, float startValue, float endValue, int startInt, int endInt, Action completeAction, Action<int> numChangeAction)
    {
        float delta = (endValue - startValue) / (endInt - startInt);//value每变动多少float值就进行数字更新
        slider.SetValueWithoutNotify(startValue);
        float nowValue = startValue;
        float oldValue = nowValue;
        int formerInt = startInt;
        int nowInt = startInt;
        float speed = 0.1f;//当前速度是定值
        while (slider.value < endValue)
        {
            nowValue += Time.deltaTime * speed;
            if (nowValue > endValue)
            {
                nowValue = endValue;
            }
            slider.SetValueWithoutNotify(nowValue);

            nowInt = (int)((nowValue - startValue) / delta);
            if (nowInt != formerInt)
            {
                formerInt = nowInt;
                numChangeAction?.Invoke(nowInt);
            }
            yield return 0;
        }
        nowInt = endInt;
        if (nowInt != formerInt)
        {
            numChangeAction?.Invoke(nowInt);
        }
        completeAction?.Invoke();
    }
}
