using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ObjectiveColumn : FancyScrollRectCell<int, Context>
{
    public Transform m_Body;
    public GameObject m_Top;
    public Image m_NormalBgImage;
    public Image m_CompletedBgImage;
    public Image m_ObjectiveImage;
    public Image m_RewardImage;
    public TextMeshProUGUI m_RewardText;
    public TextMeshProUGUILocalize m_TitleText;
    public SimpleSlider m_Slider;
    public GameObject m_RewardTitle;
    public DelayButton m_ClaimButton;
    public ClockBar m_ClockBar;

    private ObjectiveData m_Data;
    private string m_CurObjectiveName;
    private string m_CurRewardName;
    private int m_CurToggleIndex;
    private bool m_ShowAnim;

    public override void Initialize()
    {
        base.Initialize();

        m_ClaimButton.SetBtnEvent(OnClaimButtonClick);
    }

    private void Update()
    {
        if (m_CurToggleIndex == 0 && m_Top.activeSelf && m_ClockBar != null)
        {
            m_ClockBar.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }

    private void OnDisable()
    {
        m_Body.DOKill();
        m_Body.localPosition = new Vector3(0, m_Body.localPosition.y, 0);
    }

    public override void UpdateContent(int objectiveId)
    {
        bool isDaily = m_CurToggleIndex == 0;

        if (!gameObject.name.Equals(objectiveId)) gameObject.name = objectiveId.ToString();
        if (isDaily && objectiveId == -1)
        {
            if (m_Top != null)
            {
                m_Body.gameObject.SetActive(false);
                m_Top.SetActive(true);
                m_ClockBar.OnReset();
                m_ClockBar.StartCountdown(DateTime.Now.AddDays(1) - DateTime.Now.TimeOfDay);
            }
            return;
        }

        m_Body.gameObject.SetActive(true);
        if (m_Top != null)
        {
            m_Top.SetActive(false);
        }

        ObjectivePanel objectivePanel = GameManager.UI.GetUIForm("ObjectivePanel") as ObjectivePanel;
        int index = 0;
        if (isDaily)
        {
            index = GameManager.Objective.CurDailyObjectiveIds.IndexOf(objectiveId);
            if (!objectivePanel.m_DailyIdList.Contains(objectiveId))
            {
                m_ShowAnim = false;
                objectivePanel.m_DailyIdList.Add(objectiveId);
            }
        }
        else
        {
            index = GameManager.Objective.CurAllTimeObjectiveIds.IndexOf(objectiveId);
            if (!objectivePanel.m_AllTimeIdList.Contains(objectiveId))
            {
                m_ShowAnim = false;
                objectivePanel.m_AllTimeIdList.Add(objectiveId);
            }
        }

        if (!m_ShowAnim)
        {
            m_ShowAnim = true;
            m_Body.DOKill();
            m_Body.localPosition = new Vector3(1080, m_Body.localPosition.y);

            int completeNum = isDaily ? objectivePanel.m_DailyAnimCompleteNum : objectivePanel.m_AllTimeAnimCompleteNum;
            m_Body.DOLocalMoveX(-30, 0.2f).SetDelay((index - completeNum) * 0.04f).onComplete = () =>
             {
                 m_Body.DOLocalMoveX(0, 0.2f).onComplete = () =>
                 {
                     if (objectivePanel != null)
                     {
                         if (isDaily)
                             objectivePanel.m_DailyAnimCompleteNum++;
                         else
                             objectivePanel.m_AllTimeAnimCompleteNum++;
                     }
                 };
             };
        }

        transform.localScale = Vector3.one;
        GetComponent<CanvasGroup>().alpha = 1;
        m_ClaimButton.interactable = true;

        m_CurToggleIndex = GameManager.DataNode.GetData<int>("CurToggleIndex", 0);
        if (m_CurToggleIndex == 1)
        {
            DTAllTimeObjective dtObjective = GameManager.DataTable.GetDataTable<DTAllTimeObjective>().Data;
            m_Data = dtObjective.GetData(objectiveId);
        }
        else
        {
            DTDailyObjective dtObjective = GameManager.DataTable.GetDataTable<DTDailyObjective>().Data;
            m_Data = dtObjective.GetData(objectiveId);
        }

        //目标图片
        string objectiveName = m_Data.Type.ToString().ToLower();
        if (m_CurObjectiveName != objectiveName)
        {
            m_CurObjectiveName = objectiveName;

            //if (m_ObjectiveHandle.IsValid())
            //    GameManager.Resource.Release(m_ObjectiveHandle);
            //m_ObjectiveHandle = GameManager.Resource.LoadSprite(objectiveName, "Objective", sp =>
            //{
            //    m_ObjectiveImage.sprite = (Sprite)sp;
            //    m_ObjectiveImage.SetNativeSize();
            //});

            m_ObjectiveImage.sprite = objectivePanel.GetTargetSprite(m_Data.Type.ToString(), "Objective");
            m_ObjectiveImage.SetNativeSize();
        }

        //目标标题
        InitTitleText();

        //进度
        m_Slider.TotalNum = m_Data.TargetNum;
        m_Slider.CurrentNum = GameManager.Objective.GetObjectiveProgress(m_Data, m_CurToggleIndex == 1);

        //目标是否完成背景
        bool isCompleted = GameManager.Objective.CheckObjectiveCompleted(m_Data.ID, m_CurToggleIndex == 1);
        m_NormalBgImage.gameObject.SetActive(!isCompleted);
        m_CompletedBgImage.gameObject.SetActive(isCompleted);
        if (!isCompleted)
            m_TitleText.Target.color = new Color(26 / 255f, 66 / 255f, 120 / 255f);
        else
            m_TitleText.Target.color = new Color(135 / 255f, 76 / 255f, 45 / 255f);

        //奖励
        string rewardName = m_Data.RewardType.ToString();
        if (m_CurRewardName != rewardName)
        {
            m_CurRewardName = rewardName;

            //if (m_RewardHandle.IsValid())
            //    GameManager.Resource.Release(m_RewardHandle);
            //m_RewardHandle = GameManager.Resource.LoadSprite(rewardName, "TotalItemAtlas", sp =>
            //{
            //    m_RewardImage.sprite = (Sprite)sp;
            //});

            m_RewardImage.sprite = objectivePanel.GetTargetSprite(rewardName, "TotalItemAtlas");
        }

        m_RewardTitle.SetActive(!isCompleted);
        m_ClaimButton.gameObject.SetActive(isCompleted);
        if (m_Data.RewardType == TotalItemData.Coin.TotalItemType)
        {
            m_RewardText.text = m_Data.RewardNum.ToString();

            if (isCompleted)
                m_RewardImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(306, 77.5f);
            else
                m_RewardImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(306, -35.6f);
        }
        else
        {
            if (m_Data.RewardType == TotalItemData.InfiniteLifeTime.TotalItemType
                || m_Data.RewardType == TotalItemData.InfiniteFireworkBoost.TotalItemType
                || m_Data.RewardType == TotalItemData.InfiniteAddOneStepBoost.TotalItemType
                || m_Data.RewardType == TotalItemData.InfiniteMagnifierBoost.TotalItemType)
            {
                if (m_Data.RewardNum < 60)
                    m_RewardText.text = m_Data.RewardNum.ToString() + "m";
                else
                    m_RewardText.text = (m_Data.RewardNum / 60f).ToString() + "h";
            }
            else
            {
                m_RewardText.text = "x" + m_Data.RewardNum.ToString();
            }

            if (isCompleted)
                m_RewardImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(306, 77.5f);
            else
                m_RewardImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(306, -35.6f);
        }
    }

    private void InitTitleText()
    {
        m_TitleText.SetTerm(GetTitleTerm(m_Data.Type));
        m_TitleText.SetParameterValue("Num", m_Data.TargetNum.ToString());

        if (GameManager.Localization.Language == Language.Russian)
            m_TitleText.Target.lineSpacing = -20;
        else
            m_TitleText.Target.lineSpacing = 0;
    }

    public static string GetTitleTerm(ObjectiveType type)
    {
        string term = string.Empty;
        switch (type)
        {
            case ObjectiveType.Change_Avatar:
                term = "Objective.Change your avatar";
                break;
            case ObjectiveType.Set_Name:
                term = "Objective.Set your name";
                break;
            case ObjectiveType.Pass_Levels:
                term = "Objective.Pass levels";
                break;
            case ObjectiveType.Clear_Tiles:
                term = "Objective.Clear tiles";
                break;
            case ObjectiveType.Use_ExtraSlot:
                term = "Objective.Use extra slot once";
                break;
            case ObjectiveType.Use_Undo:
                term = "Objective.Use undo once";
                break;
            case ObjectiveType.Use_Crane:
                term = "Objective.Use crane once";
                break;
            case ObjectiveType.Use_Magnet:
                term = "Objective.Use magnet once";
                break;
            case ObjectiveType.Use_Shuffle:
                term = "Objective.Use shuffle once";
                break;
            case ObjectiveType.Continuous_Login:
                term = "Objective.Login for consecutive days";
                break;
            case ObjectiveType.Accumulated_Login:
                term = "Objective.Login for days in total";
                break;
            case ObjectiveType.Unlock_Scenes:
                term = "Objective.Unlock scenes";
                break;
            case ObjectiveType.Unlock_SceneSet:
                term = "Objective.Unclock set of scene";
                break;
            case ObjectiveType.Unlock_TileSets:
                term = "Objective.Unlock tilesets";
                break;
            case ObjectiveType.SyncGameProgress:
                term = "Objective.Sync game progress";
                break;
            case ObjectiveType.LogIntoTheGame:
                term = "Objective.Log into the game";
                break;
            case ObjectiveType.Complete_Chapter:
                term = "Objective.Complete Chapter";
                break;
            case ObjectiveType.Turntable:
                term = "Objective.Play Lucky Spin";
                break;
            case ObjectiveType.Complete_Level:
                term = "Objective.Complete Level";
                break;
        }
        return term;
    }

    private void OnClaimButtonClick()
    {
        ObjectivePanel objectivePanel = GameManager.UI.GetUIForm("ObjectivePanel") as ObjectivePanel;
        m_ClaimButton.interactable = false;

        //获取奖励
        Vector3 rewardStartPos = new Vector3(m_ClaimButton.transform.position.x, m_RewardImage.transform.position.y);
        TotalItemData rewardType = TotalItemData.FromInt((int)m_Data.RewardType);
        int rewardNum = m_Data.RewardNum;
        RewardManager.Instance.SaveRewardData(rewardType, rewardNum, true);

        //�����ɵ��������Ķ������ѷ�����
        //if (rewardType == TotalItemData.Coin)
        //    objectivePanel.ShowCoinBar();
        //else
        //    objectivePanel.ShowHeadPortrait();

        //RewardManager.Instance.AutoGetRewardDelayTime = 0f;

        //string assetName;
        //if (rewardType == TotalItemData.Coin)
        //    assetName = "CoinFlyReward";
        //else
        //    assetName = "DefaultFlyReward";

        //GameManager.Resource.InstantiateAsync(assetName, Vector3.zero, Quaternion.identity, RewardManager.Instance.rewardArea.transform, flyObject =>
        //{
        //    FlyReward flyReward = flyObject.GetComponent<FlyReward>();
        //    flyReward.OnInit(rewardType, rewardNum, 1, true, null);
        //    flyReward.transform.position = rewardStartPos;
        //    flyReward.transform.localScale = Vector3.one;
        //    flyReward.OnShow();
        //    if (rewardType == TotalItemData.Coin)
        //    {
        //        StartCoroutine(flyReward.ShowGetRewardAnim
        //            (rewardType, RewardManager.Instance.CoinFlyReceiver.CoinFlyTargetPos));

        //        GameManager.Task.AddDelayTriggerTask(2f, () =>
        //        {
        //            flyReward.OnHide();
        //            objectivePanel.HideCoinBar(0.1f);
        //            GameManager.Resource.ReleaseInstance(flyReward.gameObject);
        //        });
        //    }
        //    else
        //    {
        //        Vector3 rewardFlyPosition = Vector3.down * 2;
        //        var receiver = RewardManager.Instance.GetReceiverByItemType(rewardType);
        //        Vector3 targetPos = rewardFlyPosition;
        //        if (receiver != null)
        //        {
        //            targetPos = receiver.GetItemTargetPos(rewardType);
        //        }
        //        StartCoroutine(flyReward.ShowGetRewardAnim(rewardType, targetPos));

        //        GameManager.Task.AddDelayTriggerTask(0.54f, () =>
        //        {
        //            flyReward.OnHide();
        //            objectivePanel.HideHeadPortrait(0.5f);
        //            GameManager.Resource.ReleaseInstance(flyReward.gameObject);
        //        });
        //    }
        //});

        UnityUtility.InstantiateAsync("ObjectiveRewardGetTip", objectivePanel.transform, obj =>
        {
            ItemSlot slot = obj.GetComponent<ItemSlot>();

            slot.OnInit(rewardType, rewardNum);
            obj.GetComponent<CanvasGroup>().alpha = 1;

            Transform cachedTrans = obj.transform;
            cachedTrans.localScale = Vector3.zero;
            obj.SetActive(true);

            cachedTrans.position = rewardStartPos;

            cachedTrans.DOScale(0, 0).onComplete = () =>
            {
                cachedTrans.DOScale(1, 0.2f);
                cachedTrans.DOBlendableMoveBy(new Vector3(0, 0.22f, 0), 1f).SetEase(Ease.InSine);
                obj.GetComponent<CanvasGroup>().DOFade(0, 0.4f).SetDelay(0.6f).onComplete = () =>
                {
                    slot.OnRelease();
                    UnityUtility.UnloadInstance(obj);
                };
            };
        });

        GameManager.Objective.OnObjectiveClaim(m_Data, m_CurToggleIndex == 1);
        objectivePanel.m_Scrollbars[m_CurToggleIndex].Draggable = false;

        objectivePanel.m_LayoutGroups[m_CurToggleIndex].enabled = true;
        GetComponent<CanvasGroup>().DOFade(0, 0.25f);
        var animTween = transform.DOScale(new Vector3(1, 0, 1), 0.5f);
        animTween.onUpdate = () =>
        {
            objectivePanel.m_LayoutGroups[m_CurToggleIndex].SetLayoutVertical();
        };

        animTween.onComplete = () =>
        {
            if (m_CurToggleIndex == 1)
            {
                objectivePanel.m_AllTimeScrollViews.UpdateData(GameManager.Objective.CurAllTimeObjectiveIds);
            }
            else
            {
                List<int> dailyIds = new List<int>(GameManager.Objective.CurDailyObjectiveIds.Count + 1) { -1 };
                dailyIds.AddRange(GameManager.Objective.CurDailyObjectiveIds);
                objectivePanel.m_DailyScrollView.UpdateData(dailyIds);
            }
            objectivePanel.m_Scrollbars[m_CurToggleIndex].Draggable = true;
            objectivePanel.m_LayoutGroups[m_CurToggleIndex].enabled = false;
        };

        GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());

        if (rewardType == TotalItemData.Coin)
            GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(0, null));
        else if (rewardType == TotalItemData.Life || rewardType == TotalItemData.InfiniteLifeTime)
            GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(0, null));

        if (m_CurToggleIndex == 0)
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Objective_Daily_Claim);
        else
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Objective_AllTime_Claim);
    }
}
