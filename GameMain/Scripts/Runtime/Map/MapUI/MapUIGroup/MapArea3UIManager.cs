using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea3UIManager : UIGroup
{
    public override string GroupName => "Area3";

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
