using Coffee.UIExtensions;
using DG.Tweening;
using Firebase.Analytics;
using MySelf.Model;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class BalloonRisePlayerPanel : MonoBehaviour
{
    public NamePanelManager name;
    public Image avatar;
    public Slider slider;
    public Transform rankMark;
    public UIParticle effect;
    public RectTransform reward;
    public Image chest;
    public Button claimButton;
    public GameObject rankMine, rankOther;
    public TextMeshProUGUI rankText;

    private AsyncOperationHandle m_AvatarAssetHandle;
    private AsyncOperationHandle m_ChestAssetHandel;
    private BalloonRiseMainMenu m_MainMenu;
    private BalloonRisePlayerBase m_PlayerData;

    public bool IsSelf
    {
        get
        {
            if (m_PlayerData != null)
                return m_PlayerData.IsSelf;
            return false;
        }
    }
    
    public void OnInit(BalloonRisePlayerBase data, BalloonRiseMainMenu menu)
    {
        m_MainMenu = menu;
        m_PlayerData = data;

        name.OnInit(data.Name);
        SetAvatar(data.Avatar);

        SetSlider(data);

        rankMark.localScale = Vector3.one;
        rankMark.gameObject.SetActive(data.Rank == 1 && data.Score != 0);
        rankMine.SetActive(data.IsSelf);
        rankOther.SetActive(!data.IsSelf);
        if(data.IsSelf)
            rankText.color=new Color(0.8784f, 0.1765f, 0.2745f, 1f);
        else
            rankText.color=new Color(0f, 0.1725f, 0.8275f, 1f);
        rankText.text = data.Rank.ToString();
        rankMine.transform.parent.gameObject.SetActive(true);

        effect.gameObject.SetActive(false);
        reward.anchoredPosition = new Vector2(0, 285);
        SetChest(GameManager.Task.BalloonRiseManager.Stage);
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClaimButtonClick);
        claimButton.interactable = true;
    }

    private void SetAvatar(int headId)
    {
        string avatarSp = headId == 0 ? "HeadPortrait_0_0" : $"HeadPortrait_{headId}";
        avatar.sprite = null;
        UnityUtility.UnloadAssetAsync(m_AvatarAssetHandle);
        m_AvatarAssetHandle = UnityUtility.LoadAssetAsync<Sprite>(avatarSp, sp =>
        {
            avatar.sprite = sp;
        });
    }

    public void SetSlider(BalloonRisePlayerBase data)
    {
        RectTransform rectTransform = slider.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 57.5f);
        rectTransform.sizeDelta = new Vector2(0, -495);

        slider.value = (float)data.RecordScore / GameManager.Task.BalloonRiseManager.StageTarget;
        float endValue = (float)data.Score / GameManager.Task.BalloonRiseManager.StageTarget;
        // forwardTrail.SetActive(endValue > slider.value);
        // backTrail.SetActive(endValue < slider.value);
        float offset = Mathf.Abs(endValue - slider.value);
        if (!Mathf.Approximately(endValue, slider.value))
        {
            m_MainMenu.OnPause();
            //领奖
            if (data.Rank == 1 && data.Score >= GameManager.Task.BalloonRiseManager.StageTarget)
            {
                //两段
                // slider.DOValue(endValue, 0.7f).SetEase(Ease.InQuad);
                // GameManager.Task.AddDelayTriggerTask(0.6f, () =>
                // {
                //     rankMark.DOScale(0, 0.1f).onComplete = () =>
                //     {
                //         rankMark.gameObject.SetActive(false);
                //         effect.gameObject.SetActive(true);
                //         effect.Play();
                //     };
                //     slider.transform.DOLocalMoveY(370f, 0.4f).SetEase(Ease.Linear).onComplete = () =>
                //     {
                //         reward.DOAnchorPosY(48, 0.2f).onComplete = () =>
                //         {
                //             reward.DOAnchorPosY(35, 0.1f).onComplete = () =>
                //             {
                //                 reward.DOAnchorPosY(40, 0.1f);
                //                 mainMenu.OnResume();
                //             };
                //         };
                //     };
                // });

                //一段
                rectTransform.anchoredPosition = new Vector2(0, 217.5f);
                rectTransform.sizeDelta = new Vector2(0, -175);

                int? audioId = null;
#if UNITY_EDITOR
                audioId = GameManager.Sound.PlayAudio("SFX_Voyage_Forward_Back");
#endif
                slider.DOValue(endValue, 1f * offset).SetEase(Ease.InQuad).onComplete = () =>
                {
                    if (audioId != null)
                    {
                        GameManager.Sound.StopAudio(audioId.Value);
                        audioId = null;
                    }
                };
                
                GameManager.Task.AddDelayTriggerTask(0.75f * offset, () =>
                {
                    rankMine.transform.parent.gameObject.SetActive(false);
                    
                    rankMark.DOScale(0, 0.1f).onComplete = () =>
                    {
                        rankMark.gameObject.SetActive(false);
                        effect.gameObject.SetActive(true);
                        effect.Play();
                    };
                    GameManager.Task.AddDelayTriggerTask(0.25f * offset, () =>
                    {
                        GameManager.Sound.PlayAudio("SFX_Voyage_Success_Box");
                        reward.DOAnchorPosY(48, 0.2f).onComplete = () =>
                        {
                            reward.DOAnchorPosY(35, 0.1f).onComplete = () =>
                            {
                                reward.DOAnchorPosY(40, 0.1f);
                                m_MainMenu.ShowSettlementProcess();
                            };
                        };
                    });
                });
            }
            else
            {
                int? audioId = null;
#if UNITY_EDITOR
                audioId = GameManager.Sound.PlayAudio("SFX_Voyage_Forward_Back");
#endif
                slider.DOValue(endValue, 1f * offset).SetEase(Ease.InQuad).onComplete = () =>
                {
                    if (audioId != null)
                    {
                        GameManager.Sound.StopAudio(audioId.Value);
                        audioId = null;
                    }
                    m_MainMenu.OnResume();
                };
            }

            m_PlayerData.RecordScore = data.Score;
            if (data.IsSelf)
                BalloonRiseModel.Instance.RecordScore = data.Score;
        }
    }

    private void SetChest(int stage)
    {
        string chestSp = "宝箱1";
        switch (stage)
        {
            case 2:
                chestSp = "宝箱2";
                break;
            case 3:
                chestSp = "宝箱3";
                break;
        }

        chest.sprite = null;
        UnityUtility.UnloadAssetAsync(m_ChestAssetHandel);
        m_ChestAssetHandel = UnityUtility.LoadAssetAsync<Sprite>($"BalloonRise[{chestSp}]", sp =>
        {
            chest.sprite = sp;
            chest.SetNativeSize();
        });
    }

    private void OnClaimButtonClick()
    {
        if (m_PlayerData.IsSelf)
        {
            claimButton.interactable = false;
            int stageNum = GameManager.Task.BalloonRiseManager.Stage;
            BalloonRiseStage stage = GameManager.DataTable.GetDataTable<DTBalloonRiseReward>().Data.GetRewardByStageNum(stageNum);
            RewardManager.Instance.AddNeedGetReward(stage.RewardTypeList, stage.RewardNumList);
            GameManager.DataNode.SetData<Vector3>("BalloonChestPos", chest.transform.position);
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.BalloonRiseChestRewardPanel, false, () =>
            {
                GameManager.Task.BalloonRiseManager.Upgrade = true;
                GameManager.Firebase.RecordMessageByEvent("Balloon_Finish", new Parameter("Stage", stageNum));
                BalloonRiseModel.Instance.ResetStage();

                UIGroup uiGroup = GameManager.UI.GetUIGroup(UIFormType.LeftUI);
                MapTopPanelManager mapTopPanelManager = (MapTopPanelManager)GameManager.UI.GetUIForm("MapTopPanelManager");
                mapTopPanelManager.balloonRiseEntrance.OnInit(uiGroup);

                GameManager.Process.EndProcess(ProcessType.ShowBalloonRiseRewardProcess);
            });
        }
    }

    public void OnReset()
    {
        rankMark.DOKill();
        rankMark.localScale = Vector3.one;
        reward.DOKill();
        reward.anchoredPosition = new Vector2(0, 285);
        claimButton.onClick.RemoveAllListeners();
        UnityUtility.UnloadAssetAsync(m_AvatarAssetHandle);
        UnityUtility.UnloadAssetAsync(m_ChestAssetHandel);
        m_AvatarAssetHandle = default;
        m_ChestAssetHandel = default;
    }
}
