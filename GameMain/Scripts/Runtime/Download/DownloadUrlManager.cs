using System.Collections;
using System.Collections.Generic;

namespace MyDownloadUrl
{
    public class DownloadUrlManager
    {
        //public static string DownloadUrl = "https://bubbleshooterapp.com/TileStoryHelp";
        
        private static bool isUseAdmobSdk => GameManager.Firebase.GetBool(Constant.RemoteConfig.If_Use_AdmobSDK, true);
        public static string DownloadUrl
        {
            get
            {
                if (isUseAdmobSdk)
                {
                    return DefaultDownloadUrl;
                }

                //针对俄罗斯走双层处理
                if (isUseGoogleUrl)
                {
                    return DefaultDownloadUrl;
                }else
                    return RussiaDownloadUrl;
            }
        }
        public static string DefaultDownloadUrl = "https://bubbleshooterapp.com/TileStoryHelp";
        public static string RussiaDownloadUrl = "https://user12885.clients-cdnnow.ru/TileStoryHelp";

        private static bool isUseGoogleUrl = true;
        public static void SetRussiaFailCallBack()
        {
            if (!isUseAdmobSdk)
            {
                isUseGoogleUrl = !isUseGoogleUrl;
                Log.Info($"切换下载路径：{(isUseGoogleUrl?"使用谷歌cdn":"使用俄罗斯cdn")}");
            }
        }
        
        public static void ForceUseRussiaCDN()
        {
            isUseGoogleUrl = false;
            Log.Info($"强制使用俄罗斯cdn!,必须要俄罗斯的网才行！");
        }
    }
}
