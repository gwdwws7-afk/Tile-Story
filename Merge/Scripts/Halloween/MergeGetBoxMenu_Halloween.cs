using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeGetBoxMenu_Halloween : MergeGetBoxMenu
    {

        protected override void RefreshBanner(bool showMergeOfferBanner)
        {
            m_Banner2.gameObject.SetActive(showMergeOfferBanner);
            if (showMergeOfferBanner)
            {
                m_Banner1.localPosition = new Vector3(0, 37, 0);
                m_Banner3.localPosition = new Vector3(0, -380, 0);
            }
            else
            {
                m_Banner1.localPosition = new Vector3(0, -27, 0);
                m_Banner3.localPosition = new Vector3(0, -318, 0);
            }
        }
    }
}
