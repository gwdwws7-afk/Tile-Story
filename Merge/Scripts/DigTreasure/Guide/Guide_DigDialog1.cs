using UnityEngine;

namespace Merge
{
    //Welcome to the Treasure Cave! There's so much to uncover!
    public sealed class Guide_DigDialog1 : Guide
    {
        public Guide_DigDialog1(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DigDialog1;

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
                if ((Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android) && Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        m_IsGuideCompleted = true;
                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)
                    {
                        m_IsGuideCompleted = true;
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        m_IsGuideCompleted = true;
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        m_IsGuideCompleted = true;
                    }
                }
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

            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
            if (mainBoard != null)
            {
                MergeBoard_DigTreasure board = mainBoard.m_MergeBoard as MergeBoard_DigTreasure;

                m_GuideMenu.Show("DigMerge.GuideDialog1", new Vector3(0, mainBoard.m_PickaxeBarIcon.transform.position.y, 0), 0);
                //m_GuideMenu.ShowRaycastMask(board.m_ShakeBody.transform.position, 429, 572, 0.8f);
                Vector3 focusPos = (MergeManager.Merge.GetSquare(2, 1).transform.position + MergeManager.Merge.GetSquare(2, 4).transform.position) / 2f;
                m_GuideMenu.ShowRaycastMask(focusPos, 288, 72, 0.8f);
                mainBoard.ShowClickSupplyFingerAnim();
            }
            else
            {
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            GameManager.Task.RemoveDelayTriggerTask(delayTaskId);
            m_GuideMenu.Hide(false);

            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
            mainBoard?.HideFingerAnim();

            base.OnGuideFinish();

            GameManager.Task.AddDelayTriggerTask(0, () =>
            {
                m_GuideMenu?.TriggerGuide(GuideTriggerType.Guide_DigDialog2);
            });
        }
    }
}
