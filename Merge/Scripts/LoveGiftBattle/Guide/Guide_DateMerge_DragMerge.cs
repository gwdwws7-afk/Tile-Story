using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    //You've got a flower!Drag to merge two of them!
    public sealed class Guide_DateMerge_DragMerge : Guide
    {
        public Guide_DateMerge_DragMerge(MergeGuideMenu guideMenu) :base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DragMerge;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType)
                return true;

            return false;
        }

        public override void CheckCanComplete(GuideTriggerType triggerType, object userData)
        {
        }

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            m_IsShowGuideFinished = true;

            //如果用户提前合掉，结束该教程
            Square square1 = MergeManager.Merge.GetSquare(3, 2);
            Square square2 = MergeManager.Merge.GetSquare(3, 3);

            if (square1.FilledProp == null || square2.FilledProp == null || square1.FilledProp.PropId != 10101 || square2.FilledProp.PropId != 10101) 
            {
                m_IsGuideCompleted = true;
                return;
            }

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                string guideContent = "DateMerge.GuideMergeCommon";

                MergeGuideMenu_LoveGiftBattle menu = m_GuideMenu as MergeGuideMenu_LoveGiftBattle;
                menu.ShowThiefDialog = false;
                m_GuideMenu.Show(guideContent, square1.transform.position);
                m_GuideMenu.ShowFingerAnim(square1.transform.position, square2.transform.position);
                m_GuideMenu.ShowRaycastMask((square1.transform.position + square2.transform.position) / 2, 150, 75, 0.8f);
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            m_GuideMenu.Hide();

            base.OnGuideFinish();

            GameManager.Task.AddDelayTriggerTask(0.6f, () =>
            {
                m_GuideMenu?.TriggerGuide(Merge.GuideTriggerType.Guide_DateMerge5);
            });
        }
    }
}
