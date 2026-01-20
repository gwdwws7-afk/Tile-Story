using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MySelf.Model;
using UnityEngine;

/// <summary>
/// 奖励显示区域
/// </summary>
public class RewardArea : MonoBehaviour
{
    protected List<FlyReward> rewardFlyObjects = new List<FlyReward>();
    protected List<FlyReward> cardPackFlyObjects = new List<FlyReward>();
    private Dictionary<string, bool> loadedFlag = new Dictionary<string, bool>();
    protected Transform cachedTransform;
    private int rewardTypeCount;
    protected bool isRleased;
    public Func<int, Vector3> GetRewardWorldPositionFunc;
    public Func<int> GetCoinStartNum;
    public Dictionary<TotalItemData, int> RewardsDic;
    public Action onRewardAreaShow;

    protected virtual string CoinFlyRewardName => "CoinFlyReward";
    protected virtual string LifeFlyRewardName => "LifeFlyReward";
    protected virtual string DiamondFlyRewardName => "DiamondFlyReward";
    protected virtual string DefaultFlyRewardName => "DefaultFlyReward";
    protected virtual string BgOrTileFlyRewardName => "BgOrTileFlyReward";
    protected virtual string StarFlyRewardName => "StarFlyReward";

    protected virtual string GasolineFlyReward => "GasolineFlyReward";
    
    protected virtual string PkItemFlyReward => "PkItemFlyReward";
    
    protected virtual string CardPackFlyReward => "CardPackFlyReward";
    
    // 优化点1：使用预分配的HashSet进行快速查找，避免重复遍历
    static readonly HashSet<TotalItemType> CardPackTypes = new HashSet<TotalItemType>
    {
        TotalItemType.CardPack1,
        TotalItemType.CardPack2,
        TotalItemType.CardPack3,
        TotalItemType.CardPack4,
        TotalItemType.CardPack5
    };
    
    public Transform CachedTransform
    {
        get
        {
            return cachedTransform;
        }
    }

    public int RewardTypeCount
    {
        get
        {
            return rewardTypeCount;
        }
    }

    private void Awake()
    {
        cachedTransform = transform;
    }

    public virtual void OnInit(Dictionary<TotalItemData, int> rewardsDic, bool autoGetReward)
    {
        RewardsDic = rewardsDic;
        rewardTypeCount = rewardsDic.Count;
        foreach (KeyValuePair<TotalItemData, int> rewardPair in rewardsDic)
        {
            bool alreadyHave = false;
            for (int i = 0; i < rewardFlyObjects.Count; i++)
            {
                if (rewardFlyObjects[i].RewardType == rewardPair.Key)
                {
                    alreadyHave = true;
                }
            }

            if (!alreadyHave)
            {
                InternalInitReward(rewardPair.Key, rewardPair.Value, autoGetReward);
            }
        }

        isRleased = false;
    }

    protected virtual void SortRewardFlyObjects(List<FlyReward> rewardFlyObjects)
    {
        try
        {
            rewardFlyObjects.Sort((a, b) =>
            {
                if (a.SortPriority > b.SortPriority)
                    return -1;
                else if (a.SortPriority < b.SortPriority)
                    return 1;
                else
                    return 0;
            });
        }
        catch (Exception e)
        {
            Log.Error("rewardFlyObjects sort error:{0}", e.Message);
        }
    }

