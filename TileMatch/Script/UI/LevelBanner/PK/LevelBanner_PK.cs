using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class LevelBanner_PK : LevelBannerBase
{
    public PkSorce pkscore;
    
    private bool isHide = true;

    protected override void OnInitialize()
    {
        pkscore.Init();
    }

    protected override void OnRelease()
    {
        
    }
}
