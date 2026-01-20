using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeGetBoxMenu_DigTreasure : MergeGetBoxMenu
    {

        protected override void RefreshBanner(bool showMergeOfferBanner)
        {
            m_Banner2.gameObject.SetActive(showMergeOfferBanner);
            if (showMergeOfferBanner)
            {
                m_Banner1.localPosition = new Vector3(0, -77, 0);
                m_Banner3.localPosition = new Vector3(0, -494, 0);
            }
            else
            {
                m_Banner1.localPosition = new Vector3(0, -177, 0);
                m_Banner3.localPosition = new Vector3(0, -427, 0);
            }
        }
    }
}
