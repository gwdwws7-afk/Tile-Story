using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationBGDialogBubble : MonoBehaviour
{
    [SerializeField]
    private Transform scaleRoot, bg, edge, triangleEdge;
    [SerializeField]
    private TextMeshProUGUI talkText;
    
    private HorizontalLayoutGroup layoutGroup;
    private RectTransform talkTextRectTrans;
    private TextMeshProUGUILocalize talkTextLocalize;

    private Coroutine hideCoroutine;
    

    public void ShowTalk(IdleDialogueData dialogueData, Transform dialogPosRef, bool dialogFaceLeft, bool useTriangleEdge)
    {
        //Log.Info($"DecorationBGDialogBubble dialogueData.ID = {dialogueData.ID}");
        if (!string.IsNullOrEmpty(dialogueData.SoundEffect))
            GameManager.Sound.PlayAudio(dialogueData.SoundEffect);
        ShowTalk(dialogueData.Dialogue, dialogPosRef, dialogFaceLeft, useTriangleEdge);
    }

    public void ShowTalk(string term, Transform dialogPosRef, bool dialogFaceLeft, bool useTriangleEdge)
    {
        if (layoutGroup == null)
            layoutGroup = bg.GetComponent<HorizontalLayoutGroup>();
        if (talkTextRectTrans == null)
            talkTextRectTrans = talkText.GetComponent<RectTransform>();
        if (talkTextLocalize == null)
            talkTextLocalize = talkText.GetComponent<TextMeshProUGUILocalize>();

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        scaleRoot.DOKill();

        gameObject.SetActive(true);

        scaleRoot.localScale = Vector3.zero;
        scaleRoot.DOScale(1, 0.5f).SetEase(Ease.OutQuart);
        if (!dialogFaceLeft)
        {
            scaleRoot.localRotation = new Quaternion(0, 180, 0, 0);
            talkText.transform.localRotation = new Quaternion(0, 180, 0, 0);
        }
        else
        {
            scaleRoot.localRotation = new Quaternion(0, 0, 0, 0);
            talkText.transform.localRotation = new Quaternion(0, 0, 0, 0);
        }

        //先尝试以540为宽度 看看设置进Text会不会产生换行
        layoutGroup.childControlWidth = false;
        talkTextRectTrans.sizeDelta = new Vector2(540, talkTextRectTrans.sizeDelta.y);
        talkTextLocalize.SetTerm(term);
        TMP_TextInfo textInfo = talkText.GetTextInfo(talkText.text);
        //如果不会的话 把layoutGroup.childControlWidth打开 让框更紧贴Text
        if (textInfo.lineCount == 1)
        {
            layoutGroup.childControlWidth = true;
        }

        transform.position = dialogPosRef.position;
        //尝试完全根据传入位置调整气泡水平位置 保证气泡框永远在屏幕中间
        Vector3 formerBGLocalPosition = bg.transform.localPosition;
        formerBGLocalPosition.x = -transform.localPosition.x;
        if(!dialogFaceLeft)
            formerBGLocalPosition.x = -formerBGLocalPosition.x;
        bg.transform.localPosition = formerBGLocalPosition;

        edge.gameObject.SetActive(!useTriangleEdge);
        triangleEdge.gameObject.SetActive(useTriangleEdge);

        //部分情况(长度刚能换行？)下Text的高度可能会计算错误(比预期小)
        //没找到根本原因 这样延时刷新一下的话 倒是基本不会再出现了
        GameManager.Task.AddDelayTriggerTask(0.0f, () =>
        {
            talkText.SetAllDirty();

            if (bg.GetComponent<RectTransform>().sizeDelta.x < 440)
            {
                //修正edge与气泡框偏移过大问题(后来改成调成edge的位置，使气泡框始终在中心防止被UI遮挡)
                float delta = transform.localPosition.x - edge.transform.localPosition.x;
                delta = delta >= 0 ? delta : -delta;
                float maxDis = bg.GetComponent<RectTransform>().sizeDelta.x / 2f - 180;
                if (delta > maxDis)
                {
                    bg.transform.localPosition -= new Vector3((delta - maxDis), 0, 0);
                }
            }
        });

        hideCoroutine = StartCoroutine(HideTalkPanel());
    }

    IEnumerator HideTalkPanel()
    {
        yield return new WaitForSeconds(2.5f);
        scaleRoot.DOScale(0, 0.4f).SetEase(Ease.InQuart);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

}
