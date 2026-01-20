using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AttachLogic_4 : AttachLogic
{
    public override int AttachId => 4;

    public override string AttachAssetName => "OilDrum";
    
    protected override void OnInit(object userData)
    {
        SpawnComplete += (a) =>
        {
            if (AttachItem != null)
            {
                AttachItem.SetDisappearAction(() =>
                {
                    base.Release(false);
                });
            }
        };
        base.OnInit(userData);
    }

    public override void SpecialCollect(bool showEffect, bool clearId = true)
    {
        OnClick();
    }
}
