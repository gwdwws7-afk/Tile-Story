using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using MySelf.Model;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CardSetItem : MonoBehaviour
{
    public Image cover, banner;
    public TextMeshProUGUILocalize setName;
    public GameObject newTag;
    public SimpleSlider simpleSlider;
    public GameObject tick, completeText;
    public UIParticle effect1, effect2;
    public Transform claimButton;
    public Button button;
    public DelayButton claimBtn;
    public CanvasGroup canvas;

    private CardSet _cardSet;
    private int _index = 0;
    private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>();
    private Sequence _sequence;
    
    public void Init(CardSet cardSet)
    {
        canvas.alpha = 0;
        
        _cardSet = cardSet;
        string coverStr = $"Card.{cardSet.ActivityID}_{cardSet.CardSetID}";
        _index++;
        _assetHandleList.Add(
            UnityUtility.LoadAssetAsync<Sprite>(coverStr, asset =>
            {
                cover.sprite = asset as Sprite; 
                CheckAssetHandleDone();
            }));
        
        string bannerStr = $"CommonAtlas{cardSet.ActivityID}[条幅{cardSet.CardSetID % 5}]";
        _index++;
        _assetHandleList.Add(
            UnityUtility.LoadAssetAsync<Sprite>(bannerStr, asset =>
            {
                banner.sprite = asset as Sprite; 
                CheckAssetHandleDone();
            }));
        
        setName.SetTerm($"Card.{cardSet.ActivityID}_{cardSet.CardSetID}");
        
        simpleSlider.OnReset();
        simpleSlider.TotalNum = cardSet.CardDict.Count;
        
        Refresh(true);

        button.SetBtnEvent(() => OnButtonClick(cardSet,false));
        claimBtn.SetBtnEvent(() => OnButtonClick(cardSet,true));
    }

    private void CheckAssetHandleDone()
    {
        // _index--;
        // if (_index <= 0)
        // {
        //     canvas.alpha = 1;
        // }
    }
    
    public void Refresh(bool isDelay = false, float delayTime = 0f)
    {
        //领完了
        if (CardModel.Instance.CompletedCardSets.Contains(_cardSet.CardSetID))
        {
            newTag.SetActive(false);
            simpleSlider.gameObject.SetActive(false);
            tick.SetActive(true);
            completeText.SetActive(true);
            claimButton.localScale = Vector3.zero;
            effect1.gameObject.SetActive(false);
            effect2.gameObject.SetActive(false);
        }
        else
        {
            CardModel.Instance.NewCardDict.TryGetValue(_cardSet.CardSetID, out var cards);
            newTag.SetActive(cards is { Count: > 0 });
        
            simpleSlider.CurrentNum = _cardSet.CardSetCollectNum();
            simpleSlider.gameObject.SetActive(true);

            tick.SetActive(false);
            completeText.SetActive(false);
            
            if (!isDelay && Mathf.Approximately(simpleSlider.CurrentNum, simpleSlider.TotalNum))
            {
                if (claimButton.localScale == Vector3.zero)
                {
                    _sequence = DOTween.Sequence();
                    _sequence.AppendInterval(delayTime)
                        .Append(simpleSlider.transform.DOScale(1.1f, 0.2f))
                        .Append(simpleSlider.transform.DOScale(0f, 0.1f))
                        .Append(claimButton.DOScale(0.32f, 0.1f))
                        .AppendCallback(() =>
                        {
                            effect1.gameObject.SetActive(true);
                            GameManager.Sound.PlayAudio("Card_Collection_Set_Completed_Shrink_Tips");
                        })
                        .Append(claimButton.DOScale(0.35f, 0.05f))
                        .AppendCallback(() =>
                        {
                            effect2.gameObject.SetActive(true);
                            UnityUtil.EVibatorType.Medium.PlayerVibrator();
                        })
                        .Append(claimButton.DOScale(0.32f, 0.1f));
                }
                else
                {
                    simpleSlider.transform.localScale = Vector3.zero;
                    claimButton.localScale = Vector3.one * 0.32f;
                    effect1.gameObject.SetActive(false);
                    effect2.gameObject.SetActive(true);
                }
            }
            else
            {
                simpleSlider.transform.localScale = Vector3.one;
                claimButton.localScale = Vector3.zero;
                effect1.gameObject.SetActive(false);
                effect2.gameObject.SetActive(false);
            }
        }
    }
    
    public void Release()
    {
        canvas.alpha = 0;
        simpleSlider.OnReset();
        button.onClick.RemoveAllListeners();
        claimBtn.onClick.RemoveAllListeners();

        _cardSet = null;
        foreach (var assetHandle in _assetHandleList)
        {
            UnityUtility.UnloadAssetAsync(assetHandle);
        }
        _assetHandleList.Clear();

        _sequence?.Kill();
        _sequence = null;
        simpleSlider.transform.localScale = Vector3.one;
        claimButton.localScale = Vector3.zero;
    }

    private void OnButtonClick(CardSet cardSet,bool isClaim)
    {
        if (isClaim)
        {
            UnityUtil.EVibatorType.Medium.PlayerVibrator();
        }
        else
        {
            UnityUtil.EVibatorType.Short.PlayerVibrator();
        }

        GameManager.Sound.PlaySound("SFX_UI_open", "UISound");
        GameManager.UI.ShowUIForm($"CardSetBookMenu{CardModel.Instance.CardActivityID}", userData: cardSet.CardSetID);
    }
}
