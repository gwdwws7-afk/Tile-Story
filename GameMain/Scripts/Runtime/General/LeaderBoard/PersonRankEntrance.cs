using DG.Tweening;
using GameFramework.Event;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PersonRankEntrance : EntranceUIForm
{
    [SerializeField] private TextMeshProUGUI rankText, tagText;
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private Transform tagTransform, mainImageTransform;
    [SerializeField] private GameObject claimText;
    [SerializeField] private ParticleSystem reachEffect;
    [SerializeField] private Image cupImage, bgImage;
    [SerializeField] private GameObject shineEffect;
    [SerializeField] private GameObject banner, previewBanner;
    [SerializeField] private TextMeshProUGUILocalize unlockText;

    private bool _countdownOver;
    private AsyncOperationHandle _assetHandle1, _assetHandle2;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        if (SystemInfoManager.DeviceType <= DeviceType.Normal)
            shineEffect.SetActive(false);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        base.OnHide(hideSuccessAction, userData);
    }

    public override bool CheckInitComplete()
    {
        return _assetHandle1.IsDone && _assetHandle2.IsDone;
    }

    public void InitEntrance()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventRecieved);
        GameManager.Event.Subscribe(CommonEventArgs.EventId, OnEventRecieved);
        SetTimer();
        SetRank();
        var fontMatName = "Top_Caption";
        switch (GameManager.Task.PersonRankManager.RankLevel)
        {
            case PersonRankLevel.Bronze:
                fontMatName = "Title_Bronze";
                break;
            case PersonRankLevel.Silver:
                fontMatName = "Title_Silver";
                break;
            case PersonRankLevel.Gold:
                fontMatName = "Title_Gold";
                break;
            case PersonRankLevel.Supreme:
                fontMatName = "Title_Supreme";
                break;
        }
        GameManager.Localization.GetPresetMaterialAsync(fontMatName, "BANGOPRO SDF", mat =>
        {
            rankText.fontMaterial = mat;
        });

        GameManager.Task.PersonRankManager.GetPersonRankDataFromServer((flag) =>
        {
            if (flag)
            {
                SetRank();
            }
        });

        UnityUtility.UnloadAssetAsync(_assetHandle1);
        string cupName = $"Cup{(int)GameManager.Task.PersonRankManager.RankLevel + 1}";
        _assetHandle1 = UnityUtility.LoadSpriteAsync(cupName, "Map", sp =>
         {
             cupImage.sprite = sp;
             //UnityUtility.UnloadAssetAsync(_assetHandle1);
         });

        UnityUtility.UnloadAssetAsync(_assetHandle2);
        string bgName = "personRankBg1";
        if (GameManager.Task.PersonRankManager.RankLevel == PersonRankLevel.Supreme)
            bgName = "personRankBg2";
        _assetHandle2 = UnityUtility.LoadSpriteAsync(bgName, "Map", sp =>
        {
            bgImage.sprite = sp;
            //UnityUtility.UnloadAssetAsync(_assetHandle2);
        });

        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockPersonRankGameLevel)
        {
            OnLocked();
        }
        else
        {
            OnUnlocked();
        }
    }

    private void OnEventRecieved(object sender, GameEventArgs e)
    {
        var ne = e as CommonEventArgs;
        if (ne == null) return;
        if (ne.Type == CommonEventType.PersonRankChanged)
        {
            SetRank();
        }
    }

    private void SetRank()
    {
        var rank = GameManager.Task.PersonRankManager.NeedToFlyMedal
            ? GameManager.Task.PersonRankManager.LastRank
            : GameManager.Task.PersonRankManager.Rank;
        if (rank == 0 || Application.internetReachability == NetworkReachability.NotReachable)
        {
            rankText.text = " ";
            return;
        }

        rankText.text = rank.ToString();
    }

    public override void OnReset()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventRecieved);

        base.OnReset();
    }

    private void OnDestroy()
    {
        UnityUtility.UnloadAssetAsync(_assetHandle1);
        UnityUtility.UnloadAssetAsync(_assetHandle2);
        // rankText.fontMaterial = null;
    }

    public void FlyMedal()
    {
        var flyNum = GameManager.Task.PersonRankManager.ScoreInGame;
        if (flyNum <= 0 || !GameManager.Task.PersonRankManager.NeedToFlyMedal)
            return;
        tagText.text =
            $"x{GameManager.Task.PersonRankManager.GetMultipleNum(GameManager.Task.PersonRankManager.LastContinuousWinTime)}";
        tagText.color = new Color(30 / 255f, 60 / 255f, 195 / 255f);

        int level = Mathf.Clamp((int)GameManager.Task.PersonRankManager.RankLevel, 0, 3);
        var cupName = $"Cup{level + 1}";
        EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent($"Map[{cupName}]", flyNum, 21,
            new Vector3(250f, 0),
            Vector3.zero, gameObject,
            () =>
            {
                Log.Info("Start Fly Medal");
                tagTransform.gameObject.SetActive(true);
                tagTransform.DOLocalMoveX(126f, 0.5f).SetEase(Ease.Linear);
                GameManager.Task.PersonRankManager.NeedToFlyMedal = false;
                GameManager.Task.PersonRankManager.ScoreInGame = 0;
            }, () =>
            {
                Log.Info("End Fly Medal");
                tagText.text =
                    $"x{GameManager.Task.PersonRankManager.GetMultipleNum(GameManager.Task.PersonRankManager.ContinuousWinTime)}";
                if (GameManager.Task.PersonRankManager.ContinuousWinTime >
                    GameManager.Task.PersonRankManager.LastContinuousWinTime)
                    tagText.color = new Color(16 / 255f, 96 / 255f, 12 / 255f);
                tagTransform.DOLocalMoveX(0f, 0.4f).SetEase(Ease.Linear).SetDelay(0.5f).onComplete += () =>
                {
                    tagTransform.gameObject.SetActive(false);
                };

                mainImageTransform.DOScale(0.85f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
                {
                    EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
                    mainImageTransform.DOScale(1f, 0.15f).onComplete = () =>
                    {

                    };
                };
                if (reachEffect != null)
                {
                    reachEffect.Play();
                }
                GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Playing);
                GameManager.Task.PersonRankManager.LastRank = GameManager.Task.PersonRankManager.Rank;
                SetRank();
            }, false);
    }

    private void SetTimer()
    {
        var endTime = GameManager.Task.PersonRankManager.EndTime;
        if (endTime <= DateTime.Now)
        {
            claimText.SetActive(true);
            countdownTimer.timeText.gameObject.SetActive(false);
            GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Finished);
        }
        else
        {
            // countdownTimer.gameObject.SetActive(true);
            claimText.SetActive(false);
            countdownTimer.timeText.gameObject.SetActive(true);
            countdownTimer.StartCountdown(endTime);
            countdownTimer.CountdownOver += OnCountdownOver;
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        if (_countdownOver) return;
        _countdownOver = true;
        claimText.SetActive(true);
        countdownTimer.timeText.gameObject.SetActive(false);
        GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Finished);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (!SystemInfoManager.IsSuperLowMemorySize && Time.realtimeSinceStartup - GameManager.Task.PersonRankManager.LastUpdateTime > 300f) 
        {
            GameManager.Task.PersonRankManager.LastUpdateTime = Time.realtimeSinceStartup;
            GameManager.Task.PersonRankManager.GetPersonRankDataFromServer((flag) =>
            {
                if (flag)
                {
                    SetRank();
                }
            }, true);
        }
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public override void OnButtonClick()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameManager.UI.ShowWeakHint("PersonRank.Please check your internet connection", Vector3.zero);
            return;
        }

        if (GameManager.Task.PersonRankManager.TaskState == PersonRankState.End || GameManager.Task.PersonRankManager.TaskState == PersonRankState.SendingReward)
            return;

        if (GameManager.Task.PersonRankManager.TaskState == PersonRankState.Playing)
        {
            GameManager.UI.ShowUIForm("PersonRankMenu");
            return;
        }

        if (GameManager.Task.PersonRankManager.TaskState == PersonRankState.Finished)
        {
            GameManager.UI.ShowUIForm("PersonRankFinishedMenu");
            return;
        }

        GameManager.UI.ShowUIForm("PersonRankWelcomeMenu");
        GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.PersonRank_IconClick);
    }

    public override void OnLocked()
    {
        if (IsLocked)
            return;

        banner.SetActive(false);
        previewBanner.SetActive(true);
        unlockText.SetParameterValue("level", Constant.GameConfig.UnlockPersonRankGameLevel.ToString());

        bgImage.color = Color.gray;
        cupImage.color = Color.gray;
        shineEffect.SetActive(false);

        base.OnLocked();
    }

    public override void OnUnlocked()
    {
        if (!IsLocked)
            return;

        banner.SetActive(true);
        previewBanner.SetActive(false);

        bgImage.color = Color.white;
        cupImage.color = Color.white;
        if (SystemInfoManager.DeviceType > DeviceType.Normal)
            shineEffect.SetActive(true);

        base.OnUnlocked();
    }
}