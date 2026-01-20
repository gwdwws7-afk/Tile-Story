using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示Image的RaycastPadding区域
/// </summary>
public class RaycastPaddingEditor
{
    [DrawGizmo(GizmoType.Selected)]
    private static void DrawImageRaycastPaddingGizmos(Image image, GizmoType gizmoType)
    {
        if (image.raycastTarget)
        {
            var color = Color.red;
            Gizmos.color = color;

            var positions = new Vector3[4];
            var rect = GetRect(image.rectTransform);
            var pad = image.raycastPadding;

            //Image.raycastPadding 是Vector4的参数，其中x到w分别代表左、下、右和上的值。
            //根据矩阵信息设置旋转缩放
            positions[0] = (Vector3)(image.transform.localToWorldMatrix * (new Vector3(rect.x + pad.x, rect.y + pad.y) - image.transform.position)) + image.transform.position;
            positions[1] = (Vector3)(image.transform.localToWorldMatrix * (new Vector3(rect.x + rect.width - pad.z, rect.y + pad.y) - image.transform.position)) + image.transform.position;
            positions[2] = (Vector3)(image.transform.localToWorldMatrix * (new Vector3(rect.x + rect.width - pad.z, rect.y + rect.height - pad.w) - image.transform.position)) + image.transform.position;
            positions[3] = (Vector3)(image.transform.localToWorldMatrix * (new Vector3(rect.x + pad.x, rect.y + rect.height - pad.w) - image.transform.position)) + image.transform.position;

            Gizmos.DrawLine(positions[0], positions[1]);
            Gizmos.DrawLine(positions[1], positions[2]);
            Gizmos.DrawLine(positions[2], positions[3]);
            Gizmos.DrawLine(positions[3], positions[0]);
        }
    }

    private static Rect GetRect(RectTransform rectTransform)
    {
        Vector2 size = rectTransform.rect.size;

        return new Rect
        {
            center = (Vector2)rectTransform.position - new Vector2(
                rectTransform.pivot.x * size.x,
                rectTransform.pivot.y * size.y),
            size = size
        };
    }
}
