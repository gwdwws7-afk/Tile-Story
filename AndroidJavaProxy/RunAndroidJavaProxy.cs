using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCallback : AndroidJavaProxy
{
    public MyCallback() : base("com.DefaultCompany.AndroidJavaProxyCrash.MyCallback")
    {
    }

    public void Foo()
    {
        Debug.Log("Foo");
    }
}

public static class RunAndroidJavaProxy
{
    public static void StartRunAndroidJavaProxy()
    {
        #if !UNITY_EDITOR&&UNITY_ANDROID
        using var klass = new AndroidJavaClass("com.DefaultCompany.AndroidJavaProxyCrash.RunAndroidJavaProxy");
        var callback = new MyCallback();
        klass.CallStatic("InvokeCallbackAsWorkaround", callback);
        #endif
    }
}
