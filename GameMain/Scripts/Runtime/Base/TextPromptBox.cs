using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 文本提示框
/// </summary>
public class TextPromptBox : PromptBox
{
    public TextMeshProUGUI promptText;
    public RectTransform promptTextTransform;

    [Header("Text Setting")]
    public float textMaxWidth = 600;
    public float textMaxHeight = 1000;

    public void SetText(string content)
    {
        promptText.GetComponent<TextMeshProUGUILocalize>().SetTerm(content);
    }

    public override void Refresh()
    {
        promptText.GetPreferredValues(0, 0);
        float textPreferredWidth = promptText.preferredWidth < textMaxWidth ? promptText.preferredWidth : textMaxWidth;
        float textPreferredHeight = promptText.preferredHeight < textMaxHeight ? promptText.preferredHeight : textMaxHeight;

        promptTextTransform.sizeDelta = new Vector2(textPreferredWidth, textPreferredHeight);

        boxPreferredWidth = textPreferredWidth + boxHorizontalPadding;
        boxPreferredWidth = boxPreferredWidth < boxMaxWidth ? boxPreferredWidth : boxMaxWidth;
        boxPreferredWidth = boxPreferredWidth > boxMinWidth ? boxPreferredWidth : boxMinWidth;

        boxPreferredHeight = textPreferredHeight + boxVerticalPadding;
        boxPreferredHeight = boxPreferredHeight < boxMaxHeight ? boxPreferredHeight : boxMaxHeight;
        boxPreferredHeight = boxPreferredHeight > boxMinHeight ? boxPreferredHeight : boxMinHeight;

        box.sizeDelta = new Vector2(boxPreferredWidth, boxPreferredHeight);

        base.Refresh();
    }
}
