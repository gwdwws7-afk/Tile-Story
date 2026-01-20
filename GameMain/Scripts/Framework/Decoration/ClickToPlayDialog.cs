using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToPlayDialog : MonoBehaviour
{
    [SerializeField]
    private int characterIndex = 1;//��1��ʼ�� ����������еڶ�����ɫ ��ô����2
    [SerializeField]
    private Transform dialogBubblePosRef;
    [SerializeField]
    private DelayButton clickArea;
    [SerializeField]
    private bool dialogFaceLeft;
    [SerializeField]
    private bool useTriangleEdge;//使用三角形的角角 而非弧线的角角 一旦勾上 也无所谓是否dialogFaceLeft了

    private MapDecorationBGPanel parentPanel;


    public void Init(MapDecorationBGPanel inputParentPanel)
    {
        parentPanel = inputParentPanel;
        clickArea.OnInit(() => OnButtonClicked());
    }

    public void OnButtonClicked(bool skipClickSound = false)
    {
        if (parentPanel != null)
        {
            parentPanel.PlayDialogBubble(characterIndex, dialogBubblePosRef, dialogFaceLeft, useTriangleEdge, skipClickSound);
        }
    }

    public void PlayDialogue(int inputCharacterIndex, string term)
    {
        if(characterIndex == inputCharacterIndex)
        {
            if(!string.IsNullOrEmpty(term))
                parentPanel.PlayDialogBubble(dialogBubblePosRef, dialogFaceLeft, useTriangleEdge, true, term);
        }
    }

}
