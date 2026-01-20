using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemaPackEntrance : OnePlusOnePackEntranceBase
{
    protected override string PackName => "CinemaPackMenu";
    protected override DateTime StartTime => CinemaPackMenu.PackStartTime;
    protected override DateTime EndTime => CinemaPackMenu.PackEndTime;
}
