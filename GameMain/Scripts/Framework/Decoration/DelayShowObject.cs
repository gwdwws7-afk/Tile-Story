using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayShowObject : MonoBehaviour
{
    public GameObject needShowObject;
    public float delayTime = 0f;

    private void OnEnable()
    {
        Invoke("ShowObject", delayTime);
    }

    private void ShowObject()
    {
        needShowObject.SetActive(true);
    }
}
