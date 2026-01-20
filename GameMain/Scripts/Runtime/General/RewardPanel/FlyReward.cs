using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 飞行奖励基类
/// </summary>
public abstract class FlyReward : MonoBehaviour
{
    public Transform body;
    public CanvasGroup numCanvas;

    protected TotalItemData rewardType;
    protected int rewardNum;
    protected Transform cachedTransform;
    protected bool autoGetReward;
    protected bool isLocked;
    protected bool getRewardAnimFinish;

    public abstract string Name { get; }

    public virtual int SortPriority { get => 5; }

    public TotalItemData RewardType { get => rewardType; }

    public int RewardNum { get => rewardNum; set => rewardNum = value; }

    public Transform CachedTransform { get => cachedTransform; }

    public bool AutoGetReward { get => autoGetReward; }

    public bool IsLocked { get => isLocked; }

    public bool GetRewardAnimFinish { get => getRewardAnimFinish; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="rewardType">奖励类型</param>
    /// <param name="rewardNum">奖励数量</param>
    /// <param name="rewardTypeCount">奖励总数</param>
    /// <param name="autoGetReward">是否自动获取</param>
    public virtual void OnInit(TotalItemData rewardType, int rewardNum, int rewardTypeCount, bool autoGetReward, RewardArea rewardArea)
    {
        this.rewardType = rewardType;
        this.rewardNum = rewardNum;
        this.autoGetReward = autoGetReward;

        isLocked = false;
        getRewardAnimFinish = false;
        cachedTransform = transform;

        RefreshAmountText();

        switch (rewardTypeCount)
        {
            case 1:
                body.localScale = Vector3.one;
                break;
            case 2:
            case 3:
                body.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                break;
            case 4:
                body.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                break;
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                body.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                break;
            case 10:
                body.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                break;
            default:
                body.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                break;
        }
    }

    public virtual void DoubleNumberText()
    {
        rewardNum*= 2;
        DoubleRefreshAmountText();
    }

    public virtual void DoubleRefreshAmountText()
    {
        
    }

    public virtual void OnReset()
    {
        HideRewardBgEffect();
        HideRewardShowEffect();

        autoGetReward = false;
        isLocked = false;
        getRewardAnimFinish = false;
    }

    public virtual void OnRelease()
    {
        body.DOKill();
        cachedTransform = null;
    }

    public virtual void OnShow(Action callback = null)
    {
        gameObject.SetActive(true);

        callback?.Invoke();
    }

    public virtual void OnHide(Action callback = null)
    {
        gameObject.SetActive(false);

        callback?.Invoke();
    }

    public virtual void RefreshAmountText()
    {
    }

    public virtual void ShowAmountText()
    {
    }

    public virtual void HideAmountText()
    {
    }

    public virtual void ShowRewardBgEffect()
    {
    }

    public virtual void HideRewardBgEffect()
    {
    }

    public virtual void ShowRewardShowEffect()
    {
    }

    public virtual void HideRewardShowEffect()
    {
    }

    public abstract IEnumerator ShowGetRewardAnim(TotalItemData type,Vector3 targetPos);
}
