using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using MySelf.Model;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SpineCardItem : MonoBehaviour
{
    public Transform card, star;
    public GameObject oStars4, oStars5, frame, goldFrame;
    public GameObject[] ownStars;
    public TextMeshProUGUILocalize cardName;
    public RawImage cardImage;
    public Image bannerImage;
    public GameObject newTag;
    public TextMeshProUGUI starNum;
    public SimpleSlider simpleSlider;
    
    public SkeletonGraphic spine;
    public UIParticle bgEffect, cardEffect, starEffect;
    
    private CardInfo _cardInfo;
    private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>();

    private const float CardOriginalScale = 0.65f;

    public bool IsNew { get; private set; }

    public void Init(CardInfo cardInfo)
    {
        _cardInfo = cardInfo;
        
        if (cardInfo.CardStar < 5)
        {
            for (int i = 0; i < 4; i++)
            {
                ownStars[i].SetActive(i < cardInfo.CardStar);
            }
            oStars4.SetActive(true);
            oStars5.SetActive(false);
            frame.SetActive(true);
            goldFrame.SetActive(false);
        }
        else
        {
            oStars4.SetActive(false);
            oStars5.SetActive(true);
            frame.SetActive(false);
            goldFrame.SetActive(true);
        }

        string cardStr = $"Card.{CardModel.Instance.CardActivityID}_{cardInfo.CardID}";
        cardName.SetTerm(cardStr);
        _assetHandleList.Add(UnityUtility.LoadAssetAsync<Texture>(cardStr,
            asset => { cardImage.texture = asset as Texture; }));
        
        string bannerStr = $"CardAtlas{CardModel.Instance.CardActivityID}[颜色{cardInfo.CardSetID % 5}]";
        _assetHandleList.Add(UnityUtility.LoadAssetAsync<Sprite>(bannerStr,
            asset => { bannerImage.sprite = asset as Sprite; }));

        IsNew = cardInfo.isNewCollect;
        cardInfo.isNewCollect = false;
        newTag.SetActive(IsNew);
        simpleSlider.GetComponent<CanvasGroup>().alpha = 0;
        simpleSlider.OnReset();
        if (IsNew)
        {
            simpleSlider.TotalNum = 9;
            simpleSlider.CurrentNum = CardModel.Instance.CollectCardDict[cardInfo.CardSetID].Count;
        }

        spine.AnimationState.TimeScale = 0;
        spine.AnimationState.ClearTracks();
        spine.AnimationState.SetAnimation(0, "cilck", false);
        spine.Update(0);
        
        bgEffect.gameObject.SetActive(false);
        cardEffect.gameObject.SetActive(false);
        starEffect.gameObject.SetActive(false);
        
        star.localScale = Vector3.zero;
        starNum.SetText("+" + cardInfo.CardStar);
    }
    
    public void Release()
    {
        _cardInfo = null;
        foreach (var assetHandle in _assetHandleList)
        {
            UnityUtility.UnloadAssetAsync(assetHandle);
        }
        _assetHandleList.Clear();
    }

    public void FlipCard(bool playVibrator = true)
    {
        if (!IsNew)
        {
            GameManager.Sound.PlayAudio("Card_Collection_Flip_Cards");
            spine.AnimationState.TimeScale = 1;
        }
        else
        {
            // 缩小 -- 转（先缩小再放大）-- 停顿 -- 放大砸下去弹回来
            Sequence sequence = DOTween.Sequence();
            sequence.Append(card.DOScale(0.96f * CardOriginalScale, 0.04f))
                .AppendCallback(() => spine.AnimationState.TimeScale = 1.1f)
                .Append(card.DOScale(0.92f * CardOriginalScale, 0.04f))
                .Append(card.DOScale(1.18f * CardOriginalScale, 0.47f))
                .AppendInterval(0.1f)
                .Append(card.DOScale(1.3f * CardOriginalScale, 0.08f)).SetEase(Ease.OutQuad)
                .AppendCallback(() =>
                {
                    if(playVibrator)
                        UnityUtil.EVibatorType.Medium.PlayerVibrator();
                    GameManager.Sound.PlayAudio("Card_Collection_Show_New_Cards");
                    cardEffect.gameObject.SetActive(true);
                    cardEffect.Play();
                })
                .Append(card.DOScale(0.9f * CardOriginalScale, 0.03f)).SetEase(Ease.OutQuad)
                .Append(card.DOScale(CardOriginalScale, 0.06f)).SetEase(Ease.OutQuad)
                .Join(simpleSlider.GetComponent<CanvasGroup>().DOFade(1, 0.2f))
                .AppendCallback(() =>
                {
                    bgEffect.GetComponent<CanvasGroup>().alpha = 0;
                    bgEffect.gameObject.SetActive(true);
                    bgEffect.Play();
                    bgEffect.GetComponent<CanvasGroup>().DOFade(1, 0.7f);
                });

            // card.DOScale(1.3f * scale, 4f / 30).SetEase(Ease.InOutCubic).SetDelay(3f / 30)
            //     .onComplete += () =>
            // {
            //     cardEffect.gameObject.SetActive(true);
            //     cardEffect.Play();
            //     
            //     card.DOScale(scale, 7f / 30).SetEase(Ease.InOutCubic).SetDelay(7f / 30).onComplete += () =>
            //     {
            //         GameManager.Task.AddDelayTriggerTask(0f, () =>
            //         {
            //             bgEffect.gameObject.SetActive(true);
            //             bgEffect.Play();
            //         });
            //     };
            // };
        }
    }

    public void SetToFinalState()
    {
        spine.AnimationState.ClearTracks();
        spine.AnimationState.SetAnimation(0, "idle", false);
        spine.Skeleton.SetToSetupPose();

        card.DOKill();
        card.localScale = Vector3.one * CardOriginalScale;
        
        simpleSlider.GetComponent<CanvasGroup>().alpha = IsNew ? 1 : 0;
        
        cardEffect.gameObject.SetActive(false);
        bgEffect.gameObject.SetActive(false);
    }
    
    public void ConvertToStar()
    {
        if (IsNew) return;
        
        // CardModel.Instance.ExtraStarNum += _cardInfo.CardStar;
        card.DOScale(0, 0.2f).onComplete += () =>
        {
            star.gameObject.SetActive(true);
            star.DOScale(1, 0.2f);
            
            starEffect.gameObject.SetActive(true);
            starEffect.Play();
        };
    }

    public void HideSlider()
    {
        if (!IsNew) return;
        simpleSlider.GetComponent<CanvasGroup>().DOFade(0, 0.2f);
    }
}
