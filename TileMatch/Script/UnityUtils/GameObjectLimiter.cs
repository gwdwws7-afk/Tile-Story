using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectLimiter : MonoBehaviour
{
    private void Start()
    {
        if (SystemInfoManager.IsSuperLowMemorySize)
        {
            gameObject.SetActive(false);
        }
    }
}
