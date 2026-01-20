using Spine.Unity;
using System;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 道具可合成特效
    /// </summary>
    public class PropCanMergeEffect : MonoBehaviour
    {
        public SkeletonAnimation m_Spine, m_Spine2;

        public void Show()
        {
            //m_Spine2.gameObject.SetActive(false);

            m_Spine.Initialize(false);
            m_Spine.state.SetAnimation(0, "appear", false);
            m_Spine2.gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        public void Hide(Action callback)
        {
            m_Spine2.gameObject.SetActive(false);
            m_Spine.state.SetAnimation(0, "disappear", false).Complete += t =>
            {
                callback?.Invoke();
                gameObject.SetActive(false);
            };
        }

        public void SetLayer(string layerName, int sortOrder)
        {
        }
    }
}