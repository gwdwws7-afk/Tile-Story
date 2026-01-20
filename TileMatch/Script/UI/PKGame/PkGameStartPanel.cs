using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIEffects;
using DG.Tweening;
using Firebase.Analytics;
using GameFramework.Event;
using MySelf.Model;
using NSubstitute.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class PkGameStartPanel : PopupMenuForm
{
    [SerializeField] private PkSorce PkSorce;
    [SerializeField] private DelayButton CloseBtn, HelpBtn, StartBtn, PlayBtn, PreeStartBtn;
    [SerializeField] private ClockBar Timer;
    [SerializeField] private GameObject Timer_Root;
    [SerializeField] private TextMeshProUGUI Coin_Text;
    [SerializeField] private TextMeshProUGUILocalize Play_Text;
    [SerializeField] private GameObject Open_Image, NoOpen_Image;
    //读秒
    [SerializeField] private GameObject ReadTimeObj;
    [SerializeField] private TextMeshProUGUI TimeText;

    //guide
    [SerializeField] private GameObject[] RankBGs, RankFors;
    [SerializeField] private TextMeshProUGUILocalize GuideText;
    [SerializeField] private GameObject GuideParent, GuideTextParent, RankObj, BtnParent;
    [SerializeField] private DelayButton BgBtn;
    [SerializeField] private TextMeshProUGUILocalize CanGetCoinNumText;
    [SerializeField] private CanvasGroup GuideAnimParent;
    [SerializeField] private Image HeadImage;
    [SerializeField] private CanvasGroup BtnParentCanvasGroup;

    private bool isShowing = false;
    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonEvent);
        PkSorce.Init();
        BtnEvent();
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        Log.Info($"OnShow:执行");
        if (!isShowing)
        {
            isShowing = true;
            ShowGuide();
        }

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnRelease()
    {
        isShowing = false;
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonEvent);
        base.OnRelease();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (Timer.gameObject.activeInHierarchy) Timer.OnUpdate(elapseSeconds, realElapseSeconds);
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void SetBtnActive(bool isActive)
    {
        CloseBtn.enabled = isActive;
        HelpBtn.enabled = isActive;

        Debug.Log("PkGame SetBtnActive " + isActive.ToString());
    }

    private void BtnEvent()
    {
        bool isNone = PkGameModel.Instance.PkGameStatus == PkGameStatus.None;
        bool isPlaying = PkGameModel.Instance.PkGameStatus == PkGameStatus.Playing;
        PlayBtn.gameObject.SetActive(!isNone);
        PlayBtn.interactable = isPlaying ? true : false;
        Play_Text.SetMaterialPreset(MaterialPresetName.Btn_Green);

        bool isFirst = PkGameModel.Instance.Data.EnterPkGameCount == 1;
        StartBtn.gameObject.SetActive(isNone && !isFirst);
        PreeStartBtn.gameObject.SetActive(isNone && isFirst);

        Coin_Text.text = PkGameModel.Instance.NeedCoinStartPkGame.ToString();

        Timer_Root.gameObject.SetActive(isPlaying);
        Open_Image.gameObject.SetActive(!isNone);
        NoOpen_Image.gameObject.SetActive(isNone);

        Action startGameAction = () =>
        {
            ShowHideAnim();
            if (GameManager.PlayerData.UseItem(TotalItemData.Coin, PkGameModel.Instance.NeedCoinStartPkGame))
            {
            //首先进行分组
            PkGameModel.Instance.SetGroupName();
            //活动开启打点
            GameManager.Firebase.RecordMessageByEvent("PKMatch_Stage",
              new Parameter("stage", PkGameModel.Instance.Data.CurRankNum));
                GameManager.Firebase.RecordMessageByEvent("PKMatch_Start",
              new Parameter("Num", PkGameModel.Instance.Data.EnterPkGameCount));
            //设置按钮状态
            SetBtnStatus();
            //点击开始匹配 对手
            MatchTargetPlayer();
            }
            else
            {
                GameManager.Firebase.RecordMessageByEvent(
              Constant.AnalyticsEvent.Coin_Not_Enough,
              new Parameter("EntranceID", 9),
              new Parameter("Source", "PkContinue"));

            //show shop
            GameManager.UI.ShowUIForm("ShopMenuManager");
            }
        };

        if (isPlaying)
        {
            //转换时间到本地时间
            Timer.StartCountdown(PkGameModel.Instance.LocalEndDateTime);
            Timer.CountdownOver += (a, b) =>
            {
          //结束时关掉这个界面
          GameManager.UI.HideUIForm(this);
          // //结束  还是处于这个界面
          // PlayBtn_Effect.effectMode = EffectMode.Grayscale;
          // Play_Text.SetMaterialPreset(MaterialPresetName.Btn_Grey);
          // PlayBtn.enabled = false;
      };
        }

        CloseBtn.SetBtnEvent(() =>
        {
            GameManager.Process.EndProcess(ProcessType.ShowPkGame);
            GameManager.UI.HideUIForm(this);
        });
        HelpBtn.SetBtnEvent(() =>
        {
            GameManager.UI.ShowUIForm("PkRulesPanel");
        });
        StartBtn.SetBtnEvent(() =>
        {
            startGameAction();
        });
        PlayBtn.SetBtnEvent(() =>
        {
            if (isPlaying)
            {
                GameManager.UI.HideUIForm(this);
                GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
                GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI, form =>
                {
                    form.m_OnHideCompleteAction = () =>
                    {
                        GameManager.Process.EndProcess(ProcessType.ShowPkGame);
                    };
                }, () =>
                {
                    GameManager.Process.EndProcess(ProcessType.ShowPkGame);
                });
            }
        });
        PreeStartBtn.SetBtnEvent(() =>
        {
            startGameAction();
        });
    }

    private void MatchTargetPlayer()
    {
        //展示匹配对手开始动画
        PkSorce.ShowMatchTargetPlayerStartAnim();
        //倒计时
        ReadTimeEvent(true);

        SetBtnActive(false);
        //开始获取数据   获取结束 或者超时，给空对手
        //然后播放 匹配对手结束动画
        Action getDataOverEvetn = () =>
        {
            PkSorce.Init(isAutoProgressAnim: false, isSetTargetName: false);

            //执行匹配结束动画
            PkSorce.ShowMatchTargetPlayerOverAnim(() =>
            {
                //匹配完成音效
                GameManager.Sound.PlayAudio("SFX_PKDing");
                //设置活动开启
                PkGameModel.Instance.IsActivityOpen = true;

                ReadTimeEvent(false);

                SetBtnActive(true);
                //设置按钮状态
                BtnEvent();
                //发送状态变化消息
                GameManager.Event.Fire(CommonEventArgs.EventId, CommonEventArgs.Create(CommonEventType.PkGameStart));

                if (PkGameModel.Instance.Data.TargetPlayerPkData.PlayerHeadPortrait == 0)
                {
                    //未匹配到人
                    GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.PkMatch_EmptyMatch);
                }

                GameManager.UI.HideUIForm("GlobalMaskPanel");

            });
        };

        GameManager.UI.ShowUIForm("GlobalMaskPanel");
        PkGameModel.Instance.GetServicePosNum(5, b =>
        {
        try
        {
            //获取对方头像数据
            if (b) PkGameModel.Instance.GetTargetDataByService((boo) => getDataOverEvetn());
            else getDataOverEvetn();
            }
        catch(Exception e)
        {
                SetBtnActive(true);
                GameManager.UI.HideUIForm("GlobalMaskPanel");
        }
        });
  }

  private void SetBtnStatus()
  {
    StartBtn.gameObject.SetActive(false);
    PlayBtn.gameObject.SetActive(true);
    Play_Text.SetMaterialPreset(MaterialPresetName.Btn_Grey);
  }

  private Coroutine coroutine;
  private void ReadTimeEvent(bool isShow)
  {
    ReadTimeObj.gameObject.SetActive(isShow);
    if(coroutine!=null)StopCoroutine(coroutine);
    PlayBtn.gameObject.SetActive(!isShow);
    PreeStartBtn.gameObject.SetActive(!isShow);
    if (isShow)
    {
      coroutine= StartCoroutine(SetSearchTime());
    }
  }

  private IEnumerator SetSearchTime()
  {
    int num = 0;
    string[] strs =new String[4]{"",".","..","..."};
    while (ReadTimeObj.activeInHierarchy)
    {
      TimeText.text = strs[num];
      yield return new WaitForSeconds(0.2f);
      num += 1;
      num = num % 4;
    }
  }
  
  private void CommonEvent(object sender, GameEventArgs e)
  {
    CommonEventArgs ne = (CommonEventArgs)e;
    if (ne.Type == CommonEventType.PkListenData)
    {
      //获取到监听的数据之后，
      //判断游戏状态
      if (PkGameModel.Instance.PkGameStatus == PkGameStatus.Playing)
      {
        //刷新头像数据显示
        PkSorce.Init();
      }
    }
  }


  /// <summary>
  /// 展示引导【更新之后首次弹出】
  /// </summary>
  private void ShowGuide()
  {
    GuideParent.gameObject.SetActive(false);
    GuideTextParent.gameObject.SetActive(false);
    BgBtn.gameObject.SetActive(false);
    BtnParent.SetActive(false);
    if (!PkGameModel.Instance.Data.IfOpenFirstGuide)
    {
      ShowFirstGuide();
    }else if (!PkGameModel.Instance.Data.IsActivityOpen&&
              !PkGameModel.Instance.Data.IfOpenUpgradesGuide&&
              PkGameModel.Instance.Data.CurRankNum>PkGameModel.Instance.Data.LastRankNum)
    {
      ShowUpgradesGuide();
    }
    else
    {
      BtnParent.SetActive(true);

      if (PkGameModel.Instance.Data.IfOpenFirstGuide)
      {
        GuideParent.gameObject.SetActive(true);
        bool isRankUpgrade = PkGameModel.Instance.Data.LastRankNum != PkGameModel.Instance.Data.CurRankNum;
        BtnParent.SetActive(!isRankUpgrade);
       GameManager.UI.StartCoroutine(ShowPkGameLevelAnim(PkGameModel.Instance.Data.LastRankNum, PkGameModel.Instance.Data.CurRankNum,
         () =>
         {
           if (isRankUpgrade && BtnParent != null) 
           {
             BtnParent.SetActive(true);
             BtnParent.transform.DOScale(new Vector3(1.26f, 0.85f, 1f), 0.15f).SetEase(Ease.OutSine).onComplete+= () =>
             {
               BtnParent.transform.DOScale(Vector3.one,0.15f).SetEase(Ease.OutBack);
             };
           }
         }));
      }
    }
  }

  private void ShowFirstGuide()
  {
    PkGameModel.Instance.SetFirstGuide();
    //弹出引导
    GuideParent.gameObject.SetActive(true);
    GuideTextParent.gameObject.SetActive(false);
    BgBtn.gameObject.SetActive(false);
    BtnParent.SetActive(false);
    GuideText.SetTerm("Pk.Get ready for Kart Duel!");
    GameManager.UI.StartCoroutine(ShowPkGameLevelAnim(PkGameModel.Instance.Data.LastRankNum, PkGameModel.Instance.Data.CurRankNum,
      () =>
      {
        //激活按钮
        ShowAnim(() =>
        {
          //BtnParentCanvasGroup.alpha = 0;
          BtnParent.SetActive(true);
          BtnEvent();
          //BtnParentCanvasGroup.DOFade(1, 0.3f);

          BtnParent.transform.DOScale(new Vector3(1.26f, 0.85f, 1f), 0.15f).SetEase(Ease.OutSine).onComplete+= () =>
          {
              BtnParent.transform.DOScale(Vector3.one,0.15f).SetEase(Ease.OutBack);
          };
        });
        // //设置点击引导背景关闭引导
        // BgBtn.SetBtnEvent(() =>
        // {
        //   BgBtn.gameObject.SetActive(false);
        //   GuideTextParent.gameObject.SetActive(false);
        // });
      }));
  }

  /// <summary>
  /// 展示升级动画引导
  /// </summary>
  private void ShowUpgradesGuide()
  {
    PkGameModel.Instance.SetSecondGuide();
    //展示升级引导
    GuideParent.gameObject.SetActive(true);
    GuideTextParent.gameObject.SetActive(false);
    BgBtn.gameObject.SetActive(false);
    BtnParent.gameObject.SetActive(false);
    GuideText.SetTerm("Pk.Well done! You've advanced to the next stage, unlocking a greater reward multiplier!");
    GameManager.UI.StartCoroutine(ShowPkGameLevelAnim(PkGameModel.Instance.Data.LastRankNum, PkGameModel.Instance.Data.CurRankNum,
      () =>
      {
        ShowAnim(() =>
        {
          //激活按钮
          //BtnParentCanvasGroup.alpha = 0;
          BtnParent.SetActive(true);
          BtnEvent();
          BtnParent.transform.DOScale(new Vector3(1.26f, 0.85f, 1f), 0.15f).SetEase(Ease.OutSine).onComplete+= () =>
          {
            BtnParent.transform.DOScale(Vector3.one,0.15f).SetEase(Ease.OutBack);
          };
         // BtnParentCanvasGroup.DOFade(1, 0.3f);
        });
      }));
  }

  /// <summary>
  /// 展示升级动画
  /// </summary>
  private IEnumerator ShowPkGameLevelAnim(int lastLevel,int nowLevel,Action action)
  {
    lastLevel = Math.Min(lastLevel, 4);
    nowLevel = Math.Min(nowLevel, 4);
    for (int i = 0; i < RankBGs.Length; i++)
    {
      RankBGs[i].SetActive(lastLevel!=i+1);
      RankFors[i].SetActive(lastLevel==i+1);
    }
    RankObj.transform.position = new Vector3(RankFors[lastLevel-1].transform.position.x,RankObj.transform.position.y );
    SetTextMeshPro(lastLevel);
   
    if(nowLevel!=lastLevel) yield return new WaitForSeconds(0.4f);
    while (nowLevel!=lastLevel)
    {
      lastLevel=(nowLevel>lastLevel)?(lastLevel+1):(lastLevel-1);
      Vector3 newRankObjVector3 = new Vector3(RankFors[lastLevel-1].transform.position.x,RankObj.transform.position.y);
      RankObj.transform.DOJump(newRankObjVector3,0.1f,1,0.3f).onComplete+= () =>
      {
        for (int i = 0; i < RankBGs.Length; i++)
        {
          RankBGs[i].SetActive(lastLevel!=i+1);
          RankFors[i].SetActive(lastLevel==i+1);
        }
        SetTextMeshPro(lastLevel);
      };
      yield return new WaitForSeconds(0.33f);
    }
    PkGameModel.Instance.SetRank();
    SetTextMeshPro(lastLevel);
    yield return new WaitForSeconds(0.4f);
    action?.Invoke();
  }

  private void SetTextMeshPro(int rank)
  {
    int num = PkGameModel.Instance.MultipleByRank(rank) * 2;
    CanGetCoinNumText.SetParameterValue("{1}",num.ToString());
  }

  private void ShowAnim(Action finishAction)
  {
      HeadImage.transform.localScale = Vector3.one * 0.6f;
      GuideAnimParent.alpha =0;
      GuideTextParent.gameObject.SetActive(true);
      //先做动画
      HeadImage.transform.DOScale(1, 0.15f).SetEase(Ease.OutBack).onComplete += () =>
      {
        GuideAnimParent.gameObject.SetActive(true);
        GuideAnimParent.transform.localEulerAngles = new Vector3(0,0,30);
        GuideAnimParent.transform.DOLocalRotate(Vector3.zero, 0.15f).SetEase(Ease.OutBack).onComplete+= () =>
        {
          GameManager.Task.AddDelayTriggerTask(2f, () =>
          {
            finishAction?.Invoke();
          });
        };
        GuideAnimParent.DOFade(1, 0.1f);
      };
  }

  private void ShowHideAnim()
  {
    if (GuideTextParent.gameObject.activeInHierarchy)
    {
      GuideAnimParent.transform.DOLocalRotate(Vector3.zero, 0.15f).SetEase(Ease.OutBack).onComplete+= () =>
      {
        HeadImage.transform.DOScale(0.6f, 0.15f).onComplete+= () =>
        {
          GuideTextParent.gameObject.SetActive(false);
        };
      };
      GuideAnimParent.DOFade(0, 0.1f).SetDelay(0.05f);
    }
  }
}
