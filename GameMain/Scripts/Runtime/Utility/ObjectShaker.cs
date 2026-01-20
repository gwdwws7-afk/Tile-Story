using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShaker : MonoBehaviour
{
    public Vector3 positionShake;//震动幅度
    public Vector3 angleShake;   //震动角度
    public float cycleTime = 0.2f;//震动周期
    public int cycleCount = 6;    //震动次数
    public bool fixShake = false; //为真时每次幅度相同，反之则递减
    public bool unscaleTime = false;//不考虑缩放时间
    public bool bothDir = true;//双向震动
    public float shakeBetweenTime;//震动完后间隔时间
    public bool loop = true;//循环
    public bool playOnAwake = false;

    float currentTime;
    int curCycle;
    Vector3 curPositonShake;
    Vector3 curAngleShake;
    Vector3 startPosition;
    Vector3 startAngles;
    Transform myTransform;
    bool deferredShake;

    public void OnInit()
    {
        currentTime = 0f;
        curCycle = 0;
        curPositonShake = positionShake;
        curAngleShake = angleShake;
        myTransform = transform;
        startPosition = myTransform.localPosition;
        startAngles = myTransform.localEulerAngles;

        if (gameObject.activeInHierarchy)
            StartCoroutine(ShakeBetweenTime(shakeBetweenTime));
        else
            deferredShake = true;
    }

    public void OnReset()
    {
        deferredShake = false;

        if (myTransform != null)
        {
            myTransform.localPosition = startPosition;
            myTransform.localEulerAngles = startAngles;
            StopAllCoroutines();
        }
    }

    private void OnEnable()
    {
        if(deferredShake)
        {
            deferredShake = false;
            OnReset();
            OnInit();
        }

        if (playOnAwake)
        {
            OnReset();
            OnInit();
        }
    }

    IEnumerator ShakeBetweenTime(float time)
    {
        WaitForSeconds delay = new WaitForSeconds(time);
        while (true)
        {
            if (curCycle >= cycleCount)
            {
                if (loop)
                {
                    yield return delay;
                    Restart();
                }
                else
                {
                    yield break;
                }
            }

            float deltaTime = unscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
            currentTime += deltaTime;
            while (currentTime >= cycleTime)
            {
                currentTime -= cycleTime;
                curCycle++;
                if (curCycle >= cycleCount)
                {
                    myTransform.localPosition = startPosition;
                    myTransform.localEulerAngles = startAngles;
                    break;
                }

                if (!fixShake)
                {
                    if (positionShake != Vector3.zero)
                        curPositonShake = (cycleCount - curCycle) * positionShake / cycleCount;
                    if (angleShake != Vector3.zero)
                        curAngleShake = (cycleCount - curCycle) * angleShake / cycleCount;
                }
            }

            if (curCycle < cycleCount)
            {
                float offsetScale = Mathf.Sin((bothDir ? 2 : 1) * Mathf.PI * currentTime / cycleTime);
                if (positionShake != Vector3.zero)
                    myTransform.localPosition = startPosition + curPositonShake * offsetScale;
                if (angleShake != Vector3.zero)
                    myTransform.localEulerAngles = startAngles + curAngleShake * offsetScale;
            }
            yield return null;

        }
    }

    //重置
    public void Restart()
    {
        if (enabled)
        {
            currentTime = 0f;
            curCycle = 0;
            curPositonShake = positionShake;
            curAngleShake = angleShake;
            myTransform.localPosition = startPosition;
            myTransform.localEulerAngles = startAngles;
        }
        else
            enabled = true;
    }
}
