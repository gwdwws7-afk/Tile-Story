using DG.Tweening;
using GameFramework.Event;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PersonRankWelcomeMenu : UIForm, ICustomOnEscapeBtnClicked
{
    public DelayButton closeButton, playButton, greyButton, infoButton;
    public ClockBar clock;
    public TextMeshProUGUILocalize buttonText, describeText,unlockDescribeText;
    [SerializeField] private Transform cachedTransform;
    [SerializeField] private Image bgImage, cupImage, curBg;
    private AsyncOperationHandle m_LoadAssetHandle;
    private bool m_CountdownOver;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        // title.SetTerm($"PersonRank.{GameManager.Task.PersonRankManager.RankLevel.ToString()} League");
        // GameManager.Event.Subscribe(CommonEventArgs.EventId, OnShowDelayGuide);
        SetTimer();
        closeButton.SetBtnEvent(CloseEvent);
        infoButton.SetBtnEvent(() => { GameManager.UI.ShowUIForm("PersonRankRules"); });
        playButton.SetBtnEvent(OnPlayButtonClick);
        greyButton.SetBtnEvent(OnGreyButtonClick);
        buttonText.SetParameterValue("level", Constant.GameConfig.UnlockPersonRankGameLevel.ToString());
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockPersonRankGameLevel)
        {
            playButton.gameObject.SetActive(false);
            greyButton.gameObject.SetActive(true);
            describeText.gameObject.SetActive(false);
            unlockDescribeText.gameObject.SetActive(true);
        }
        else
        {
            GameManager.Task.PersonRankManager.HasShownWelcome = true;
            playButton.gameObject.SetActive(true);
            greyButton.gameObject.SetActive(false);
            describeText.gameObject.SetActive(true);
            unlockDescribeText.gameObject.SetActive(false);
        }

        describeText.SetParameterValue("0", "<color=#FFDA05>");
        describeText.SetParameterValue("1", "</color>");

        if (m_LoadAssetHandle.IsValid())
            Addressables.Release(m_LoadAssetHandle);
        var cupName = $"RankCup{(int)GameManager.Task.PersonRankManager.RankLevel + 1}";
        // loadAssetHandle = Addressables.LoadAssetAsync<SpriteAtlas>("LeaderBoard3");
        // loadAssetHandle.Completed+= handle =>
        // {
        //     var atlas = handle.Result as SpriteAtlas;
        //     if (atlas != null)
        //     {
        //         var sprite = atlas.GetSprite(cupName);
        //         cupImage.sprite = sprite;
        //     }
        // };
        m_LoadAssetHandle = UnityUtility.LoadSpriteAsync(cupName, "LeaderBoard3", sp =>
          {
              cupImage.sprite = sp;

              if (!cupImage.gameObject.activeSelf)
                  cupImage.gameObject.SetActive(true);
          });

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        if (cachedTransform)
        {
            cachedTransform.DOKill();
            cachedTransform.localScale = Vector3.one;
            cachedTransform.DOScale(1.03f, 0.12f).onComplete = () =>
            {
                cachedTransform.DOScale(0.99f, 0.1f).onComplete = () =>
                {
                    cachedTransform.DOScale(1f, 0.1f);
                    m_IsAvailable = true;

                    // StartCoroutine(ShowDelayGuide());
                    // ShowPersonRankGuide();
                };
            };
        }

        if (bgImage != null)
        {
            bgImage.DOKill();
            bgImage.DOColor(new Color(1, 1, 1, 0.01f), 0).OnComplete(() =>
                {
                    bgImage.DOFade(1, 0.2f);
                });
        }
        gameObject.SetActive(true);

        GameManager.Sound.PlayUIOpenSound();

        showSuccessAction?.Invoke(this);

    }
    
    public override bool CheckInitComplete()
    {
        if (!m_LoadAssetHandle.IsDone)
        {
            return false;
        }
        
        return true;
    }

    private void ShowPersonRankGuide()
    {
        if (!GameManager.Task.PersonRankManager.HasShownPersonRankGuide &&
            GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockPersonRankGameLevel)
        {
            GameManager.UI.ShowUIForm("CommonGuideMenu",form =>
            {
                GameManager.Task.PersonRankManager.HasShownPersonRankGuide = true;
                form.gameObject.SetActive(false);
                var guideMenu = form.GetComponent<CommonGuideMenu>();
                var originParent = infoButton.transform.parent;
                infoButton.transform.SetParent(form.transform);
                // guideMenu.tipBox.gameObject.SetActive(false);
                var position = infoButton.transform.position + new Vector3(0.2f, 0, 0);
                guideMenu.ShowGuideArrow(position, position + new Vector3(0.15f, 0, 0), PromptBoxShowDirection.Left);

                // guideMenu.tipBox.transform.position = new Vector3(0, position.y + 0.45f, 0);
                guideMenu.OnShow(null, null);

                void ClickAction()
                {
                    infoButton.transform.SetParent(originParent);
                    GameManager.UI.HideUIForm(form);
                    guideMenu.guideImage.onTargetAreaClick = null;
                    infoButton.onClick.RemoveListener(ClickAction);
                }

                infoButton.onClick.AddListener(ClickAction);
            });
        }
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        OnReset();

        if (cachedTransform) cachedTransform.localScale = Vector3.one;
        gameObject.SetActive(false);

        if (UIGroup.UIFormCount == 0)
        {
            GameManager.Sound.PlayUIOpenSound();
        }
        base.OnHide(hideSuccessAction, userData);

        GameManager.Process.EndProcess(ProcessType.PersonRankFinish);
    }

    public override void OnReset()
    {
        m_CountdownOver = false;
        StopAllCoroutines();
        base.OnReset();
    }

    public override void OnRelease()
    {
        if (m_LoadAssetHandle.IsValid())
        {
            Addressables.Release(m_LoadAssetHandle);
            m_LoadAssetHandle = default;
        }
        base.OnRelease();
    }

    private void CloseEvent()
    {
        StopAllCoroutines();
        GameManager.Process.EndProcess(ProcessType.PersonRankStart);
        GameManager.UI.HideUIForm(this);
    }

    private void OnGreyButtonClick()
    {
    }

    private void OnShowDelayGuide(object sender, GameEventArgs gameEventArgs)
    {
        if (!(gameEventArgs is CommonEventArgs { Type: CommonEventType.PersonRankRuleHide }))
            return;
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockPersonRankGameLevel)
            return;
        StartCoroutine(ShowDelayGuide());
    }

    IEnumerator ShowDelayGuide()
    {
        yield return new WaitForSeconds(15f);
        GameManager.UI.ShowUIForm("FingerGuideMenu",form =>
        {
            form.gameObject.SetActive(false);
            var guideMenu = form.GetComponent<FingerGuideMenu>();
            var position = playButton.transform.position - new Vector3(0, 0.05f, 0);
            guideMenu.finger.transform.position = position;
            guideMenu.OnShow();
            guideMenu.tipBox.gameObject.SetActive(false);
            guideMenu.AutoHide = true;
        });
    }

    private void OnPlayButtonClick()
    {
        if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockPersonRankGameLevel)
        {
            // GameManager.UI.ShowWeakHint($"Unlock at level {Constant.GameConfig.UnlockPersonRankGameLevel}");
            return;
        }

        GameManager.Process.EndProcess(ProcessType.PersonRankStart);

        //ProcedureUtil.ProcedureMapToGame();
        GameManager.UI.HideUIForm(this);
        GameManager.DataNode.SetData<int>("CurLevelPlayType", (int)LevelPlayType.Play);
        GameManager.UI.ShowUIForm("LevelPlayMenu",UIFormType.PopupUI);
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        clock.OnUpdate(elapseSeconds, realElapseSeconds);
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void SetTimer()
    {
        var endTime = GameManager.Task.PersonRankManager.EndTime;
        if (endTime <= DateTime.Now)
        {
            clock.SetFinishState();
            GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Finished);
        }
        else
        {
            clock.gameObject.SetActive(true);
            clock.StartCountdown(endTime);
            clock.CountdownOver += OnCountdownOver;
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        if (m_CountdownOver) return;
        m_CountdownOver = true;
        clock.SetFinishState();
        GameManager.Task.PersonRankManager.SetTaskState(PersonRankState.Finished);
    }

    public void OnEscapeBtnClicked()
    {
        CloseEvent();
    }
}