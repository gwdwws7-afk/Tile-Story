using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElementDescribeArea : MonoBehaviour
{
    public TextMeshProUGUI describeText;
    public TextMeshProUGUILocalize titleText;

    public TextMeshProUGUILocalize lockText;

    public Transform textObject;



    public void SetLockState(bool isLock)
    {
        lockText.gameObject.SetActive(isLock);
    }

    public void SetTextPosition()
    {
        TMP_TextInfo textInfo = describeText.GetTextInfo(describeText.text);
        int lineCount = 3;

        if (textInfo != null)
        {
            lineCount = describeText.GetTextInfo(describeText.text).lineCount;
        }
        titleText.FitFontSize(true, false);
        switch (lineCount)
        {
            case 0:
                textObject.SetLocalPositionY(-34);
                break;
            case 1:
                textObject.SetLocalPositionY(-34);
                break;
            case 2:
                textObject.SetLocalPositionY(-17);
                break;
            case 3:
                textObject.SetLocalPositionY(0);
                break;
        }
        if(GameManager.Localization.Language == Language.ChineseSimplified || GameManager.Localization.Language == Language.ChineseTraditional)
        {
            describeText.lineSpacing = -40;
        }
        else
        {
            describeText.lineSpacing = -10;
        }
    }

    public void SetLevelText(int num)
    {
        lockText.SetParameterValue("LevelNum",num.ToString());
    }

}
