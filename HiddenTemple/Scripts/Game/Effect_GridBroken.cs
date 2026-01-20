using Spine.Unity;
using UnityEngine;

namespace HiddenTemple
{
    public sealed class Effect_GridBroken : MonoBehaviour
    {
        public SkeletonGraphic m_EffectSpine;

        private bool m_IsUsing;

        public bool IsUsing => m_IsUsing;

        public void Show(Vector3 pos)
        {
            m_IsUsing = true;
            transform.position = pos;

            m_EffectSpine.Initialize(true);
            m_EffectSpine.AnimationState.SetAnimation(0, "breakStone", false).Complete += Effect_GridBroken_Complete;
            m_EffectSpine.gameObject.SetActive(true);
        }

        private void Effect_GridBroken_Complete(Spine.TrackEntry trackEntry)
        {
            m_EffectSpine.gameObject.SetActive(false);
            m_IsUsing = false;
        }
    }
}
