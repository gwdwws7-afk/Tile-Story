using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/FontCurve")]
public class FontCurve : BaseMeshEffect
{
    [SerializeField]
    AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 10);

    [SerializeField]
    float curveMultiplier = 1;

    RectTransform rectTransform;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        for (int index = 0; index < verts.Count; index++)
        {
            var uiVertex = verts[index];
            uiVertex.position.y += curve.Evaluate(rectTransform.rect.width * rectTransform.pivot.x + uiVertex.position.x) * curveMultiplier;
            verts[index] = uiVertex;
        }
        vh.AddUIVertexTriangleStream(verts);
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (curve[0].time != 0)
        {
            var tmpRect = curve[0];
            tmpRect.time = 0;
            curve.MoveKey(0, tmpRect);
        }
        if (rectTransform== null)
            rectTransform= GetComponent<RectTransform>();
        if (curve[curve.length - 1].time != rectTransform.rect.width)
            OnRectTransformDimensionsChange();
    }
#endif

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        OnRectTransformDimensionsChange();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        rectTransform = GetComponent<RectTransform>();
        OnRectTransformDimensionsChange();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        var temp = curve[curve.length - 1];
        if (rectTransform != null)
        {
            temp.time = rectTransform.rect.width;
            curve.MoveKey(curve.length - 1, temp);
        }
    }
}
