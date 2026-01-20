using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class FakeEntrance : UIForm
{
    public CountdownTimer countdownTimer;
    public Transform mainImageTransform;
    public SkeletonGraphic book;
    public ParticleSystem entranceEffect;
    public Transform stripTransform;
    public Image cardSetCover, cardSetBanner;
    // public TextMeshProUGUILocalize cardSetName;
    
    private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>();
    private bool _isStripOut = false;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        countdownTimer.OnReset();
        countdownTimer.StartCountdown(CardModel.Instance.CardEndTime);

        book.AnimationState.ClearTracks();
        book.AnimationState.SetAnimation(0, "open", false).Complete += entry =>
        {
            book.AnimationState.SetAnimation(0, "open_idle", true);
        };
        book.AnimationState.TimeScale = 0;
        book.Update(0);
        
        int newCompleteCardSet = CardModel.Instance.NewCompletedCardSet;
        if (newCompleteCardSet > 0)
        {
            string coverStr = $"Card.{CardModel.Instance.CardActivityID}_{newCompleteCardSet}";
            _assetHandleList.Add(
                UnityUtility.LoadAssetAsync<Sprite>(coverStr, asset => { cardSetCover.sprite = asset as Sprite; }));
        
            string bannerStr = $"CommonAtlas{CardModel.Instance.CardActivityID}[条幅{newCompleteCardSet % 5}]";
            _assetHandleList.Add(
                UnityUtility.LoadAssetAsync<Sprite>(bannerStr, asset => { cardSetBanner.sprite = asset as Sprite; }));

            // cardSetName.SetTerm(coverStr);
        }

        stripTransform.localPosition = new Vector3(-400, 0, 0);
        
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnRelease()
    {
        countdownTimer.OnReset();
        
        foreach (var assetHandle in _assetHandleList)
        {
            UnityUtility.UnloadAssetAsync(assetHandle);
        }
        _assetHandleList.Clear();
        
        base.OnRelease();
    }

    public void OnOpen()
    {
        GetComponent<CanvasGroup>().alpha = 0;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        transform.DOScale(1.1f, 0.2f).onComplete += () =>
        {
            transform.DOScale(1f, 0.1f);
        };
        GetComponent<CanvasGroup>().DOFade(1, 0.3f).onComplete += () =>
        {
            book.AnimationState.TimeScale = 1;
        };
    }

    public void OnPunch()
    {
        GameManager.Sound.PlayAudio("Card_Collection_Recycle_Cards");
        mainImageTransform.DOScale(0.7f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
        {
            mainImageTransform.DOScale(0.8f, 0.15f);
        };
        entranceEffect.Play();
    }

    public void ShowStripOut()
    {
        if (_isStripOut)
            return;
        
        if (CardModel.Instance.NewCompletedCardSet > 0)
        {
            _isStripOut = true;
            stripTransform.DOScaleY(0.7f, 0.2f).onComplete += () =>
            {
                UnityUtil.EVibatorType.Medium.PlayerVibrator();
                GameManager.Sound.PlayAudio("Card_Collection_Set_Completed_Shrink_Tips");
                stripTransform.DOScaleY(1f, 0.1f);
            };
            stripTransform.DOLocalMoveX(0f, 0.2f).onComplete = () => { stripTransform.DOLocalMoveX(-90f, 0.1f); };
        }
    }

    public void ShowStripIn(Action action)
    {
        if (_isStripOut)
        {
            _isStripOut = false;
            CardModel.Instance.NewCompletedCardSet = 0;
            
            //收横条关书
            GameManager.Task.AddDelayTriggerTask(1f, () =>
            {
                stripTransform.DOScaleY(0.7f, 0.2f).onComplete += () => { stripTransform.DOScaleY(1f, 0.2f); };
                stripTransform.DOLocalMoveX(0f, 0.2f).onComplete += () =>
                {
                    stripTransform.DOLocalMoveX(-400f, 0.2f).onComplete += () =>
                    {
                        book.AnimationState.SetAnimation(0, "close", false).Complete += entry =>
                        {
                            transform.DOScale(0, 0.3f);
                            GetComponent<CanvasGroup>().DOFade(0f, 0.3f).onComplete +=()=>
                            {
                                gameObject.SetActive(false);
                                action?.InvokeSafely();
                            };
                        };
                    };
                };
            });
        }
        else
        {
            book.AnimationState.SetAnimation(0, "close", false).Complete += entry =>
            {
                transform.DOScale(0, 0.3f);
                GetComponent<CanvasGroup>().DOFade(0f, 0.3f).onComplete +=()=>
                {
                    gameObject.SetActive(false);
                    action?.InvokeSafely();
                };
            };
        }
    }
}
