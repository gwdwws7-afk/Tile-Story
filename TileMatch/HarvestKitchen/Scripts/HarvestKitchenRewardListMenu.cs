using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestKitchenRewardListMenu : PopupMenuForm
{
    public ScrollArea scrollArea;
    public ClockBar clockBar;
    public DelayButton closeButton;
    public ItemPromptBox itemPromptBox;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        
        closeButton.OnInit(OnCloseButtonClick);
        clockBar.StartCountdown(HarvestKitchenManager.Instance.EndTime);
        
        List<DTHarvestKitchenTaskDatas> stageList = HarvestKitchenManager.Instance.TaskData.KitchenTaskDatas;
        for (int i = stageList.Count - 1; i >= 0; i--)
        {
            scrollArea.AddColumnLast(new HarvestKitchenRewardScrollColumn("HarvestKitchenRewardColumn", stageList[i], this, 235f));
        }
        scrollArea.currentIndex = stageList.Count - HarvestKitchenManager.Instance.TaskId;
        scrollArea.OnInit(GetComponent<RectTransform>());
    }

    public override void OnReset()
    {
        itemPromptBox.HidePromptBox();
        clockBar.OnReset();
        scrollArea.OnReset();
        time = 5;
        
        base.OnReset();
    }

    public override void OnRelease()
    {
        scrollArea.OnRelease();
        
        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
        scrollArea.OnUpdate(elapseSeconds, realElapseSeconds);
        
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
        {
            itemPromptBox.HidePromptBox();
        }
    }

    private int time = 5;
    
    public override bool CheckInitComplete()
    {
        time--;
        if (time > 0)
            return false;
        
        return base.CheckInitComplete();
    }

    private void OnCloseButtonClick()
    {
        GameManager.UI.HideUIForm(this);
    }
}
