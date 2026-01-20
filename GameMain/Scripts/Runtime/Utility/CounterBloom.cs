using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class CounterBloom : MonoBehaviour
{
    public TextMeshProUGUI outputText;
    public TextMeshProUGUI markText;
    public GameObject charText;

    public Vector3 maxSize;
    public float punchTime = 0.2f;
    public Ease scaleEase;

    private int nowNum;
    private Queue<Transform> charTextClones = new Queue<Transform>();

    public void OnInit(int startNum)
    {
        nowNum = startNum;
        outputText.text = startNum.ToString();
        markText.text = startNum.ToString();
    }

    public void TextNumChange(int targetNum)
    {
        if (targetNum == nowNum) return;

        var s = nowNum.ToString();
        var t = targetNum.ToString();

        markText.text = t;
        markText.ForceMeshUpdate();

        for (int i = 0; i < t.Length; i++)
        {
            if (i >= s.Length || s[i] != t[i])
            {
                float centerX = (markText.textInfo.characterInfo[i].bottomLeft.x + markText.textInfo.characterInfo[i].bottomRight.x) / 2f;

                Transform charTextClone = null;
                if (charTextClones.Count == 0)
                {
                    charTextClone = Instantiate(charText, transform).transform;
                }
                else
                {
                    charTextClone = charTextClones.Dequeue();
                }

                var cloneText = charTextClone.GetComponent<TextMeshProUGUI>();
                cloneText.text = t[i].ToString();
                cloneText.color = new Color(1, 1, 1, 0);
                charTextClone.localPosition = new Vector3(centerX, 0f, 0f);
                charTextClone.localScale = maxSize;
                charTextClone.gameObject.SetActive(true);
                cloneText.DOFade(1f, punchTime - 0.05f);

                charTextClone.DOScale(1f, punchTime).SetEase(scaleEase).OnComplete(() => {
                    ResetCharTextClone(charTextClone);
                });
            }
        }

        //生成并安排一次撞击与改变
        nowNum = targetNum;

        Invoke("RefreshOutputText", punchTime - 0.1f);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    private void RefreshOutputText()
    {
        outputText.text = nowNum.ToString();
    }

    /// <summary>
    /// 回收
    /// </summary>
    private void ResetCharTextClone(Transform charTextClone)
    {
        charTextClone.gameObject.SetActive(false);
        charTextClones.Enqueue(charTextClone);
    }
}
