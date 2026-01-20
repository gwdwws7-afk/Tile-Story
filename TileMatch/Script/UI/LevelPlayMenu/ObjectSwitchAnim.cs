using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ObjectSwitchAnim : MonoBehaviour
{
    public Transform[] switchObjects;
    public float switchInterval = 1.2f;
    private int index;

    private void OnEnable()
    {
        if (index >= switchObjects.Length)
        {
            index = 0;
        }

        for (int i = 0; i < switchObjects.Length; i++)
        {
            if (i == index)
                switchObjects[i].localScale = Vector3.one;
            //switchObjects[i].gameObject.SetActive(i == index);
        }

        StartCoroutine(SwitchAnim());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        index = 0;
        for (int i = 0; i < switchObjects.Length; i++)
        {
            switchObjects[i].DOKill();
        }
    }

    IEnumerator SwitchAnim()
    {
        WaitForSeconds switchIntervalYield = new WaitForSeconds(switchInterval);

        while (true)
        {
            //switchObjects[index].localScale = Vector3.zero;
            //switchObjects[index].gameObject.SetActive(true);
            switchObjects[index].DOScale(1.1f, 0.2f).onComplete = () =>
            {
                switchObjects[index].DOScale(0.95f, 0.2f).onComplete = () =>
                {
                    switchObjects[index].DOScale(1f, 0.2f);
                };
            };

            yield return switchIntervalYield;

            //switchObjects[index].gameObject.SetActive(false);
            switchObjects[index].localScale = Vector3.zero;
            index++;
            if (index >= switchObjects.Length)
                index = 0;
        }
    }
}
