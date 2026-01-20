using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalLosePanel : BaseGameFailPanel
{
    public override bool IsShowFailPanel => true;

    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.NormalGameFailPanel;

    public GameObject AdsPanel, NoAdsPanel;
    
    public override void ShowFailPanel(Action finishAction)
    {
        bool isShowAdsBtn = GameManager.DataNode.GetData("isShowAdsBtn", false);
        
        AdsPanel.SetActive(isShowAdsBtn);
        NoAdsPanel.SetActive(!isShowAdsBtn);
    }
}
