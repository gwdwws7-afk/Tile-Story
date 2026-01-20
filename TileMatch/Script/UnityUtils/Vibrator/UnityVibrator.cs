using UnityEngine;
using System.Runtime.InteropServices;

//https://iphonedev.wiki/index.php/AudioServices
namespace UnityUtil
{
    public enum EVibatorType
    {
        [Tooltip("超短 40毫秒")]
        VeryShort,
        [Tooltip("短 80毫秒")]
        Short,
        [Tooltip("一般 15000毫秒")]
        Medium,
        [Tooltip("长 500毫秒")]
        Heavy,
        [Tooltip("超长 900毫秒")]
        Success,
        [Tooltip("超长 900毫秒")]
        Failure,
    }
    public static class UnityVibrator
    {
        public static int[] VeryShort_Datas;
        public static int[] Short_Datas;
        public static void PlayerVibrator(EVibatorType type)
        {
            switch (type)
            {
                case EVibatorType.VeryShort:
                    Play(1519, 1, 40,50);
                    break;
                case EVibatorType.Short:
                    Play(1519, 1, 50,60);
                    break;
                case EVibatorType.Medium:
                    Play(1350, 1, 150,255);
                    break;
                case EVibatorType.Heavy:
                    Play(1350, 2, 500,255);
                    break;
                case EVibatorType.Success:
                    Play(1350, 3, 900,255);
                    break;
                case EVibatorType.Failure:
                    Play(1350, 3, 900,255);
                    break;
            }
        }

        public static void Play(int soundId, int loopCount, long millisec,int amplitude)
        {
            if (SystemInfo.supportsVibration)
            {
#if !UNITY_EDITOR&&(UNITY_IOS ||UNITY_IPHONE)
            PlaySystemSound(soundId, loopCount);
#elif !UNITY_EDITOR&&UNITY_ANDROID
            Vibrate(millisec,amplitude);
#endif
            }
        }

#if !UNITY_EDITOR && UNITY_IOS
        [DllImport ("__Internal")]
        static extern void _playSystemSound(int soundId, int loopCount);

        public static void PlaySystemSound(int soundId, int loopCount)
        {
            _playSystemSound(soundId, loopCount);
        }
#endif


  #if !UNITY_EDITOR && UNITY_ANDROID
        public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        private static AndroidJavaClass vibrationEffect=null;

        private static int version = 0;
        public static int AndroidVersion 
        {
            get 
            {
                if (version == 0 && Application.platform == RuntimePlatform.Android) 
                {
                    try
                    {
                        string androidVersion = SystemInfo.operatingSystem;
                        int sdkPos = androidVersion.IndexOf("API-");
                        version = int.Parse(androidVersion.Substring(sdkPos + 4, 2).ToString());
                    }
                    catch
                    {
                        version = 1;
                    }
                }
                return version;
            }
        }

        // android.permission.VIBRATE
        private static void _permission(){ Handheld.Vibrate(); }

        public static void Vibrate(long millisec,int amplitude)
        {
            void playVibrateOld()
            {
                if(vibrator!=null) vibrator.Call("vibrate", millisec);
            }

            if ( AndroidVersion >= 26 )
            {
                if (vibrationEffect == null) vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");

                if (vibrationEffect != null)
                {
                    AndroidJavaObject createOneShot = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", new object[] { millisec, amplitude });

                    if (createOneShot != null && vibrator != null)
                    {
                        vibrator.Call("vibrate", createOneShot);
                    }
                    else
                        playVibrateOld();
                }else
                    playVibrateOld();
            } else 
            {
                playVibrateOld();
            }
        }
#endif
    }
}
