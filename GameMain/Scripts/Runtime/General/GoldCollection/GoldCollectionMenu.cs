using System;
using System.Collections.Generic;
using UnityEngine;

public class GoldCollectionMenu : PopupMenuForm
{
    public DelayButton tipButton;
    public DelayButton closeButton;
    public SimpleSlider slider;
    public TaskFlyReward taskReward;
    public DelayButton chestButton;
    public ClockBar clockBar;
    public ScrollArea scrollArea;
    public ItemPromptBox itemPromptBox;
    public TextPromptBox textPromptBox;

    private DTGoldCollectionData goldCollectionData;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        tipButton.OnInit(OnTipButtonClick);
        closeButton.OnInit(OnClose);
        chestButton.OnInit(OnChestButtonClick);
        SetClockBar();

        goldCollectionData = GameManager.Task.GoldCollectionTaskManager.DataTable;
        SetSlider();
        SetScrollArea();

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        scrollArea.OnUpdate(elapseSeconds, realElapseSeconds);

        if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) && Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                itemPromptBox.HidePromptBox();
                textPromptBox.HidePromptBox();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                itemPromptBox.HidePromptBox();
                textPromptBox.HidePromptBox();
            }
        }
    }

    public override void OnReset()
    {
        base.OnReset();
        tipButton.OnReset();
        closeButton.OnReset();
        chestButton.OnReset();
        slider.OnReset();
        taskReward.OnReset();
        scrollArea.OnReset();
        timer = 6;
    }

    public override void OnRelease()
    {
        base.OnRelease();
        taskReward.OnRelease();
        scrollArea.OnRelease();
        itemPromptBox.OnRelease();
        textPromptBox.OnRelease();
    }

    private int timer = 6;
    public override bool CheckInitComplete()
    {
        if (!scrollArea.CheckSpawnComplete())
            return false;

        if (timer > 0)
        {
            timer--;
            return false;
        }
        
        return base.CheckInitComplete();
    }

    public override void OnClose()
    {
        base.OnClose();
        GameManager.UI.HideUIForm(this);
    }

    private void OnTipButtonClick()
    {
        GameManager.UI.ShowUIForm("GoldCollectionRules");
    }

    private void SetClockBar()
    {
        DateTime endTime = GameManager.Task.GoldCollectionTaskManager.EndTime;
        if (endTime <= DateTime.Now)
        {
            clockBar.gameObject.SetActive(false);
        }
        else
        {
            clockBar.OnReset();
            clockBar.StartCountdown(endTime);
            clockBar.CountdownOver += OnCountdownOver;
            clockBar.gameObject.SetActive(true);
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        clockBar.gameObject.SetActive(false);
    }

    private void SetSlider()
    {
        if (GameManager.Task.GoldCollectionTaskManager.CurrentTask != null)
        {
            int totalCollectNum = GameManager.Task.GoldCollectionTaskManager.TotalCollectNum;
            GameManager.Task.GoldCollectionTaskManager.LastRecordTotalCollectNum = totalCollectNum;
            RewardTask currentTask = GameManager.Task.GoldCollectionTaskManager.CurrentTask;
            int lastIndexTotalCollectNum = goldCollectionData.GetNeedCollectNum(currentTask.Index - 1);
            slider.TotalNum = currentTask.TargetNum;
            slider.CurrentNum = totalCollectNum - lastIndexTotalCollectNum;

            taskReward.OnReset();
            taskReward.Init(currentTask.Reward, currentTask.RewardNum, currentTask.Reward.Count, false);
        }
        else
        {
            slider.transform.parent.gameObject.SetActive(false);
        }
    }

    private void SetScrollArea()
    {
        List<GoldCollectionStage> stageList = goldCollectionData.GoldCollectionStages;
        for (int i = stageList.Count - 1; i >= 0; i--)
        {
            scrollArea.AddColumnLast(new GoldCollectionScrollColumn("GoldCollectionColumn", stageList[i], this, 215f));
        }
        scrollArea.currentIndex = stageList.Count - GameManager.Task.GoldCollectionTaskManager.CurrentIndex;
        scrollArea.OnInit(GetComponent<RectTransform>());
    }

    private void OnChestButtonClick()
    {
        if (taskReward.RewardTypeList.Count > 1)
        {
            itemPromptBox.Init(taskReward.RewardTypeList, taskReward.RewardNumList);
            itemPromptBox.ShowPromptBox(PromptBoxShowDirection.Down, taskReward.transform.position);
        }
    }
}
