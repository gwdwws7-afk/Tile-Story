using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationChestRewardArea : ChestRewardArea
{
    public Transform areaParent;
    // Start is called before the first frame update
    protected override Vector3 GetRewardLocalPosition(int index)
    {
        if (areaParent != null)
        {
            int totalCount = rewardFlyObjects.Count;
            if (areaParent.childCount >= totalCount)
            {
                return areaParent.GetChild(totalCount - 1).GetChild(index).transform.localPosition;
            }
        }
        return base.GetRewardLocalPosition(index);
    }
}
