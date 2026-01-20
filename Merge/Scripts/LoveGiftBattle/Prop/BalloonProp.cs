using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class BalloonProp : NormalProp
    {
        public override void OnBounceEnd()
        {
            MergeMainMenuBase mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainMenu != null && PropLogic.AttachmentId == 0 && !MergeGuideMenu.CheckGuideIsComplete(GuideTriggerType.Guide_MergeBalloon))
            {
                GameManager.Task.AddDelayTriggerTask(0.8f, () =>
                {
                    if (mainMenu != null && PropLogic != null && !MergeGuideMenu.CheckGuideIsComplete(GuideTriggerType.Guide_MergeBalloon)) 
                        mainMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_MergeBalloon, PropLogic);
                });
            }

            base.OnBounceEnd();
        }
    }
}
