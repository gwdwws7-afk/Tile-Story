using System;
using TMPro;

public class GlacierQuestLosePanel : BaseGameFailPanel
{
    public TextMeshProUGUI curProgress;
    public TextMeshProUGUILocalize describeText;

    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.GlacierQuestLosePanel;

    public override bool IsShowFailPanel => GameManager.Task.GlacierQuestTaskManager.ActivityState == GlacierQuestState.Open;

    public override void ShowFailPanel(Action finishAction)
    {
        if (GameManager.Localization.Language != Language.Arabic)
        {
            describeText.SetParameterValue("0", "<color=#EF002A>");
            describeText.SetParameterValue("1", "</color>");
        }
        else
        {
            describeText.SetParameterValue("0", "<color=#EF002A>");
            describeText.SetParameterValue("1", "</color>");
        }

        curProgress.text = (GameManager.Task.GlacierQuestTaskManager.CurLevel + 1).ToString();
    }
}
