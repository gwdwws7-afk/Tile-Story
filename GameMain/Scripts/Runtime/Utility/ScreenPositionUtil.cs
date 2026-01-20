using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class ScreenPositionUtil
{

    public static float RealHeight
    {
        get
        {
            if (Screen.height / Screen.width > 1920f / 1080f)//高一点
            {
                return Screen.height / Screen.width * 1080f;
            }
            else
            {
                return 1920f;
            }
        }
    }

    /// <summary>
    /// 模拟点击并寻找上级中的目标Component(Screen)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="position"></param>
    /// <returns></returns>
    public static T RaycastFindClick<T>(Vector3 position)
    {
        if (EventSystem.current == null) return default(T);
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = position;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        //向点击位置发射一条射线，检测是否点击UI
        EventSystem.current.RaycastAll(eventData, raycastResults);
        T result = default;
        if (raycastResults!=null&&raycastResults.Count > 0)
        {
            for (int i = 0; i < 1; i++)
            {
                result = raycastResults[i].gameObject.GetComponent<T>();
                if (result != null) return result;
            }
        }
        return result;
    }
    public static T RaycastFindClickMul<T>(Vector3 position)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = position;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        //向点击位置发射一条射线，检测是否点击UI
        EventSystem.current.RaycastAll(eventData, raycastResults);
        T result = default;
        if (raycastResults.Count > 0)
        {
            for (int i = 0; i < raycastResults.Count; i++)
            {
                result = raycastResults[i].gameObject.GetComponent<T>();
                if (result != null) return result;
            }
        }
        return result;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="targetY">直接的中心点</param>
    /// <param name="type">0:上边界锁定延展 1:下边界锁定延展 2:</param>
    public static void RectTransfromResize(this RectTransform rect, Camera from, float targetY, int type = 0)
    {
        float selfcenter = from.WorldToScreenPoint(rect.position).y;
        float delta = selfcenter - targetY;//修正偏移值
        //偏移值是纵向的原始屏幕距离，需要缩放
        delta = delta / Screen.height * RealHeight;
        delta -= rect.sizeDelta.y / 2;//高度
        rect.anchoredPosition -= new Vector2(0, (delta) / 2);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y + delta);
    }
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/AnchorsAdapt")]
    static void SetAdaptAnchor()
    {
        foreach (var go in UnityEditor.Selection.gameObjects)
        {
            if (go.GetComponent<RectTransform>() != null)
                AnchorsPivotAdapt(go);
        }
    }
#endif
    public static void AnchorsPivotAdapt(GameObject go)
    {
        RectTransform parent = go.transform.parent as RectTransform;
        RectTransform self = go.transform as RectTransform;
        Vector2 anchor = new Vector2((self.position.x - parent.position.x) / parent.rect.width + .5f, (self.position.y - parent.position.y) / parent.rect.height + .5f);
        self.anchorMax = anchor;
        self.anchorMin = anchor;
        self.anchoredPosition = Vector2.zero;
    }

    public static Vector3 GetScreenCenterPos()
    {
        Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(center);
        return center;
    }
}
