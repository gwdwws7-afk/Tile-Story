using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SkeletonGraphicControl : MonoBehaviour
{
   private static float RandomDelayTime=3;
   
   private SkeletonGraphic skeletonGraphic;

   private void Awake()
   {
      skeletonGraphic = GetComponent<SkeletonGraphic>();
      skeletonGraphic.startingLoop = false;
   }

   private void Start()
   {
      StartCoroutine(RandomPlayAnim());
   }

   private IEnumerator RandomPlayAnim()
   {
      yield return new WaitForSeconds(UnityEngine.Random.Range(1,RandomDelayTime));
      if (skeletonGraphic != null&&skeletonGraphic.AnimationState != null && skeletonGraphic.AnimationState.GetCurrent(0) != null)
      {
         skeletonGraphic.AnimationState.GetCurrent(0).Complete -= CompleteCallBack;
         skeletonGraphic.AnimationState.GetCurrent(0).Complete += CompleteCallBack;
      }
   }

   private void CompleteCallBack(TrackEntry s)
   {
      StartCoroutine(RandomPlayAnim());
   }
}
