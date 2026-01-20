using Firebase.Firestore;
using GameFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class FirebaseManager : GameFrameworkModule, IFirebaseManager
{
    public void InitFirestore()
    {
        // Get the root reference location of the database.
        FirebaseFirestore.DefaultInstance.Settings.PersistenceEnabled = false;

        isInitFirestore = true;
    }
}
