using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationOperationSlider : MonoBehaviour
{
    public GameObject[] backGroundGOs;
    public GameObject[] foreGroundMarks;
    public TextMeshProUGUI[] numText;

    public RectTransform fillAreaRectTrans;
    public Transform FillAreaStartPositionRefFor11;
    public int nodeWidth = 69;
    public int fillAreaRectTransMaxWidth = 818;

    public int testTotalNum;
    public int testFinishNum;
    public bool toTest;

    private void Update()
    {
        if(toTest)
        {
            toTest = false;
            SetSliderValue(testFinishNum, testTotalNum);
        }
    }


    public void SetSliderValue(int finishNum, int totalNum)
    {
        for (int i = 0; i < backGroundGOs.Length; ++i)
        {
            //头尾一定保留
            if (i == 0 || i == backGroundGOs.Length - 1)
                backGroundGOs[i].SetActive(true);
            else
            {
                if (i < totalNum - 1)
                    backGroundGOs[i].SetActive(true);
                else
                    backGroundGOs[i].SetActive(false);
            }
        }
        for (int i = 0; i < foreGroundMarks.Length; ++i)
        {
            if ((foreGroundMarks.Length - i) > (foreGroundMarks.Length - totalNum + finishNum))
                foreGroundMarks[i].SetActive(false);
            else if(foreGroundMarks.Length - i <= foreGroundMarks.Length - totalNum)
                foreGroundMarks[i].SetActive(false);
            else
                foreGroundMarks[i].SetActive(true);
        }

        for(int i = 0; i < numText.Length; ++i)
        {
            if (i < numText.Length - totalNum)
                numText[i].gameObject.SetActive(false);
            else
            {
                if (i < numText.Length - totalNum + finishNum)
                    numText[i].gameObject.SetActive(false);
                else
                    numText[i].gameObject.SetActive(true);
                numText[i].text = (i - numText.Length + totalNum + 1).ToString();
            }
        }

        fillAreaRectTrans.transform.localPosition = FillAreaStartPositionRefFor11.localPosition + new Vector3(nodeWidth, 0, 0) * (backGroundGOs.Length - totalNum);

        Vector2 formerV2 = fillAreaRectTrans.sizeDelta;
        fillAreaRectTrans.sizeDelta = new Vector2(54 + nodeWidth * (finishNum - 1) + (finishNum == totalNum? 162: 0), formerV2.y);

        //slider.value = finishNum / (float)totalNum;
    }

    public Transform GetShinningFlyDestinationPosition()
    {
        for (int i = 0; i < numText.Length; ++i)
        {
            if (numText[i].gameObject.activeSelf)
                return numText[i].transform;
        }
        return transform;
    }
}
