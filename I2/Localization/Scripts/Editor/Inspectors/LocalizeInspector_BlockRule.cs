using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 处理加载
/// </summary>
public class LocalizeInspector_BlockRule
{
    public static bool BlockTerms(string term)
    {
        if (term.StartsWith("CardSet_") || term.StartsWith("Card_"))
            return true;
        if (term.StartsWith("HeadIconName_") || term.StartsWith("HeadIconAccess_"))
            return true;
        return false;
    }
}
