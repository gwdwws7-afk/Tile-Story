using UnityEngine;
using DG.Tweening;

public class PlayerItemObject : ObjectBase
{
    public override void Release(bool isShutdown)
    {
        if (Target != null)
        {
            GameObject targetObj = (GameObject)Target;

            if (targetObj != null)
            {
                targetObj.transform.DOKill();
                targetObj.SetActive(false);

                UnityUtility.UnloadInstance(targetObj);
            }
        }
    }
}
