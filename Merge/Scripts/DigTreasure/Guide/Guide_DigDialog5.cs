using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    //Max level reached! Collect your rare crystal for points!
    public sealed class Guide_DigDialog5 : Guide
    {
        private PropLogic m_TargetProp;

        public Guide_DigDialog5(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DigDialog5;

        public override PropLogic TargetProp => m_TargetProp;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType && userData != null)
            {
                m_TargetProp = (PropLogic)userData;

                if (MergeManager.Merge.LastSelectedProp != m_TargetProp)
                {
                    MergeManager.Merge.SelectedProp = m_TargetProp;
                    MergeManager.Merge.SelectedProp = m_TargetProp;
                }

                return true;
            }

            return false;
        }

        public override void CheckCanComplete(GuideTriggerType triggerType, object userData)
        {
        }

        public override GuideState GetGuideState()
        {
            if (m_IsShowGuideFinished)
            {
                if (m_TargetProp == null || m_TargetProp.Prop == null)
                    return GuideState.Completed;
            }

            return base.GetGuideState();
        }

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            m_IsShowGuideFinished = true;

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null && m_TargetProp != null && m_TargetProp.Square != null)
            {
                m_TargetProp.Prop.SetLayer("UI", 11);

                m_GuideMenu.Show("DigMerge.GuideDialog5", m_TargetProp.Square.transform.position, 0.3f);
                m_GuideMenu.ShowFingerAnim(m_TargetProp.Square.transform.position, m_TargetProp.Square.transform.position);
                m_GuideMenu.ShowRaycastMask(m_TargetProp.Square.transform.position, 30, 30, 0.8f);
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            m_GuideMenu.Hide();
            if (m_TargetProp != null && m_TargetProp.Prop != null)
                m_TargetProp.Prop.SetLayer("UI", 7);

            base.OnGuideFinish();

            GameManager.Task.AddDelayTriggerTask(0.7f, () =>
            {
                m_GuideMenu?.TriggerGuide(GuideTriggerType.Guide_DigDialog6);
            });
        }
    }
}
