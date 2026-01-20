using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerRelatedObject : MonoBehaviour
{
    public RectTransform self;
    public float originAnchoredPositionY;
    public float effectDleta = 1f;
    int recordBannerHeight;

    void Update()
    {
        if (recordBannerHeight != GetBannerAdsHeight())
        {
            recordBannerHeight = GetBannerAdsHeight();
            self.anchoredPosition = new Vector2(self.anchoredPosition.x, originAnchoredPositionY - recordBannerHeight * effectDleta);
        }
    }
    private int GetBannerAdsHeight()
    {
        var adsComponect = GameManager.Ads;
        int adsHeight = adsComponect != null ? adsComponect.BannerAdsHeight : 0;
        return adsHeight;
    }
}
