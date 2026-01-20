using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    //When the board is full,special items will be stored here!Tap to take it out to the board.
    public sealed class Guide_BoardFull : Guide
    {
        public Guide_BoardFull(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_BoardFull;

        private GameObject m_BtnObj;

        public override bool CheckCanTrigger(GuideTriggerType triggerType, object userData)
        {
            if (triggerType == GuideType)
            {
                return true;
            }

            return false;
        }

        public override void CheckCanComplete(GuideTriggerType triggerType, object userData)
        {
        }

        public override GuideState GetGuideState()
        {
            return base.GetGuideState();
        }

        public override void OnTriggerGuide()
        {
            base.OnTriggerGuide();

            m_IsShowGuideFinished = true;

            MergeMainMenu mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu;
            if (mainBoard != null)
            {
                m_GuideMenu.Show("Merge.Guide7", mainBoard.m_StorageButton.transform.position, 0.5f);
                m_GuideMenu.ShowArrowAnim(mainBoard.m_StorageButton.transform.position + new Vector3(0, 0.15f, 0), Quaternion.Euler(0, 0, 0));
                m_GuideMenu.ShowRaycastMask(Vector3.zero, 0, 0, 0.8f);
                m_BtnObj = Object.Instantiate(mainBoard.m_StorageButton.gameObject, mainBoard.m_StorageButton.transform.position, Quaternion.identity, m_GuideMenu.transform);
                m_BtnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    m_IsGuideCompleted = true;
                    mainBoard.OnStorageButtonClick();
                    Object.Destroy(m_BtnObj);
                    m_BtnObj = null;
                });
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            if (m_BtnObj != null)
            {
                Object.Destroy(m_BtnObj);
                m_BtnObj = null;
            }
            m_GuideMenu.Hide();

            base.OnGuideFinish();
        }
    }
}
