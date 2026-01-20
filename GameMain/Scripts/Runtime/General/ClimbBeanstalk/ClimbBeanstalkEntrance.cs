using GameFramework.Event;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClimbBeanstalkEntrance : EntranceUIForm
{
    [SerializeField]
    private TextMeshProUGUI winStreakNumText;
    [SerializeField]
    private CountdownTimer countdownTimer;
    [SerializeField] private ParticleSystem ParticleSystem;
    [SerializeField] private GameObject previewBanner;
    [SerializeField] private Image mainImage;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        Log.Info("ClimbBeanstalk：初始化爬藤入口");
        UpdateBtnActiveSelfAndNumText();
        SetTimer();

        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnEventRecieved);

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventRecieved);

        base.OnRelease();
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public override void OnButtonClick()
    {
        if(GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockClimbBeanstalkEventLevel)
        {
            ShowUnlockPromptBox(Constant.GameConfig.UnlockClimbBeanstalkEventLevel);
            return;
        }

        if (GameManager.Process.Count > 0)
            return;

        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkEventLevel && ClimbBeanstalkManager.Instance.CheckActivityHasStarted())
            GameManager.UI.ShowUIForm("ClimbBeanstalkMenu");
        // else if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkButtonLevel)
        //     GameManager.UI.ShowUIForm("ClimbBeanstalkWelcomeMenu");
        //GameManager.UI.ShowUIForm<ClimbBeanstalkWelcomeMenu>();
    }

    public override void OnLocked()
    {
        previewBanner.SetActive(true);

        mainImage.color = Color.gray;

        base.OnLocked();
    }

    public override void OnUnlocked()
    {
        previewBanner.SetActive(false);

        mainImage.color = Color.white;

        base.OnUnlocked();
    }

    private void OnEventRecieved(object sender, GameEventArgs e)
    {
        var ne = e as CommonEventArgs;
        if (ne == null) return;
        if (ne.Type == CommonEventType.ClimbBeanstalkInfoChanged)
        {
            UpdateBtnActiveSelfAndNumText();
            SetTimer();
        }
    }

    private void UpdateBtnActiveSelfAndNumText()
    {
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkEventLevel && ClimbBeanstalkManager.Instance.CheckActivityHasStarted())
        {
            OnUnlocked();
            gameObject.SetActive(true);
            winStreakNumText.gameObject.SetActive(true);
            if (ClimbBeanstalkManager.Instance.NeedFlyReward)//会在 FlyReward 过程中再修改会正确值
                winStreakNumText.text = (ClimbBeanstalkManager.Instance.CurrentWinStreak - 1).ToString();
            else
                winStreakNumText.text = ClimbBeanstalkManager.Instance.CurrentWinStreak.ToString();
        }
        else if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkEventLevel &&
                 ClimbBeanstalkManager.Instance.CheckActivityHasStarted())
        {
            OnUnlocked();
            gameObject.SetActive(true);
            winStreakNumText.gameObject.SetActive(true);
            winStreakNumText.text = "0";
        }
        //else if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockClimbBeanstalkPreviewLevel &&
        //    GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockClimbBeanstalkEventLevel &&
        //    GameManager.Activity.CheckActivityCanStart(ClimbBeanstalkManager.Instance.ActivityID)) 
        //{
        //    OnLocked();
        //    winStreakNumText.gameObject.SetActive(false);
        //    gameObject.SetActive(true);
        //}
        else
        {
            gameObject.SetActive(false);
            winStreakNumText.gameObject.SetActive(false);
        }
    }

    private void SetTimer()
    {
        DateTime endTime = GameManager.Activity.GetCurActivityEndTime();//GameManager.DataTable.GetDataTable<DTClimbBeanstalkScheduleData>().Data.GetNowActiveActivityEndTime();
        Log.Info($"ClimbBeanstalk：当前的活动剩余时间{endTime}");
        if (endTime <= DateTime.Now)
        {
            countdownTimer.timeText.gameObject.SetActive(false);
        }
        else
        {
            countdownTimer.timeText.gameObject.SetActive(true);
            countdownTimer.StartCountdown(endTime);
            countdownTimer.CountdownOver += OnCountdownOver;
        }
    }
    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        countdownTimer.timeText.gameObject.SetActive(false);
    }

    private void Update()
    {
        countdownTimer.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
    }

    public void FlyReward()
    {
        int currentWinStreak = ClimbBeanstalkManager.Instance.CurrentWinStreak;

        if (ClimbBeanstalkManager.Instance.NeedFlyReward)
        {
            winStreakNumText.text = (currentWinStreak - 1).ToString();
            int flyNum = 1;
            EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent(GetFlyIconName(), flyNum, 21,
                new Vector3(250f, 0),
                Vector3.zero, gameObject,
                () =>
                {
                    ClimbBeanstalkManager.Instance.NeedFlyReward = false;
                }, () =>
                {
                    if (ParticleSystem) ParticleSystem.Play();
                    winStreakNumText.text = currentWinStreak.ToString();
                    EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
                    //if (reachEffect != null)
                    //{
                    //    reachEffect.Play();
                    //}
                }, false);
        }
    }

    public virtual string GetFlyIconName()
    {
        return "ClimbBeanstalk_EasterCommon[up]";
    }
}
