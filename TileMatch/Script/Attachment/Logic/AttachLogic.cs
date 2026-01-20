using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 附属物逻辑
/// </summary>
public abstract class AttachLogic
{
    private AttachItem attachItem;
    private TileItem tileItem;
    private AsyncOperationHandle<GameObject> instanceHandle;
    public int attachState = 1;
    private Action<AttachItem> spawnCompleteAction;
    private bool isInit;

    public abstract int AttachId { get; }

    public abstract string AttachAssetName { get; }

    public AttachItem AttachItem => attachItem;

    public TileItem TileItem => tileItem;

    public event Action<AttachItem> SpawnComplete
    {
        add
        {
            spawnCompleteAction += value;
        }
        remove
        {
            spawnCompleteAction -= value;
        }
    }

    public void Init(TileItem tileItem, object userData = null)
    {
        if (isInit)
            return;
        isInit = true;

        this.tileItem = tileItem;

        OnInit(userData);
        SpawnInstance();
    }

    public void Release(bool showEffect, bool clearId = true, bool isDestroy = false)
    {
        if (!isInit)
            return;
        isInit = false;

        OnRelease(showEffect);
        ReleaseInstance();

        if(clearId)
            tileItem.Data.AttachID = 0;
        tileItem.AttachLogic = null;
        if(!isDestroy)
            tileItem.SetColor();
        tileItem = null;
        attachState = 1;
        spawnCompleteAction = null;
    }

    public virtual void SpecialCollect(bool showEffect, bool clearId = true)
    {
        Release(showEffect, clearId);
    }
    
    private void SpawnInstance()
    {
        instanceHandle = GameManager.ObjectPool.Spawn<EffectObject>(AttachAssetName, "AttachmentItemPool", Vector3.one, Quaternion.identity, tileItem.transform, (obj) =>
        {
            GameObject target = (GameObject)obj.Target;
            attachItem = target.GetComponent<AttachItem>();
            target.gameObject.SetActive(true);
            target.transform.localScale = Vector3.one;
            target.transform.localPosition = Vector3.zero;
            attachItem.Init(this);
            spawnCompleteAction?.Invoke(attachItem);
            spawnCompleteAction = null;
        });
    }

    protected virtual void ReleaseInstance()
    {
        if (attachItem != null)
        {
            attachItem.Release();
            attachItem.gameObject.SetActive(false);

#if UNITY_EDITOR
            if (GameManager.ObjectPool.transform != null && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !UnityEditor.EditorApplication.isCompiling)
#else
            if (GameManager.ObjectPool.transform != null) 
#endif
            {
                attachItem.transform.SetParent(GameManager.ObjectPool.transform);
                GameManager.ObjectPool.Unspawn<EffectObject>("AttachmentItemPool", attachItem.gameObject);
                instanceHandle = default;
            }

            attachItem = null;
        }
    }

    public virtual void Show()
    {
        if (AttachItem != null)
        {
            AttachItem.Show();
        }
        else
        {
            SpawnComplete += res =>
            {
                res.Show();
            };
        }
    }

    public virtual void Hide()
    {
        if (AttachItem != null)
        {
            AttachItem.Hide();
        }
        else
        {
            SpawnComplete += res =>
            {
                res.Hide();
            };
        }
    }

    public virtual void SetColor(bool isBeCover)
    {
        if (AttachItem != null)
        {
            AttachItem.SetColor(isBeCover);
        }
        else
        {
            SpawnComplete += res =>
            {
                res.SetColor(isBeCover);
            };
        }
    }

    public virtual void StopAllAnim()
    {
    }

    public virtual void StartAllAnim()
    {
    }

    protected virtual void OnInit(object userData)
    {
    }

    protected virtual void OnRelease(bool showEffect)
    {
    }

    public virtual void OnClick()
    {
        if (attachItem != null)
            attachItem.OnClick();
    }

    /// <summary>
    /// 当周围有tile拿取时
    /// </summary>
    /// <param name="type">拿取的技能类型</param>
    public virtual void OnAroundTileGet(TotalItemData type)
    {
    }

    /// <summary>
    /// 当周围没有无附属物的tile时
    /// </summary>
    public virtual void OnAroundTileEmpty()
    {
    }

    /// <summary>
    /// 当全局任意tile拿取时
    /// </summary>
    public virtual void OnAnyTileGet()
    {
    }

    /// <summary>
    /// 设置金块碎裂状态
    /// </summary>
    public virtual void SetSprite(int state)
    {
        attachItem.SetSprite(state);
    }
}
