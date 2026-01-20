using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameFramework.Event;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CardSetMainMenu : CenterForm
{
    public DelayButton tipButton, closeButton, starButton;
    public GameObject starWarningSign;
    public ClockBar clockBar;
    public GameObject rewardArea;
    public SimpleSlider simpleSlider;
    public RectTransform content;
    public SkeletonGraphic parrot, title;
    public GameObject cardSetItemPrefab;

    private int _activityID;
    List<TotalItemData> _finalRewardTypeList;
    List<int> _finalRewardNumList;
    private List<CardSetItem> _cardSetItemList = new List<CardSetItem>();
    // private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>(15);
    private bool _isAnimFinish;
    private RectTransform _tipTrans, _closeTrans, _starTrans;
    private Sequence _sequence;
    private GridLayoutGroup _contentGrid;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        SetAnimOriginalState();
        
        _activityID = CardModel.Instance.CardActivityID;
        
        tipButton.OnInit(() => GameManager.UI.ShowUIForm($"CardSetRulesPanel{_activityID}"));
        closeButton.OnInit(() =>
        {
            GameManager.UI.HideUIForm(this);
            MapTopPanelManager mapTopPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTopPanel?.cardSetEntrance.SetClaim();
        });
        starButton.OnInit(() => GameManager.UI.ShowUIForm($"CardSetStorePanel{_activityID}"));

        SetRewardArea();
        SetScrollArea();
        
        clockBar.OnReset();
        clockBar.StartCountdown(CardModel.Instance.CardEndTime);
        
        simpleSlider.OnReset();
        simpleSlider.TotalNum = CardModel.Instance.TotalCardNum;
        simpleSlider.CurrentNum = CardModel.Instance.TotalCollectNum;
        
        GameManager.Event.Subscribe(CardChangeEventArgs.EventId, OnCardChange);
        
        GameManager.Ads.HideBanner();
        
        base.OnInit(uiGroup, completeAction, userData);
    }
    
    private void SetAnimOriginalState()
    {
        _isAnimFinish = false;
        
        // _tipTrans = tipButton.GetComponent<RectTransform>();
        // _tipTrans.anchoredPosition = new Vector2(_tipTrans.anchoredPosition.x, 300);
        //
        // _closeTrans = closeButton.GetComponent<RectTransform>();
        // _closeTrans.anchoredPosition = new Vector2(_closeTrans.anchoredPosition.x, 300);

        _starTrans = starButton.GetComponent<RectTransform>();
        _starTrans.anchoredPosition = new Vector2(0, _starTrans.anchoredPosition.y);

        starWarningSign.SetActive(false);
        parrot?.gameObject.SetActive(false);
        
        KillAllDoTween();
    }
    
    private void SetAnimFinishState()
    {
        KillAllDoTween();
        _starTrans.anchoredPosition = new Vector2(-188, _starTrans.anchoredPosition.y);
        _contentGrid.padding.top = 0;
        _contentGrid.enabled = true;
        foreach (var cardSetItem in _cardSetItemList)
        {
            cardSetItem.canvas.alpha = 1;
        }
        _isAnimFinish = true;
    }
    
    private void KillAllDoTween()
    {
        content.DOKill();
        _tipTrans?.DOKill();
        _closeTrans?.DOKill();
        _starTrans?.DOKill();
        _sequence?.Kill();
    }

    private IEnumerator InitCoroutine()
    {
        Debug.LogError("");
        yield return null;
        
        ShowGuide();
        // _tipTrans.DOAnchorPosY(-202, 0.2f).SetEase(Ease.OutBack).onComplete = ShowGuide;
        // _closeTrans.DOAnchorPosY(-202, 0.2f).SetEase(Ease.OutBack);
        _starTrans.DOAnchorPosX(-188, 0.2f).SetEase(Ease.OutBack).SetDelay(0f).onComplete = SetStarWarningSign;
        
        yield return new WaitForSeconds(0.2f);
        
        float finishTime = PlayEntranceAnimation();
        
        yield return new WaitForSeconds(0.2f);
        if (parrot != null)
        {
            parrot.AnimationState.SetAnimation(0, "move", false).Complete += entry =>
            {
                parrot.AnimationState.SetAnimation(0, "idle", true);
            };
            parrot.Update(0);
            parrot.gameObject.SetActive(true);
        }
        
        if (title != null)
        {
            title.AnimationState.SetAnimation(0, "active", false).Complete += entry =>
            {
                title.AnimationState.SetAnimation(0, "idle", true);
            };
            title.gameObject.SetActive(true);
        }
        
        yield return new WaitForSeconds(0.2f);
        GameManager.Sound.PlayMusic(CardModel.Instance.BgmName);
        
        yield return new WaitForSeconds(finishTime - 0.4f);
        _isAnimFinish = true;
    }
    
    private float PlayEntranceAnimation()
    {
        _contentGrid.enabled = false;
        int setsPerRow = _contentGrid.constraintCount;
        float moveDistance = _contentGrid.padding.top; // 向上滑动的距离
        float rowDelay = 0.15f; // 每行之间的延迟
        float columnDelay = 0.0f; // 每列之间的延迟
        float fadeDuration = 0.5f;
        
        _sequence = DOTween.Sequence();
        // 【防护】绑定生命周期。如果当前 GameObject 被销毁，动画自动 Kill，防报错。
        _sequence.SetLink(gameObject);
        
        for (int i = 0; i < _cardSetItemList.Count; i++)
        {
            int rowIndex = i / setsPerRow;
            int columnIndex = i % setsPerRow;
            // 同一行的卡片延迟相同（自改）
            float delay = rowIndex * rowDelay + columnIndex * columnDelay;

            CanvasGroup cg = _cardSetItemList[i].canvas;
            Transform trans = cg.transform;

            // 每一行的淡入和位移
            _sequence.Insert(delay, cg.DOFade(1, fadeDuration).SetEase(Ease.OutCubic));
            _sequence.Insert(delay, trans.DOLocalMoveY(trans.localPosition.y + moveDistance, fadeDuration).SetEase(Ease.OutBack));
        }

        float finishTime = 2 * rowDelay + fadeDuration;
        return finishTime;
        // _sequence.OnComplete(() =>
        // {
        //     _isAnimFinish = true;
        // });
    }
    
    private IEnumerator PlayScrollAnimCoroutine(float duration = 0)
    {
        yield return new WaitUntil(() => _isAnimFinish);
        
        int index = -1;
        foreach (var cardSet in CardModel.Instance.CardSetDict)
        {
            if (cardSet.Value.CardSetCollectNum() == cardSet.Value.CardDict.Count &&
                !CardModel.Instance.CompletedCardSets.Contains(cardSet.Key))
            {
                index = cardSet.Key;
                break;
            }
        }
        if (index == -1)
        {
            foreach (var cardSet in CardModel.Instance.CardSetDict)
            {
                CardModel.Instance.NewCardDict.TryGetValue(cardSet.Key, out var cards);
                if (cards is { Count: > 0 })
                {
                    index = cardSet.Key;
                    break;
                }
            }
        }

        if (index != -1)
        {
            float contentHeight = content.rect.height;
            float viewportHeight = content.parent.GetComponent<RectTransform>().rect.height;
            int rowIndex = (index - 1) / _contentGrid.constraintCount;
            float setHeight = _contentGrid.cellSize.y;
            float endValue = Mathf.Min(rowIndex * setHeight, contentHeight - viewportHeight);
            duration = Mathf.Abs(content.anchoredPosition.y - endValue) * 0.001f;
            duration = Mathf.Min(duration, 0.5f);
            content.DOAnchorPosY(endValue, duration).SetEase(Ease.OutQuad);
        }
        
        foreach (var cardSetItem in _cardSetItemList)
        {
            cardSetItem.Refresh(delayTime: duration);
        }
    }

    public override void OnRelease()
    {
        tipButton.OnReset();
        closeButton.OnReset();
        starButton.OnReset();
        clockBar.OnReset();
        simpleSlider.OnReset();
        
        foreach (Transform child in rewardArea.transform)
        {
            child.GetComponent<CardSetRewardSlot>().Release();
        }
        
        foreach (var cardSetItem in _cardSetItemList)
        {
            cardSetItem.Release();
        }
        _cardSetItemList.Clear();
        // foreach (var assetHandle in _assetHandleList)
        // {
        //     if (!assetHandle.IsDone)
        //     {
        //         assetHandle.Completed += (a) =>
        //         {
        //             UnityUtility.UnloadInstance((GameObject)a.Result);
        //         };
        //     }
        //     else
        //     {
        //         UnityUtility.UnloadInstance((GameObject) assetHandle.Result);
        //     }
        // }
        // _assetHandleList.Clear();
        
        GameManager.Event.Unsubscribe(CardChangeEventArgs.EventId, OnCardChange);
        
        StopAllCoroutines();
        KillAllDoTween();
        
        GameManager.Ads.ShowBanner();

        GameManager.Sound.PlayMusic(GameManager.PlayerData.BGMusicName);

        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        Debug.LogError("OnShow");
        base.OnShow(showSuccessAction, userData);
        if (!_isAnimFinish) StartCoroutine(InitCoroutine());
        StartCoroutine(PlayScrollAnimCoroutine());
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);
        SetAnimFinishState();
    }

    public override void OnReveal()
    {
        Debug.LogError("OnReveal");
        gameObject.SetActive(true);
        StartCoroutine(PlayScrollAnimCoroutine());
        ShowFinalReward();
    }
    
    private void OnCardChange(object sender, GameEventArgs e)
    {
        simpleSlider.CurrentNum = CardModel.Instance.TotalCollectNum;
        StartCoroutine(PlayScrollAnimCoroutine());
    }
    
    private void SetRewardArea()
    {
        (_finalRewardTypeList, _finalRewardNumList) = GameManager.DataTable.GetDataTable<DTCardSetData>().Data
            .GetCurrentFinalRewardByActivityID(_activityID);
        
        // UnityUtility.FillGameObjectWithFirstChild<CardSetRewardSlot>(rewardArea, _finalRewardNumList.Count, (index, comp) =>
        // {
        //     comp.Init(_finalRewardTypeList[index], _finalRewardNumList[index]);
        // });

        for (int i = 0; i < rewardArea.transform.childCount; i++)
        {
            rewardArea.transform.GetChild(i).GetComponent<CardSetRewardSlot>().Init(_finalRewardTypeList[i], _finalRewardNumList[i]);
        }
    }

    private void SetScrollArea()
    {
        // foreach (var cardSet in CardModel.Instance.CardSetDict)
        // {
        //     AsyncOperationHandle assetHandle = UnityUtility.InstantiateAsync(
        //         $"CardSetItem{_activityID}", content, asset =>
        //         {
        //             CardSetItem cardSetItem = asset.GetComponent<CardSetItem>();
        //             cardSetItem.Init(cardSet.Value);
        //             _cardSetItemList.Add(cardSetItem);
        //         });
        //     _assetHandleList.Add(assetHandle);
        // }
        
        _contentGrid = content.GetComponent<GridLayoutGroup>();
        _contentGrid.padding.top = 10;
        _contentGrid.enabled = true;
        content.DestroyAllChild();
        content.anchoredPosition = Vector2.zero;
        foreach (var cardSet in CardModel.Instance.CardSetDict)
        {
            GameObject obj = Instantiate(cardSetItemPrefab, content);
            CardSetItem cardSetItem = obj.GetComponent<CardSetItem>();
            cardSetItem.Init(cardSet.Value);
            _cardSetItemList.Add(cardSetItem);
        }
    }
    
    public void SetStarWarningSign()
    {
        int leastNeedStarNum = GameManager.DataTable.GetDataTable<DTCardStarRewardData>().Data.GetRewardByID(1).StarNum;
        starWarningSign.SetActive(CardModel.Instance.ExtraStarNum >= leastNeedStarNum || CardModel.Instance.CanUseCoinForPack);
    }

    private void ShowFinalReward()
    {
        if (CardModel.Instance.CompletedAll) return;
        if (CardModel.Instance.CompletedCardSets.Count == CardModel.Instance.CardSetDict.Count)
        {
            for (int i = 0; i < _finalRewardTypeList.Count; i++)
            {
                RewardManager.Instance.AddNeedGetReward(_finalRewardTypeList[i], _finalRewardNumList[i]);
            }

            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardFinalChestRewardPanel, false, () =>
            {
                CardModel.Instance.CompletedAll = true;
            }, () =>
            {
                GameManager.UI.HideUIForm(this);
                MapTopPanelManager mapTopPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                mapTopPanel?.cardSetEntrance.SetClaim();
            });
        }
    }

    private void ShowGuide()
    {
        if (CardModel.Instance.ShowedGuide) return;
        
        GameManager.UI.ShowUIForm("CardSetGuideMenu", form =>
        {
            CardSetGuideMenu guideMenu = form as CardSetGuideMenu;
            
            Transform originalParent = tipButton.transform.parent;
            tipButton.transform.SetParent(form.transform);
            tipButton.transform.SetAsLastSibling();
            
            guideMenu?.ShowArrow(tipButton.transform.position + new Vector3(0.2f, 0));
            tipButton.OnInit(() =>
            {
                CardModel.Instance.ShowedGuide = true;
                
                tipButton.transform.SetParent(originalParent);
                tipButton.transform.SetAsLastSibling();
                
                guideMenu?.guideArrow.gameObject.SetActive(false);
                GameManager.UI.ShowUIForm($"CardSetRulesPanel{_activityID}", menu =>
                {
                    GameManager.UI.HideUIForm(form);
                });

                tipButton.OnInit(() => GameManager.UI.ShowUIForm($"CardSetRulesPanel{_activityID}"));
            });
        });
    }
}
