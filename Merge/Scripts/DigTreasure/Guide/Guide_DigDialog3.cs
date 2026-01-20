using UnityEngine;

namespace Merge
{
    //That's it! Try digging again!
    public sealed class Guide_DigDialog3 : Guide
    {
        public Guide_DigDialog3(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DigDialog3;

        private int delayTaskId;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType)
                return true;

            return false;
        }

        public override void CheckCanComplete(GuideTriggerType triggerType, object userData)
        {
        }

        public override GuideState GetGuideState()
        {
            if (m_IsShowGuideFinished)
            {
                if (MergeManager.Merge.GetSquare(2, 3).FilledProp == null) 
                    return GuideState.Completed;
            }

            return base.GetGuideState();
        }

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            delayTaskId = GameManager.Task.AddDelayTriggerTask(0.2f, () =>
              {
                  m_IsShowGuideFinished = true;
              });

            Square targetSquare = MergeManager.Merge.GetSquare(2, 3);
            if (targetSquare.FilledProp == null)
            {
                m_IsGuideCompleted = true;
                return;
            }

            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
            if (mainBoard != null)
            {
                MergeBoard_DigTreasure board = mainBoard.m_MergeBoard as MergeBoard_DigTreasure;

                m_GuideMenu.Show("DigMerge.GuideDialog3", new Vector3(0, mainBoard.m_PickaxeBarIcon.transform.position.y, 0), 0);
                m_GuideMenu.ShowFingerAnim(targetSquare.transform.position, targetSquare.transform.position);
                m_GuideMenu.ShowRaycastMask(targetSquare.transform.position, 72, 72, 0.8f);
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            GameManager.Task.RemoveDelayTriggerTask(delayTaskId);
            m_GuideMenu.Hide();

            base.OnGuideFinish();

            GameManager.Task.AddDelayTriggerTask(0.5f, () =>
            {
                m_GuideMenu?.TriggerGuide(GuideTriggerType.Guide_DigDialog4);
            });
        }
    }
}
