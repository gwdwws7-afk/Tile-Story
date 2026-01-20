using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public sealed class LevelBannerLogic_PK : LevelBannerLogicBase
{
    public override LevelBannerType BannerType => LevelBannerType.PKBanner;

    public override bool CheckBannerCanShow()
    {
        return PkGameModel.Instance.IsActivityOpen && !GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge;
    }
}
