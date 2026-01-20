using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MapBgUIManager : UIGroup
{
    public override string GroupName => "BgUI";

    public override UIGroupType GroupType => UIGroupType.Static;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public override void OnShow()
    {
        gameObject.SetActive(true);

        base.OnShow();
    }

}
