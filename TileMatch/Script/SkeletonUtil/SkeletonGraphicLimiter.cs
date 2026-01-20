using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SkeletonGraphicLimiter : MonoBehaviour
{
    public enum DeviceTier
    {
        SuperLowMemory,
        SuperLowDevice,
    }
    
    [SerializeField]
    private SkeletonGraphic skeletonGraphic;
    [SerializeField]
    private DeviceTier deviceTier;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        skeletonGraphic = GetComponent<SkeletonGraphic>();
    }
#endif

    private void Start()
    {
        if (deviceTier == DeviceTier.SuperLowMemory)
        {
            if(SystemInfoManager.IsSuperLowMemorySize)
            {
                skeletonGraphic.freeze = true;
            }   
        }
        else if (deviceTier == DeviceTier.SuperLowDevice)
        {
            if(SystemInfoManager.DeviceType <= DeviceType.SurpLow)
            {
                skeletonGraphic.freeze = true;
            } 
        }
    }
}
