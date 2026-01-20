using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class Guide_DigDialog7 : Guide
    {
        public Guide_DigDialog7(MergeGuideMenu guideMenu) : base(guideMenu)
        {
        }

        public override GuideTriggerType GuideType => GuideTriggerType.Guide_DigDialog7;

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

            MergeMainMenu_DigTreasure mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_DigTreasure;
            if (mainBoard != null)
            {
                m_GuideMenu.Show("Merge.Guide7", Vector3.zero, 0);
                m_GuideMenu.ShowFingerAnim(mainBoard.m_StorageButton.transform.position, mainBoard.m_StorageButton.transform.position);
                m_GuideMenu.ShowRaycastMask(Vector3.zero, 0, 0, 0.8f);
                mainBoard.m_StoragePropImg.DOKill();
                mainBoard.m_StoragePropImg.color = Color.white;
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
