using DG.Tweening;
using MySelf.Model;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EndlessChestItem : MonoBehaviour
{
    [SerializeField] private Transform ItemRoot;
    [SerializeField] private DelayButton RewardBtn;
    [SerializeField] private DelayButton HelpBtn;
    [SerializeField] private DelayButton ChestBtn;
    [SerializeField] private GameObject ChestObj, OkObj, NoGetRewardImageBg, HasGetRewardImageBg;
    [SerializeField] private SkeletonGraphic LockSpine;
    [SerializeField] private TextMeshProUGUILocalize PriceText;
    [SerializeField] private HorizontalLayoutGroup HorizontalLayoutGroup;

    private EndlessTreasureDatas data;
    private int layIndex;
    private bool isPlayAnim;
    private Action<int, bool> buyAction;
    private Action<int> startGetRewardAction;
    private Action<int> overGetRewardAction;
    private Action<List<TotalItemData>, List<int>, Vector3> ItemPromptBoxAction;
    private ProductNameType productNameType = ProductNameType.None;

    private Image rewardImage;
    private Image RewardImage
    {
        get
        {
            if (rewardImage == null)
            {
                rewardImage = HasGetRewardImageBg.GetComponent<Image>();
            }
            return rewardImage;
        }
    }

    public void Init(
       int layerIndex,
       EndlessTreasureDatas data,
       Action<int, bool> buyAction,
       Action<int> startGetRewardAction,
       Action<int> overGetRewardAction,
       Action<List<TotalItemData>, List<int>, Vector3> itemPromptBoxAction,
       bool isPlayAnim = false)
    {
        //数据
        this.data = data;
        this.layIndex = layerIndex;
        this.buyAction = buyAction;
        this.overGetRewardAction = overGetRewardAction;
        this.startGetRewardAction = startGetRewardAction;
        this.ItemPromptBoxAction = itemPromptBoxAction;
        this.isPlayAnim = isPlayAnim;
        //设置商品类型
        string type = GameManager.DataTable.GetDataTable<DTProductID>().Data.GetProductNameById(data.ProductID);
        if (!string.IsNullOrEmpty(type)) productNameType = (ProductNameType)Enum.Parse(typeof(ProductNameType), type);
        //
        this.gameObject.name = $"{layerIndex}_{data.ActivityID}";
        OkObj.gameObject.SetActive(false);
        //ui
        ShowItem();
        //按钮事件
        SetBtnEvent();
    }

    public void SetClickStatus(bool isCanClick)
    {
        RewardBtn.enabled = isCanClick;
    }

    private void ShowItem()
    {
        //根据data情况展示
        //宝箱或者道具
        if (data.ProductID != 0 && EndlessChestModel.Instance.Data.CurBuyChestId != data.ProductID)
        {
            string price = GameManager.Purchase.GetPrice(productNameType);
            PriceText.SetTerm(!string.IsNullOrEmpty(price) ? price : "Shop.Buy");
        }
        else
            PriceText.SetTerm("Level.Free");

        RewardBtn.DOKill();
        RewardBtn.transform.localScale = Vector3.one * 0.5f;
        RewardBtn.gameObject.SetActive(true);
        ChestObj.gameObject.SetActive(data.IsHaveChest);
        ItemRoot.gameObject.SetActive(!data.IsHaveChest);
        HelpBtn.gameObject.SetActive(data.IsHaveChest);//有宝箱时展示help按钮

        NoGetRewardImageBg.gameObject.SetActive(true);
        HasGetRewardImageBg.gameObject.SetActive(false);
        //当前跟存储一致解锁
        bool isUnLock = EndlessChestModel.Instance.Data.CurChestId >= layIndex;
        if (isUnLock)
        {
            LockSpine.gameObject.SetActive(true);
            if (isPlayAnim)
            {
                RewardImage.DOFade(1, 0.6f);
                HasGetRewardImageBg.gameObject.SetActive(true);

                // GameManager.Sound.PlayAudio(SoundType.SFX_goldenpass_presentlocked.ToString());
                // LockSpine.AnimationState.SetAnimation(0, "active_lock", false).Complete += (s) =>
                //  {
                //      LockSpine.gameObject.SetActive(false);
                //  };
            }
            else
            {
                LockSpine.gameObject.SetActive(false);
                RewardImage.DOFade(1, 0f);
                HasGetRewardImageBg.gameObject.SetActive(true);
            }
        }
        else
        {
            var anim = LockSpine.AnimationState.SetAnimation(0, "shake_lock", false);
            anim.AnimationStart = 0;
            anim.AnimationEnd = 0;
            LockSpine.gameObject.SetActive(true);
        }


        if (!data.IsHaveChest)
        {
            int count = data.RewardIdsDict.Count;
            UnityUtility.FillGameObjectWithFirstChild<ItemPrefab>(ItemRoot.gameObject, data.RewardIdsDict.Count, (index, comp) =>
             {
                 TotalItemData id = data.RewardIdsDict.ElementAt(index).Key;
                 int num = data.RewardIdsDict.ElementAt(index).Value;
                 comp.Init(id, num);
                 // comp.ChangePos(count);
             });
        }
        SetHorizontalLayoutGroupSize();
    }

    //领取之后 改变状态
    private void SetStatus(bool isHaveGetReward)
    {
        RewardBtn.DOKill();
        RewardBtn.transform.DOScale(0, 0.2f).SetEase(Ease.InBack).onComplete += () =>
         {
             OkObj.transform.localScale = Vector3.zero;
             OkObj.gameObject.SetActive(true);
             OkObj.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack);
         };
    }

    private void SetBtnEvent()
    {
        RewardBtn.SetBtnEvent(() =>
        {
            if (EndlessChestModel.Instance.Data.CurChestId != layIndex)
            {
                LockSpine.AnimationState.SetAnimation(0, "shake_lock", false);
                //提示
                GameManager.UI.ShowWeakHint("Endless.Claim previous offer to unlock",
                    startPos: new Vector3(0, 0.15f, 0));
                return;
            }

            Action getRewardAction = () =>
            {
                GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());
                //改变ui状态
                SetStatus(true);
                //记录领取
                EndlessChestModel.Instance.RecordGetRewardChestId(layIndex);
                //领取奖励
                var rewards = new Dictionary<TotalItemData, int>(data.RewardIdsDict);
                startGetRewardAction?.InvokeSafely(data.ActivityID);

                foreach (var reward in data.RewardIdsDict)
                {
                    if (reward.Key.Equals(TotalItemData.CardPack1) ||
                        reward.Key.Equals(TotalItemData.CardPack2) ||
                        reward.Key.Equals(TotalItemData.CardPack3) ||
                        reward.Key.Equals(TotalItemData.CardPack4) ||
                        reward.Key.Equals(TotalItemData.CardPack5))
                    {
                        RewardManager.Instance.AddNeedGetReward(reward.Key, reward.Value);
                        rewards.Remove(reward.Key);
                    }
                    else
                    {
                        if (reward.Key.Equals(TotalItemData.Coin))
                            GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(reward.Value, null));
                        if (reward.Key.Equals(TotalItemData.Life) || reward.Key.Equals(TotalItemData.InfiniteLifeTime))
                            GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(reward.Value, null));

                        GameManager.PlayerData.AddItemNum(reward.Key, reward.Value);
                    }
                }

                GameManager.PlayerData.SyncAllItemData();
                //直接飞奖励
                ShowRewardAnim(rewards);
                // bool isChest = data.IsHaveChest;
                // if (isChest)
                // {
                //    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.ChestRewardPanel, true, () =>
                //    {
                //       //发送消息
                //       overGetRewardAction?.InvokeSafely(data.ActivityID);
                //       //改变ui状态
                //       SetStatus(true);
                //    });
                // }
                // else
                // {
                //    RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.None, true, () =>
                //    {
                //       //发送消息
                //       overGetRewardAction?.InvokeSafely(data.ActivityID);
                //       //改变ui状态
                //       SetStatus(true);
                //    });
                // }
            };

            if (data.ProductID == 0)
            {
                getRewardAction?.InvokeSafely();
                return;
            }

            GameManager.Purchase.BuyProduct(productNameType,
                () =>
                {
                    //成功回调 发送消息
                    buyAction?.InvokeSafely(data.ActivityID, true);
                    getRewardAction?.InvokeSafely();
                },
                e => { buyAction?.InvokeSafely(data.ActivityID, false); });
        });
        HelpBtn.SetBtnEvent(() =>
        {
            //展示宝箱可以获取的奖励内容
            ItemPromptBoxAction?.Invoke(data.RewardIdsDict.Keys.ToList(), data.RewardIdsDict.Values.ToList(),
                HelpBtn.transform.position);
        });

        ChestBtn.SetBtnEvent(() =>
        {
            //展示宝箱可以获取的奖励内容
            ItemPromptBoxAction?.Invoke(data.RewardIdsDict.Keys.ToList(), data.RewardIdsDict.Values.ToList(),
                HelpBtn.transform.position);
        });
    }

    private void SetHorizontalLayoutGroupSize()
    {
        int count = data.IsHaveChest ? 1 : data.RewardIdsDict.Count;
        switch (count)
        {
            case 1:
                HorizontalLayoutGroup.spacing = 0;
                HorizontalLayoutGroup.transform.localScale = Vector3.one;
                return;
            case 2:
                HorizontalLayoutGroup.spacing = 130;
                HorizontalLayoutGroup.transform.localScale = Vector3.one;
                return;
            case 3:
                HorizontalLayoutGroup.spacing = 125;
                HorizontalLayoutGroup.transform.localScale = Vector3.one * 0.9f;
                return;
        }
    }

    private void ShowRewardAnim(Dictionary<TotalItemData, int> rewards)
    {
        // var rewards = data.RewardIdsDict;
        var parent = transform.parent;
        var pos = transform.localPosition;

        for (int i = 0; i < rewards.Count; i++)
        {
            var key = rewards.ElementAt(i).Key;
            var value = rewards.ElementAt(i).Value;
            int num = i;
            GameManager.Task.AddDelayTriggerTask(i * 0.2f, () =>
            {
                UnityUtility.InstantiateAsync("ItemPrefab", parent, (obj) =>
                {
                    var item = obj.GetComponent<ItemPrefab>();
                    item.Init(key, value);
                    item.gameObject.SetActive(false);
                    item.transform.localPosition = pos + GetItemPos(num + 1, rewards.Count) + Vector3.up * 80;
                    item.transform.localScale = Vector3.zero;
                    UnityUtility.InstantiateAsync("effect_Tile_Newstar", item.transform,
                        effect =>
                        {
                            GameManager.Task.AddDelayTriggerTask(0.5f,
                                () => { UnityUtility.UnloadInstance(effect); });
                        });
                    GameManager.Task.AddDelayTriggerTask(0.03f, () =>
                    {
                        item.gameObject.SetActive(true);
                        item.transform.DOScale(0.8f, 0.2f).SetEase(Ease.OutBack);
                        item.transform.DOLocalMoveY(item.transform.localPosition.y + 50, 1f).SetEase(Ease.InSine);
                        var canvasGroup = item.gameObject.AddComponent<CanvasGroup>();
                        canvasGroup.DOFade(0, 0.3f).SetDelay(0.8f).onComplete += () =>
                        {
                            UnityUtility.UnloadInstance(obj);
                        };
                    });
                });
            });
        }

        GameManager.Task.AddDelayTriggerTask(0.3f * rewards.Count, () =>
        {
            RewardManager.Instance.AutoGetRewardDelayTime = 0f;
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true, () =>
            {
                RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                overGetRewardAction?.InvokeSafely(data.ActivityID);
            });
        });
    }

    private Vector3 GetItemPos(int index, int totalNum)
    {
        switch (totalNum)
        {
            case 1:
                return new Vector3(0, 160, 0);
            case 2:
                switch (index)
                {
                    case 1:
                        return new Vector3(-60, 160, 0);
                    case 2:
                        return new Vector3(60, 160, 0);
                }
                break;
            case 3:
                switch (index)
                {
                    case 1:
                        return new Vector3(-140, 160, 0);
                    case 2:
                        return new Vector3(0, 160, 0);
                    case 3:
                        return new Vector3(140, 160, 0);
                }
                break;
            case 4:
                switch (index)
                {
                    case 1:
                        return new Vector3(-60, 220, 0);
                    case 2:
                        return new Vector3(60, 220, 0);
                    case 3:
                        return new Vector3(-60, 100, 0);
                    case 4:
                        return new Vector3(60, 100, 0);
                }
                break;

        }
        return new Vector3(0, 160, 0);
    }

    public void PlayBgAnim()
    {
        RewardImage.gameObject.SetActive(true);
        RewardImage.color = new Color(1, 1, 1, 0);
        RewardImage.DOFade(1, 1f);

        if (EndlessChestModel.Instance.Data.CurChestId >= layIndex)
        {
            GameManager.Sound.PlayAudio(SoundType.SFX_goldenpass_presentlocked.ToString());
            LockSpine.AnimationState.SetAnimation(0, "active_lock", false).Complete += (s) =>
            {
                LockSpine.gameObject.SetActive(false);
            };   
        }
    }
}
