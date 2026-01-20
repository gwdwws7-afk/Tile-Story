using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class HarvestKitchenTileItem : MonoBehaviour
{
    // 棋子的类型ID
    public int itemID = 0;
    // 棋子在父节点中的编号
    public int itemIndex = 0;
    public Image tileBg;
    public Image icon;
    public DelayButton btn;
    public Action<int> clickAction1;
    private Action<HarvestKitchenTileItem> clickAction2;
    public bool isSelected;
    public bool isClicked;

    private List<int> delayList;
    
    public void Init(int index, int id, Transform parent, Action<int> action, Action<HarvestKitchenTileItem> clickTileAction)
    {
        itemID = id;
        itemIndex = index;
        btn.enabled = true;
        tileBg.gameObject.SetActive(true);
        icon.sprite = HarvestKitchenManager.Instance.GetTileSpriteById(itemID);
        icon.SetNativeSize();
        
        transform.DOKill();
        transform.SetParent(parent);
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        delayList = new List<int>();

        clickAction1 = action;
        clickAction2 = clickTileAction;
        btn.SetBtnEvent(OnBtnClick);
        gameObject.SetActive(true);
        isSelected = false;
        isClicked = false;
    }

    public void OnBtnClick()
    {
        if (!HarvestKitchenManager.Instance.canClickTile) return;

        if (isClicked)
            return;
        isClicked = true;

        transform.DOKill();
        // 点击棋子后，关闭棋子的点击事件，防止多次点击同一个棋子
        btn.enabled = false;
        // 处理棋子的相关逻辑
        // 将棋子的信息传递给 KitchenGameMenu
        clickAction2.Invoke(this);
        clickAction2 = null;
        // 点击生效后削减在TileItemPanel中的记录
        clickAction1?.Invoke(itemIndex);
        clickAction1 = null;
        // gameObject.SetActive(false);
    }

    public void RefreshClickAction1(Action<int> action)
    {
        clickAction1 = null;
        clickAction1 = action;
    }
    
    public void DestroyTile(bool isPlayAudio = true, string destroySound = null, CallBack<HarvestKitchenTileItem> finishCallBack = null)
    {
        try
        {
            if (isPlayAudio)
            {
                //播放震动
                UnityUtil.EVibatorType.Short.PlayerVibrator();
                //播放音效
                GameManager.Sound.PlayAudio(destroySound ?? "SFX_DestroyTile_new");
            }
            //播放特效
            delayList.Add(GameManager.Task.AddDelayTriggerTask(0.02f, () =>
            {
                PlayEffect(null);
            }));
        }
        catch(Exception e)
        {
            Log.Error("Kitchen DestroyTile fail - " + e.Message);
        }

        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutSine);
        delayList.Add(GameManager.Task.AddDelayTriggerTask(0.2f, () =>
        {
            transform.DOKill();
            finishCallBack?.Invoke(this);
            gameObject.SetActive(false);
            // Destroy(gameObject);
        }));
    }

    public void FlyToTarget(Transform target, Vector3 targetPos, Action callback = null)
    {
        transform.SetParent(target);
        btn.enabled = false;
        tileBg.gameObject.SetActive(false);
        transform.DOMove(targetPos, 0.5f).onComplete += () =>
        {
            transform.DOKill();
            callback?.Invoke();
        };
    }
    
    private void PlayEffect(CallBack finishCallBack)
    {
        GameManager.ObjectPool.SpawnWithRecycle<EffectObject>(
            "TileDestroyEffect",
            "TileItemDestroyEffectPool",
            1f,
            transform.position,
            transform.rotation,
            transform.parent,
            obj =>
            {
                finishCallBack?.Invoke();
            });
    }

    public void OnRelease()
    {
        transform.DOKill();
        
        for (int i = 0; i < delayList.Count; i++)
        {
            GameManager.Task.RemoveDelayTriggerTask(delayList[i]);
        }

        clickAction1 = null;
        clickAction2 = null;
        
        if(gameObject)
            Destroy(gameObject);
    }
}
