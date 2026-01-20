using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 道具附属物
    /// </summary>
    public class PropAttachment : MonoBehaviour
    {
        private PropLogic m_Prop;

        /// <summary>
        /// 被附属的道具
        /// </summary>
        public PropLogic Prop
        {
            get
            {
                return m_Prop;
            }
        }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                return m_Prop != null;
            }
        }

        public virtual void Initialize(PropLogic prop)
        {
            m_Prop = prop;
        }

        public virtual void OnSelected()
        {
        }

        /// <summary>
        /// 设置层级
        /// </summary>
        public virtual void SetLayer(string layerName, int sortOrder)
        {
        }
    }
}