    public virtual void OnShow(Action callback = null)
    {
        SortRewardFlyObjects(rewardFlyObjects);
        
        // 补充：如果不在卡牌活动中，卡包移除
        if (!CardModel.Instance.IsInCardActivity)
        {
            var cardPackList = new List<FlyReward>();
            foreach (var reward in rewardFlyObjects)
            {
                if (CardPackTypes.Contains(reward.RewardType.TotalItemType))
                    cardPackList.Add(reward);
            }

            foreach (var cardPack in cardPackList)
            {
                rewardFlyObjects.Remove(cardPack);
            }
        }

        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            if (GetRewardWorldPositionFunc == null)
                rewardFlyObjects[i].transform.localPosition = GetRewardLocalPosition(i);
            else
                rewardFlyObjects[i].transform.position = GetRewardWorldPositionFunc(i);

            int index = i;
            float delayTime = i * 0.2f;

            if (i == rewardFlyObjects.Count - 1)
            {
                GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                  {
                      rewardFlyObjects[index].OnShow();
                  });

                GameManager.Task.AddDelayTriggerTask(delayTime + 0.27f, () =>
                  {
                      callback?.Invoke();
                  });
            }
            else
            {
                GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                {
                    rewardFlyObjects[index].OnShow();
                });
            }
        }

        onRewardAreaShow?.Invoke();
        onRewardAreaShow = null;
    }

    public virtual void OnHide()
    {
        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            rewardFlyObjects[i].OnHide();
        }
        GameManager.PlayerData.SyncAllItemData();
    }

    public virtual void OnReset()
    {
        loadedFlag.Clear();
        GetRewardWorldPositionFunc = null;
        GetCoinStartNum = null;

        rewardTypeCount = 0;
        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            if (rewardFlyObjects[i] != null && !rewardFlyObjects[i].IsLocked) 
            {
                rewardFlyObjects[i].OnReset();
                rewardFlyObjects[i].OnRelease();
                if (GameManager.ObjectPool.HasObjectPool(rewardFlyObjects[i].Name))
                    GameManager.ObjectPool.Unspawn<FlyRewardObject>("FlyRewardPool", rewardFlyObjects[i].gameObject);
            }
        }
        rewardFlyObjects.Clear();
    }

    public virtual void OnRelease()
    {
        isRleased = true;
        StopAllCoroutines();
        GetRewardWorldPositionFunc = null;
        GetCoinStartNum = null;

        loadedFlag.Clear();

        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            rewardFlyObjects[i].OnRelease();
            if (GameManager.ObjectPool.HasObjectPool(rewardFlyObjects[i].Name))
                GameManager.ObjectPool.Unspawn<FlyRewardObject>("FlyRewardPool", rewardFlyObjects[i].gameObject);
        }
        rewardFlyObjects.Clear();

        //GameManager.ObjectPool.DestroyObjectPool("RotateCoin");
        GameManager.ObjectPool.DestroyObjectPool("FlyRewardPool");
    }

    /// <summary>
    /// 增加飞行奖励
    /// </summary>
    /// <param name="flyReward"></param>
    public void AddRewardFlyObject(FlyReward flyReward)
    {
        rewardFlyObjects.Add(flyReward);
    }

    public void DoubleNumberText(bool isInBack = false, Action callback = null)
    {
        if (isInBack)
        {
            foreach (var flyReward in rewardFlyObjects)
            {
                flyReward.RewardNum = flyReward.RewardNum / 2;
                flyReward.RefreshAmountText();
            }
            return;
        }

        foreach (var flyReward in rewardFlyObjects)
        {
            flyReward.DoubleNumberText();
        }

        GameManager.Task.AddDelayTriggerTask(1.3f, () =>
        {
            callback?.Invoke();
        });
    }

    /// <summary>
    /// 播放奖励获取的动画
    /// </summary>
    public virtual void ShowGetRewardAnim(Action onGetRewardComplete)
    {
        if (isRleased)
        {
            onGetRewardComplete?.Invoke();
            return;
        }

        try
        {
            StartCoroutine(ShowGetRewardAnimCor(onGetRewardComplete));
        }
        catch (Exception e)
        {
            OnHide();
            Debug.LogError(e.Message);
        }
    }
    
    IEnumerator ShowCardPackAnim()
    {
        List<FlyReward> cardPackList = new List<FlyReward>();
        foreach (FlyReward obj in rewardFlyObjects)
        {
            if (CardPackTypes.Contains(obj.RewardType.TotalItemType))
            {
                cardPackList.Add(obj);
            }
        }
        if (cardPackList.Count <= 0) yield break;
        
        foreach (var obj in cardPackList)
        {
            rewardFlyObjects.Remove(obj);
        }
        //存一份，动画完成后还回来一起释放
        cardPackFlyObjects = new List<FlyReward>(cardPackList);
        
        FlyReward theFirstPack = cardPackList[0];
        cardPackList.Remove(theFirstPack);
        
        // 隐藏其他奖励
        foreach (var flyReward in cardPackList)
        {
            flyReward.transform.DOScale(Vector3.zero, 0.2f);
        }
        foreach (var flyReward in rewardFlyObjects)
        {
            flyReward.transform.DOScale(Vector3.zero, 0.2f);
        }
        yield return new WaitForSeconds(0.2f);
        
        //撕卡包
        yield return StartCoroutine(theFirstPack.ShowGetRewardAnim(theFirstPack.RewardType, Vector3.zero));
        foreach (var flyReward in cardPackList)
        {
            flyReward.transform.DOScale(Vector3.one, 0);
            yield return StartCoroutine(flyReward.ShowGetRewardAnim(flyReward.RewardType, Vector3.zero));
        }
        GameManager.UI.HideUIForm("GetCardPackPanel");
    }

    IEnumerator ShowGetRewardAnimCor(Action onGetRewardComplete)
    {
        yield return StartCoroutine(ShowCardPackAnim());
        
        foreach (var flyReward in rewardFlyObjects)
        {
            if (flyReward.Name != "TaskFlyReward")
                flyReward.transform.localScale = Vector3.one;
        }
        
        Vector3 rewardFlyPosition = Vector3.down * 2;
        FlyReward specialFlyReward = null;

        for (int i = 0; i < rewardFlyObjects.Count; i++)
        {
            var flyer = rewardFlyObjects[i];
            TotalItemData rewardType = rewardFlyObjects[i].RewardType;
            if (rewardType == TotalItemData.Coin)
            {
                try
                {
                    if (RewardManager.Instance.CoinFlyReceiver != null)
                    {
                        RewardManager.Instance.CoinFlyReceiver.Show();
                        StartCoroutine(flyer.ShowGetRewardAnim(rewardType, RewardManager.Instance.CoinFlyReceiver.CoinFlyTargetPos));
                    }
                    else
                    {
                        StartCoroutine(flyer.ShowGetRewardAnim(rewardType, new Vector3(-1, 1, 0)));
                    }
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }
                yield return new WaitForSeconds(0.1f);
            }
            else if (rewardType == TotalItemData.Life || rewardType == TotalItemData.InfiniteLifeTime)
            {
                try
                {
                    if (RewardManager.Instance.LifeFlyReceiver != null)
                    {
                        RewardManager.Instance.LifeFlyReceiver.Show();
                        StartCoroutine(flyer.ShowGetRewardAnim(rewardType, RewardManager.Instance.LifeFlyReceiver.LifeFlyTargetPos));
                    }
                    else
                    {
                        StartCoroutine(flyer.ShowGetRewardAnim(rewardType, new Vector3(-1, 1, 0)));
                    }
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }
                yield return new WaitForSeconds(0.1f);
            }
            else if (rewardType.TotalItemType == TotalItemType.Item_BgID ||
                rewardType.TotalItemType == TotalItemType.Item_TileID)
            {
                specialFlyReward = flyer;

                try
                {
                    StartCoroutine(flyer.ShowGetRewardAnim(rewardType, Vector3.zero));
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                try
                {
                    var receiver = RewardManager.Instance.GetReceiverByItemType(rewardType);
                    Vector3 targetPos = rewardFlyPosition;
                    if (receiver != null)
                    {
                        targetPos = receiver.GetItemTargetPos(rewardType);
                    }
                    StartCoroutine(flyer.ShowGetRewardAnim(rewardType, targetPos));
                }
                catch (Exception e)
                {
                    OnHide();
                    Debug.LogError(e.Message);
                }

                yield return new WaitForSeconds(0.15f);
            }
        }

        bool isAnimFinish = false;
        while (!isAnimFinish)
        {
            isAnimFinish = true;
            for (int i = 0; i < rewardFlyObjects.Count; i++)
            {
                if (!rewardFlyObjects[i].GetRewardAnimFinish)
                {
                    isAnimFinish = false;
                    break;
                }
            }
            yield return null;
        }

        //卡包奖励还回来
        rewardFlyObjects.AddRange(cardPackFlyObjects);
        cardPackFlyObjects = new List<FlyReward>();
        
        //请留意! 暂时没有考虑奖励中包含多个“特殊的需要额外展示流程的道具”(比如背景/棋子/头像)的情况
        //只展示最后一个
        if (specialFlyReward != null)
        {
            if (specialFlyReward.RewardType.TotalItemType == TotalItemType.Item_BgID ||
                specialFlyReward.RewardType.TotalItemType == TotalItemType.Item_TileID)
                GameManager.UI.ShowUIForm("GetBGOrTileItemPanel",(u) =>
                {
                    GetBGOrTileItemPanel panel = u as GetBGOrTileItemPanel;

                    GameManager.Sound.PlayAudio(SoundType.SFX_ShowBGPanel.ToString());
                    u.m_OnHideCompleteAction = () =>
                    {
                        onGetRewardComplete?.Invoke();
                    };
                    //这个SetClearBg是OnGetRewardComplete的一环 但是我们需要提前触发 来避免GetBGOrTileItemPanel点不到
                    if (RewardManager.Instance.RewardPanel != null)
                        RewardManager.Instance.RewardPanel.SetClearBgActive(false);
                    OnHide();
                }, userData: specialFlyReward );
        }
        else
        {
            onGetRewardComplete?.Invoke();
            OnHide();
        }
    }

    protected virtual string GetFlyRewardName(TotalItemData type)
    {
        string assetName;
        if (type == TotalItemData.Coin)
        {
            assetName = CoinFlyRewardName;
        }
        else if(type==TotalItemData.Life||
                type == TotalItemData.InfiniteLifeTime)
        {
            assetName = LifeFlyRewardName;
        }
        else if (type == TotalItemData.Star)
        {
            assetName = StarFlyRewardName;
        }
        else if (type.TotalItemType == TotalItemType.Item_BgID ||
                 type.TotalItemType == TotalItemType.Item_TileID)
        {
            assetName = BgOrTileFlyRewardName;
        }
        else if (type.TotalItemType == TotalItemType.Gasoline)
        {
            assetName = GasolineFlyReward;
        }else if (type.TotalItemType == TotalItemType.PkItem)
        {
            assetName = PkItemFlyReward;
        }
        else if (CardPackTypes.Contains(type.TotalItemType))
        {
            assetName = CardPackFlyReward;
        }
        else
        {
            assetName = DefaultFlyRewardName;
        }

        return assetName;
    }
    
    protected virtual void InternalInitReward(TotalItemData type, int num, bool autoGetReward)
    {
        if (type == TotalItemData.None)
        {
            Log.Error("Get reward type is none");
            return;
        }

        string key = type.GetHashCode().ToString();
        loadedFlag[key] = false;
        GameManager.ObjectPool.Spawn<FlyRewardObject>(GetFlyRewardName(type), "FlyRewardPool", Vector3.zero, Quaternion.identity, cachedTransform, (obj) =>
        {
            GameObject flyObject = (GameObject)obj.Target;
            FlyReward flyReward = flyObject.GetComponent<FlyReward>();
            flyReward.OnInit(type, num, rewardTypeCount, autoGetReward, this);
            AddRewardFlyObject(flyReward);
            
            loadedFlag[key] = true;
        });
    }

    public virtual bool CheckLoadComplete()
    {
        foreach (KeyValuePair<string, bool> flag in loadedFlag)
        {
            if (!flag.Value)
            {
                return false;
            }
        }
        return true;
    }

    protected virtual Vector3 GetRewardLocalPosition(int index)
    {
        int totalCount = rewardFlyObjects.Count;

        if (totalCount <= 0)
        {
            Log.Error("Reward Area rewardFlyObjects count is invalid");
        }

        if (totalCount == 1)
        {
            return Vector3.zero;
        }
        else if (totalCount == 2)
        {
            return new Vector3(-170 + index * 340, 0);
        }
        else if (totalCount == 3)
        {
            return new Vector3(-310 + index * 310, 0);
        }
        else if (totalCount == 4)
        {
            return new Vector3(-180 + (index % 2) * 360, 180 - (index / 2) * 360);
        }
        else if (totalCount == 5)
        {
            if (index < 3)
            {
                return new Vector3(-310 + index * 310, 150);
            }
            else
            {
                return new Vector3(-160 + (index - 3) * 320, -150);
            }
        }
        else if (totalCount == 6)
        {
            return new Vector3(-310 + (index % 3) * 310, 150 - (index / 3) * 300);
        }
        else if (totalCount == 7)
        {
            if (index < 2)
            {
                return new Vector3(-160 + index * 320, 270);
            }
            else if (index < 5)
            {
                return new Vector3(-310 + (index - 2) * 310, 0);
            }
            else
            {
                return new Vector3(-160 + (index - 5) * 320, -270);
            }
        }
        else if (totalCount == 8)
        {
            if (index < 3)
            {
                return new Vector3(-300 + index * 300, 270);
            }
            else if (index < 6)
            {
                return new Vector3(-300 + (index - 3) * 300, 0);
            }
            else
            {
                return new Vector3(-170 + (index - 6) * 340, -270);
            }
        }
        else if (totalCount == 9)
        {
            if (index < 3)
            {
                return new Vector3(-300 + index * 300, 270);
            }
            else if (index < 6)
            {
                return new Vector3(-300 + (index - 3) * 300, 0);
            }
            else
            {
                return new Vector3(-300 + (index - 6) * 300, -270);
            }
        }
        else if (totalCount == 10)
        {
            if (index < 3)
            {
                return new Vector3(-270 + index * 270, 260);
            }
            else if (index < 7)
            {
                return new Vector3(-390 + (index - 3) * 260, 0);
            }
            else
            {
                return new Vector3(-270 + (index - 7) * 270, -260);
            }
        }
        else if (totalCount == 11)
        {
            if (index < 3)
            {
                return new Vector3(-270 + index * 270, 260);
            }
            else if (index < 7)
            {
                return new Vector3(-390 + (index - 3) * 260, 0);
            }
            else
            {
                return new Vector3(-390 + (index - 7) * 260, -260);
            }
        }
        else if (totalCount == 12)
        {
            if (index < 4)
            {
                return new Vector3(-390 + index * 260, 260);
            }
            else if (index < 8)
            {
                return new Vector3(-390 + (index - 4) * 260, 0);
            }
            else
            {
                return new Vector3(-390 + (index - 8) * 260, -260);
            }
        }

        return GetRewardSectorLocalPosition(index);
    }

    protected Vector3 GetRewardSectorLocalPosition(int index)
    {
        int totalCount = rewardFlyObjects.Count;
        int count;
        float halfCount;
        bool isEvenCount;
        int ratioX;
        int ratioY;
        int deltaY;

        if (index < 5)
        {
            count = Mathf.Clamp(totalCount, 0, 5);

            isEvenCount = count % 2 == 0;

            ratioX = count > 3 ? 200 : 300;
            ratioY = count == 1 ? 200 : 300;

            if (isEvenCount)
            {
                ratioX += 30;
                halfCount = (count - 1) / 2f;
            }
            else
            {
                halfCount = count / 2;
            }

            deltaY = totalCount > 5 ? 0 : -200;

            float temp = index - halfCount;
            if (!isEvenCount)
            {
                if (temp == -1)
                    temp = -1.1f;
                if (temp == 1)
                    temp = 1.1f;
            }
            else
            {
                if (temp == -0.5f)
                    temp = -0.55f;
                if (temp == 0.5f)
                    temp = 0.55f;
            }

            return new Vector3(temp * ratioX, Mathf.Cos(temp * 0.65f) * ratioY + deltaY);
        }
        else
        {
            count = totalCount - 5;

            if (count <= 0)
            {
                Log.Error("index is beyoung totalCount");
                count = 1;
            }

            ratioX = count > 3 ? 180 : 200;
            ratioY = count == 1 ? 0 : 200;

            isEvenCount = count % 2 == 0;
            if (isEvenCount)
            {
                ratioX += 30;
                halfCount = (count - 1) / 2f;
            }
            else
            {
                halfCount = count / 2;
            }

            deltaY = -260;

            return new Vector3((index - 5 - halfCount) * ratioX, Mathf.Cos(index - 5 - halfCount) * ratioY + deltaY);
        }
    }
}
