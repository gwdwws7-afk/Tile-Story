using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;

namespace HiddenTemple
{
    /// <summary>
    /// ¸å×Ó
    /// </summary>
    public sealed class Pickaxe : MonoBehaviour
    {
        public SkeletonGraphic m_Spine;

        private bool m_IsUsing;

        public bool IsUsing => m_IsUsing;

        public void ShowPickaxeAnim(Vector3 digPos)
        {
            m_IsUsing = true;
            transform.position = digPos;

            m_Spine.Initialize(true);
            m_Spine.AnimationState.SetAnimation(0, "chuizi", false).Complete += Pickaxe_Complete;
            m_Spine.gameObject.SetActive(true);
        }

        private void Pickaxe_Complete(Spine.TrackEntry trackEntry)
        {
            m_Spine.gameObject.SetActive(false);
            m_IsUsing = false;
        }

        public void Release()
        {

        }
    }
}
