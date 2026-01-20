using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    //Tap the box to get an item!
    public sealed class Guide_TapBox : Guide
    {
        private GameObject m_Obj;

        public Guide_TapBox(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_TapBox;

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

            MergeMainMenu mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu;
            if (mainBoard != null)
            {
                m_GuideMenu.Show("Merge.Guide1", mainBoard.m_SupplyButton.transform.position, 0.5f);
                m_GuideMenu.ShowArrowAnim(mainBoard.m_SupplyButton.transform.position + new Vector3(0, 0.2f, 0), Quaternion.Euler(0, 0, 0));
                m_GuideMenu.ShowRaycastMask(Vector3.zero, 0, 0, 0.8f);
                m_Obj = Object.Instantiate(mainBoard.m_SupplyButton.gameObject, mainBoard.m_SupplyButton.transform.position, Quaternion.identity, m_GuideMenu.transform);
                m_Obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    m_IsGuideCompleted = true;
                    mainBoard.OnSupplyButtonClick();
                    Object.Destroy(m_Obj);
                    m_Obj = null;
                });
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            if (m_Obj != null)
            {
                Object.Destroy(m_Obj);
                m_Obj = null;
            }
            m_GuideMenu.Hide();

            base.OnGuideFinish();
        }
    }
}
