using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public sealed class MeshRendererLayerChanger : MonoBehaviour
{
    public string sortingLayerName;
    public int sortingOrder;

    private void Awake()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = sortingOrder;
    }
}
