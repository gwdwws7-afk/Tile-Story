using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;

public class DaliyWatchRedPoint : MonoBehaviour
{
    [SerializeField] private GameObject RedPoint_Obj;
    private void OnEnable()
    {
        GameManager.Event.Subscribe(RewardAdLoadCompleteEventArgs.EventId,RewardAdLoad);
        
        bool isHaveRV = GameManager.Ads.CheckRewardedAdIsLoaded();
        try
        {
            RedPoint_Obj.SetActive(isHaveRV);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void Update()
    {
        if (Time.frameCount % 20==0)
        {
            RedPoint_Obj.SetActive(GameManager.Ads.CheckRewardedAdIsLoaded());
        }
    }

    private void OnDisable()
    {
        GameManager.Event.Unsubscribe(RewardAdLoadCompleteEventArgs.EventId,RewardAdLoad);
    }

    private void RewardAdLoad(object sender,GameEventArgs message)
    {
        bool isHaveRV = GameManager.Ads.CheckRewardedAdIsLoaded();
        try
        {
            RedPoint_Obj.SetActive(isHaveRV);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
