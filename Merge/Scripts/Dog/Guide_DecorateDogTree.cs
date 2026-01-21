using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    //Let's go decorate the Dog tree
    public sealed class Guide_DecorateDogTree : Guide
    {
        private GameObject m_Obj;

        public Guide_DecorateDogTree(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DecorateDogTree;

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

            GameManager.Task.AddDelayTriggerTask(0.2f, () =>
            {
                m_IsShowGuideFinished = true;

                MergeMainMenu_Dog mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_Dog;
                if (mainBoard != null)
                {
                    m_GuideMenu.Show("Merge.Guide10", mainBoard.m_DogEntrance.transform.position);
                    m_GuideMenu.ShowFingerAnim(mainBoard.m_DogEntrance.transform.position, mainBoard.m_DogEntrance.transform.position);
                    m_GuideMenu.ShowRaycastMask(Vector3.zero, 0, 0, 0.8f);
                    m_Obj = Object.Instantiate(mainBoard.m_DogEntrance.gameObject, mainBoard.m_DogEntrance.transform.position, Quaternion.identity, m_GuideMenu.transform);
                    m_Obj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        m_IsGuideCompleted = true;
                        mainBoard.OnDogEntranceButtonClick();
                        Object.Destroy(m_Obj);
                        m_Obj = null;
                    });
                }
                else
                {
                    m_IsGuideCompleted = true;
                }
            });
        }

        public override void OnGuideFinish()
        {
            m_GuideMenu.Hide();

            base.OnGuideFinish();
        }
    }
}
