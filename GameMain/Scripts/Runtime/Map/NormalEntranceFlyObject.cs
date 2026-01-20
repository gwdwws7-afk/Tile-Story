using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class NormalEntranceFlyObject : EntranceFlyObject
{
    public Image flyImg;
    public TextMeshProUGUI flyText;
    public TextMeshProUGUI greenFlyText;

    private List<AsyncOperationHandle> loadAssetList = new List<AsyncOperationHandle>();
    private int num;
    public override void OnInit(string assetName, int num)
    {
        for (int i = 0; i < loadAssetList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(loadAssetList[i]);
        }
        loadAssetList.Clear();

        loadAssetList.Add(UnityUtility.LoadAssetAsync<Sprite>(assetName, sp =>
         {
             flyImg.sprite = sp;
         }));
        this.num = num;
        if (num > 1)
        {
            flyText.text = "+" + num.ToString();
            greenFlyText.text = "+" + (num*2).ToString();
            flyText.gameObject.SetActive(true);
        }
        else
        {
            flyText.gameObject.SetActive(false);
        }
    }

    public override void OnRelease()
    {
        for (int i = 0; i < loadAssetList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(loadAssetList[i]);
        }
        greenFlyText.gameObject.SetActive(false);
        loadAssetList.Clear();
        base.OnRelease();
    }

    protected override void FadeFlyObject(int value, float time)
    {
        base.FadeFlyObject(value, time);

        if (time <= 0)
        {
            flyImg.color = new Color(1, 1, 1, value);
        }
        else
        {
            flyImg.DOFade(value, time);
        }
    }

    public void ShowFlyToTargetAnim(Vector3 startPos, Vector3 targetPos, GameObject target, DoubleTagFlyObject doubleTagFlyObject,float delayTime = 0)
    {
        flyText.text = "+" + num.ToString();
        flyText.gameObject.SetActive(true);

        FadeFlyObject(0, 0);
        body.localPosition = startPos;
        body.gameObject.SetActive(true);
        FadeFlyObject(1, 0.4f);

        Vector3 startPos1;
        if (startPos.x - targetPos.x < 0)
        {
            startPos1 = flyImg.transform.localPosition - new Vector3(120, 0);
            startPos1 += new Vector3(0, -40.5f);
        }
        else
        {
            startPos1 = flyImg.transform.localPosition + new Vector3(120, 0);
            startPos1 += new Vector3(0, -40.5f);
        }
        Vector3 targetPos1 = flyImg.transform.localPosition + new Vector3(0, -40.5f);
        

        doubleTagFlyObject.onFlyObjectReach = () =>
        {
            flyText.gameObject.SetActive(false);
            greenFlyText.gameObject.SetActive(true);
            body.DOScale(new Vector3(1.07f, 0.93f), 0.1f).SetDelay(delayTime).onComplete = () =>
            {
                body.DOScale(new Vector3(0.93f, 1.07f), 0.07f).onComplete = () =>
                {
                    body.DOScale(Vector3.one, 0.07f).onComplete = () =>
                    {
                        body.DOScale(new Vector3(1.1f, 0.9f), 0.15f).SetDelay(0.1f).SetEase(Ease.InQuad).onComplete = () =>
                        {
                            body.DOScale(new Vector3(0.9f, 1.1f), 0.1f).SetEase(Ease.OutQuad).onComplete += () =>
                            {
                                body.DOScale(new Vector3(0.4f, 0.4f), 0.2f).SetEase(Ease.OutQuad);
                            };

                            body.DOLocalJump(targetPos, 110f, 1, 0.3f).SetEase(Ease.InOutQuad).onComplete = () =>
                            {
                                body.gameObject.SetActive(false);

                                ShowFlyObjectReachAnim(target);
                            };
                        };
                    };
                };
            };
        };

        body.DOScale(Vector3.one, 0.1f);
        doubleTagFlyObject.FlyDoubleTag(startPos1, targetPos1, greenFlyText.gameObject);


    }
}
