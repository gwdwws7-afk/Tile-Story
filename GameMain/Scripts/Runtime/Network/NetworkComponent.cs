using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class NetworkComponent : GameFrameworkComponent
{
    public bool CheckInternetIsNotReachable()
    {
        return Application.internetReachability == NetworkReachability.NotReachable && GameManager.Firebase.GetBool(Constant.RemoteConfig.Enable_Offline_Restrictions, true);
    }
}
