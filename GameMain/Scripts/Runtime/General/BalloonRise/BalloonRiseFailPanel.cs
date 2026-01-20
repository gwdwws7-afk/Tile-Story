using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BalloonRiseFailPanel : BaseGameFailPanel
{
    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.BalloonRiseLosePanel;

    public override bool IsShowFailPanel
    {
        get
        {
            return GameManager.Task.BalloonRiseManager.Score != 0 &&
                   !GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge;
        }
    }

    public TextMeshProUGUI score, target;
    public TextMeshProUGUILocalize describeText;
    public Transform targetTrans, cross;
    
    public override void ShowFailPanel(Action finishAction)
    {
        target.SetText($" / {GameManager.Task.BalloonRiseManager.StageTarget}");
        score.SetText(GameManager.Task.BalloonRiseManager.Score.ToString());

        describeText.SetParameterValue("0", "<color=#217F04>");
        describeText.SetParameterValue("1", "</color>");

        targetTrans.localScale = Vector3.zero;
        cross.localScale = Vector3.zero;
        targetTrans.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
        cross.DOScale(0.55f, 0.2f).SetEase(Ease.OutBack).SetDelay(0.1f);
    }
}
