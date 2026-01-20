using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 道具生成特效
    /// </summary>
    public class PropGenerateEffect : MonoBehaviour
    {
        public Renderer[] m_Renderers;

        public void Show()
        {
            transform.localScale = new Vector3(1300, 1300, 1300);
            gameObject.SetActive(true);
        }

        public void SetLayer(string layerName, int sortOrder)
        {
            for (int i = 0; i < m_Renderers.Length; i++)
            {
                m_Renderers[i].sortingLayerName = layerName;
                m_Renderers[i].sortingOrder = sortOrder;
            }
        }
    }
}