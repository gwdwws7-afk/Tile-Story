using Coffee.UIExtensions;
using DG.Tweening;
using GameFramework.Event;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class DecorationObject : MonoBehaviour
{
    //三种不同的升级效果
    public enum DecorationObjectUpgradeEffectType
    {
        FurnitureStyle_ScaleDownScaleUp,//旧scale小 新scale大 用于常规意义的家具
        CharacterSpineStyle_DisableEnable,//旧直接消失 新直接出现 用于Spine角色
        BackgroundStyle_AlphaUp,//旧垫在背后 新alpha出现 (这里旧的可以再消失) 用于背景
        Special_19_4,//19章 第4个 海豚+海
        CustomBehaviour,//DecorationObjectPrefab 
    }

    [SerializeField]
    private DecorationObjectUpgradeEffectType upgradeEffect = DecorationObjectUpgradeEffectType.FurnitureStyle_ScaleDownScaleUp;

    [Tooltip("如果配置了 那么对于FurnitureStyle_ScaleDownScaleUp类型 生成特效将在这个位置出现。 如果没有 就大致生成在新家具的中间")]
    [SerializeField]
    private Transform upgradeEffectOverrideTransform;
    [Tooltip("对于FurnitureStyle_ScaleDownScaleUp类型 生成特效将有这样一个缩放。默认是1。")]
    [SerializeField]
    private float upgradeEffectOverrideScale = 1;

    //初始化赋值
    [HideInInspector]
    public int areaID;
    [HideInInspector]
    public int positionID;

    //解锁按钮的位置
    public Transform unlockBtnPos;

    //关联的 spine动画的挂载节点
    public Transform[] animParents;

    private GameObject[] decorationPrefabGOs;//SkeletonGraphic[] useSkeletons;
    private List<AsyncOperationHandle> asyncOperationHandles = new List<AsyncOperationHandle>();
    private List<int> shownType = new List<int>();

    [NonSerialized]
    public System.Action afterAnimPlay;
    public bool IsInitComplete { get; private set; }

    public void OnInit(int nowUseAnimFlag, Action onFinished = null)
    {
        if (decorationPrefabGOs == null)
        {
            decorationPrefabGOs = new GameObject[animParents.Length];
        }

        ChangeUseAnim(nowUseAnimFlag, true, () =>
         {
             IsInitComplete = true;
             if (onFinished != null)
                 onFinished();
         }, true);
    }

    public void OnRelease()
    {
        for (int i = 0; i < asyncOperationHandles.Count; i++)
        {
            Addressables.ReleaseInstance(asyncOperationHandles[i]);
        }
        asyncOperationHandles.Clear();
        shownType.Clear();
        IsInitComplete = false;

        try
        {
            if (decorationPrefabGOs != null)
                for (int i = 0; i < decorationPrefabGOs.Length; ++i)
                {
                    if (decorationPrefabGOs[i] != null)
                    {
                        DecorationObjectPrefab script = decorationPrefabGOs[i].GetComponent<DecorationObjectPrefab>();
                        if (script != null)
                            script.OnRelease();
                    }
                }
        }
        catch (System.Exception e)
        {
            Debug.LogError("DecorationObject OnRelease error:" + e.Message);
        }
    }


    void ChangeUseAnim(int animationType, bool isEnd, System.Action callBack, bool isShowingAll = false, float timescale = 1f, bool isViewMode = false)
    {
        if(!shownType.Contains(animationType))
        {
            shownType.Add(animationType);
        }

        //0 显示的是 修理前
        //if (animationType == 0)
        //{
        //    foreach (var ske in useSkeletons)
        //    {
        //        if (ske != null)
        //            DestroyImmediate(ske.gameObject);
        //    }
        //    callBack?.Invoke();
        //    return;
        //}
        int count = decorationPrefabGOs.Length;

        for (int i = 0; i < decorationPrefabGOs.Length; i++)
        {
            int flag = i;
            string useAnimName = $"Area{areaID}_{animationType}_{positionID}" + (decorationPrefabGOs.Length > 1 ? $"_{flag + 1}" : "");

            if (flag < animParents.Length)
            {
                useAnimName = $"Area{areaID}_{animationType}_{positionID}" + (decorationPrefabGOs.Length > 1 ? $"_{flag + 1}" : "");//如果有多分则使用附属
            }

            SkeletonGraphic graphic = null;
            if (decorationPrefabGOs[flag] != null)
                graphic = decorationPrefabGOs[flag].GetComponent<SkeletonGraphic>();
            if (graphic != null && decorationPrefabGOs[flag].gameObject.name.Equals(useAnimName))
            {
                if (isEnd)
                {
                    graphic.SetToEnd(/*isViewMode*/);
                    //装修完毕的DecorationItem在初始化时会用这个isEnd模式来初始化
                    //但对于部分有idle动画的DecorationItem实际上需要再播放它们的Idle
                    //具体来说 比如Area的城堡 比如Area8的泳池和泳池上的玩具
                    if (graphic.IsSpineAnimNameExist("idle"))
                    {
                        graphic.AnimationState.SetAnimation(0, "idle", true);
                    }
                }
                else
                {
                    graphic.SetToFirst();
                    graphic.AnimationState.TimeScale = timescale;
                }
                count--;
                if (count == 0)
                {
                    callBack?.Invoke();
                }
            }
            else
            {
                Transform parent = animParents[flag].transform;
                AsyncOperationHandle<GameObject> asyncHandle = Addressables.InstantiateAsync(useAnimName, parent);
                asyncHandle.Completed += res =>
                {
                    if (res.Status == AsyncOperationStatus.Succeeded)
                    {
                        var obj = res.Result;
                        if (decorationPrefabGOs[flag] != null)
                        {
                            Transform targetTransform = decorationPrefabGOs[flag].transform.GetChild(0);
                            AnimateOldDecorationPrefab(targetTransform, flag);
                        }
#if UNITY_EDITOR
                        obj.gameObject.name = useAnimName;
#endif
                    
                        decorationPrefabGOs[flag] = obj;
                        SkeletonGraphic graphic = obj.GetComponent<SkeletonGraphic>();

                        if (graphic != null)
                        {
                            if (isEnd)
                            {
                                graphic.SetToEnd();
                                //useSkeletons[flag].SetToEnd(isViewMode);
                                //装修完毕的DecorationItem在初始化时会用这个isEnd模式来初始化
                                //但对于部分有idle动画的DecorationItem实际上需要再播放它们的Idle
                                //具体来说 比如Area的城堡 比如Area8的泳池和泳池上的玩具
                                if (graphic.IsSpineAnimNameExist("idle"))
                                {
                                    graphic.AnimationState.SetAnimation(0, "idle", true);
                                }
                            }
                            else
                            {
                                graphic.SetToFirst();
                            }
                        }
#if UNITY_EDITOR
                        obj.name = useAnimName;
#endif
                        count--;
                        if (count == 0)
                        {
                            callBack?.Invoke();
                        }   
                    }
                    else
                    {
                        count--;
                        if (count == 0)
                        {
                            callBack?.Invoke();
                        }   
                    }
                };
                if (!asyncOperationHandles.Contains(asyncHandle))
                    asyncOperationHandles.Add(asyncHandle);
            }
        }
    }

    public void ShowUpgradeAnim(int animationType, bool useShine = true, bool isShowingAll = false, float timeScale = 1f)
    {


        int waitComplete = decorationPrefabGOs.Length;
        ChangeUseAnim(animationType, false, () =>
        {
            GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());
            //这段是确保所有动画组件都加载完毕时的回调
            for (int i = 0; i < decorationPrefabGOs.Length; i++)
            {
                int flag = i;

                float delayTime = 0.03f;
                Transform targetTransform = decorationPrefabGOs[flag].transform.GetChild(0);
                if (targetTransform != null)
                    delayTime = AnimateNewDecorationPrefab(targetTransform, i);
                else
                    Log.Error("ShowUpgradeAnim targetTransform is null");

                if (flag == 0)
                {
                    GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                    {
                        afterAnimPlay?.Invoke();
                        afterAnimPlay = null;
                    });
                }
            }
        }, isShowingAll, timeScale);
    }


    private void AnimateOldDecorationPrefab(Transform targetTransform, int index)
    {
        if (upgradeEffect == DecorationObjectUpgradeEffectType.FurnitureStyle_ScaleDownScaleUp)
        {
            //旧装修(家具)缩小后消失
            targetTransform.DOScale(0.1f, 0.1f).onComplete = () =>
            {
                targetTransform.transform.DOKill();
                DestroyImmediate(targetTransform.parent.gameObject);
            };
        }
        else if (upgradeEffect == DecorationObjectUpgradeEffectType.CharacterSpineStyle_DisableEnable)
        {
            DecorationObjectPrefab script = targetTransform.parent.GetComponent<DecorationObjectPrefab>();
            Transform effectRoot = targetTransform;
            if (script != null)
                effectRoot = script.effectRootForSpineDisappear;
            if (effectRoot != null)
            {
                //播放遮挡特效
                GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
                    "DecorationObjectUpgradeEffect",
                    "TileItemDestroyEffectPool",
                    1.0f,
                    effectRoot.position,
                    Quaternion.identity,
                    targetTransform.parent.parent, (t) =>
                    {
                        var effect = t?.Target as GameObject;
                        if (effect != null)
                        {
                            var skeleton = effect.transform.GetComponentInChildren<SkeletonGraphic>();
                            if (skeleton != null)
                            {
                                skeleton.AnimationState.ClearTracks();
                                skeleton.Skeleton.SetToSetupPose();
                                skeleton.AnimationState.SetAnimation(0, "active", false);
                            }
                        }
                    });
            }
            //旧装修(人物)延时消失
            GameManager.Task.AddDelayTriggerTask(0.3f, () =>
            {
                DestroyImmediate(targetTransform.parent.gameObject);
            });
        }
        else if (upgradeEffect == DecorationObjectUpgradeEffectType.BackgroundStyle_AlphaUp)
        {
            if (index == 0)
            {
                GameManager.Task.AddDelayTriggerTask(4.5f, () =>
                {
                    if (targetTransform != null && targetTransform.parent != null && targetTransform.parent.gameObject != null)
                    {
                        AutoPlaySound script = targetTransform.parent.GetComponent<AutoPlaySound>();
                        if (script != null)
                            script.MuteSound();
                    }
                });

                //旧装修(背景)保留 延时(等新装修完成后）销毁
                GameManager.Task.AddDelayTriggerTask(10.0f, () =>
                {
                    if (targetTransform != null && targetTransform.parent != null && targetTransform.parent.gameObject != null)
                        DestroyImmediate(targetTransform.parent.gameObject);
                });
            }
            else
            {
                //第5,16章特殊处理特效直接消失
                if (areaID == 5 || areaID == 16 || areaID == 20 || areaID == 21 || areaID == 22 || areaID == 23 || areaID == 24 || areaID == 26 || areaID == 27 || areaID == 35 || areaID == 36 || areaID == 37 || areaID == 42 || areaID == 43|| areaID == 46|| areaID == 47)        
                {
                    GameManager.Task.AddDelayTriggerTask(3.4f, () => { targetTransform.parent.gameObject.SetActive(false); });
                }
                else
                {
                    RawImage[] allChildRawImages = targetTransform.parent.GetComponentsInChildren<RawImage>();
                    for (int i = 0; i < allChildRawImages.Length; ++i)
                    {
                        allChildRawImages[i].color = Color.white;
                        allChildRawImages[i].DOFade(0, 1.5f);
                    }
                }

                GameManager.Task.AddDelayTriggerTask(5.0f, () =>
                {
                    if (targetTransform != null && targetTransform.parent != null && targetTransform.parent.gameObject != null)
                        DestroyImmediate(targetTransform.parent.gameObject);
                });
            }
        }
        else if(upgradeEffect == DecorationObjectUpgradeEffectType.Special_19_4)
        {
            if (index == 0)
            {
                GameManager.Task.AddDelayTriggerTask(0.5f, () =>
                {
                    DecorationObjectPrefab script = targetTransform.parent.GetComponent<DecorationObjectPrefab>();
                    Transform effectRoot = targetTransform;
                    if (script != null)
                        effectRoot = script.effectRootForSpineDisappear;
                    if (effectRoot != null)
                    {
                        //播放遮挡特效
                        GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
                            "DecorationObjectUpgradeEffect",
                            "TileItemDestroyEffectPool",
                            1.0f,
                            effectRoot.position,
                            Quaternion.identity,
                            targetTransform.parent.parent, (t) =>
                            {
                                var effect = t?.Target as GameObject;
                                if (effect != null)
                                {
                                    var skeleton = effect.transform.GetComponentInChildren<SkeletonGraphic>();
                                    if (skeleton != null)
                                    {
                                        skeleton.AnimationState.ClearTracks();
                                        skeleton.Skeleton.SetToSetupPose();
                                        skeleton.AnimationState.SetAnimation(0, "active", false);
                                    }
                                }
                            });
                    }
                    //旧装修(人物)延时消失
                    GameManager.Task.AddDelayTriggerTask(0.3f, () =>
                    {
                        DestroyImmediate(targetTransform.parent.gameObject);
                    });
                });
            }
            else
            {
                RawImage[] allChildRawImages = targetTransform.parent.GetComponentsInChildren<RawImage>();
                GameManager.Task.AddDelayTriggerTask(1.5f, () =>
                {
                    for (int i = 0; i < allChildRawImages.Length; ++i)
                    {
                        allChildRawImages[i].color = Color.white;
                        allChildRawImages[i].DOFade(0, 1.5f);
                    }
                });

                GameManager.Task.AddDelayTriggerTask(5.0f, () =>
                {
                    if (targetTransform != null && targetTransform.parent != null && targetTransform.parent.gameObject != null)
                        DestroyImmediate(targetTransform.parent.gameObject);
                });
            }
        }
        else if(upgradeEffect == DecorationObjectUpgradeEffectType.CustomBehaviour)
        {
            DecorationObjectPrefab script = targetTransform.parent.GetComponent<DecorationObjectPrefab>();
            script.AnimateOldDecorationObjectPrefab(index);
        }
    }


    private float AnimateNewDecorationPrefab(Transform targetTransform, int index)
    {
        if (upgradeEffect == DecorationObjectUpgradeEffectType.FurnitureStyle_ScaleDownScaleUp)
        {
            //新装修放大
            targetTransform.localScale = Vector3.one / 5.0f;
            targetTransform.DOScale(Vector3.one * 1.15f, 0.3f).SetEase(Ease.InOutSine).onComplete = () =>
            {
                targetTransform.DOScale(Vector3.one * 0.9f, 0.2f).SetEase(Ease.InOutSine).onComplete = () =>
                {
                    targetTransform.DOScale(Vector3.one * 1.0f, 0.2f).SetEase(Ease.InOutSine);
                };
            };

            AutoPlaySound script = targetTransform.parent.GetComponentInChildren<AutoPlaySound>();
            if (script != null)
                script.ResumeSound();

            if (index == 0)
            {
                GameManager.Task.AddDelayTriggerTask(0.25f, () =>
                {
                    Vector3 effectPosition = targetTransform.position;
                    if (targetTransform.childCount > 0)
                    {
                        Transform childTransform = targetTransform.GetChild(0);
                        effectPosition = childTransform.position;
                    }
                    if (upgradeEffectOverrideTransform != null)
                    {
                        effectPosition = upgradeEffectOverrideTransform.position;
                    }
                    GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
                        "DecorationObjectFinishEffect",
                        "TileItemDestroyEffectPool",
                        1.0f,
                        effectPosition,
                        Quaternion.identity,
                        targetTransform.parent.parent.parent,
                        (t) =>
                        {
                            (t.Target as GameObject).GetComponent<UIParticle>().scale = 1000 * upgradeEffectOverrideScale;
                        });

                });
            }
        }
        else if (upgradeEffect == DecorationObjectUpgradeEffectType.CharacterSpineStyle_DisableEnable)
        {
            //新装修(人物)延时出现
            targetTransform.gameObject.SetActive(false);
            SkeletonGraphic spine = targetTransform.GetComponent<SkeletonGraphic>();
            if (spine == null)
                spine = targetTransform.GetComponentInChildren<SkeletonGraphic>();

            float delayTime = 0.3f;

            if (spine != null)
            {
                GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                {
                    spine.AnimationState.ClearTracks();

                    if (spine.IsSpineAnimNameExist("active"))
                    {
                        spine.AnimationState.SetAnimation(0, "active", false).Complete += e =>
                        {
                            if (spine.IsSpineAnimNameExist("idle"))
                            {
                                spine.AnimationState.AddAnimation(0, "idle", true, 0);
                            }
                        };
                    }
                    else
                    {
                        if (spine.IsSpineAnimNameExist("idle"))
                        {
                            spine.AnimationState.SetAnimation(0, "idle", true);
                        }
                    }

                    targetTransform.gameObject.SetActive(true);
                });

                var anim = spine.Skeleton.Data.FindAnimation("active");
                if (anim != null) delayTime += anim.Duration;
            }

            return delayTime;
        }
        else if (upgradeEffect == DecorationObjectUpgradeEffectType.BackgroundStyle_AlphaUp)
        {
            Image[] allChildImages = targetTransform.parent.GetComponentsInChildren<Image>();
            for (int i = 0; i < allChildImages.Length; ++i)
            {
                allChildImages[i].color = new Color(1, 1, 1, 0);
            }
            RawImage[] allChildRawImages = targetTransform.parent.GetComponentsInChildren<RawImage>();
            for (int i = 0; i < allChildRawImages.Length; ++i)
            {
                allChildRawImages[i].color = new Color(1, 1, 1, 0);
            }

            if (index == 0)
            {
                DecorationOperationPanel operationPanel = (DecorationOperationPanel)GameManager.UI.GetUIForm("DecorationOperationPanel");
                if (operationPanel != null)
                {
                    operationPanel.PlayBroomAnim(() =>
                    {
                        operationPanel.PlayBgUpgradeEffect();

                        GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                        {
                            //新装修(背景) Alpha渐显
                            Image[] allChildImages = targetTransform.parent.GetComponentsInChildren<Image>();
                            for (int i = 0; i < allChildImages.Length; ++i)
                            {
                                allChildImages[i].color = new Color(1, 1, 1, 0);
                                allChildImages[i].DOFade(1, 1.5f);
                            }
                            RawImage[] allChildRawImages = targetTransform.parent.GetComponentsInChildren<RawImage>();
                            for (int i = 0; i < allChildRawImages.Length; ++i)
                            {
                                allChildRawImages[i].color = new Color(1, 1, 1, 0);
                                allChildRawImages[i].DOFade(1, 1.5f);
                            }
                        });
                    });
                }
            }
            else
            {
                //硬等了一个 PlayBroomAnim 的时间
                //这里比较正确的应该是额外加到 PlayBroomAnim 的回调里
                GameManager.Task.AddDelayTriggerTask(3.4f, () =>
                {
                    for (int i = 0; i < allChildImages.Length; ++i)
                    {
                        allChildImages[i].color = new Color(1, 1, 1, 0);
                        allChildImages[i].DOFade(1, 1.5f);
                    }
                    for (int i = 0; i < allChildRawImages.Length; ++i)
                    {
                        allChildRawImages[i].color = new Color(1, 1, 1, 0);
                        allChildRawImages[i].DOFade(1, 1.5f);
                    }
                });
            }

            return 4.5f;
        }
        else if (upgradeEffect == DecorationObjectUpgradeEffectType.Special_19_4)
        {
            if (index == 0)
            {
                targetTransform.gameObject.SetActive(false);
                GameManager.Task.AddDelayTriggerTask(0.5f + 0.3f, () =>
                {
                    targetTransform.gameObject.SetActive(true);
                });
            }
            else
            {
                RawImage[] allChildRawImages = targetTransform.parent.GetComponentsInChildren<RawImage>();
                for (int i = 0; i < allChildRawImages.Length; ++i)
                {
                    allChildRawImages[i].color = new Color(1, 1, 1, 0);
                    allChildRawImages[i].DOFade(1, 1.5f);
                }
            }
        }
        else if(upgradeEffect == DecorationObjectUpgradeEffectType.CustomBehaviour)
        {
            DecorationObjectPrefab script = targetTransform.parent.GetComponent<DecorationObjectPrefab>();
            return script.AnimateNewDecorationObjectPrefab(index);
        }

        return 0.03f;
    }

    public void SetDecorationObjectPrefabToNormalMaterial()
    {
        if (decorationPrefabGOs == null)
            return;

        for(int i = 0; i < decorationPrefabGOs.Length; ++i)
        {
            if (decorationPrefabGOs[i] == null)
                continue;
            DecorationObjectPrefab script = decorationPrefabGOs[i].GetComponent<DecorationObjectPrefab>();
            if (script != null)
                script.SetToNormalMaterial();
        }
    }

    public void SetDecorationObjectPrefabToRedShineMaterial()
    {
        for (int i = 0; i < decorationPrefabGOs.Length; ++i)
        {
            DecorationObjectPrefab script = decorationPrefabGOs[i].GetComponent<DecorationObjectPrefab>();
            if (script != null)
            {
                bool isLowerDevice = SystemInfoManager.IsLowPerformanceMachine();

                if (isLowerDevice) script.SetToNormalMaterial();
                else script.SetToRedShineMaterial();
            }
        }
    }


    private List<SkeletonGraphic> skeletonGraphicCacheList = new List<SkeletonGraphic>();

    /// <summary>
    /// （如果有就）播放角色Spine的快乐动画
    /// </summary>
    /// <returns></returns>
    public void PlaySpineCharacterActiveAnim()
    {
        //不完全对
        if (upgradeEffect == DecorationObjectUpgradeEffectType.CharacterSpineStyle_DisableEnable)
        {
            for (int i = 0; i < shownType.Count; i++)
            {
                // 目前角色装修后只有active动画，不存在idle动画
                if (shownType[i] > 0)
                {
                    return;
                }
            }
            
            if (skeletonGraphicCacheList.Count == 0)
            {
                for (int i = 0; i < decorationPrefabGOs.Length; ++i)
                {
                    SkeletonGraphic[] scriptsArray = decorationPrefabGOs[i].GetComponentsInChildren<SkeletonGraphic>();
                    skeletonGraphicCacheList.AddRange(scriptsArray);
                }
            }
        }

        for (int i = 0; i < skeletonGraphicCacheList.Count; ++i)
        {
            int index = i;
            if (skeletonGraphicCacheList[i] != null)
            {
                if (skeletonGraphicCacheList[i].IsSpineAnimNameExist("active"))
                {
                    if (skeletonGraphicCacheList[i].IsSpineAnimNameExist("idle"))
                    {
                        skeletonGraphicCacheList[i].AnimationState.SetAnimation(0, "active", false).Complete += (t) =>
                        {
                            skeletonGraphicCacheList[index].AnimationState.SetAnimation(0, "idle", true);
                        };
                    }
                }
            }
        }
    }
}
