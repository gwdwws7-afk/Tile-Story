using UnityEngine;
using UnityEngine.EventSystems;

namespace Merge
{
    /// <summary>
    /// 格子类
    /// </summary>
    public class Square : MonoBehaviour, IPointerClickHandler
    {
        public int m_Row;
        public int m_Col;

        private PropLogic m_FilledProp;
        private bool m_IsReserved;
        private bool m_IsLocked;

        /// <summary>
        /// 填充的道具
        /// </summary>
        public PropLogic FilledProp
        {
            get
            {
                return m_FilledProp;
            }
            set
            {
                m_FilledProp = value;

                if (m_FilledProp == null)
                {
                    m_IsReserved = false;
                }
            }
        }

        /// <summary>
        /// 是否被预定
        /// </summary>
        public bool IsReserved
        {
            get
            {
                return m_IsReserved;
            }
            set
            {
                m_IsReserved = value;
            }
        }

        /// <summary>
        /// 是否被锁住（无法填充道具）
        /// </summary>
        public bool IsLocked
        {
            get
            {
                return m_IsLocked;
            }
            set
            {
                m_IsLocked = value;
            }
        }

        public virtual void Initialize(int row, int col)
        {
            m_Row = row;
            m_Col = col;
            m_IsReserved = false;
            m_IsLocked = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_FilledProp != null && m_FilledProp.MovementState == PropMovementState.Static && m_FilledProp.Prop != null)
            {
                m_FilledProp.OnSelected();

                if (MergeManager.Merge.LastSelectedProp == m_FilledProp && m_FilledProp != null)
                {
                    m_FilledProp.OnClick();
                }
            }
        }
    }
}
