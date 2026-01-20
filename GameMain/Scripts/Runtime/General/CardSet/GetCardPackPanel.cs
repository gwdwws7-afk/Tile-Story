using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using DG.Tweening;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GetCardPackPanel : UIForm
{
    public BlackBgManager blackBg;
    public Transform tapToOpen, tapToCollect;
    public Transform packRoot;
    public SkeletonGraphic packSpine;
    public UIParticle openEffect;
    public UIParticle fastOpenEffect;
    public Transform cardArea;
    public FakeEntrance entrance;
    
    private Action _completeAction;
    private int _packType;
    private List<SpineCardItem> _cardItemList = new List<SpineCardItem>();
    private List<SpineCardItem> _newCards = new List<SpineCardItem>();
    private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>();
    private bool _needGlobalMask = false;
    private List<Vector3> _cardPosList = new List<Vector3>();
    
    private ShakeController shakeController;

    private ShakeController ShakeController
    {
        get
        {
            if (shakeController == null)
            {
                shakeController = Camera.main.gameObject.Get<ShakeController>();
            }

            return shakeController;
        }
    }
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        blackBg.OnShow(1f);
        
        tapToOpen.localScale = Vector3.zero;
        tapToCollect.localScale = Vector3.zero;
        
        packRoot.gameObject.SetActive(false);
        // packRoot.position = new Vector3(0f, 0.14f);
        packRoot.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        _packType = Convert.ToInt32(userData);
        packSpine.Skeleton.SetSkin($"cardbag{_packType}");
        packSpine.Skeleton.SetToSetupPose();
        packSpine.AnimationState.TimeScale = 0;
        packSpine.AnimationState.ClearTracks();
        packSpine.AnimationState.SetAnimation(0, "idle", true);
        packSpine.Update(0);
        
        openEffect.gameObject.SetActive(false);
        fastOpenEffect.gameObject.SetActive(false);
        
        cardArea.gameObject.SetActive(false);
        OpenCardPack();
        
        entrance.gameObject.SetActive(false);
        entrance.OnInit(null);
        
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        
        entrance.OnRelease();
        
        _newCards.Clear();
        foreach (var cardItem in _cardItemList)
        {
            cardItem.Release();
        }
        _cardItemList.Clear();
        foreach (var assetHandle in _assetHandleList)
        {
            UnityUtility.UnloadAssetAsync(assetHandle);
        }
        _assetHandleList.Clear();
    }

    public void SetCompleteAction(Action action)
    {
        _completeAction = action;
    }
    
    /// <summary>
    /// 开卡预生成
    /// </summary>
    private void OpenCardPack()
    {
        List<CardInfo> cardInfoList = CardUtil.OpenCardPack(_packType);
        GameManager.Event.Fire(this, CardChangeEventArgs.Create());
        _cardPosList = GetCardPosition(cardInfoList.Count);
        
        foreach (var card in cardInfoList)
        {
            AsyncOperationHandle assetHandle = UnityUtility.InstantiateAsync(
                $"SpineCardItem{CardModel.Instance.CardActivityID}", cardArea, asset =>
                {
                    SpineCardItem cardItem = asset.GetComponent<SpineCardItem>();
                    cardItem.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    cardItem.transform.localPosition = new Vector3(0, 20f);

                    cardItem.Init(card);

                    _cardItemList.Add(cardItem);
                });
            _assetHandleList.Add(assetHandle);
        }
    }
    
    public void ShowPackAnim()
    {
        packRoot.gameObject.SetActive(true);
        packSpine.AnimationState.TimeScale = 1;
        OnTapToOpen();
        
        // tapToOpen.transform.DOScale(1.1f, 0.2f).onComplete = () =>
        // {
        //     tapToOpen.transform.DOScale(1f, 0.2f);
        //     blackBg.clickButton.SetBtnEvent(OnTapToOpen);
            
            //处理点击
            if (RewardManager.Instance.RewardPanel != null)
                RewardManager.Instance.RewardPanel.SetClearBgActive(false);
            UIForm globalMask = GameManager.UI.GetUIForm("GlobalMaskPanel");
            if (globalMask != null)
            {
                GameManager.UI.HideUIForm(globalMask);
                _needGlobalMask = true;
            }
        // };
    }

    public void ShowPackAnimForTaskFlyReward()
    {
        // blackBg.OnShow();
        packRoot.localPosition = Vector3.zero;
        packRoot.localScale = new Vector3(0.5f, 0.5f);
        packRoot.gameObject.SetActive(true);
        packRoot.DOScale(1.6f, 0.5f).onComplete = () =>
        {
            packRoot.DOScale(1.5f, 0.2f);
        
            packSpine.AnimationState.TimeScale = 1;
            OnTapToOpen();
            
            // tapToOpen.transform.DOScale(1.1f, 0.2f).onComplete = () =>
            // {
            //     tapToOpen.transform.DOScale(1f, 0.2f);
            //     blackBg.clickButton.SetBtnEvent(OnTapToOpen);
                
                //处理点击
                if (RewardManager.Instance.RewardPanel != null)
                    RewardManager.Instance.RewardPanel.SetClearBgActive(false);
                UIForm globalMask = GameManager.UI.GetUIForm("GlobalMaskPanel");
                if (globalMask != null)
                {
                    GameManager.UI.HideUIForm(globalMask);
                    _needGlobalMask = true;
                }
            // };
        };
    }

    
    private void OnTapToOpen()
    {
        // Debug.LogError("");
        //_cardPosList = GetCardPosition(_cardItemList.Count);
        
        blackBg.clickButton.onClick.RemoveAllListeners();
        // 快速开卡包
        // blackBg.clickButton.SetBtnEvent(OnTapToFastOpen);
        
        tapToOpen.DOScale(0, 0.2f);
        // root.DOScale(1.4f, 0.2f);
        // root.DOMoveY(-0.5f, 0.2f).onComplete = () =>
        {
            // GameManager.Sound.PlayAudio("Card_Collection_Tear_Card_Pack");
            //UnityUtil.EVibatorType.Short.PlayerVibrator();
            
            GameManager.Sound.PlayAudio("Card_Collection_Quickly_Open_Card_Pack");
            packSpine.AnimationState.SetAnimation(0, "open", false);
            StartCoroutine(NewOpenCardCoroutine());
        };
    }

    /// <summary>
    /// 开卡动画
    /// </summary>
    //已弃用
    private void OpenCardAnim()
    {
        cardArea.gameObject.SetActive(true);
        //List<Vector3> cardPosList = GetCardPosition(_cardItemList.Count);

        float delayTime = -0.2f;
        for (int i = 0; i < _cardItemList.Count; i++)
        {
            int index = i;
            delayTime += 0.2f;
            GameManager.Task.AddDelayTriggerTask(delayTime,
                () => { GameManager.Sound.PlayAudio("Card_Collection_Distribute_Cards"); });
            _cardItemList[i].transform.DOScale(_cardItemList.Count == 6 ? 0.9f : 1f, 0.5f).SetDelay(delayTime);
            _cardItemList[i].transform.DOLocalMove(_cardPosList[i], 0.5f).SetDelay(delayTime).onComplete += () =>
            {
                if (_cardItemList[index].IsNew) _newCards.Add(_cardItemList[index]);
                // 非新卡直接翻转
                else _cardItemList[index].FlipCard();

                if (index == _cardItemList.Count - 1)
                {
                    // 新卡最后翻转
                    GameManager.Task.AddDelayTriggerTask(_cardItemList[index].IsNew ? 0.2f : 0.6f, () =>
                    {
                        if (_newCards.Count > 0) GameManager.Sound.PlayAudio("Card_Collection_Flip_Cards");
                        foreach (var card in _newCards)
                            card.FlipCard();
                    });

                    //没有新卡，延迟0.6
                    //有新卡，最后一张是新卡，延迟0.8
                    //有新卡，最后一张不是新卡，延迟1.2
                    tapToCollect.transform.DOScale(1.1f, 0.2f)
                        .SetDelay(_newCards.Count == 0 || _cardItemList[index].IsNew ? 0.8f : 1.2f).onComplete += () =>
                    {
                        tapToCollect.transform.DOScale(1f, 0.2f);
                        blackBg.clickButton.SetBtnEvent(OnTapToCollect);
                    };
                }
            };
        }
    }
    //已弃用
    IEnumerator OpenCardCoroutine()
    {
        yield return null;
        cardArea.gameObject.SetActive(true);

        for (int i = 0; i < _cardItemList.Count; i++)
        {
            SpineCardItem cardItem = _cardItemList[i];
            GameManager.Sound.PlayAudio("Card_Collection_Distribute_Cards");
            cardItem.transform.DOScale(1f, 0.5f);
            cardItem.transform.DOLocalMove(_cardPosList[i], 0.5f).onComplete += () =>
            {
                if (cardItem.IsNew) _newCards.Add(cardItem);
                // 非新卡翻转
                else cardItem.FlipCard();
            };
            
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(0.5f);

        // 有新卡
        if (_newCards.Count > 0)
        {
            if (!_cardItemList[^1].IsNew)
                yield return new WaitForSeconds(0.2f);
                
            // 新卡翻转
            GameManager.Sound.PlayAudio("Card_Collection_Flip_Cards");
            foreach (var card in _newCards) card.FlipCard();
            yield return new WaitForSeconds(0.6f);  //新卡翻转需要0.64s
        }
        else
        {
            yield return new WaitForSeconds(0.2f);  //非新卡翻转需要0.4s
        }
        
        tapToCollect.transform.DOScale(1.1f, 0.2f).onComplete += () =>
        {
            tapToCollect.transform.DOScale(1f, 0.2f);
            blackBg.clickButton.SetBtnEvent(OnTapToCollect);
        };
    }
    //快速开卡，已弃用
    private void OnTapToFastOpen()
    {
        Debug.LogError("");
        blackBg.clickButton.onClick.RemoveAllListeners();
        StopAllCoroutines();

        GameManager.Sound.PlayAudio("Card_Collection_Quickly_Open_Card_Pack");
        fastOpenEffect.gameObject.SetActive(true);
        fastOpenEffect.Play();
        
        tapToOpen.DOKill();
        tapToOpen.localScale = Vector3.zero;
        
        packRoot.DOKill();
        packSpine.AnimationState.ClearTracks();
        packRoot.GetComponent<CanvasGroup>().DOFade(0, 0.15f).onComplete += () =>
        {
            packRoot.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            packRoot.position = new Vector3(0f, -0.5f, 0f);
            
            UnityUtil.EVibatorType.Medium.PlayerVibrator();
            
            packSpine.AnimationState.SetAnimation(0, "idle2", false);
            packSpine.Skeleton.SetToSetupPose();
            
            packRoot.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
        };

        foreach (var cardItem in _cardItemList)
        {
            if (cardItem.IsNew) _newCards.Add(cardItem);
            cardItem.transform.DOKill();
        }
        cardArea.GetComponent<CanvasGroup>().DOFade(0,  cardArea.gameObject.activeSelf? 0.15f : 0f);
        cardArea.DOScale(0.88f,  cardArea.gameObject.activeSelf? 0.15f : 0f);
        GameManager.Task.AddDelayTriggerTask(0.15f, () =>
        {
            // fastOpenEffect.gameObject.SetActive(true);
            // fastOpenEffect.Play();
            
            for (int i = 0; i < _cardItemList.Count; i++)
            {
                SpineCardItem cardItem = _cardItemList[i];
                cardItem.transform.localScale = Vector3.one;
                cardItem.transform.localPosition = _cardPosList[i];
                cardItem.SetToFinalState();
            }

            cardArea.gameObject.SetActive(true);
            cardArea.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
            cardArea.DOScale(1.05f, 0.2f).onComplete += () => cardArea.DOScale(1, 0.2f);

            tapToCollect.transform.DOScale(1.1f, 0.2f).SetDelay(0.3f).onComplete += () =>
            {
                tapToCollect.transform.DOScale(1f, 0.2f);
                blackBg.clickButton.SetBtnEvent(OnTapToCollect);
            };
        });
    }
    
    
    private void OnTapToCollect()
    {
        StopAllCoroutines();
        blackBg.clickButton.onClick.RemoveAllListeners();

        //UnityUtil.EVibatorType.Short.PlayerVibrator();
        tapToCollect.DOScale(0, 0.2f);
        //packRoot.DOMoveY(-0.45f, 0.2f).onComplete += () => { packRoot.DOMoveY(-1.5f, 0.5f); };
        CollectCardAnim();
    }

    /// <summary>
    /// 收卡动画
    /// </summary>
    private void CollectCardAnim()
    {
        entrance.OnOpen();

        bool isShowVibrator = false;
        // 转化成星星
        foreach (var card in _cardItemList)
        {
            if (!isShowVibrator)
            {
                if (!card.IsNew)
                {
                    isShowVibrator = true;
                }
            }

            card.ConvertToStar();
        }

        if (isShowVibrator)
        {
            UnityUtil.EVibatorType.Short.PlayerVibrator();
        }

        if (_newCards.Count != _cardItemList.Count)
        {
            GameManager.Sound.PlayAudio("Card_Collection_Break_Into_Stars");
        }

        GameManager.Task.AddDelayTriggerTask(_newCards.Count == _cardItemList.Count ? 0.2f : 0.6f, () =>
        {
            // 隐藏进度条
            foreach (var card in _cardItemList)
            {
                card.HideSlider();
            }

            float delayTime = _newCards.Count == 0 ? -0.2f : 0;
            for (int i = 0; i < _cardItemList.Count; i++)
            {
                int index = i;
                delayTime += 0.2f;
                _cardItemList[i].card.DOLocalRotate(new Vector3(0, 0, -30), 0.5f).SetEase(Ease.InCubic)
                    .SetDelay(delayTime);
                _cardItemList[i].transform.DOScale(0, 0.5f).SetEase(Ease.InCubic).SetDelay(delayTime);
                _cardItemList[i].transform.DOMove(entrance.transform.position, 0.5f).SetEase(Ease.InCubic)
                        .SetDelay(delayTime).onComplete +=
                    () =>
                    {
                        UnityUtil.EVibatorType.Short.PlayerVibrator();
                        entrance.OnPunch();
                        //出横条
                        if (Mathf.Approximately(_cardItemList[index].simpleSlider.CurrentNum, 9))
                            entrance.ShowStripOut();
                        
                        if (index == _cardItemList.Count - 1)
                        {
                            //收横条
                            entrance.ShowStripIn(() =>
                            {
                                OnRelease();
                                _completeAction?.InvokeSafely();
                                if (_needGlobalMask)
                                    GameManager.UI.ShowUIForm("GlobalMaskPanel");
                            });
                        }
                    };
            }
        });
    }

    private List<Vector3> GetCardPosition(int count)
    {
        List<Vector3> posList = new List<Vector3>();
        switch (count)
        {
            case 2:
                posList.Add(new Vector3(-190, 0));
                posList.Add(new Vector3(190, 0));
                break;
            case 3:
                posList.Add(new Vector3(-180, 235));
                posList.Add(new Vector3(180, 235));
                posList.Add(new Vector3(0, -235));
                break;
            case 4:
                posList.Add(new Vector3(-180, 235));
                posList.Add(new Vector3(180, 235));
                posList.Add(new Vector3(-180, -235));
                posList.Add(new Vector3(180, -235));
                break;
            case 6:
                posList.Add(new Vector3(-355, 235));
                posList.Add(new Vector3(0, 235));
                posList.Add(new Vector3(355, 235));
                posList.Add(new Vector3(-355, -235));
                posList.Add(new Vector3(0, -235));
                posList.Add(new Vector3(355, -235));
                break;
        }
        return posList;
    }

    private Vector3 CalculateRandomArcPoint(Vector3 pointA, Vector3 pointB)
    {
        // 1. 计算AB向量和半径
        Vector3 abVector = pointB - pointA;
        float radius = Random.Range(480, 500); //abVector.magnitude + 50f;
        float abAngle = Mathf.Atan2(abVector.y, abVector.x);
        
        // 2. 创建随机数生成器
        System.Random random = new System.Random();
        
        // 3. 随机选择左侧或右侧区域
        bool isLeft = random.Next(2) == 0; // 50%概率选择左侧
        
        // 4. 在选定区域内生成随机角度
        float minAngle = isLeft ? -20f : 10f;
        float maxAngle = isLeft ? -10f : 20f;
        float randomDegree = minAngle + (float)random.NextDouble() * (maxAngle - minAngle);
        
        // 5. 转换为弧度并计算点坐标
        float randomAngle = randomDegree * Mathf.Deg2Rad;
        float cX = pointA.x + radius * Mathf.Cos(abAngle + randomAngle);
        float cY = pointA.y + radius * Mathf.Sin(abAngle + randomAngle);
        
        return new Vector3(cX, cY);
    }

    /// <summary>
    /// 爆炸效果
    /// </summary>
    private void PlayExplosionEffect(float explosionDuration)
    {
        for (int i = 0; i < _cardItemList.Count; i++)
        {
            Transform cardItem = _cardItemList[i].transform;
            //放大
            cardItem.DOScale(1.1f, explosionDuration).SetEase(Ease.OutQuad);
            //旋转
            float angle = Random.Range(-15, 16);
            cardItem.DOLocalRotate(new Vector3(0, 0, angle), explosionDuration).SetEase(Ease.OutQuad);
            //移动
            Vector3 endPoint = _cardPosList[i];
            Vector3 midPoint = CalculateRandomArcPoint(new Vector3(0, 20), endPoint);
            cardItem.DOLocalMove(midPoint, explosionDuration).SetEase(Ease.OutQuad);

            //UnityUtil.EVibatorType.Short.PlayerVibrator();
        }
    }

    /// <summary>
    /// 回归效果
    /// </summary>
    private float PlayReturnEffect(float returnDuration)
    {
        float flipDelayTime = 0f;
        for (int i = 0; i < _cardItemList.Count; i++)
        {
            int index = i;
            SpineCardItem cardItem = _cardItemList[index];
            //缩小
            cardItem.transform.DOScale(1f, returnDuration).SetEase(Ease.InOutQuad);//.SetDelay(i * 0.1f);
            //旋转
            cardItem.transform.DOLocalRotate(Vector3.zero, returnDuration).SetEase(Ease.InOutQuad);//.SetDelay(i * 0.1f);
            //移动
            Vector3 endPoint = _cardPosList[index];
            cardItem.transform.DOLocalMove(endPoint, returnDuration).SetEase(Ease.InOutQuad);//.onComplete += () =>
            
            //新卡记录
            if (cardItem.IsNew) _newCards.Add(cardItem);
            //非新卡翻转
            else
            {
                flipDelayTime += 0.2f;
                GameManager.Task.AddDelayTriggerTask(returnDuration + flipDelayTime, () => cardItem.FlipCard());
            }
        }

        return flipDelayTime;
    }
    
    IEnumerator NewOpenCardCoroutine()
    {
        //屏幕抖动
        yield return new WaitForSeconds(0.2f);
        UnityUtil.EVibatorType.Medium.PlayerVibrator();
        ShakeController.StartShake(20, 0.5f, 1f);
        
        //特效
        yield return new WaitForSeconds(0.3f);
        openEffect.gameObject.SetActive(true);
        openEffect.Play();
        
        float explosionDuration = 0.2f;
        float returnDuration = 0.4f;
        cardArea.gameObject.SetActive(true);
        PlayExplosionEffect(explosionDuration);
        
        yield return new WaitForSeconds(explosionDuration + 0.1f);
        
        float flipDelayTime = PlayReturnEffect(returnDuration);

        yield return new WaitForSeconds(returnDuration + flipDelayTime + 0.4f);  //非新卡翻转需要0.4s

        //有新卡
        if (_newCards.Count > 0)
        {   
            //如果不全是新卡，中间停顿一下
            if (_newCards.Count != _cardItemList.Count)
                yield return new WaitForSeconds(0.06f);
            
            //新卡翻转
            GameManager.Sound.PlayAudio("Card_Collection_Flip_Cards");
            bool playVibrator = true;
            foreach (var card in _newCards)
            {
                bool isNew = card.IsNew;
                card.FlipCard(playVibrator);
                if (isNew) playVibrator = false;
            }
            yield return new WaitForSeconds(0.8f);  //新卡翻转需要0.64s --> 0.76s
            // ShakeController.StartShake(20, 0.5f, 0.1f);
        }
        
        tapToCollect.transform.DOScale(1.1f, 0.2f).onComplete += () =>
        {
            tapToCollect.transform.DOScale(1f, 0.2f);
            blackBg.clickButton.SetBtnEvent(OnTapToCollect);
        };
    }
}
