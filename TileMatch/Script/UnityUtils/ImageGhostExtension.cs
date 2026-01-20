using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageGhostExtension : BaseMeshEffect
{
    public enum Type
    {
        Double,
        Quad,
    }

    [SerializeField]
    private Type m_type = Type.Double;

    [SerializeField]
    private bool m_UseGraphicAlpha = true;

    private Vector2 xy;

    private RectTransform m_RectTransform;

    public Type GhostType
    {
        get { return m_type; }
        set
        {
            if (m_type == value)
                return;
            m_type = value;

            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        xy = this.GetComponent<RectTransform>().sizeDelta;
        base.OnValidate();
        if (graphic != null)
            graphic.SetVerticesDirty();
    }
#endif
    protected override void OnEnable()
    {
        base.OnEnable();
        if (graphic != null)
            graphic.SetVerticesDirty();
        m_RectTransform = GetComponent<RectTransform>();

        xy = m_RectTransform.sizeDelta;
    }

    public bool useGraphicAlpha
    {
        get { return m_UseGraphicAlpha; }
        set
        {
            m_UseGraphicAlpha = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }
    protected void ApplyGhostDouble(List<UIVertex> verts, int start, int end, float x1, float y1, float x2, float y2, bool self = false)
    {
        UIVertex vt;

        var neededCpacity = verts.Count * 2;
        if (verts.Capacity < neededCpacity)
            verts.Capacity = neededCpacity;


        for (int i = start; i < end; i++)
        {
            vt = verts[i];
            if (!self)
                verts.Add(vt);//添加一遍mesh

            Vector3 v = vt.position;
            int offset = i % 6;
            switch (offset)
            {
                case 0:
                case 1:
                case 5: v.x += x1; break;
                case 3:
                case 4:
                case 2: v.x += x2; break;
            }
            switch (offset)
            {
                case 1:
                case 2:
                case 3: v.y += y1; break;
                case 0:
                case 4:
                case 5: v.y += y2; break;
            }

            vt.position = v;
            verts[i] = vt;
        }
    }

    protected void ApplyGhost(List<UIVertex> verts, int start, int end)
    {
        if (m_type == Type.Double)
        {
            var neededCpacity = verts.Count * 2;
            if (verts.Capacity < neededCpacity)
                verts.Capacity = neededCpacity;
            ApplyGhostDouble(verts, start, end, 0, 0, -xy.x / 2, 0);
            start = end;
            end = verts.Count;
            ApplyGhostDouble(verts, start, end, xy.x, 0, -xy.x / 2, 0, true);
        }
        else
        {
            var neededCpacity = verts.Count * 4;
            if (verts.Capacity < neededCpacity)
                verts.Capacity = neededCpacity;
            start = 0;
            end = verts.Count;
            ApplyGhostDouble(verts, start, end, 0, 0, -xy.x / 2, xy.y / 2);
            start = end;
            end = verts.Count;
            ApplyGhostDouble(verts, start, end, xy.x, 0, -xy.x / 2, xy.y / 2);
            start = end;
            end = verts.Count;
            ApplyGhostDouble(verts, start, end, 0, -xy.y, -xy.x / 2, xy.y / 2);
            start = end;
            end = verts.Count;
            ApplyGhostDouble(verts, start, end, xy.x, -xy.y, -xy.x / 2, xy.y / 2, true);
        }

        StopAllCoroutines();
        StartCoroutine(ChangeImageSize());
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;
        xy = m_RectTransform.sizeDelta;

        List<UIVertex> output = new List<UIVertex>();
        vh.GetUIVertexStream(output);
        ApplyGhost(output, 0, output.Count);
        vh.Clear();
        vh.AddUIVertexTriangleStream(output);
    }

    private Vector2 GetImageXY()
    {
        var image = this.GetComponent<Image>();
        if (image != null)
        {
            return new Vector2(image.mainTexture.width*2,image.mainTexture.height*(m_type == Type.Double?1:2));
        }
        else
        {
            var rawImage = this.GetComponent<RawImage>();
            if (rawImage != null)
            {
                return new Vector2(rawImage.mainTexture.width*2, rawImage.mainTexture.height * (m_type == Type.Double ? 1 : 2));
            }
        }
        return Vector2.zero;
    }

    IEnumerator<GameObject> ChangeImageSize()
    {
        yield return null;
        m_RectTransform.sizeDelta = GetImageXY();
    }
}