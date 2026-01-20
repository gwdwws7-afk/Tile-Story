using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeInfoScrollView : MonoBehaviour
    {
        public MergeInfoItemSlot[] m_InfoSlots;

        public void Init(int stage)
        {
            for (int i = 0; i < m_InfoSlots.Length; i++)
            {
                m_InfoSlots[i].m_UnknownImg.SetActive(i > stage);
                m_InfoSlots[i].m_KnownImg.SetActive(i <= stage);
            }
        }

        public void Init(int[] propIds)
        {
            for (int i = 0; i < propIds.Length; i++)
            {
                bool isUnlock = MergeManager.PlayerData.GetPropIsUnlock(propIds[i]);
                m_InfoSlots[i].m_UnknownImg.SetActive(!isUnlock);
                m_InfoSlots[i].m_KnownImg.SetActive(isUnlock);
            }
        }
    }
}
