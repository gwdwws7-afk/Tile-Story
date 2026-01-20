using TMPro;
using UnityEngine;

public sealed class CircleTextMesh : MonoBehaviour
{
    public float radius = 300;

    private bool registeredEvent = false;

    private void Start()
    {
        RegisterEvent();
    }

    public void RegisterEvent()
    {
        if (!registeredEvent)
        {
            registeredEvent = true;

            TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
            tmp.OnPreRenderText += RingText;
        }
    }

    private void RingText(TMP_TextInfo info)
    {
        Vector3 textCenter = Vector3.zero;
        int textVerticesNum = 0;
        foreach (TMP_MeshInfo meshInfo in info.meshInfo)
        {
            if (meshInfo.vertices == null)
                continue;
            for (int i = 0; i < meshInfo.vertices.Length; i++)
            {
                textCenter += meshInfo.vertices[i];
                textVerticesNum++;
            }
        }

        textCenter /= textVerticesNum;
        float perimeter = radius * 2 * Mathf.PI;
        Vector3 center = textCenter - new Vector3(0, radius, 0);
        foreach (TMP_MeshInfo meshInfo in info.meshInfo)
        {
            if (meshInfo.vertices == null)
                continue;
            for (int i = 0; i < meshInfo.vertices.Length; i++)
            {
                float angle = (meshInfo.vertices[i].x / perimeter * 360) % 360;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.back) * new Vector3(0, 1, 0);
                float dis = meshInfo.vertices[i].y - center.y;
                meshInfo.vertices[i] = center + dir * dis;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
        if (tmp != null && !string.IsNullOrEmpty(tmp.text) && tmp.mesh != null) 
        {
            tmp.ForceMeshUpdate();
            TMP_TextInfo info = tmp.textInfo;
            if (info != null)
            {
                RingText(info);
                tmp.UpdateVertexData();
            }
        }
    }
#endif
}
