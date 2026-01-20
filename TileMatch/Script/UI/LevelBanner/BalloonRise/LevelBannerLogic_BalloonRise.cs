using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class LevelBannerLogic_BalloonRise : LevelBannerLogicBase
{
    public override LevelBannerType BannerType => LevelBannerType.BalloonRiseBanner;

    public override bool CheckBannerCanShow()
    {
        if (GameManager.Network.CheckInternetIsNotReachable())
            return false;

        return GameManager.Task.BalloonRiseManager.CheckIsOpen;
    }
}
