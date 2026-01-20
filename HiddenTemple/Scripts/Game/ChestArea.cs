using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenTemple
{
    /// <summary>
    /// 宝箱区域
    /// </summary>
    public sealed class ChestArea : MonoBehaviour
    {
        public Slider m_Slider;
        public Transform m_CurStageBanner1, m_CurStageBanner2, m_ChestFlyRoot;
        public ChestSlot[] m_ChestSlots;
        public ItemPromptBox m_ItemPromptBox;

        private int m_Stage;
        private HiddenTempleBaseMenu m_Menu;

        public void Initialize(int stage, HiddenTempleBaseMenu menu)
        {
            m_Stage = stage;
            m_Menu = menu;

            for (int i = 0; i < m_ChestSlots.Length; i++)
            {
                m_ChestSlots[i].Initialize(i + 1, stage, this);
            }

            Refresh(false);
        }

        public void Release()
        {
            m_Slider.DOKill();
            m_CurStageBanner1.DOKill();
            m_CurStageBanner2.DOKill();

            m_ItemPromptBox.OnRelease();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
            {
                HideItemPromptBox();
            }
        }

        private void Refresh(bool showAnim)
        {
            int stage = Mathf.Clamp(m_Stage, 1, 5);

            float posX = m_ChestSlots[stage - 1].transform.position.x;
            if (showAnim)
            {
                m_Slider.DOValue((stage - 1) * 0.25f, 0.3f);
                m_CurStageBanner1.DOMoveX(posX + 0.005f, 0.4f).onComplete = () =>
                  {
                      m_CurStageBanner1.DOMoveX(posX, 0.2f);
                  };
                m_CurStageBanner2.DOMoveX(posX + 0.005f, 0.4f).onComplete = () =>
                {
                    m_CurStageBanner2.DOMoveX(posX, 0.2f);
                };
            }
            else
            {
                m_Slider.value = (stage - 1) * 0.25f;
                m_CurStageBanner1.position = new Vector3(posX, m_CurStageBanner1.position.y, 0);
                m_CurStageBanner2.position = new Vector3(posX, m_CurStageBanner2.position.y, 0);
            }
        }

        public void ShowNextStageChestAnim()
        {
            int stage = HiddenTempleManager.PlayerData.GetCurrentStage();
            if (m_Stage < stage)
            {
                m_Stage = stage;
                Refresh(true);
            }
        }

        public Vector3 GetChestSlotPos(int stage)
        {
            return m_ChestSlots[stage - 1].transform.position;
        }

        public void ShowChestSlotUnlockAnim(int stage)
        {
            m_ChestSlots[stage - 1].ShowChestSlotUnlockAnim();
        }

        public void ShowCurChestItemPromptBox()
        {
            m_ChestSlots[m_Stage - 1].OnTipButtonClick();
        }

        public void ShowItemPromptBox(int chestStage, Vector3 position)
        {
            IDataTable<DRChestData> dataTable = HiddenTempleManager.DataTable.GetDataTable<DRChestData>();
            DRChestData data = dataTable.GetDataRow(chestStage);

            m_ItemPromptBox.Init(data.RewardsId, data.RewardsNum);
            switch (chestStage)
            {
                case 1:
                    m_ItemPromptBox.triangelOffset = 210f;
                    break;
                case 2:
                    m_ItemPromptBox.triangelOffset = 200f;
                    break;
                case 3:
                    m_ItemPromptBox.triangelOffset = 250f;
                    break;
                case 4:
                    m_ItemPromptBox.triangelOffset = 200f;
                    break;
                case 5:
                    m_ItemPromptBox.triangelOffset = 110f;
                    break;
            }

            m_ItemPromptBox.ShowPromptBox(PromptBoxShowDirection.Up, position);
        }

        public void HideItemPromptBox()
        {
            m_ItemPromptBox.HidePromptBox();
        }
    }
}
