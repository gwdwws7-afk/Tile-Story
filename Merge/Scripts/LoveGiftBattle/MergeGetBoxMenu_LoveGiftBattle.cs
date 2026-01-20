using GameFramework.Event;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeGetBoxMenu_LoveGiftBattle : MergeGetBoxMenu
    {
        public override int RewardAdGetBoxNum => 1;

        protected override void RefreshBanner(bool showMergeOfferBanner)
        {
            m_Banner2.gameObject.SetActive(showMergeOfferBanner);
            if (showMergeOfferBanner)
            {
                m_Banner1.localPosition = new Vector3(0, 260, 0);
                m_Banner3.localPosition = new Vector3(0, -157, 0);
            }
            else
            {
                m_Banner1.localPosition = new Vector3(0, 182, 0);
                m_Banner3.localPosition = new Vector3(0, -81, 0);
            }
        }
    }
}
