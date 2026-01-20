using System;
using TMPro;
using UnityEngine;

namespace Merge
{
    public class MergeLevelLoseMenu : BaseGameFailPanel
    {
        [SerializeField] private TextMeshProUGUI Num_Text;

        public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.MergeLevelLosePanel;

        public override bool IsShowFailPanel => MergeManager.Instance.CheckActivityHasStarted();

        public override void ShowFailPanel(Action action)
        {

        }
    }
}
