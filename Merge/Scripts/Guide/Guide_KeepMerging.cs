using DG.Tweening;
using UnityEngine;

namespace Merge
{
    //Keep merging to unlock the max level exclusive fish and the ultimate prize!
    public sealed class Guide_KeepMerging : Guide
    {
        private GameObject m_FinalChest;

        public Guide_KeepMerging(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_KeepMerging;

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

            MergeMainMenu mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu;
            if (mainBoard != null)
            {
                mainBoard.m_ProgressBoard.m_ScrollRect.DOHorizontalNormalizedPos(1f, 0.3f).onComplete = () =>
                {
                    m_FinalChest = Object.Instantiate(mainBoard.m_ProgressBoard.m_Stages[mainBoard.m_ProgressBoard.m_Stages.Length - 1].gameObject, mainBoard.m_ProgressBoard.m_Stages[mainBoard.m_ProgressBoard.m_Stages.Length - 1].position, Quaternion.identity, m_GuideMenu.transform);
                    m_FinalChest.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);

                    m_GuideMenu.Show("Merge.Guide3", mainBoard.m_ProgressBoard.m_Stages[mainBoard.m_ProgressBoard.m_Stages.Length - 1].position);
                    m_GuideMenu.ShowArrowAnim(mainBoard.m_ProgressBoard.m_Stages[mainBoard.m_ProgressBoard.m_Stages.Length - 1].position + new Vector3(-0.2f, 0, 0), Quaternion.Euler(0, 0, 90));
                    m_GuideMenu.ShowRaycastMask(Vector3.zero, 0, 0, 0.8f);
                };

                GameManager.Task.AddDelayTriggerTask(0.6f, () =>
                {
                    m_IsShowGuideFinished = true;
                });
            }
            else
            {
                m_IsShowGuideFinished = true;
                m_IsGuideCompleted = true;
            }
        }

        public override void OnGuideFinish()
        {
            m_GuideMenu.Hide();
            if (m_FinalChest != null)
            {
                Object.Destroy(m_FinalChest);
                m_FinalChest = null;
            }

            MergeMainMenu mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu;
            if (mainBoard != null)
            {
                mainBoard.m_ProgressBoard.RefreshProgress(true);
                mainBoard.OnFinalRewardButtonClick();
            }

            base.OnGuideFinish();
        }
    }
}
