using System;
using System.Collections.Generic;

[Serializable]
public class DecorateItem
{
    public int ID;
    public string Name;
    public int Belong;
    public string RequireString;//1002,1007,1008,1009

    public int CanChange;
    public int Cost;
    public int Root;
    public string UnlockIcon;
    public int PlayDialogueCharacterIndex;
    public string PlayDialogueTerm;
    public string PlayAudio;

    private int[] requireArray;
    public int[] Require
    {
        get
        {
            if (requireArray != null && requireArray.Length > 0)
                return requireArray;

            if (string.IsNullOrEmpty(RequireString))
                requireArray = new int[0];
            else
            {
                string[] splitString = RequireString.Split(',');
                requireArray = new int[splitString.Length];
                for (int i = 0; i < splitString.Length; ++i)
                {
                    int singleRequireInt = int.Parse(splitString[i]);
                    requireArray[i] = singleRequireInt;
                }
            }

            return requireArray;
        }
    }
}
