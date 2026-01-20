using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    //Double tap the bouquet to knock the gifts off the thief!
    public sealed class Guide_TapMaxLevelBouquet : Guide
    {
        private PropLogic m_TargetProp;

        public Guide_TapMaxLevelBouquet(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_TapMaxLevelBouquet;

        public override PropLogic TargetProp => m_TargetProp;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideTriggerType.Guide_TapMaxLevelBouquet && userData != null)
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

                MergeGuideMenu_LoveGiftBattle menu = m_GuideMenu as MergeGuideMenu_LoveGiftBattle;
                menu.ShowThiefDialog = false;
                m_GuideMenu.Show("DateMerge.GuideDialog7", m_TargetProp.Square.transform.position, 0.3f);
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
        }
    }
}
