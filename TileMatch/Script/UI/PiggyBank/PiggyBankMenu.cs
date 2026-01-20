using System;
using System.Linq;
using DG.Tweening;
using Firebase.Analytics;
using MySelf.Model;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PiggyBankMenu : PopupMenuForm
{
    [SerializeField] private DelayButton BackBtn,BuyBtn,GrayBtn;
    [SerializeField] private TextMeshProUGUI[] CoinTexts;
    [SerializeField] private GameObject GrayBtnObj,GreenBtnObj;
    [SerializeField] private Transform[] CoinStartAndEndTransforms;
    [SerializeField] private TextMeshProUGUILocalize GrayText,GreenText;
    [SerializeField] private GameObject DiscountObj;
    [SerializeField]private SkeletonGraphic SkeletonGraphic;
    [SerializeField] private Image SliderImage;

    [SerializeField] private Transform TopImage;
    [SerializeField] private TextMeshProUGUI TopCoinText;

    [SerializeField] private TextMeshProUGUILocalize PiggyBankText;

    [SerializeField] private Transform TopCoinTrans1,TopCoinTrans2;
    private int lastStatus = 0;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        int lastStatus =
            PiggyBankModel.Instance.GetBankStatusBySliderValue(PiggyBankModel.Instance.Data.RecordLastBankSliderValue);
        
        Init();
        ShowSliderAnim();
        if (lastStatus > 1)
            GameManager.Sound.PlayAudio("SFX_Bonus_Bank_Pop");
        SetBGImage(lastStatus);
        SetBtnEvent();
    }

    public override void OnRelease()
    {
        isAnim = false;
        SkeletonGraphic.AnimationState.ClearTracks();
        GameManager.Task.RemoveDelayTriggerTask(delayIndex);
        base.OnRelease();
    }

    private void Init()
    {
        bool isCanBuy = PiggyBankModel.Instance.IsCanBuy;
        bool isFull = PiggyBankModel.Instance.IsPiggyBankFull;
        GrayBtnObj.gameObject.SetActive(!isCanBuy);
        GreenBtnObj.gameObject.SetActive(isCanBuy);
        DiscountObj.gameObject.SetActive(isFull);
        
        var piggyBankDataByCurLevel = PiggyBankModel.Instance.PiggyBankDataByCurLevel;

        float xPos =
            (CoinStartAndEndTransforms[2].transform.localPosition.x -CoinStartAndEndTransforms[0].transform.localPosition.x) 
            * piggyBankDataByCurLevel[1] 
            /piggyBankDataByCurLevel[2] +CoinStartAndEndTransforms[0].transform.localPosition.x;
        
        CoinStartAndEndTransforms[1].transform.localPosition 
            = new Vector3(xPos,CoinStartAndEndTransforms[1].transform.localPosition.y,0);

        for (int i = 0; i < CoinTexts.Length; i++)
        {
            CoinTexts[i].text = piggyBankDataByCurLevel[i].ToString();
        }

        string price = GameManager.Purchase.GetPrice(PiggyBankModel.Instance.PiggyBankProductNameType);
        GrayText.SetTerm(price);
        GreenText.SetTerm(price);
    }

    private void SetBtnEvent()
    {
        BackBtn.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
        
        BuyBtn.SetBtnEvent(() =>
        {
            //购买
            int canGetCoinNum =PiggyBankModel.Instance.Data.PigTotalCoins;
            GameManager.Firebase.RecordMessageByEvent("Bank_Buy_Click", new Parameter("Stage", PiggyBankModel.Instance.Data.PigLevel));
            GameManager.Purchase.BuyProduct(PiggyBankModel.Instance.PiggyBankProductNameType, () =>
            {
                //刷新数据
                PiggyBankModel.Instance.UpgradePigLevelByBuySuccess();
                //关闭界面
                GameManager.UI.HideUIForm(this);
                //买完之后  发送刷新消息
                GameManager.Event.Fire(this,CommonEventArgs.Create(CommonEventType.RefreshPiggyBank));
                //展示奖励
                RewardManager.Instance.AddNeedGetReward(TotalItemData.Coin,canGetCoinNum);
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.RoyalChestRewardPanel, true, null);
                GameManager.Firebase.RecordMessageByEvent("Bank_Buy_Success", new Parameter("Stage", PiggyBankModel.Instance.Data.PigLevel));
            });
        });
        GrayBtn.enabled = false;
        GrayBtn.SetBtnEvent(() =>
        {
            //提示 需要收集满
        });
    }

    private int delayIndex;
    private void SetBGImage(int status)
    {
        this.lastStatus = status;
        string animName = $"idle_Jump{(status==1?"":status)}";
        SkeletonGraphic.AnimationState.SetAnimation(0, animName, false).Complete += entry =>
        {
            delayIndex = GameManager.Task.AddDelayTriggerTask(2f, () =>
            {
                SetBGImage(this.lastStatus);
            });
        };

        ShowTopImageAnimByStatus(status);//播放动画

        SetPiggyBankText(status);
        
        TopCoinTrans1.gameObject.SetActive(status<3);
        TopCoinTrans2.gameObject.SetActive(status>=3);
        //string imageName = $"BankBGImage{status}";
        // Texture lastTexture = BGImage.texture;
        //
        // if(lastTexture!=null&&lastTexture.name==imageName)return;
        // if(lastTexture==null)BGImage.color = new Color(1, 1, 1, 0);
        // LoadAssetAsync<Texture>(imageName, (s) =>
        // {
        //     BGImage.texture = s as Texture;
        //     
        //     BGImage.DOFade(0, 0.1f).onComplete += () =>
        //     {
        //         BGImage.texture=s as Texture;
        //         BGImage.DOFade(1, 0.1f);
        //     };
        // });
    }

    private void ShowSliderAnim()
    {
        float sliderValue = PiggyBankModel.Instance.CurSliderNum;
        float lastSliderValue = PiggyBankModel.Instance.Data.RecordLastBankSliderValue;

        float activeSliderValue=PiggyBankModel.Instance.ActiveBuyBtnSliderValue;
        bool isBtnActive = lastSliderValue >= activeSliderValue;
        GrayBtnObj.gameObject.SetActive(!isBtnActive);
        GreenBtnObj.gameObject.SetActive(isBtnActive);

        PiggyBankModel.Instance.RecordLastSliderValue();//先记录进度
        DOTween.To(() =>lastSliderValue, t =>
        {
            SliderImage.fillAmount = t;
            SetTopCoinNumBySilder(t,PiggyBankModel.Instance.Data.PigTotalCoins);
            if (t>= activeSliderValue)
            {
                GrayBtnObj.gameObject.SetActive(false);
                GreenBtnObj.gameObject.SetActive(true);
            }

            int newStatus = PiggyBankModel.Instance.GetBankStatusBySliderValue(t);
            
            if (newStatus > lastStatus)
            {
                if (lastStatus == 1)
                    GameManager.Sound.PlayAudio("SFX_Bonus_Bank_Pop");
                else if (newStatus == 3) 
                    GameManager.Sound.PlayAudio("SFX_Bonus_Bank_Pop");
                SetBGImage(newStatus);
            }
        }, sliderValue, 1f);
    }

    private void SetPiggyBankText(int status)
    {
        switch (status)
        {
            case 1:
                PiggyBankText.SetTerm("PiggyBank.Play levels to save coins in the Bonus Bank and grab them at a great price!");
                return;
            case 2:
                PiggyBankText.SetTerm("PiggyBank.Buy the Bonus Bank now or save more coins!");
                return;
            default:
                PiggyBankText.SetTerm("PiggyBank.The Bonus Bank is full! Hurry and buy it at a great price!");
                return;
        }
    }

    private void SetTopCoinNumBySilder(float sliderNum,int maxNum)
    {
        TopCoinText.text = $"{(int)Math.Min(sliderNum*PiggyBankModel.Instance.PiggyBankDataByCurLevel.LastOrDefault(),maxNum)}";
    }

    private bool isAnim = false;
    private void ShowTopImageAnimByStatus(int status)
    {
        if (status >= 2)
        {
            if(isAnim)return;
            
            isAnim=true;
            TopImage.DOLocalMoveY(480, 2f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            isAnim=false;
            TopImage.localPosition = new Vector3(TopImage.localPosition.x, 430, 0);
            TopImage.DOKill();
        }
    }
}
