using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GlacierQuestStartMenu : UIForm
{
    public DelayButton closeButton, playButton;
    public GameObject startText, failText, successText;
    [SerializeField] private Transform cachedTransform;
    [SerializeField] private Image bgImage;
    [SerializeField] private ClockBar clockBar;
    // 倒计时回调
    private Action m_Callback;
    private Vector3 m_Pos = Vector3.zero;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        DateTime endTime = DateTime.MinValue;
        closeButton.SetBtnEvent(OnClickCloseBtn);
        Transform clockBarTrans = clockBar.transform;
        switch (GameManager.Task.GlacierQuestTaskManager.ActivityState)
        {
            case GlacierQuestState.Wait:
                // 显示等待开始界面
                m_Callback = () =>
                {
                    GameManager.UI.HideUIForm(this);
                    GameManager.Process.EndProcess(ProcessType.GlacierQuest);
                };
                playButton.SetBtnEvent(OnClickPlayBtn);
                playButton.gameObject.SetActive(true);
                startText.SetActive(true);
                failText.SetActive(false);
                successText.SetActive(false);
                if (m_Pos != Vector3.zero)
                {
                    clockBarTrans.position = m_Pos;
                    clockBarTrans.localScale /= 1.25f;
                    m_Pos = Vector3.zero;
                }

                endTime = DateTime.Now.AddDays(1).Date;
                if (endTime.DayOfWeek == DayOfWeek.Friday && endTime.Hour > 15)
                    endTime = new DateTime(endTime.Year, endTime.Month, endTime.Day, 15, 0, 0);
                else if (endTime.DayOfWeek > DayOfWeek.Friday)// 结束时间大于周四，设置为当天的下午3点结束活动
                    endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 0, 0);
                break;
            case GlacierQuestState.Time:
            case GlacierQuestState.ClearTime:
                // 显示失败界面
                m_Pos = clockBarTrans.position;
                clockBarTrans.localScale *= 1.25f;
                clockBarTrans.position = playButton.transform.position;
                m_Callback = () =>
                {
                    GameManager.Task.GlacierQuestTaskManager.ActivityState = GlacierQuestState.Wait;
                    if (m_Pos != Vector3.zero)
                    {
                        clockBarTrans.position = m_Pos;
                        clockBarTrans.localScale /= 1.25f;
                        m_Pos = Vector3.zero;
                    }
                    OnInit(uiGroup);
                };
                playButton.gameObject.SetActive(false);
                startText.SetActive(false);
                failText.SetActive(true);
                successText.SetActive(false);
                endTime = GameManager.Task.GlacierQuestTaskManager.RestartTime;
                break;
            case GlacierQuestState.Clear:
                m_Callback = null;
                playButton.SetBtnEvent(() => GameManager.UI.HideUIForm(this));
                playButton.gameObject.SetActive(true);
                startText.SetActive(false);
                failText.SetActive(false);
                successText.SetActive(true);
                clockBar.gameObject.SetActive(false);
                break;
        }
        // 设置冰川副本的结束事件
        if (endTime != DateTime.MinValue)
        {
            clockBar.CountdownOver += OnCountDownOver;
            clockBar.StartCountdown(endTime);
        }

        bgImage.DOKill();
        bgImage.DOColor(new Color(1, 1, 1, 0.01f), 0);

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        GameManager.Task.GlacierQuestTaskManager.IsFirstPop = false;
        GameManager.Task.GlacierQuestTaskManager.NeedPopStart = false;
        if (cachedTransform)
        {
            cachedTransform.DOKill();
            cachedTransform.localScale = Vector3.one;
            cachedTransform.DOScale(1.03f, 0.12f).onComplete = () =>
            {
                cachedTransform.DOScale(0.99f, 0.1f).onComplete = () =>
                {
                    cachedTransform.DOScale(1f, 0.1f);
                    m_IsAvailable = true;
                };
            };
        }

        bgImage.DOFade(1, 0.2f);

        gameObject.SetActive(true);

        GameManager.Sound.PlayUIOpenSound();

        showSuccessAction?.Invoke(this);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        OnReset();
        if (cachedTransform) cachedTransform.localScale = Vector3.one;
        gameObject.SetActive(false);
        base.OnHide(hideSuccessAction, userData);
    }

    public override void OnReset()
    {
        clockBar.OnReset();
        StopAllCoroutines();
        base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        if (m_Pos != Vector3.zero)
        {
            Transform clockBarTrans = clockBar.transform;
            clockBarTrans.position = m_Pos;
            clockBarTrans.localScale /= 1.25f;
            m_Pos = Vector3.zero;
        }
        base.OnRelease();
    }

    private void OnCountDownOver(object sender, CountdownOverEventArgs e)
    {
        m_Callback?.Invoke();
    }

    private void OnClickCloseBtn()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.GlacierQuest);
    }

    private void OnClickPlayBtn()
    {
        GameManager.UI.HideUIForm(this);
        Log.Info("GlacierQuest：开启活动");
        if (GameManager.Task.GlacierQuestTaskManager.IsFirstOpen)
        {
            GameManager.Task.GlacierQuestTaskManager.IsFirstOpen = false;
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.LavaQuest_First_Open);
        }
        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.LavaQuest_Challenge_Start);
        GameManager.Task.GlacierQuestTaskManager.ActivityState = GlacierQuestState.Open;
        GameManager.Task.GlacierQuestTaskManager.IsRechallenge = false;
        GameManager.Task.GlacierQuestTaskManager.EndTime = DateTime.Now.AddDays(1);
        // 弹出组队面板
        GameManager.UI.ShowUIForm("GlacierQuestTeamMenu");
    }
}
