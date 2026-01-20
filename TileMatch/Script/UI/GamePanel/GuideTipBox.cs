using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideTipBox : MonoBehaviour
{
    public DelayButton okButton;
    public TextMeshProAdapterBox adapterBox;

    public void OnShow(Vector3 localPos)
    {
        Transform boxTrans = transform;

        Vector3 originalPos = localPos;
        boxTrans.localScale = new Vector3(0.5f, 0.5f);
        boxTrans.localPosition = new Vector3(10000, 0);
        gameObject.SetActive(true);

        float delayTime = Time.deltaTime * 2;

        boxTrans.DOScale(0.5f, delayTime).onComplete = () =>
        {
            boxTrans.localPosition = originalPos;

            adapterBox.Refresh();

            boxTrans.DOScale(1.1f, 0.15f).onComplete = () =>
            {
                boxTrans.DOScale(1f, 0.15f);
            };
        };
    }

    public void OnHide()
    {
        transform.DOKill();

        gameObject.SetActive(false);
    }

    public void SetText(string content)
    {
        adapterBox.SetText(content);
    }

    public void SetOkButton(bool isShow)
    {
        if (isShow)
        {
            okButton.gameObject.SetActive(true);
            adapterBox.boxBottomPadding = 230;
        }
        else
        {
            okButton.gameObject.SetActive(false);
            adapterBox.boxBottomPadding = 50;
        }
    }
}
