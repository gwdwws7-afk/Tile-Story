using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GoldCollectionLosePanel : BaseGameFailPanel
{
    public override bool IsSpecialPanel => false;

    public override bool IsShowFailPanel => DTLevelUtil.IsSpecialGoldTile(GameManager.PlayerData.RealLevel()) &&
                                            GameManager.DataNode.GetData("GoldTileCurrentCount", 0) > 0;

    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.GoldCollectionLosePanel;

    public GameObject goldArea;
    public TextMeshProUGUI goldCount;
    public GameObject sliderArea;
    public SimpleSlider slider;
    public TaskFlyReward taskReward;
    public TextMeshProUGUILocalize describeText;

    public override void ShowFailPanel(Action finishAction)
    {
        int levelCollectNum = GameManager.DataNode.GetData("GoldTileCurrentCount", 0);
        int totalCollectNum = GameManager.Task.GoldCollectionTaskManager.TotalCollectNum;
        DTGoldCollectionData dataTable = GameManager.Task.GoldCollectionTaskManager.DataTable;
        int needCollectNum = dataTable.GetNeedCollectNum(GameManager.Task.GoldCollectionTaskManager.CurrentIndex);
        if (totalCollectNum + levelCollectNum < needCollectNum)
        {
            goldCount.text = levelCollectNum.ToString();
            goldArea.transform.localScale = Vector3.zero;
            goldArea.SetActive(true);
            goldArea.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack,1.4f);
            sliderArea.SetActive(false);
            describeText.SetTerm("Gold.LoseGolds");
            describeText.SetParameterValue("0", "<color=#217F02>");
            describeText.SetParameterValue("1", "</color>");
        }
        else
        {
            RewardTask currentTask = GameManager.Task.GoldCollectionTaskManager.CurrentTask;
            if (currentTask != null)
            {
                slider.CurrentNum = slider.TotalNum = currentTask.TargetNum;
                taskReward.OnReset();
                taskReward.Init(currentTask.Reward, currentTask.RewardNum, currentTask.Reward.Count, false);
                goldArea.SetActive(false);
                sliderArea.SetActive(true);
                describeText.SetTerm("Gold.LoseTaskReward");
            }
            else
            {
                Log.Error("GoldCollection current task is null!");
            }
        }
    }
}
