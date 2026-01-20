using UnityEngine;
using DG.Tweening;

public class EffectObject : ObjectBase
{
    public override void Release(bool isShutdown)
    {
        if (Target != null)
        {
            GameObject obj = (GameObject)Target;

            if (obj != null)
            {
                obj.transform.DOKill();
                obj.gameObject.SetActive(false);
            }
            UnityUtility.UnloadInstance(obj);
        }
    }
}
