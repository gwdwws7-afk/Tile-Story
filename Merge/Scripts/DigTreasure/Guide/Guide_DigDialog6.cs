using UnityEngine;

namespace Merge
{
    //Keep collecting crystals to earn more points and rewards!
    public sealed class Guide_DigDialog6 : Guide
    {
        public Guide_DigDialog6(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DigDialog6;

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

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            delayTaskId = GameManager.Task.AddDelayTriggerTask(0.2f, () =>
              {
                  m_IsShowGuideFinished = true;
              });

            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
            if (mainBoard != null && mainBoard.gameObject.activeSelf) 
            {
                MergeBoard_DigTreasure board = mainBoard.m_MergeBoard as MergeBoard_DigTreasure;

                Vector3 targetPos = mainBoard.m_ProgressBarSlider.transform.position;
                m_GuideMenu.Show("DigMerge.GuideDialog6", Vector3.zero, 0);
                m_GuideMenu.ShowFingerAnim(targetPos, targetPos);
                m_GuideMenu.ShowRaycastMask(targetPos, 280, 75, 0.8f);
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

            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.DigTreasure_Merge_Task_Unlock, new Firebase.Analytics.Parameter("Stage", 0));

            base.OnGuideFinish();
        }
    }
}
