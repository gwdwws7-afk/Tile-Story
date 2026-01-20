using UnityEngine;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {
        static string mCurrentDeviceLanguage;

        public static string GetCurrentDeviceLanguage( bool force = false )
        {
            if (force || string.IsNullOrEmpty(mCurrentDeviceLanguage))
                DetectDeviceLanguage();

            return mCurrentDeviceLanguage;
        }

        static void DetectDeviceLanguage()
        {
            mCurrentDeviceLanguage = Application.systemLanguage.ToString();
            if (mCurrentDeviceLanguage == "ChineseSimplified") mCurrentDeviceLanguage = "Chinese (Simplified)";
            if (mCurrentDeviceLanguage == "ChineseTraditional") mCurrentDeviceLanguage = "Chinese (Traditional)";
        }
    }
}