using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemaPackMenu : OnePlusOnePackMenuBase
{
    public static DateTime PackStartTime = new DateTime(2025, 11, 2, 0, 0, 0);
    public static DateTime PackEndTime = new DateTime(2025, 11, 10, 0, 0, 0);
    
    protected override string PackName => "CinemaPackMenu";
    protected override ProductNameType LowerProductType => ProductNameType.Film_One_Plus_One_Small;
    protected override ProductNameType HigherProductType => ProductNameType.Film_One_Plus_One_Big;
    protected override DateTime StartTime => PackStartTime;
    protected override DateTime EndTime => PackEndTime;
}
