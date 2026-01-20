using Spine.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GlacierQuestHeadIcon : MonoBehaviour
{
    public Image headIcon;
    public SkeletonGraphic skele, iceSkele;
    public BoneFollowerGraphic sinkBone;
    public GameObject effect;
    public Mask mask;

    private AsyncOperationHandle handle;
    public void Sink(float endValue, float duration)
    {
        if (effect)
            effect.SetActive(true);
        skele.gameObject.SetActive(false);
        sinkBone.enabled = true;
        iceSkele.gameObject.SetActive(true);
        // 头像下沉动画
        // headIcon.transform.parent.DOLocalMoveY(endValue, duration).onComplete += () => gameObject.SetActive(false);
    }

    public void Init(int headIconIndex, bool isChoose)
    {
        string headName = $"HeadPortrait_{headIconIndex}{(isChoose ? $"_{headIconIndex}" : "")}";
        UnityUtility.LoadAssetAsync<Sprite>(headName, s =>
        {
            headIcon.sprite = s;
        });
    }

    public void Release()
    {
        UnityUtility.UnloadAssetAsync(handle);
        handle = default;
    }

    public void ReInit()
    {
        Transform trans = transform.GetChild(0).GetChild(0);
        trans.localPosition = Vector3.zero;
        trans.localRotation = new Quaternion(0, 0, 0, 0);
        if (effect)
            effect.SetActive(false);
        sinkBone.enabled = false;
    }
}
