using System.Collections;
using System.Collections.Generic;
using MySelf.Model;
using Spine.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CardItem : MonoBehaviour
{
    public GameObject notOwn, own;
    public GameObject noStars4, oStars4, noStars5, oStars5, frame, goldFrame;
    public GameObject[] notOwnStars;
    public GameObject[] ownStars;
    public TextMeshProUGUILocalize cardName;
    public RawImage cardImage;
    public Image bannerImage;
    public GameObject newTag;
    public SkeletonGraphic spineLight;
    public Button button;

    private CardInfo _cardInfo;
    private bool _isCollected, _isNew;
    private List<AsyncOperationHandle> _assetHandleList = new List<AsyncOperationHandle>();

    public void Init(CardInfo cardInfo)
    {
        _cardInfo = cardInfo;
        string cardStr = $"Card.{CardModel.Instance.CardActivityID}_{cardInfo.CardID}";
        cardName.SetTerm(cardStr);
        
        button.onClick.AddListener(OnButtonClick);

        if (cardInfo.CardStar < 5)
        {
            for (int i = 0; i < 4; i++)
            {
                notOwnStars[i].SetActive(i < cardInfo.CardStar);
                ownStars[i].SetActive(i < cardInfo.CardStar);
            }
            noStars4.SetActive(true);
            oStars4.SetActive(true);
            noStars5.SetActive(false);
            oStars5.SetActive(false);
            frame.SetActive(true);
            goldFrame.SetActive(false);
        }
        else
        {
            noStars4.SetActive(false);
            oStars4.SetActive(false);
            noStars5.SetActive(true);
            oStars5.SetActive(true);
            frame.SetActive(false);
            goldFrame.SetActive(true);
        }

        _isCollected = cardInfo.IsCollected();
        _isNew = cardInfo.IsNew();
        notOwn.SetActive(!_isCollected);
        own.SetActive(_isCollected);
        if (_isCollected)
        {
            _assetHandleList.Add(UnityUtility.LoadAssetAsync<Texture>(cardStr,
                asset => { cardImage.texture = asset as Texture; }));
            
            string bannerStr = $"CardAtlas{CardModel.Instance.CardActivityID}[颜色{cardInfo.CardSetID % 5}]";
            _assetHandleList.Add(UnityUtility.LoadAssetAsync<Sprite>(bannerStr,
                asset => { bannerImage.sprite = asset as Sprite; }));
            
            newTag.SetActive(_isNew);
            spineLight.enabled = _isNew;
            spineLight.AnimationState.TimeScale = 0;
        }
    }

    public void Release()
    {
        _cardInfo = null;
        foreach (var assetHandle in _assetHandleList)
        {
            UnityUtility.UnloadAssetAsync(assetHandle);
        }
        _assetHandleList.Clear();
        
        button.onClick.RemoveAllListeners();
    }
    
    private void OnButtonClick()
    {
        GameManager.Sound.PlaySound("SFX_UI_open", "UISound");

        GameManager.UI.ShowUIForm($"ShowCardPanel{CardModel.Instance.CardActivityID}", form =>
            {
                notOwn.SetActive(true);
                own.SetActive(false);
                newTag.SetActive(false);
                spineLight.enabled = false;

                form.SetHideAction(() =>
                {
                    notOwn.SetActive(!_isCollected);
                    own.SetActive(_isCollected);
                });
            },
            userData: new
            {
                cardInfo = _cardInfo,
                startPos = transform.position
            }
        );
    }
}
