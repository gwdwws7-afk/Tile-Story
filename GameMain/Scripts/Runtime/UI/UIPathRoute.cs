using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPathRoute : MonoBehaviour
{
    public Transform routeParent;
    public RectTransform ablePanel;
#if UNITY_EDITOR
    public bool autoShoot;
    public int autoShootWait;
    public GameObject testObject;
    public GameObject testTarget;
    int count;
    // Update is called once per frame
    void Update()
    {
        if (autoShoot)
        {
            count++;
            if ((count % autoShootWait) == 0)
                FlyEffectTo(testObject, testTarget.transform.position, 0.6f, (g)=> {
                    GameManager.Task.AddDelayTriggerTask(0.1f, () =>
                    {
                        g.gameObject.SetActive(false);
                        g.transform.DOKill();
                        Destroy(g.gameObject);
                    });
                });
        }
    }
#endif

    public void FlyEffectTo(GameObject originItem, Vector3 targetPosition, float useTime = 0.6f, Action<GameObject> onHit = null)
    {
        Vector2 target = GameManager.Scene.MainCamera.WorldToScreenPoint(targetPosition);
        ablePanel.RectTransfromResize(GameManager.Scene.MainCamera, target.y, 0);
        Vector3 bornPosition = routeParent.GetChild(0).position;
        GameObject flyObject = Instantiate(originItem, ablePanel.parent);
        flyObject.transform.position = bornPosition;
        Vector3[] path = new Vector3[routeParent.childCount - 1];
        for (int i = 0; i < path.Length; i++)
        {
            path[i] = routeParent.GetChild(i + 1).transform.position;
        }
        flyObject.transform.DOPath(path, useTime, PathType.CubicBezier, PathMode.TopDown2D, 20).onComplete = () =>
        {
            onHit?.Invoke(flyObject);
        };
    }
}
