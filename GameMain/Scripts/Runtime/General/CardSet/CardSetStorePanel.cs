using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using GameFramework.Event;
using MySelf.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardSetStorePanel : UIForm
{
    public DelayButton closeButton;
    public TextMeshProUGUILocalize starNum;
    public ItemPromptBox itemPromptBox;
    public Button chest1, chest2, chest3;
    public DelayButton starBtn1, starBtn2, starBtn3;
    public TextMeshProUGUILocalize starText1, starText2, starText3;
    public DelayButton coinBtn;
    public TextMeshProUGUILocalize coinText, limitText, refreshText;
    public CountdownTimer countdownTimer;
    
    private CardStarReward _reward1, _reward2, _reward3;
    private const int CoinNum = 3000;
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        closeButton.OnInit(() => GameManager.UI.HideUIForm(this));
        
        DTCardStarRewardData cardStarRewardData = GameManager.DataTable.GetDataTable<DTCardStarRewardData>().Data;
        _reward1 = cardStarRewardData.GetRewardByID(1);
        _reward2 = cardStarRewardData.GetRewardByID(2);
        _reward3 = cardStarRewardData.GetRewardByID(3);
        
        Refresh();
        RefreshCoinToPackBar();
        coinText.SetTerm(CoinNum.ToString());
        coinBtn.OnInit(OnCoinButtonClick);
        GameManager.Event.Subscribe(CoinNumChangeEventArgs.EventId, OnCoinNumChange);
        
        chest1.onClick.AddListener(() => OnChestButtonClick(_reward1, chest1.transform.position));
        chest2.onClick.AddListener(() => OnChestButtonClick(_reward2, chest2.transform.position));
        chest3.onClick.AddListener(() => OnChestButtonClick(_reward3, chest3.transform.position));

        starBtn1.OnInit(() => OnStarButtonClick(starBtn1.transform, _reward1));
        starBtn2.OnInit(() => OnStarButtonClick(starBtn2.transform, _reward2));
        starBtn3.OnInit(() => OnStarButtonClick(starBtn3.transform, _reward3));
        
        starText1.SetTerm(_reward1.StarNum.ToString());
        starText2.SetTerm(_reward2.StarNum.ToString());
        starText3.SetTerm(_reward3.StarNum.ToString());
        
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        Transform cachedTransform = transform.Find("Root");
        if (cachedTransform)
        {
            cachedTransform.DOScale(1.03f, 0.12f).onComplete = () =>
            {
                cachedTransform.DOScale(0.99f, 0.1f).onComplete = () =>
                {
                    cachedTransform.DOScale(1f, 0.1f);
                    m_IsAvailable = true;
                };
            };
        }
        gameObject.SetActive(true);

        GameManager.Sound.PlayUIOpenSound();
        
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
        refreshText.SetParameterValue("0", countdownTimer.timeText.text);
        
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                itemPromptBox.HidePromptBox();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                itemPromptBox.HidePromptBox();
            }
        }
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CoinNumChangeEventArgs.EventId, OnCoinNumChange);
        closeButton.OnReset();
        coinBtn.OnReset();
        countdownTimer.OnReset();
        itemPromptBox.OnRelease();
        chest1.onClick.RemoveAllListeners();
        chest2.onClick.RemoveAllListeners();
        chest3.onClick.RemoveAllListeners();
        starBtn1.OnReset();
        starBtn2.OnReset();
        starBtn3.OnReset();
        base.OnRelease();
    }

    private void Refresh()
    {
        int extraStarNum = CardModel.Instance.ExtraStarNum;
        starNum.SetParameterValue("0", extraStarNum.ToString());

        starBtn1.GetComponentInChildren<UIEffect>().enabled = extraStarNum < _reward1.StarNum;
        starBtn2.GetComponentInChildren<UIEffect>().enabled = extraStarNum < _reward2.StarNum;
        starBtn3.GetComponentInChildren<UIEffect>().enabled = extraStarNum < _reward3.StarNum;
        
        starText1.SetMaterialPreset(extraStarNum < _reward1.StarNum ? MaterialPresetName.Btn_Grey : MaterialPresetName.Btn_Green);
        starText2.SetMaterialPreset(extraStarNum < _reward2.StarNum ? MaterialPresetName.Btn_Grey : MaterialPresetName.Btn_Green);
        starText3.SetMaterialPreset(extraStarNum < _reward3.StarNum ? MaterialPresetName.Btn_Grey : MaterialPresetName.Btn_Green);
    }

    private void RefreshCoinToPackBar()
    {
        if (CardModel.Instance.CanUseCoinForPack)
        {
            coinBtn.GetComponentInChildren<UIEffect>().enabled = false;
            coinText.SetMaterialPreset(MaterialPresetName.Btn_Green);
            limitText.gameObject.SetActive(true);
            refreshText.gameObject.SetActive(false);
            countdownTimer.OnReset();
        }
        else
        {
            coinBtn.GetComponentInChildren<UIEffect>().enabled = true;
            coinText.SetMaterialPreset(MaterialPresetName.Btn_Grey);
            limitText.gameObject.SetActive(false);
            refreshText.gameObject.SetActive(true);
            
            countdownTimer.OnReset();
            countdownTimer.StartCountdown(DateTime.Today.AddDays(1));
            countdownTimer.CountdownOver += (sender, args) => RefreshCoinToPackBar();
        }
    }

    private void OnCoinNumChange(object sender, GameEventArgs args)
    {
        RefreshCoinToPackBar();
    }

    private void OnChestButtonClick(CardStarReward reward, Vector3 pos)
    {
        itemPromptBox.Init(reward.RewardTypeList, reward.RewardNumList);
        itemPromptBox.ShowPromptBox(PromptBoxShowDirection.Up, pos);
    }
    
    private void OnStarButtonClick(Transform button, CardStarReward reward)
    {
        if (CardModel.Instance.ExtraStarNum < reward.StarNum)
        {
            GameManager.UI.ShowWeakHint("Card.NotEnoughStars", new Vector3(0, button.position.y));
        }
        else
        {
            CardModel.Instance.ExtraStarNum -= reward.StarNum;
            CardUtil.RecordUseStars(reward.ID);
            
            RewardManager.Instance.AddNeedGetReward(reward.RewardTypeList, reward.RewardNumList);
            CardStarChestRewardPanel.ChestType = reward.ID;
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardStarChestRewardPanel, false, () =>
            {
                //开完卡包还要更新星星数
                Refresh();
                CardSetMainMenu mainMenu = GameManager.UI.GetUIForm($"CardSetMainMenu{CardModel.Instance.CardActivityID}") as CardSetMainMenu;
                mainMenu?.SetStarWarningSign();
            });
        }
    }

    private void OnCoinButtonClick()
    {
        if (!CardModel.Instance.CanUseCoinForPack)
        {
            GameManager.UI.ShowWeakHint("Card.Refreshing", new Vector3(0, coinBtn.transform.position.y));
            return;
        }

        if (GameManager.PlayerData.CoinNum < CoinNum)
        {
            GameManager.UI.ShowUIForm("ShopMenuManager");
            return;
        }

        CardUtil.RecordUseCoin();
        CardModel.Instance.CoinToPackTime = DateTime.UtcNow;
        GameManager.PlayerData.UseItem(TotalItemData.Coin, CoinNum);
        RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack5, 1);
        RewardManager.Instance.AutoGetRewardDelayTime = 0f;
        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true, () =>
        {
            RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
            Refresh();
            RefreshCoinToPackBar();
            CardSetMainMenu mainMenu = GameManager.UI.GetUIForm($"CardSetMainMenu{CardModel.Instance.CardActivityID}") as CardSetMainMenu;
            mainMenu?.SetStarWarningSign();
        });
    }
}
