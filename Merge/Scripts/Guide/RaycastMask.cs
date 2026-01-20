using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class RaycastMask : Image
    {
        private Vector2 m_Center = Vector2.zero;
        private float m_SliderX = 0f;
        private float m_SliderY = 0f;

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (screenPoint.x >= m_Center.x - m_SliderX
                && screenPoint.x <= m_Center.x + m_SliderX
                && screenPoint.y >= m_Center.y - m_SliderY
                && screenPoint.y <= m_Center.y + m_SliderY)
            {
                return false;
            }

            return base.IsRaycastLocationValid(screenPoint, eventCamera);
        }

        public bool SetFocusTarget(Vector3 worldPos, float sliderX, float sliderY, float edgeWidth)
        {
            return SetFocusTarget(canvas.transform as RectTransform, worldPos, sliderX, sliderY, edgeWidth);
        }

        public bool SetFocusTarget(RectTransform rect, Vector3 worldPos, float sliderX, float sliderY, float edgeWidth)
        {
            m_Center = Camera.main.WorldToScreenPoint(worldPos);
            m_SliderX = sliderX / 1080 * Screen.width;
            m_SliderY = sliderY / 1080 * Screen.width;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, worldPos, null, out Vector2 localPos))
            {
                material.SetVector("_Center", new Vector4(localPos.x, localPos.y, 0, 0));
                material.SetFloat("_SliderX", sliderX);
                material.SetFloat("_SliderY", sliderY);
                material.SetFloat("_Edge", edgeWidth);

                return true;
            }

            return false;
        }
    }
}
