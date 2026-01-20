using System;
using TMPro;
using UnityEngine;

public class ClimbBeanstalkLosePanel : BaseGameFailPanel
{
    [SerializeField]
    public TextMeshProUGUI recentWinStreakText, lastWinStreakText;

    public override bool IsShowFailPanel => ClimbBeanstalkManager.Instance.CheckActivityHasStarted() && ClimbBeanstalkManager.Instance.CurrentWinStreak > 0;

    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.ClimbBeanstalkLosePanel;

    public override void ShowFailPanel(Action finishAction)
    {
        int currentWinStreak = ClimbBeanstalkManager.Instance.CurrentWinStreak;
        recentWinStreakText.text = currentWinStreak.ToString();
        lastWinStreakText.text = (currentWinStreak - 1).ToString();
    }
}
