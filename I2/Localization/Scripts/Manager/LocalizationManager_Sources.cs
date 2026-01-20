using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {

        #region Variables: Misc

        public static List<LanguageSourceData> Sources = new List<LanguageSourceData>();
		public static string[] GlobalSources = { "DefaultLanguageData" };

        #endregion

        #region Sources

        public static bool UpdateSources()
		{
			return Sources.Count>0;
		}

		public static Func<LanguageSourceData, bool> Callback_AllowSyncFromGoogle = null;

		internal static void AddSource ( LanguageSourceData Source )
		{
			if (Sources.Contains (Source))
				return;

            Sources.Add( Source );

            //if (force)
            {
                for (int i = 0; i < Source.mLanguages.Count; ++i)
                    Source.mLanguages[i].SetLoaded(true);
            }

            if (Source.mDictionary.Count==0)
				Source.UpdateDictionary(true);
		}

		internal static void RemoveSource (LanguageSourceData Source )
		{
			//Debug.Log ("RemoveSource " + Source+" " + Source.GetInstanceID());
			Sources.Remove( Source );
		}

		public static LanguageSourceData GetSourceContaining( string term, bool fallbackToFirst = true )
		{
			if (!string.IsNullOrEmpty(term))
			{
				for (int i=0, imax=Sources.Count; i<imax; ++i)
				{
					if (Sources[i].GetTermData(term) != null)
						return Sources[i];
				}
			}
			
			return fallbackToFirst && Sources.Count>0 ? Sources[0] :  null;
		}

		public static Object FindAsset (string value)
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				Object Obj = Sources[i].FindAsset(value);
				if (Obj)
					return Obj;
			}
			return null;
		}

        #endregion

    }
}
