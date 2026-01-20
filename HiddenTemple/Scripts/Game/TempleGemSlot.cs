using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenTemple
{
    /// <summary>
    /// …Ò√Ì±¶ Ø≤€
    /// </summary>
    public sealed class TempleGemSlot : MonoBehaviour
    {
        [SerializeField]
        private int m_GemId;
        [SerializeField]
        private string m_FillGemAnimName;
        [SerializeField]
        private SkeletonGraphic m_FillGemEffect;

        private DigGem m_FilledGem;

        /// <summary>
        /// ±¶ Ø±‡∫≈
        /// </summary>
        public int GemId => m_GemId;

        /// <summary>
        /// ±¶ Ø≤€ «∑Ò±ª±¶ ØÃÓ≥‰
        /// </summary>
        public bool IsFilled
        {
            get => m_FilledGem != null;
        }

        public void SetFilledGem(DigGem gem)
        {
            m_FilledGem = gem;
        }

        public void ClearFilledGem()
        {
            m_FilledGem = null;
        }

        public void ShowFillGemEffect()
        {
            m_FillGemEffect.Initialize(false);
            m_FillGemEffect.AnimationState.SetAnimation(0, m_FillGemAnimName, false);
            m_FillGemEffect.gameObject.SetActive(true);
        }
    }
}
