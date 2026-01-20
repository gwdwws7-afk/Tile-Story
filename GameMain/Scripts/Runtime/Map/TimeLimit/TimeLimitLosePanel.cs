using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeLimitLosePanel : BaseGameFailPanel
{
    public TextMeshProUGUILocalize infoText, tileText;
    public TextMeshProUGUI addTimeText1, addTimeText2;
    public GameObject noTileFail, noTimeFail;
    public int addTime = 0;
    public bool isOverTime = false;

    public override bool IsSpecialPanel => true;
    
    public override bool IsShowFailPanel 
    { 
        get
        {
            var panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;

            if (panel == null) return false;
            return panel.IsTimeLimitLevel();
        }
    }

    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.TimeLimitLosePanel;
    
    public override void ShowFailPanel(Action finishAction)
    {
        isOverTime = GameManager.DataNode.GetData("TimeLimit_IsTimeOver", false);
        addTime = GameManager.DataNode.GetData("TimeLimit_AddTime", 0);
        if (!isOverTime)
        {
            noTileFail.SetActive(true);
            noTimeFail.SetActive(false);
            addTimeText1.text = $"+{addTime}";
            tileText.SetTerm("Shop.No More Space!");
            infoText.SetTerm("Calendar.Undo the last 3 tiles in the rack and add {0} seconds to the timer!");
        }
        else
        {
            noTileFail.SetActive(false);
            noTimeFail.SetActive(true);
            addTimeText2.text = $"+{addTime}";
            tileText.SetTerm("Calendar.Time Is Up!");
            infoText.SetTerm("Calendar.Add {0} seconds to the timer and continue playing!");
        }
        infoText.SetParameterValue("0", addTime.ToString());
    }
}
