using Spine.Unity;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 道具合成特效
    /// </summary>
    public class PropMergeEffect : MonoBehaviour
    {
        public SkeletonAnimation m_Spine;

        public void Show()
        {
            m_Spine.Initialize(false);
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
            m_Spine.state.SetAnimation(0, "01", false);
        }

        public void SetLayer(string layerName, int sortOrder)
        {
        }
    }
}