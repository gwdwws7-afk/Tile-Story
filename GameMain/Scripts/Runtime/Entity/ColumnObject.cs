using UnityEngine;

public class ColumnObject : ObjectBase
{
    public override void Release(bool isShutdown)
    {
        if (Target != null)
        {
            GameObject obj = (GameObject)Target;

            UnityUtility.UnloadInstance(obj);
        }
    }
}
