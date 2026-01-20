using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonGuideImage : Image
{
    public HighlightAreaType areaType;

    public Vector3 highlightAreaCenter;
    public Vector3 raycastCenter;
    public bool raycastAll;

    public Action onAreaClick;
    public Action onTargetAreaClick;
    public Action onNonTargetAreaClick;

    private float highlightAreaRadius;
    private float highlightAreaRectX;
    private float highlightAreaRectY;

    private float localHighlightAreaRadius;
    private float localHighlightAreaRectX;
    private float localHighlightAreaRectY;

    private Material guideMaterial;
    private bool isInit;
    private bool isRaycastValid;

    private float inputDelayTime = 0.5f;
    private float timer;

    private const string circleShaderPath = "UI/Default_Mask";
    private const string rectShaderPath = "UI/Default_Mask_Rect";

    public void OnInit(Vector3 center, float radius)
    {
        areaType = HighlightAreaType.Circle;
        highlightAreaRadius = radius;

        localHighlightAreaRadius = radius / (Screen.height / 2f);

        Init(center);

        timer = inputDelayTime;
    }

    public void OnInit(Vector3 center, float rectX, float rectY)
    {
        areaType = HighlightAreaType.Rect;
        highlightAreaRectX = rectX;
        highlightAreaRectY = rectY;

        localHighlightAreaRectX = rectX / (Screen.height / 2f);
        localHighlightAreaRectY = rectY / (Screen.height / 2f);

        Init(center);
    }

    private void Init(Vector3 areaCenter)
    {
        highlightAreaCenter = areaCenter;

        switch (areaType)
        {
            case HighlightAreaType.Circle:
                guideMaterial = new Material(Shader.Find(circleShaderPath));
                break;
            case HighlightAreaType.Rect:
                guideMaterial = new Material(Shader.Find(rectShaderPath));
                break;
        }
        material = guideMaterial;

        Vector3 pos = GameManager.Scene.MainCamera.WorldToScreenPoint(areaCenter);

        Canvas[] c = GetComponentsInParent<Canvas>();

        RectTransform rect;
        if (c != null && c.Length > 0)
        {
            rect = c[c.Length - 1].GetComponent<RectTransform>();
        }
        else
        {
            rect = GetComponent<RectTransform>();
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, pos, GameManager.Scene.MainCamera, out Vector2 localPos);

        var center = new Vector4(localPos.x, localPos.y, 0f, 0f);
        material.SetVector("_Center", center);

        switch (areaType)
        {
            case HighlightAreaType.Circle:
                material.SetFloat("_Silder", highlightAreaRadius);
                break;
            case HighlightAreaType.Rect:
                material.SetFloat("_SliderX", highlightAreaRectX);
                material.SetFloat("_SliderY", highlightAreaRectY);
                break;
        }

        isInit = true;
    }

    public void InitClick()
    {
        isInit = true;
    }

    public void OnReset()
    {
        isInit = false;

        if (guideMaterial != null)
        {
            Destroy(guideMaterial);
        }
        highlightAreaCenter = Vector3.zero;
        raycastCenter = Vector3.zero;
        highlightAreaRadius = 0;
        highlightAreaRectX = 0;
        highlightAreaRectY = 0;
        localHighlightAreaRadius = 0;
        localHighlightAreaRectX = 0;
        localHighlightAreaRectY = 0;
        raycastAll = false;
        onAreaClick = null;
        onTargetAreaClick = null;
        onNonTargetAreaClick = null;
        isRaycastValid = false;
        raycastTarget = true;
    }

    private void Update()
    {
        if (!isInit && !raycastAll) 
        {
            return;
        }

        CheckInput(isRaycastValid);
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!isInit || raycastAll)  
        {
            isRaycastValid = base.IsRaycastLocationValid(screenPoint, eventCamera);

            return isRaycastValid;
        }

        bool isValid = false;
        Vector3 nowRaycastCenter = raycastCenter == Vector3.zero ? highlightAreaCenter : raycastCenter;
        Vector3 worldPos = eventCamera.ScreenToWorldPoint(screenPoint);

        if (areaType == HighlightAreaType.Circle)
        {
            float dis = Vector2.Distance(worldPos, nowRaycastCenter);
            isValid = dis > localHighlightAreaRadius;
        }
        else if (areaType == HighlightAreaType.Rect) 
        {
            isValid = Mathf.Abs(worldPos.x - nowRaycastCenter.x) > localHighlightAreaRectX || Mathf.Abs(worldPos.y - nowRaycastCenter.y) > localHighlightAreaRectY;
        }

        isRaycastValid = isValid;

        return isValid;
    }

    public void CheckInput(bool isValid)
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            OnAreaClick();
            if (isValid)
            {
                OnNonTargetAreaClick();
            }
            else
            {
                OnTargetAreaClick();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            OnAreaClick();
            if (isValid)
            {
                OnNonTargetAreaClick();
            }
            else
            {
                OnTargetAreaClick();
            }
        }
    }

    public void OnAreaClick()
    {
        Log.Info("OnAreaClick");

        onAreaClick?.Invoke();
        onAreaClick = null;
    }

    public void OnTargetAreaClick()
    {
        Log.Info("OnTargetAreaClick");

        onTargetAreaClick?.Invoke();
        onTargetAreaClick = null;
    }

    public void OnNonTargetAreaClick()
    {
        Log.Info("OnNonTargetAreaClick");

        onNonTargetAreaClick?.Invoke();
        onNonTargetAreaClick = null;
    }
}

public enum HighlightAreaType
{
    Circle,
    Rect
}
