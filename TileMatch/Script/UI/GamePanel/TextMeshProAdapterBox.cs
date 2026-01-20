using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextMeshProAdapterBox : MonoBehaviour
{
    public RectTransform box;
    public RectTransform textTransform;
    public TextMeshProUGUI text;

    [Header("Text Size")]
    public float textMaxWidth = 600;
    public float textMaxHeight = 1000;

    [Header("Box Size")]
    public float boxMinWidth = 600;
    public float boxMinHeight = 190;
    public float boxMaxWidth = 600;
    public float boxMaxHeight = 1000;

    [Header("Box Padding")]
    public float boxLeftPadding = 10;
    public float boxRightPadding = 10;
    public float boxTopPadding = 50;
    public float boxBottomPadding = 50;

    [Header("Text Delta")]
    public float textPosDeltaY = 9;

    private bool deferredRefresh = false;

    private void Update()
    {
        if (deferredRefresh)
        {
            deferredRefresh = false;
            Refresh();
        }
    }

    public void SetText(string content)
    {
        text.GetComponent<TextMeshProUGUILocalize>().SetTerm(content);
        deferredRefresh = true;
    }

    public void Refresh()
    {
        deferredRefresh = false;

        //refresh text layout
        text.GetPreferredValues();
        float textPreferredWidth = text.preferredWidth;
        float textPreferredHeight = text.preferredHeight;

        textPreferredWidth = text.preferredWidth < textMaxWidth ? text.preferredWidth : textMaxWidth;
        textPreferredHeight = text.preferredHeight < textMaxHeight ? text.preferredHeight : textMaxHeight;

        if (textPreferredWidth + boxLeftPadding + boxRightPadding > boxMaxWidth) 
        {
            textPreferredWidth = boxMaxWidth - boxLeftPadding - boxRightPadding;
        }

        textTransform.sizeDelta = new Vector2(textPreferredWidth, textPreferredHeight);

        //refresh box layout
        float boxPreferredWidth = textPreferredWidth + boxLeftPadding + boxRightPadding;
        boxPreferredWidth = boxPreferredWidth < boxMaxWidth ? boxPreferredWidth : boxMaxWidth;
        boxPreferredWidth = boxPreferredWidth > boxMinWidth ? boxPreferredWidth : boxMinWidth;

        float boxPreferredHeight = textPreferredHeight + boxTopPadding + boxBottomPadding;
        boxPreferredHeight = boxPreferredHeight < boxMaxHeight ? boxPreferredHeight : boxMaxHeight;
        boxPreferredHeight = boxPreferredHeight > boxMinHeight ? boxPreferredHeight : boxMinHeight;

        box.sizeDelta = new Vector2(boxPreferredWidth, boxPreferredHeight);

        //refresh text pos
        float deltaX = boxPreferredWidth - textPreferredWidth > 0 ? boxPreferredWidth - textPreferredWidth : textPreferredWidth - boxPreferredWidth;
        float deltaY = boxPreferredHeight - textPreferredHeight > 0 ? boxPreferredHeight - textPreferredHeight : textPreferredHeight - boxPreferredHeight;

        float textPosX = 0;
        if (textPreferredWidth + boxLeftPadding + boxRightPadding >= boxPreferredWidth && textPreferredWidth + boxLeftPadding + boxRightPadding <= boxMaxWidth) 
        {
            textPosX = boxPreferredWidth / 2f - boxLeftPadding - textPreferredWidth / 2f;
        }

        float textPosY = 0;
        if (textPreferredHeight + boxTopPadding + boxBottomPadding + 5 >= boxPreferredHeight)  
        {
            textPosY = boxPreferredHeight / 2f - boxTopPadding - textPreferredHeight / 2f;
        }

        textTransform.localPosition = new Vector3(textPosX, textPosY + textPosDeltaY);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Refresh();
    }
#endif
}
