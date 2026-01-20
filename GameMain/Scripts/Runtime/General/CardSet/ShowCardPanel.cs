using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShowCardPanel : UIForm
{
    public Image blackBg;
    public CardItem cardItem;
    public GameObject notOwnDes;
    public Transform textArea;
    public DelayButton delayButton;
    public TextMeshProUGUILocalize cardName;

    private CardInfo _cardInfo;
    private Vector3 _startPos;
    // private Vector3 _endPos;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        var data = userData.ChangeType(new
        {
            cardInfo = new CardInfo(),
            startPos = Vector3.zero
        });
        _cardInfo = data.cardInfo;
        _startPos = data.startPos;

        blackBg.DOFade(0, 0);
        cardItem.Init(_cardInfo);
        cardItem.spineLight.enabled = false;
        cardItem.transform.position = _startPos;
        cardItem.notOwn.transform.localScale = Vector3.one;
        cardItem.own.transform.localScale = Vector3.one;
        cardName.SetTerm(cardItem.cardName.Term);
        notOwnDes.SetActive(cardItem.notOwn.activeSelf);
        
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        blackBg.DOFade(1, 0.2f);

        cardItem.transform.DOMove(new Vector3(0, 0.18f, 0), 0.2f);
        cardItem.notOwn.transform.DOScale(1.68f, 0.2f).onComplete += () =>
        {
            cardItem.notOwn.transform.DOScale(1.5f, 0.1f);
        };
        cardItem.own.transform.DOScale(2.18f, 0.2f).onComplete += () =>
        {
            cardItem.own.transform.DOScale(1.95f, 0.1f);
        };
        
        textArea.DOLocalMoveY(60, 0.2f).onComplete += () =>
        {
            textArea.DOLocalMoveY(0, 0.1f).onComplete += () =>
            {
                delayButton.OnInit(OnButtonClick);
            };
        };
        
        base.OnShow(showSuccessAction, userData);
    }

    private void OnButtonClick()
    {
        _cardInfo.RemoveNew();
        blackBg.DOFade(0, 0.2f);
        cardItem.transform.DOMove(_startPos, 0.2f);
        cardItem.notOwn.transform.DOScale(1, 0.2f);
        cardItem.own.transform.DOScale(1, 0.2f);
        textArea.DOLocalMoveY(-1100, 0.2f).onComplete += () =>
        {
            GameManager.UI.HideUIForm(this);
        };
    }

    public override void OnRelease()
    {
        cardItem.Release();
        delayButton.OnReset();
        base.OnRelease();
    }
}
