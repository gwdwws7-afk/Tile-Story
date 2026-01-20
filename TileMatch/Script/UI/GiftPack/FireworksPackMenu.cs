using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// »¨»ð½ÚÀñ°ü
/// </summary>
public class FireworksPackMenu : ThreeColumnPackMenuBase
{
    public static DateTime GiftPackStartTime = new DateTime(2025, 8, 7, 0, 0, 0);
    public static DateTime GiftPackEndTime = new DateTime(2025, 8, 15, 0, 0, 0);

    public GameObject[] fireworkEffects;
    public float[] fireworkDelayTimes;
    private float timer;
    private int index = -1;

    protected override ProductNameType ProductType_1 => ProductNameType.Fire_Work_Small;

    protected override ProductNameType ProductType_2 => ProductNameType.Fire_Work_Middle;

    protected override ProductNameType ProductType_3 => ProductNameType.Fire_Work_Big;

    protected override DateTime StartDate => GiftPackStartTime;

    protected override DateTime EndDate => GiftPackEndTime;

    private void OnEnable()
    {
        timer = 0;
        //foreach (var effect in fireworkEffects)
        //{
        //    effect.SetActive(false);
        //}
    }

    private void Update()
    {
        timer += Time.deltaTime;
        for (int i = 0; i < fireworkEffects.Length; i++)
        {
            if (timer > fireworkDelayTimes[i]) 
            {
                if (index < i)
                {
                    index = i;

                    fireworkEffects[index].GetComponent<ParticleSystem>().Play(true);
                }
            }
        }

        //if (index != -1)
        //{
        //    //ParticleSystem[] effects = fireworkEffects[index].GetComponentsInChildren<ParticleSystem>();
        //    //foreach (var effect in effects)
        //    //{
        //    //    effect.Play();
        //    //}
        //    fireworkEffects[index].GetComponent<ParticleSystem>().Play(true);
        //}

        if (index == fireworkEffects.Length - 1)
        {
            timer = 0;
            index = -1;
        }
    }
}
