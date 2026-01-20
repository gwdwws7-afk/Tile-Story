using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ³É¾Í¹Ø¿¨ºá·ùÂß¼­
/// </summary>
public sealed class LevelBannerLogic_Objective : LevelBannerLogicBase
{
    public override LevelBannerType BannerType => LevelBannerType.ObjectiveLevelBanner;

    public override bool CheckBannerCanShow()
    {
        return GameManager.Objective.CheckObjectiveUnlock() && (GameManager.Objective.GetDailyCompletedObjectiveData() != null || GameManager.Objective.GetAllTimeCompletedObjectiveData() != null || GameManager.Objective.GetAllTimeFirstObjectiveData() != null);
    }
}
