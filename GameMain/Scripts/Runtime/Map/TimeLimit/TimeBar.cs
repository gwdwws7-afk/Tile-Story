using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class TimeBar : UIForm
{
    private bool isStart = false;
    private bool isFocus = true;
    private bool IsRun => isStart && isFocus;
    public bool IsTimeOver => totalTime > 0 && remainingTime <= 0;
    private bool isStartAddTime = false;
    private float totalTime = -1, remainingTime;
    public int ContinueAddTime => (int)totalTime - (int)remainingTime;
    [SerializeField] private Image fill, redFlash;
    [SerializeField] private TextMeshProUGUI countDown;
    [SerializeField] private Mask mask, fillMask;
    public GameObject body;
    [SerializeField] private Transform clock, pointer, fillTrans;
    private Action<bool> callback;
    private Action noFocusAction;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        ResetData();
        base.OnInit(uiGroup, completeAction, userData);
    }

    public void ResetData()
    {
        // 初始化逻辑
        fill.fillAmount = 1f;
        mask.enabled = true;
        fillMask.enabled = false;
        fillTrans.localPosition = Vector3.left * 400;
        redFlash.DOKill();
        redFlash.gameObject.SetActive(false);
    }

    public void PlayShowAnim()
    {
        // 出现动画
        body.SetActive(true);
        fillTrans.DOLocalMoveX(0, 0.5f).onComplete += () =>
        {
            mask.enabled = false;
            fillMask.enabled = true;
            UpdateClock();
            StartClock();
        };
    }

    public override void OnRelease()
    {
        // 重置标志位
        isStart = false;
        // 结束动画，防止界面未重用导致显示错误
        fill.DOKill();
        fillTrans.DOKill();
        //Log.Error("Event 移除");
        // 移除响应
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnStartClock);
        base.OnRelease();
    }

    public void SetLevelTotalTime(float second)
    {
        if (second <= 0) return;
        totalTime = remainingTime = second;
        //Log.Error("Event 注册");
        // 注册响应
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnStartClock);
        UpdateClock();
    }

    public void AddRemainingTime(float second)
    {
        if (totalTime <= 0) return;
        float time = remainingTime + second;
        if (time > totalTime)
            totalTime = time;
        fill.DOFillAmount(time / totalTime, (time - remainingTime) / (totalTime * 2)).SetEase(Ease.Linear);
        isStartAddTime = true;
        timer = 0;
    }

    public void RefreshCountdown()
    {
        if (totalTime <= 0) return;
        fill.DOFillAmount(1, (totalTime - remainingTime) / (totalTime * 2)).SetEase(Ease.Linear);
        isStartAddTime = true;
        timer = 0;
    }

    public void OnStartClock(Object obj, GameEventArgs args)
    {
        if (totalTime <= 0) return;
        CommonEventArgs ne = (CommonEventArgs)args;

        if (ne.Type == CommonEventType.PauseLevelTime)
        {
            PauseClock();
        }
        else if(ne.Type == CommonEventType.ContinueLevelTime || ne.Type == CommonEventType.ShopBuyGetRewardComplete)
        {
            StartClock();
        }
    }
    
    public void StartClock()
    {
        isStart = true;
        timer = 0;
        if (remainingTime > totalTime)
            totalTime = remainingTime;
        fill.fillAmount = (float)remainingTime / totalTime;
        float time = remainingTime;
        if (IsRun)
        {
            fill.DOKill();
            fill.DOFillAmount(0, time).SetEase(Ease.Linear);
        }
    }

    public void PauseClock()
    {
        isStart = false;
        if (redFlash)
        {
            redFlash.DOKill();
            redFlash.gameObject.SetActive(false);
        }
        fill.DOKill();
        // 重置闹钟状态
        isShakeClock = false;
        clock.DOKill();
        clock.localScale = Vector3.one * 0.4f;
        if (soundId != null)
        {
            GameManager.Sound.StopSound(soundId.Value);
            soundId = null;
        }
    }

    private float timer = 0;
    private float pointerTimer = 0;
    private float redTimer = 0;
    private bool isShakeClock = false;
    private int? soundId = null;
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (IsRun)
        {
            timer += elapseSeconds;
            pointerTimer += elapseSeconds;
            remainingTime -= elapseSeconds;

            // 需要在CountDownOver之前调用，之后调用会导致redFlash的动画被重新开启
            if (remainingTime < 10f)
            {
                if (!redFlash.gameObject.activeSelf)
                {
                    Color color = redFlash.color;
                    color.a = 0;
                    redFlash.color = color;
                    redFlash.gameObject.SetActive(true);
                    redFlash.DOFade(1, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
                if (!isShakeClock)
                {
                    // 增加启动钟表晃动的逻辑
                    clock.localScale *= 1.2f;
                    clock.DOShakeRotation(1f, new Vector3(15, 0, 15), 6, 180, false).SetLoops(-1, LoopType.Yoyo);
                    isShakeClock = true;
                    // 播放钟表音效
                    soundId = GameManager.Sound.PlayAudio(SoundType.FPX_Clock_Ticking.ToString(), 10, true);
                }
            }
            else
            {
                if (redFlash.gameObject.activeSelf)
                {
                    redFlash.DOKill();
                    redFlash.gameObject.SetActive(false);
                }
            }
            
            if (timer >= 0.2f)
            {
                timer -= 0.2f;
                if (remainingTime <= 0)
                {
                    CountDownOver();
                }
                UpdateClock();
            }

            if (pointerTimer >= 1f)
            {
                pointerTimer = 0;
                pointer.DOBlendableLocalRotateBy(new Vector3(0, 0, -90f), 0.2f);
            }
        }

        if (isStartAddTime)
        {
            if (!Mathf.Approximately(totalTime, remainingTime))
            {
                timer += elapseSeconds;
                if (timer > 0.1f)
                {
                    timer -= 0.1f;
                    float dValue = totalTime / (0.5f / 0.1f);// 每0.1s的增长速度,0.5s为动画的最长时长
                    remainingTime = Mathf.Min(remainingTime + dValue, totalTime);
                    UpdateClock();
                }
            }
            if (remainingTime >= totalTime)
            {
                remainingTime = totalTime;
                StartClock();
                isStartAddTime = false;
            }
        }
        #if UNITY_EDITOR
        /// 关卡编辑计时器
        if (isEditorTimer)
        {
            timeTimer += elapseSeconds;
            UpdateEditorTimer();
        }
        #endif
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void UpdateClock()
    {
        int min = (int)remainingTime / 60;
        int s = (int)remainingTime % 60;
        countDown.text = string.Format("{0:D2}:{1:D2}", min, s);
    }

    public void SetCountDownAction(Action<bool> action)
    {
        callback = action;
    }
    
    private void CountDownOver()
    {
        // PauseClock();
        // 触发通关失败
        callback?.InvokeSafely(IsTimeOver);
    }

    public void SetNoFoucsAction(Action action)
    {
        noFocusAction = action;
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        if (totalTime <= 0) return;
        isFocus = hasFocus;
        if (IsRun)
        {
            StartClock();
        }
        else
        {
            fill.DOKill();
            if (isStart)
            {
                noFocusAction?.Invoke();
            }
        }
    }
    
    public Vector3 GetClockTarget()
    {
        return clock.position;
    }

#if UNITY_EDITOR
    private bool isEditorTimer = false;
    private float timeTimer = 0;
    public TextMeshProUGUI editorTimer;

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        timeTimer = 0;
        if(editorTimer!=null)editorTimer.gameObject.SetActive(false);
        base.OnHide(hideSuccessAction, userData);
    }

    public void EditorTimer()
    {
        totalTime = -1;
        timeTimer = 0;
        isStart = false;
        isEditorTimer = true;
        editorTimer.gameObject.SetActive(true);
        editorTimer.text = "0";
    }

    public void UpdateEditorTimer()
    {
        editorTimer.text = string.Format("通关时长：{0:D3}s", (int)timeTimer);
    }
#endif
}
