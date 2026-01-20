using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeGuideMenu_Christmas : MergeGuideMenu
    {
        protected override void InitializeGuideList()
        {
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapBox)) m_GuideList.Add(new Guide_TapBox(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DragMerge)) m_GuideList.Add(new Guide_DragMerge(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_KeepMerging)) m_GuideList.Add(new Guide_KeepMerging(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_SpecialItems)) m_GuideList.Add(new Guide_SpecialItems(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapSpecialItem)) m_GuideList.Add(new Guide_TapSpecialItem(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_MaxLevel)) m_GuideList.Add(new Guide_MaxLevel(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_BoardFull)) m_GuideList.Add(new Guide_BoardFull(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_TapMaxReward)) m_GuideList.Add(new Guide_TapMaxReward(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_Web)) m_GuideList.Add(new Guide_Web(this));
            if (!CheckGuideIsComplete(GuideTriggerType.Guide_DecorateChristmasTree)) m_GuideList.Add(new Guide_DecorateChristmasTree(this));
        }
    }   
}
