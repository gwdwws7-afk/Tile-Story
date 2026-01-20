using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework.Event;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public sealed class HarvestKitchenMainMenu : PopupMenuForm,IItemFlyReceiver
{
    public DelayButton playButton, closeButton, explainButton, finishedButton, dishButton, sliderButton;
    public Image dishImg;
    public Slider slider;
    public TextMeshProUGUI sliderText, startConsumeNumText;
    public SkeletonGraphic girlSpine;
    public ClockBar clockBar;
    public BasketBar basketBar;
    public HarvestKitchenStageArea stageArea;
    public ItemPromptBox itemPromptBox;
    public Transform normalEnterDialogBox;
    public TextMeshProUGUILocalize normalEnterText;
    public HarvestKitchenNewDishRewardPanel newDishRewardPanel;
    public Image guide;
    public Transform guideDialogBox;
    public DelayButton guideButton;
    
    // 飞获得的点赞
    public GameObject flyReward;
    public Transform praiseReward, flyTarget;
    public TextMeshProUGUI praiseNumText;
    public ParticleSystem buttonPunchEffect;
    public SkeletonGraphic sliderFullEffect;
    
    private DTHarvestKitchenTaskDatas currentTask;
    private float autoHidePromptTimer;
    private AsyncOperationHandle dishHandle;
    private int longTimeIndex = 0;
    private List<int> delayTaskIdList = new List<int>();
    
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);
        
        RewardManager.Instance.RegisterItemFlyReceiver(this);
        
        playButton.OnInit(OnPlayButtonClick);
        closeButton.OnInit(OnCloseBtnClick);
        explainButton.OnInit(OnExplainButtonClick);
        dishButton.OnInit(OnDishButtonClick);
        guideButton.OnInit(OnPlayButtonClick);
        sliderButton.OnInit(OnSliderButtonClick);
        playButton.gameObject.SetActive(true);
        guide.gameObject.SetActive(false);
        
        int praiseNum = HarvestKitchenManager.Instance.PraiseNum;
        // 是否存在旧数据
        if (HarvestKitchenManager.Instance.OldPraiseNum != -1) // 不存在
        {
            praiseNum = HarvestKitchenManager.Instance.OldPraiseNum;
        }
        currentTask = HarvestKitchenManager.Instance.GetCurrentTaskDatas();
        slider.value = (float)praiseNum/currentTask.TargetPraise;
        sliderText.text = $"{praiseNum}/{currentTask.TargetPraise}";

        RefreshDishImage();
        basketBar.Init();
        stageArea.Init();
        
        startConsumeNumText.text = $"-{HarvestKitchenManager.Instance.GetCurrentTaskConsumeBasketNum()}";
        
        clockBar.StartCountdown(HarvestKitchenManager.Instance.EndTime);
        clockBar.CountdownOver += OnCountDownOver;
        
        GameManager.Sound.PlayMusic( SoundType.SFX_Kitchen_Match_Level_Harvest_BGM.ToString());
    }

    public override void OnReset()
    {
        RewardManager.Instance.UnregisterItemFlyReceiver(this);
        
        foreach (var id in delayTaskIdList)
        {
            GameManager.Task.RemoveDelayTriggerTask(id);
        }
        delayTaskIdList.Clear();
        
        StopAllCoroutines();
        autoHidePromptTimer = 0f;
        itemPromptBox.OnRelease();
        normalEnterDialogBox.transform.DOKill();
        UnityUtility.UnloadAssetAsync(dishHandle);
        dishHandle = default;
        
        base.OnReset();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        
        clockBar.OnUpdate(elapseSeconds, realElapseSeconds);

        if (autoHidePromptTimer > 0)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
#endif
            {
                HideItemPromptBox();

                return;
            }
            
            autoHidePromptTimer-=elapseSeconds;
            if (autoHidePromptTimer <= 0)
            {
                HideItemPromptBox();
            }
        }
    }

    public override bool CheckInitComplete()
    {
        if (dishHandle.IsValid() && !dishHandle.IsDone) 
        {
            return false;
        }
        
        return base.CheckInitComplete();
    }
    
    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);
        
        normalEnterDialogBox.gameObject.SetActive(false);

        if (HarvestKitchenManager.Instance.CheckActivityCanEnd())
        {
            SetFinishedState();
        }
        
        if (HarvestKitchenManager.Instance.IsFirstOpenMainMenu)
        {
            HarvestKitchenManager.Instance.IsFirstOpenMainMenu = false;

            SetAllButtonInteractable(false);

            // normalEnterText.SetTerm($"Kitchen.NormalOpen1");
            // normalEnterDialogBox.localScale = Vector3.zero;
            // normalEnterDialogBox.gameObject.SetActive(true);
            // normalEnterDialogBox.DOScale(1, 0.3f);
            
            GameManager.Task.AddDelayTriggerTask(0.2f, () =>
            {
                guide.DOFade(1, 0.2f);
                guide.gameObject.SetActive(true);
                playButton.gameObject.SetActive(false);

                guideDialogBox.DOScale(1.1f, 0.2f).onComplete = () =>
                {
                    guideDialogBox.DOScale(1f, 0.2f);
                };
                
                SetAllButtonInteractable(true);
            });
        }
        else
        {
            string animName = "idle";
            string textTerm = string.Empty;
            string audioName = SoundType.SFX_Story_Child_Sigh.ToString();
            float delayTime = 0f;
            switch (GameManager.DataNode.GetData(HarvestKitchenManager.ENTER_MAIN_MENU_TYPE, 0))
            {
                case 0:
                    int textIndex = Random.Range(1, 5);// 文案的序号
                    textTerm = $"HarvestKitchen.Dialog{textIndex}";
                    delayTime = 0f;
                    break;
                case 1:// 关卡胜利后回来
                    animName = "good";
                    if (HarvestKitchenManager.Instance.PraiseNum >= currentTask.TargetPraise)
                    {
                        // 存在可领取的奖励
                        textTerm = "HarvestKitchen.DialogUnlockNewDish";
                    }
                    else
                    {
                        textTerm = "HarvestKitchen.DialogWin";
                    }
                    audioName = "SFX_Story_Child_fighter_laugh_1";
                    delayTime = 1.3f;
                    longTimeIndex = 0;
                    GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
                    break;
                case 2:// 关卡失败后回来
                    animName = "worry";
                    textTerm = "HarvestKitchen.DialogLose";
                    delayTime = 1.3f;
                    longTimeIndex = 2;
                    GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
                    break;
            }

            GameManager.DataNode.RemoveNode(HarvestKitchenManager.ENTER_MAIN_MENU_TYPE);
            // if (animName.Equals("idle"))
            // {
            //     girlSpine.AnimationState.SetAnimation(0, animName, true);
            // }
            // else
            // {
            //     girlSpine.AnimationState.SetAnimation(0, animName, false).Complete += entry =>
            //     {
            //         girlSpine.AnimationState.SetAnimation(0, $"{animName}_idle", true);
            //     };
            // }
            
            // 显示文案
            delayTaskIdList.Add(GameManager.Task.AddDelayTriggerTask(delayTime, () =>
            {
                //GameManager.Sound.PlayAudio(audioName);
                ShowDialog(textTerm);
            }));
            
            StopCoroutine("ShowNormalEnterDialogBox");
            delayTaskIdList.Add(GameManager.Task.AddDelayTriggerTask(2f + delayTime, () =>
            {
                normalEnterDialogBox.DOScale(0, 0.3f);
                StartCoroutine("ShowNormalEnterDialogBox");
            }));
        
            if (HarvestKitchenManager.Instance.OldPraiseNum != -1)
            {
                int getPraiseNum = HarvestKitchenManager.Instance.GetAddPraise();
                praiseNumText.text = $"x {getPraiseNum}";
                HarvestKitchenManager.Instance.OldPraiseNum = -1;
                SetAllButtonInteractable(false);
                StartCoroutine(PlaySliderAnimCor(getPraiseNum));
            }
            // else
            // {
            //     OnDishButtonClick();
            // }
        }
    }

    public override void OnReveal()
    {
        gameObject.SetActive(true);
        //base.OnReveal();
    }
    
    private void SetAllButtonInteractable(bool state)
    {
        closeButton.interactable = state;
        explainButton.interactable = state;
        playButton.interactable = state;
    }
    
    public void SetFinishedState()
    {
        playButton.gameObject.SetActive(false);
        finishedButton.OnInit(OnCloseBtnClick);
        finishedButton.gameObject.SetActive((true));
    }

    public void ShowDialog(string textTerm)
    {
        normalEnterText.SetTerm(textTerm);
        normalEnterDialogBox.DOKill();
        normalEnterDialogBox.localScale = Vector3.zero;
        normalEnterDialogBox.gameObject.SetActive(true);
        normalEnterDialogBox.DOScale(1, 0.3f);
    }
    
    private void RefreshDishImage(Action callback = null)
    {
        UnityUtility.UnloadAssetAsync(dishHandle);
        dishHandle = UnityUtility.LoadSpriteAsync("dishes_" + HarvestKitchenManager.Instance.TaskId.ToString(),
            "HarvestKitchenDishes",
            sp =>
            {
                dishImg.sprite = sp;
                //dishImg.SetNativeSize();
                dishImg.color = Color.white;
                
                callback?.Invoke();
            });
    }
    
    IEnumerator ShowNormalEnterDialogBox()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            int index = Random.Range(1, 5);
            ShowDialog($"HarvestKitchen.Dialog{index.ToString()}");
            yield return new WaitForSeconds(3f);
            normalEnterDialogBox.DOScale(0, 0.3f);
        }
    }
    
    IEnumerator PlaySliderAnimCor(int getPraiseNum)
    {
        yield return null;
        
        HideItemPromptBox();
        // 获得的点赞飞到进度条
        praiseReward.localScale = Vector3.zero;
        praiseReward.localPosition = Vector3.zero;
        praiseNumText.gameObject.SetActive(true);
        flyReward.SetActive(true);
        praiseReward.DOScale(1f, 0.3f).SetEase(Ease.OutBack,3);
        praiseReward.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        
        flyReward.SetActive(false);
        praiseNumText.gameObject.SetActive(false);
        //GameManager.Sound.PlayAudio(SoundType.SFX_Collection_OilDrum_Appear.ToString());

        praiseReward.DOMove(flyTarget.position, 0.3f).SetEase(Ease.InQuart);
        praiseReward.DOScale(0.55f, 0.26f).SetEase(Ease.InQuart).onComplete = () =>
        {
            praiseReward.gameObject.SetActive(false);
        };
        
        yield return new WaitForSeconds(0.26f);
        
        flyTarget.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.2f, 1, 0f);
        UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
        
        yield return new WaitForSeconds(0.2f);
        
        // 播放进度条的动画
        while (true) 
        {
            if (currentTask == null)
            {
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenEntranceUpdate));
                SetAllButtonInteractable(true);
                yield break;
            }
            
            bool claimedReward = false;
            float fillAmount = Mathf.Min((float)HarvestKitchenManager.Instance.PraiseNum / currentTask.TargetPraise, 1);
            float duration = 0.4f;

            var sliderAnim = slider.DOValue(fillAmount, duration).SetEase(Ease.InOutCubic);
            sliderAnim.onUpdate = () =>
            {
                sliderText.text = $"{(int)(slider.value*currentTask.TargetPraise)}/{currentTask.TargetPraise}";
            };
            sliderAnim.onComplete = () =>
            {
                int finalNum = HarvestKitchenManager.Instance.PraiseNum > currentTask.TargetPraise
                    ? currentTask.TargetPraise
                    : HarvestKitchenManager.Instance.PraiseNum;
                sliderText.text = $"{finalNum}/{currentTask.TargetPraise}";
            };

            yield return new WaitForSeconds(duration);
            
            // 当前任务进度已满，领取奖励
            if (HarvestKitchenManager.Instance.PraiseNum >= currentTask.TargetPraise)
            {
                HideItemPromptBox();
                
                sliderFullEffect.AnimationState.SetAnimation(0, "Progressbar_effects2", false);
                GameManager.Sound.PlayAudio("SFX_goldenpass_missioncomplete");
                UnityUtil.EVibatorType.Medium.PlayerVibrator();
                
                // 领取奖励后，削减点赞数量
                HarvestKitchenManager.Instance.PraiseNum -= currentTask.TargetPraise;
                for (int i = 0; i < currentTask.RewardsList.Count; i++)
                {
                    RewardManager.Instance.AddNeedGetReward(currentTask.RewardsList[i], currentTask.RewardsNumList[i]);
                }
                var nextTask = HarvestKitchenManager.Instance.AccpetNextTaskDatas();
                if (nextTask == null)
                    HarvestKitchenManager.Instance.TaskId++;
                
                yield return new WaitForSeconds(0.4f);

                Transform flyDish = Instantiate(dishButton, dishButton.transform.position, Quaternion.identity,
                    newDishRewardPanel.dishRoot).transform;
                newDishRewardPanel.dishRoot.localScale = Vector3.one;
                newDishRewardPanel.gameObject.SetActive(true);
                dishButton.transform.localScale=Vector3.zero;
                float targetScale = 1.5f;
                flyDish.DOScale(targetScale, 0.4f).SetEase(Ease.InOutQuad);
                flyDish.DOLocalJump(Vector3.zero, 200f, 1, 0.55f).SetEase(Ease.InOutQuad);
                newDishRewardPanel.blackBg.OnShow(0.2f);
                
                yield return new WaitForSeconds(0.5f);
                
                flyDish.DOScale(new Vector3(targetScale*1.06f, targetScale*0.94f, 1f), 0.13f).onComplete = () =>
                {
                    flyDish.DOScale(new Vector3(targetScale*0.97f, targetScale*1.03f, 1f), 0.13f).onComplete = () =>
                    {
                        flyDish.DOScale(targetScale, 0.13f);
                    };
                };
                newDishRewardPanel.dishBgEffect.gameObject.SetActive(true);
                
                if (nextTask == null)  
                    slider.gameObject.SetActive(false);
                
                RewardManager.Instance.ShowNeedGetRewards(newDishRewardPanel, false, () =>
                {
                    claimedReward = true;
                    
                    flyDish.gameObject.SetActive(false);
                });
                
                yield return new WaitUntil(() => claimedReward);

                bool stageFinished = false;
                // 更新下方的阶段奖励显示
                stageArea.RefreshChest(() =>
                {
                    stageFinished = true;

                    if (nextTask == null) 
                        SetFinishedState();
                });
                
                yield return new WaitUntil(() => stageFinished);
                
                // 获取下一个任务
                currentTask = nextTask;

                if (currentTask != null)
                {
                    HarvestKitchenManager.Instance.RefreshChallengeNum(currentTask.ChallengeToolNumber);
                    // 清空进度条
                    slider.value = 0;
                    sliderText.text = $"0/{currentTask.TargetPraise}";

                    // 切换奖励图标
                    RefreshDishImage(() =>
                    {
                        dishButton.transform.DOScale(0.65f, 0.15f).onComplete = () =>
                        {
                            dishButton.transform.DOScale(0.55f, 0.1f).onComplete = () =>
                            {
                                OnDishButtonClick();
                            };
                        };
                    });
                        
                    // 刷新道具消耗显示
                    startConsumeNumText.text = $"-{currentTask.ChallengeToolNumber.ToString()}";
                    GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenInfoChanged));
                    
                    yield return new WaitForSeconds(0.25f);
                }
                
                // if (currentTask == null)
                // {
                //     RefreshStageReward(-1);
                //     // 提前关闭活动
                //     HarvestKitchenManager.Instance.EndActivity();
                //     // 刷新按钮逻辑
                //     finishedButton.OnInit(OnCloseBtnClick);
                //     finishedButton.gameObject.SetActive(true);
                //     playButton.gameObject.SetActive(false);
                //     SetAllButtonInteractable(true);
                //
                //     slider.gameObject.SetActive(false);
                //     normalEnterText.SetTerm("Kitchen.Complete");
                //     normalEnterDialogBox.localScale = Vector3.zero;
                //     normalEnterDialogBox.gameObject.SetActive(true);
                //     normalEnterDialogBox.DOScale(1, 0.3f);
                //
                //     yield break;
                // }
            }
            else
            {
                GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.KitchenEntranceUpdate));
                SetAllButtonInteractable(true);
                yield break;
            }
        }
    }
    
    private void OnPlayButtonClick()
    {
        // 判断是否有足够的道具，并进行消耗
        if (HarvestKitchenManager.Instance.UseBasketNumOpenLevel())
        {
            basketBar.Refresh();
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Kitchen_Match_Challenge_Start);
            GameManager.UI.HideUIForm(this);
            GameManager.UI.ShowUIForm("HarvestKitchenGameMenu");
        }
        else
        {
            // 开启获得更多厨师帽界面
            GameManager.UI.ShowUIForm("HarvestKitchenGetMoreMenu");
        }
    }

    private void OnExplainButtonClick()
    {
        GameManager.UI.ShowUIForm("HarvestKitchenExplainMenu");
    }
    
    private void OnCloseBtnClick()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.Process.EndProcess(ProcessType.CheckHarvestKitchen);

        //活动已完成
        if (HarvestKitchenManager.Instance.CheckActivityCanEnd())
        {
            HarvestKitchenManager.Instance.ActivityEndProcess();
            
            GameManager.UI.ShowUIForm("HarvestKitchenEndMenu");
        }

        GameManager.Sound.PlayBgMusic(GameManager.PlayerData.BGMusicName);
    }

    private void OnDishButtonClick()
    {
        if (currentTask != null)
        {
            ShowItemPromptBox(currentTask.RewardsList, currentTask.RewardsNumList, dishImg.transform.position);
        }
    }

    private void OnSliderButtonClick()
    {
        GameManager.UI.ShowUIForm("HarvestKitchenRewardListMenu", UIFormType.PopupUI);
    }
    
    public void ShowItemPromptBox(List<TotalItemData> types, List<int> nums, Vector3 position)
    {
        itemPromptBox.transform.DOKill();
        itemPromptBox.Init(types, nums);
        itemPromptBox.ShowPromptBox(PromptBoxShowDirection.Up, position);

        autoHidePromptTimer = 3f;
    }

    public void HideItemPromptBox()
    {
        autoHidePromptTimer = 0;
        itemPromptBox.transform.DOScale(0, 0.15f).onComplete = () =>
        {
            itemPromptBox.HidePromptBox();
        };
    }
    
    private void OnCountDownOver(object sender, CountdownOverEventArgs e)
    {
        OnCloseBtnClick();
    }
    
    #region receiver

    public ReceiverType ReceiverType => ReceiverType.Common;

    public GameObject GetReceiverGameObject() => playButton.gameObject;

    public void OnFlyHit(TotalItemData type)
    {
    }

    public void OnFlyEnd(TotalItemData type)
    {
        GameManager.Sound.PlayAudio("SFX_itemget");

        if (type == TotalItemData.KitchenBasket)
        {
            basketBar.transform.DOKill();
            basketBar.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            basketBar.transform.DOScale(1f, 0.1f).SetEase(Ease.OutCubic).onComplete = () =>
            {
                basketBar.Refresh();
                basketBar.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutCubic);
            };
            
            basketBar.punchEffect.Play();
        }
        else
        {
            playButton.transform.DOKill();
            playButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            playButton.transform.DOScale(0.7f, 0.1f).SetEase(Ease.OutCubic).onComplete = () =>
            {
                playButton.transform.DOScale(0.8f, 0.1f).SetEase(Ease.OutCubic);
            };
        
            buttonPunchEffect.Play();
        }
        
        UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
    }

    public Vector3 GetItemTargetPos(TotalItemData type)
    {
        if (type == TotalItemData.KitchenBasket) 
            return basketBar.transform.position;
        else
            return playButton.transform.position;
    }

    #endregion
}
