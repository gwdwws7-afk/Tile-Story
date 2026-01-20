using Firebase;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

public sealed partial class FirebaseManager : GameFrameworkModule, IFirebaseManager
{
    private bool isInitFirebaseApp;
    private bool isInitRemoteConfig;
    private bool isFetchRemoteConfig;
    private bool isInitFirestore;
    private bool isInitAuth;

    public FirebaseManager()
    {
        isInitFirebaseApp = false;
        isInitRemoteConfig = false;
        isFetchRemoteConfig = false;
        isInitFirestore = false;
        isInitAuth = false;
    }

    public bool IsInitFirebaseApp { get => isInitFirebaseApp; }
    public bool IsInitRemoteConfig { get => isInitRemoteConfig; }
    public bool IsFetchRemoteConfig { get => isFetchRemoteConfig; }
    public bool IsInitFirestore { get => isInitFirestore; }

    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
    }

    public override void Shutdown()
    {
    }

    public void InitFirebaseApp()
    {
        if (isInitFirebaseApp)
        {
            return;
        }

        Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Log.Info("Firebase Initialize Success");
                isInitFirebaseApp = true;

                GameManager.Firebase.RecordAppOpenEvent();

                InitAuth();
                InitRemoteConfig();
                InitFirestore();
            }
            else
            {
                Log.Warning("Could not resolve all Firebase dependencies: {0}", dependencyStatus);
            }
        });
    }
}
