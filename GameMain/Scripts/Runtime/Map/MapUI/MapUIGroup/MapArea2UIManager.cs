using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea2UIManager : UIGroup
{
    public override string GroupName => "Area2";

    public override UIGroupType GroupType => UIGroupType.Static;

    public override void OnShow()
    {
        gameObject.SetActive(true);
    }

    public override void OnHide(bool skipAnim)
    {
        gameObject.SetActive(false);
    }
}
