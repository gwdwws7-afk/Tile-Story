using System;
using UnityEngine;
using UnityEngine.UI;

public class GlacierQuestEntrance : EntranceUIForm
{
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private Image slider;
    [SerializeField] private GameObject startText;
    [SerializeField] private GameObject blue, green, red;
    [SerializeField] private GameObject previewBanner;
    [SerializeField] private Image bg;
    [SerializeField] private RawImage mainImage;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Task.GlacierQuestTaskManager.NewDayRefresh();
        UpdateBtnActiveSelfAndNumText();
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        UpdateTimer();
        UpdateTextShow();
        UpdateState();
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void UpdateTimer()
    {
        DateTime time = DateTime.MinValue;
        switch (GameManager.Task.GlacierQuestTaskManager.ActivityState)
        {
            case GlacierQuestState.Open:
                time = GameManager.Task.GlacierQuestTaskManager.EndTime;
                break;
            case GlacierQuestState.Time:
            case GlacierQuestState.ClearTime:
                time = GameManager.Task.GlacierQuestTaskManager.RestartTime;
                break;
            case GlacierQuestState.Clear:
            case GlacierQuestState.Wait:
            case GlacierQuestState.Close:
                time = DateTime.Now.AddDays(1) - DateTime.Now.TimeOfDay;
                break;
        }
        countdownTimer.OnReset();
        countdownTimer.CountdownOver += OnCountdownOver;
        countdownTimer.StartCountdown(time);
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        GameManager.Task.GlacierQuestTaskManager.UpdateActivityState();
        OnInit(null);
        OnShow();
    }

    public override void OnButtonClick()
    {
        if (!GameManager.Task.GlacierQuestTaskManager.IsUnlock)
        {
            ShowUnlockPromptBox(Constant.GameConfig.UnlockGlacierQuestLevel);
            return;
        }

        switch (GameManager.Task.GlacierQuestTaskManager.ActivityState)
        {
            case GlacierQuestState.Open:
            case GlacierQuestState.Clear:
                GameManager.UI.ShowUIForm("GlacierQuestMenu");
                break;
            case GlacierQuestState.Time:
            case GlacierQuestState.Wait:
            case GlacierQuestState.ClearTime:
                if (GameManager.Task.GlacierQuestTaskManager.IsCanClaimedReward)
                {
                    GameManager.UI.ShowUIForm("GlacierQuestRewardSet",form =>
                    {
                        (form as GlacierQuestRewardSet).SetClaimEvent(() =>
                        {
                            GameManager.UI.HideUIForm(form);
                            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
                            {
                                // 活动正在等待参与，弹出提示框
                                GameManager.UI.ShowUIForm("GlacierQuestStartMenu");
                            });
                        });
                    });
                }
                else
                {
                    GameManager.UI.ShowUIForm("GlacierQuestStartMenu");
                }
                break;
            default:
                break;
        }
    }

    public override void OnLocked()
    {
        if (IsLocked)
            return;

        previewBanner.SetActive(true);
        bg.color = Color.gray;
        mainImage.color = Color.gray;

        base.OnLocked();
    }

    public override void OnUnlocked()
    {
        if (!IsLocked)
            return;

        previewBanner.SetActive(false);
        bg.color = Color.white;
        mainImage.color = Color.white;

        base.OnUnlocked();
    }

    public void UpdateState()
    {
        switch (GameManager.Task.GlacierQuestTaskManager.ActivityState)
        {
            case GlacierQuestState.Clear:
            case GlacierQuestState.Close:
                // GameManager.UI.HideUIForm(this);
                gameObject.SetActive(false);
                break;
            case GlacierQuestState.Open:
                //进度条可视区域 起始值位0.174   结束值为0.826
                float value = (0.835f - 0.163f) * GameManager.Task.GlacierQuestTaskManager.CurLevel / 7f + 0.163f;
                slider.fillAmount = value;
                break;
            default:
                slider.fillAmount = 0;
                break;
        }
    }

    private void UpdateBtnActiveSelfAndNumText()
    {
        bool isShowEntrance = false;
        if (AddressableUtils.IsHaveAsset("GlacierQuestMenu"))
        {
            if (GameManager.Task.GlacierQuestTaskManager.CheckActivityIsOpen())
            {
                // 判断当前星期几
                if (GameManager.Task.GlacierQuestTaskManager.CheckIsInActivityOpenDay() ||
                    GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Open)
                {
                    isShowEntrance = true;
                    OnUnlocked();
                }
            }
            else if (!GameManager.Task.GlacierQuestTaskManager.IsUnlock 
                && GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockPreviewGlacierQuestLevel 
                && GameManager.Task.GlacierQuestTaskManager.CheckIsInActivityOpenDay()) 
            {
                isShowEntrance = true;
                OnLocked();
            }
        }

        gameObject.SetActive(isShowEntrance);
    }

    public void UpdateTextShow()
    {
        // 活动等待、开启的情况下 改为显示Start而不显示倒计时
        if (GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Wait)
        {
            green.SetActive(true);
            blue.SetActive(false);
            red.SetActive(false);
            countdownTimer.timeText.gameObject.SetActive(false);
            startText.SetActive(true);
        }
        else if (GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Open)
        {
            green.SetActive(false);
            blue.SetActive(true);
            red.SetActive(false);
            countdownTimer.timeText.gameObject.SetActive(true);
            GameManager.Localization.GetPresetMaterialAsync("Btn_Blue", countdownTimer.timeText.font.name, mat =>
            {
                countdownTimer.timeText.fontSharedMaterial = mat;
            });
            startText.SetActive(false);
        }
        else
        {
            green.SetActive(false);
            blue.SetActive(false);
            red.SetActive(true);
            GameManager.Localization.GetPresetMaterialAsync("Btn_Red", countdownTimer.timeText.font.name, mat =>
            {
                countdownTimer.timeText.fontSharedMaterial = mat;
            });
            countdownTimer.timeText.gameObject.SetActive(true);
            startText.SetActive(false);
        }
    }
}
