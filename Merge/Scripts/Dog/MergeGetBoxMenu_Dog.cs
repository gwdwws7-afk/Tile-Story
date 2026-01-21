using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeGetBoxMenu_Dog : MergeGetBoxMenu
    {

        protected override void RefreshBanner(bool showMergeOfferBanner)
        {
            m_Banner2.gameObject.SetActive(showMergeOfferBanner);
            if (showMergeOfferBanner)
            {
                m_Banner1.localPosition = new Vector3(0, 45, 0);
                m_Banner3.localPosition = new Vector3(0, -372, 0);
            }
            else
            {
                m_Banner1.localPosition = new Vector3(0, -23, 0);
                m_Banner3.localPosition = new Vector3(0, -307, 0);
            }
        }
    }
}
