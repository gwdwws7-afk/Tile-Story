using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeStageRewardColumn : MonoBehaviour
    {
        public TextMeshProUGUI m_IndexText;
        public Slider m_Slider;
        public Transform m_Root;
        public GameObject m_ShineBg;
        public GameObject m_Tick;
        public List<MergeOfferItemSlot> m_RewardSlotList = new List<MergeOfferItemSlot>();

        public virtual void Initialize(DRMergeStageReward data)
        {
            m_IndexText.text = data.Id.ToString();

            int stage = MergeManager.PlayerData.GetDigTreasureRewardStage();
            if (data.Id >= stage)
            {
                Vector3 deltaPos = data.RewardPropIds.Count >= 5 ? new Vector3(104, 0, 0) : new Vector3(150, 0, 0);
                var posList = UnityUtility.GetAveragePosition(Vector3.zero, deltaPos, data.RewardPropIds.Count);
                for (int i = 0; i < m_RewardSlotList.Count; i++)
                {
                    if (i < data.RewardPropIds.Count)
                    {
                        m_RewardSlotList[i].OnInit(TotalItemData.FromInt(data.RewardPropIds[i]), data.RewardPropNums[i]);
                        m_RewardSlotList[i].transform.localPosition = posList[i];
                        m_RewardSlotList[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        m_RewardSlotList[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_RewardSlotList.Count; i++)
                {
                    m_RewardSlotList[i].gameObject.SetActive(false);
                }
            }

            if (data.Id < stage)
            {
                m_Slider.value = 1;
            }
            else if (data.Id == stage)
            {
                m_Slider.value = MergeManager.PlayerData.GetDigTreasureRewardProgress() / (float)data.TargetNum;
            }
            else
            {
                m_Slider.value = 0;
            }

            if (m_ShineBg != null) 
                m_ShineBg.SetActive(data.Id == stage);
            m_Tick.SetActive(data.Id < stage);
        }

        public virtual void Release()
        {
            for (int i = 0; i < m_RewardSlotList.Count; i++)
            {
                m_RewardSlotList[i].OnRelease();
                m_RewardSlotList[i].gameObject.SetActive(false);
            }
        }

        public float Refresh()
        {
            return 0;
        }
    }
}
