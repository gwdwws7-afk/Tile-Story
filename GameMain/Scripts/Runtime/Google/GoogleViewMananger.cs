using System;
using System.Collections;
using System.Collections.Generic;
#if !UNITY_IOS && !UNITY_IPHONE
using Google.Play.Common;
using Google.Play.Review;
using Google.Play.Core;
#endif
using UnityEngine;


public class GoogleViewMananger : MonoBehaviour
{
#if !UNITY_IOS && !UNITY_IPHONE
    static GoogleViewMananger mManager;

    public static GoogleViewMananger Mananger
    {
        get
        {
            if (mManager == null)
            {
                mManager = GameObject.FindObjectOfType<GoogleViewMananger>();
                if (mManager == null)
                    mManager = new GameObject("GoogleViewManager").AddComponent<GoogleViewMananger>();
            }
            return mManager;
        }
    }

    private ReviewManager mReview;

    private PlayAsyncOperation<PlayReviewInfo, ReviewErrorCode> mRequestFlowOperation;


    public void RequestReview(Action<bool> action = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Mananger.StartCoroutine(RequestReviewFlow(action));
#endif
    }

    public void LaunchReview(Action<bool> action = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Mananger.StartCoroutine(LaunchReviewFlow(action));
#endif
    }

    public bool IsCanLaunch()
    {
        return mRequestFlowOperation != null && mRequestFlowOperation.Error == ReviewErrorCode.NoError;
    }

    IEnumerator RequestReviewFlow(Action<bool> callback)
    {
        if (mReview == null) mReview = new ReviewManager();
        if (mRequestFlowOperation == null) mRequestFlowOperation = mReview.RequestReviewFlow();
        yield return mRequestFlowOperation;
        callback?.Invoke(mRequestFlowOperation.Error == ReviewErrorCode.NoError);

        if(mRequestFlowOperation.Error == ReviewErrorCode.NoError)
        {
            Log.Info("RequestFlowOperation Successful");
        }
        else
        {
            Log.Warning("RequestFlowOperation Fail.Reason:{0}", mRequestFlowOperation.Error);
        }
    }

    IEnumerator LaunchReviewFlow(Action<bool> callback)
    {
        if (mRequestFlowOperation != null && mRequestFlowOperation.IsDone && mRequestFlowOperation.Error == ReviewErrorCode.NoError)
        {
            var launchFlowOperation = mReview.LaunchReviewFlow(mRequestFlowOperation.GetResult());
            yield return launchFlowOperation;
            callback?.Invoke(launchFlowOperation.Error == ReviewErrorCode.NoError);

            if (launchFlowOperation.Error == ReviewErrorCode.NoError)
            {
                Log.Info("launchFlowOperation Successful");
            }
            else
            {
                Log.Info("launchFlowOperation Fail.Reason:{0]", launchFlowOperation.Error);
            }
        }
    }
#endif
}
