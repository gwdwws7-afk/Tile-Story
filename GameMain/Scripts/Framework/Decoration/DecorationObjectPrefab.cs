using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//形如 Area1_0_1.Prefab上挂的脚本，用于
//1 控制其上图片在待装修时的outline效果
//2 标记Spine类型装修Object的遮挡特效的位置
public class DecorationObjectPrefab : MonoBehaviour
{
    [SerializeField]
    private bool setToNormal;
    [SerializeField]
    private bool setToWaitForDecorate;
    
    public Transform effectRootForSpineDisappear;

    private List<Material> targetMaterialList = new List<Material>();

    private int shaderStep = 3;

    void Start()
    {
        RawImage[] allRawImages = GetComponentsInChildren<RawImage>();
        Image[] allImages = GetComponentsInChildren<Image>();

        targetMaterialList.Clear();
        for(int i = 0; i < allRawImages.Length; ++i)
        {
            Material newInstance = new Material(allRawImages[i].material);
            allRawImages[i].material = newInstance;
            targetMaterialList.Add(newInstance);
        }
        for (int i = 0; i < allImages.Length; ++i)
        {
            Material newInstance = new Material(allImages[i].material);
            allImages[i].material = newInstance;
            targetMaterialList.Add(newInstance);
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (setToNormal)
        {
            setToNormal = false;
            SetToNormalMaterial();
        }

        if (setToWaitForDecorate)
        {
            setToWaitForDecorate = false;
            SetToRedShineMaterial();
        }
    }
#endif

    public virtual void SetToNormalMaterial()
    {
        for (int i = 0; i < targetMaterialList.Count; ++i)
        {
            targetMaterialList[i].DOKill();

            //用 Spine/Outline/Sprite/Unlit 时调的参数
            //targetMaterialList[i].SetColor("_OutlineColor", Color.clear);//相当于关掉Outline

            //用Custom_Shader/Outlight 时调的参数
            targetMaterialList[i].SetColor("_LightColor", new Color(0, 0, 0, 0));
            targetMaterialList[i].SetFloat("_Size", 1);
        }
    }

    public virtual void SetToRedShineMaterial()
    {
        for (int i = 0; i < targetMaterialList.Count; ++i)
        {
            targetMaterialList[i].DOKill();

            //用 Spine/Outline/Sprite/Unlit 时调的参数
            //targetMaterialList[i].SetColor("_OutlineColor", new Color(181 / 255.0f, 52 / 255.0f, 52 / 255.0f));
            //targetMaterialList[i].SetFloat("_OutlineWidth", 0);
            //targetMaterialList[i].DOFloat(8, "_OutlineWidth", 1.0f).SetLoops(-1, LoopType.Yoyo);

            //用Custom_Shader/Outlight 时调的参数
            targetMaterialList[i].SetColor("_LightColor", new Color(181 / 255.0f, 52 / 255.0f, 52 / 255.0f, 1.0f));
            targetMaterialList[i].SetFloat("_Size", 10);
            //targetMaterialList[i].DOFloat(10, "_Size", 1.0f).SetLoops(-1, LoopType.Yoyo);//另一种可选的 修改Size的动画
            targetMaterialList[i].DOColor(new Color(181 / 255.0f, 52 / 255.0f, 52 / 255.0f, 0.6f), "_LightColor", 1.0f).SetLoops(-1, LoopType.Yoyo);

            targetMaterialList[i].SetFloat("_Step", shaderStep);
        }
    }

    public void OnRelease()
    {
        for (int i = 0; i < targetMaterialList.Count; ++i)
        {
            targetMaterialList[i].DOKill();
            Destroy(targetMaterialList[i]);
        }
    }

    public virtual void AnimateOldDecorationObjectPrefab(int index)
    {
        Transform targetTransform = transform.GetChild(0);
        targetTransform.DOScale(0.1f, 0.1f).onComplete = () =>
        {
            targetTransform.transform.DOKill();
            DestroyImmediate(gameObject);
        };
    }

    public virtual float AnimateNewDecorationObjectPrefab(int index)
    {
        Transform targetTransform = transform.GetChild(0);
        targetTransform.gameObject.SetActive(false);
        GameManager.Task.AddDelayTriggerTask(0.3f, () =>
        {
            targetTransform.gameObject.SetActive(true);
        });
        return 0.03f;
    }
}
