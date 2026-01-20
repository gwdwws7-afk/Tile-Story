namespace Merge
{
    //You've got a swimfin!Drag to merge two of them!
    public sealed class Guide_DragMerge_Halloween : Guide
    {
        public Guide_DragMerge_Halloween(MergeGuideMenu guideMenu) : base(guideMenu)
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
            Square square1 = MergeManager.Merge.GetSquare(1, 2);
            Square square2 = MergeManager.Merge.GetSquare(1, 3);
            if (square1.FilledProp == null || square2.FilledProp == null || square1.FilledProp.PropId != 10101 || square2.FilledProp.PropId != 10101)
            {
                m_IsGuideCompleted = true;
                return;
            }
        }

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            m_IsShowGuideFinished = true;

            //����û���ǰ�ϵ��������ý̳�
            Square square1 = MergeManager.Merge.GetSquare(1, 2);
            Square square2 = MergeManager.Merge.GetSquare(1, 3);
            if (square1.FilledProp == null || square2.FilledProp == null || square1.FilledProp.PropId != 10101 || square2.FilledProp.PropId != 10101)
            {
                m_IsGuideCompleted = true;
                return;
            }

            MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
            if (mainBoard != null)
            {
                m_GuideMenu.Show("Merge.Guide2", square1.transform.position, 0.3f);
                m_GuideMenu.ShowFingerAnim(square1.transform.position, square2.transform.position);
                m_GuideMenu.ShowRaycastMask((square1.transform.position + square2.transform.position) / 2, 170, 85, 0.8f);
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
        }
    }
}
