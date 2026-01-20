using UnityEngine;
using DG.Tweening;

public class ElfObject : ObjectBase
{
    public override void Release(bool isShutdown)
    {
        if (Target != null)
        {
            GameObject elf = (GameObject)Target;

            if (elf != null)
            {
                elf.transform.DOKill();

                UnityUtility.UnloadInstance(elf);
            }
        }
    }
}
