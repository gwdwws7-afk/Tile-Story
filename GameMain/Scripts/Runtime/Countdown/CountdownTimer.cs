using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 倒计时器
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    // 用于显示提示文本(如活动结束、无网络连接等)
    public GameObject tipText;
    public float refreshInterval;

    protected DateTime targetTime;
    protected TimeSpan leftTime;

    private EventHandler<CountdownOverEventArgs> countdownOverEventHandler;
    private Action countdownOverAction;
    private float currentRefreshInterval;
    private bool isStart;
    private bool countDownTextUseDay = true;
    
    
    private bool isRefreshTime;
    
    public TimeSpan LeftTime => leftTime;
    
    private TextMeshProUGUILocalize tmpText=null;
    private TextMeshProUGUILocalize TMPText
    {
        get
        {
            if (tmpText == null)
            {
                tmpText = tipText.GetComponent<TextMeshProUGUILocalize>();
                if (tmpText == null) tmpText = tipText.AddComponent<TextMeshProUGUILocalize>();
            }
            return tmpText;
        }
    }

    private const string Offline = "Common.Offline";
    private const string Finished = "Story.Finished";

    /// <summary>
    /// 倒计时器是否启动
    /// </summary>
    public bool IsStart
    {
        get { return isStart; }
    }

    public bool CountDownTextUseDay
    {
        set { countDownTextUseDay = value; }
    }

    /// <summary>
    /// 倒计时结束的事件
    /// </summary>
    public event EventHandler<CountdownOverEventArgs> CountdownOver
    {
        add
        {
            countdownOverEventHandler -= value;
            countdownOverEventHandler += value; 
        }
        remove { countdownOverEventHandler -= value; }
    }

    public void StartCountdown(DateTime targetTime)
    {
        this.targetTime = targetTime;
        countdownOverAction = null;
        OnRefresh();

        isStart = true;
        isRefreshTime = true;
    }

    public void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (countdownOverAction != null)
        {
            countdownOverAction?.Invoke();
            countdownOverAction = null;
        }

        if (!isStart)
        {
            return;
        }
        
        if (!isRefreshTime)
        {
            return;
        }

        if (currentRefreshInterval >= refreshInterval)
        {
            currentRefreshInterval = 0;
            OnRefresh();
        }
        else
        {
            currentRefreshInterval += realElapseSeconds;
        }
    }

    public void OnReset()
    {
        isRefreshTime = false;
        isStart = false;

        if (countdownOverEventHandler != null)
        {
            Delegate[] delArray = countdownOverEventHandler.GetInvocationList();
            for (int i = 0; i < delArray.Length; i++)
            {
                countdownOverEventHandler -= (EventHandler<CountdownOverEventArgs>)delArray[i];
            }
        }

        countdownOverAction = null;

        leftTime = TimeSpan.Zero;
        currentRefreshInterval = 0;
    }

    private void OnRefresh()
    {
        leftTime = targetTime - DateTime.Now;

        RefreshTimeText();

        if (leftTime.TotalSeconds <= 0)
        {
            //下一帧触发倒计时结束事件
            countdownOverAction = OnCountdownOver;
        }
    }

    private void RefreshTimeText()
    {
        if (ReferenceEquals(timeText,null))
        {
            return;
        }
        
        if (leftTime.TotalSeconds <= 0)
        {
            if (tipText && !TMPText.Term.Equals(Offline))
            {
                timeText.gameObject.SetActive(false);
                tipText.SetActive(true);
                TMPText.SetTerm(Finished);
            }
        }else
        {
            if (tipText)
            {
                timeText.gameObject.SetActive(true);
                tipText.SetActive(false);
            }
            
            if (leftTime.Days <= 0 || !countDownTextUseDay) 
            {
                int hour = leftTime.Days <= 0 ? leftTime.Hours : leftTime.Days * 24 + leftTime.Hours;

                timeText.text = hour <= 0
                    ? $"{leftTime.Minutes:D2}:{leftTime.Seconds:D2}"
                    : $"{hour:D2}:{leftTime.Minutes:D2}:{leftTime.Seconds:D2}";
            }
            else
            {
                timeText.text = $"{leftTime.Days}d {leftTime.Hours}h";
            }
        }
    }

    private void OnCountdownOver()
    {
        isRefreshTime = false;
        isStart = false;

        if (countdownOverEventHandler != null)
        {
            countdownOverEventHandler.Invoke(this, new CountdownOverEventArgs());
        }
    }
    
    private bool offlineStatus = false;
    public void SetOffline(bool isOffline)
    {
        if (offlineStatus != isOffline)
        {
            if (isOffline)
            {
                isRefreshTime = false;
                if (tipText != null)
                {
                    timeText.gameObject.SetActive(false);
                    tipText.SetActive(true);
                    TMPText.SetTerm(Offline);
                }
            }
            isRefreshTime = !isOffline;
            offlineStatus = isOffline;
        }
    }
}