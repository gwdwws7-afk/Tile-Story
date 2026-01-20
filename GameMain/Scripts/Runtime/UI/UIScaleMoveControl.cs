using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaleMoveControl : MonoBehaviour
{
    public bool canActive;
    bool isActiveDrag;
    bool isScaled;
    float standardDistance;
    void Update()
    {
        if (!canActive)
        {
            //TODO:滞销放大操作
            return;
        }
        if (!isActiveDrag)
        {
            //判断是否能放大 切换进放大模式
            if (Input.touchCount >= 2)
            {

            }
        }
    }
}
