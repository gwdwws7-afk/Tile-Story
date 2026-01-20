using Spine.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


/// <summary>
/// ���ٽ����ϵ�GameObject
/// </summary>
public class ClimbBeanstalkGO : MonoBehaviour
{
    public Image finishIcon;
    public Image platformImage;
    public SkeletonGraphic chestSpine;
    public RectTransform chestShadow;
    public TextMeshProUGUI stageText;

    public Button chestButton;
    public GameObject tick;

    [SerializeField]
    private Transform contentRoot;
    [SerializeField]
    private Transform leftPosRef;
    [SerializeField]
    private Transform rightPosRef;
    [SerializeField]
    private GameObject decorationFor4x;
    [SerializeField]
    private GameObject decorationFor2x;

    [SerializeField]
    private SkeletonGraphic lightSpine;

    [SerializeField]
    private float claimRewardDelayTime = 0.5f;

    private int stage;
    private ClimbBeanstalkTaskDatas data;
    protected ClimbBeanstalkMenu climbBeanstalkMenu;

    protected int eggEffectIndex;
    protected string chestOpenAnimName;

    //private List<ResourceOperationHandle> loadAssetList = new List<ResourceOperationHandle>();

    public void Init(ClimbBeanstalkTaskDatas stageData, ClimbBeanstalkMenu menu)
    {
        data = stageData;
        stage = data.TargetNum;

        climbBeanstalkMenu = menu;

        chestButton.onClick.RemoveAllListeners();

        chestButton.onClick.AddListener(OnChestButtonClick);


        stageText.text = stageData.TargetNum.ToString();


        InitChest();
        InitPlatformImage();
        ChangeToLeftOrRightAccordingToParity();

        //����GO�� ���ݵ���˸
        if (lightSpine != null)
        {
            lightSpine.Initialize(true);
            lightSpine.AnimationState.SetAnimation(0, "idle02", true);
            float randomTime = Random.Range(0, 5.0f);
            lightSpine.AnimationState.GetCurrent(0).TrackTime = randomTime;
        }

        bool isFinished = ClimbBeanstalkManager.Instance.LastWinStreakNum >= stage;
        // bool isFinished = ClimbBeanstalkManager.Instance.CurrentWinStreak >= stage;
        Refresh(isFinished, true);
    }

    public void Release()
    {
        //for (int i = 0; i < loadAssetList.Count; i++)
        //{
        //    GameManager.Resource.Release(loadAssetList[i]);
        //}
        //loadAssetList.Clear();


        data = null;
    }

    private void InitChest()
    {
        //Log.Warning($"GO InitChest called {stage}");
        bool isGetReward = stage <= ClimbBeanstalkManager.Instance.HighestClaimRewardNum;
        tick.gameObject.SetActive(data.ChestType > 0 && isGetReward);

        string animName = null;

        chestSpine.gameObject.SetActive(data.ChestType > 0);
        chestButton.gameObject.SetActive(data.ChestType > 0 && !isGetReward);
        if (chestShadow != null)
            chestShadow.gameObject.SetActive(data.ChestType > 0 && !isGetReward);
        GetAnimName(data.ChestType, isGetReward);
    }

    public virtual void GetAnimName(int chestType, bool isGetReward)
    {
        string animName = null;
        switch (chestType)
        {
            case 1:
                chestOpenAnimName = "01";
                animName = isGetReward ? "01-2" : "01-3";
                chestSpine.AnimationState.SetAnimation(0, animName, false);
                break;
            case 2:
                chestOpenAnimName = "02";
                animName = isGetReward ? "02-2" : "02-3";
                chestSpine.AnimationState.SetAnimation(0, animName, false);
                break;
            case 3:
                chestOpenAnimName = "03";
                animName = isGetReward ? "03-2" : "03-3";
                chestSpine.AnimationState.SetAnimation(0, animName, false);
                break;
            case 4:
                chestOpenAnimName = "04";
                animName = isGetReward ? "04-2" : "04-3";
                chestSpine.AnimationState.SetAnimation(0, animName, false);
                break;
            default:
                break;
        }
    }

    private void InitPlatformImage()
    {
        if (data.ChestType > 0)
            platformImage.gameObject.SetActive(true);
        else
            platformImage.gameObject.SetActive(false);

        //string platformImageKey;
        //switch (data.ChestType)
        //{
        //    case 0:
        //        platformImageKey = "platform_1";
        //        platformImage.transform.localPosition = new Vector3(-111f, 297f, 0);
        //        break;
        //    case 1:
        //        platformImageKey = "platform_2";
        //        platformImage.transform.localPosition = new Vector3(-111f, 292.3f, 0);
        //        break;
        //    case 2:
        //        platformImageKey = "platform_3";
        //        platformImage.transform.localPosition = new Vector3(-111f, 272.1f, 0);
        //        break;
        //    default:
        //        platformImageKey = "platform_3";
        //        platformImage.transform.localPosition = new Vector3(-111f, 272.1f, 0);
        //        break;
        //}

        //ResourceOperationHandle<Sprite> handle = GameManager.Resource.LoadAtlasSpriteAsync(platformImageKey, "BallonRiseAtlas");
        //handle.Completed += result =>
        //{
        //    if (result.Status == ResourceOperationStatus.Succeeded)
        //    {
        //        platformImage.sprite = result.Result;
        //        platformImage.SetNativeSize();
        //    }
        //};
        //loadAssetList.Add(handle);
    }

    private void ChangeToLeftOrRightAccordingToParity()
    {
        contentRoot.gameObject.SetActive(true);
        if (stage % 2 == 0)
        {
            contentRoot.localPosition = rightPosRef.localPosition;

        }
        else
        {
            contentRoot.localPosition = leftPosRef.localPosition;

        }

        if (decorationFor2x != null)
        {
            if (stage % 4 == 0)
            {
                decorationFor2x?.SetActive(false);
                decorationFor4x?.SetActive(true);
            }
            else if (stage % 2 == 0)
            {
                decorationFor2x?.SetActive(true);
                decorationFor4x?.SetActive(false);
            }
            else
            {
                decorationFor2x?.SetActive(false);
                decorationFor4x?.SetActive(false);
            }
        }
    }

    public float RefreshLayout(bool isSkipAnim, float inputAnimationTime = 0.7f)
    {
        bool isFinished = ClimbBeanstalkManager.Instance.CurrentWinStreak >= stage;

        bool isFinishedWithLastWinStreak = ClimbBeanstalkManager.Instance.LastWinStreakNum >= stage;
        if (!isFinishedWithLastWinStreak && isFinished)
        {
            GameManager.Task.AddDelayTriggerTask(inputAnimationTime, () =>
            {
                Refresh(isFinished, isSkipAnim);
            });
            return inputAnimationTime;
        }
        else
        {
            Refresh(isFinished, isSkipAnim);
            return 0;
        }
    }

    private void Refresh(bool isFinished, bool isSkipAnim)
    {
        //Log.Info($"ClimbBeanstalkGO Stage = {stage} isFinished = {isFinished} isSkipAnim = {isSkipAnim}");
        if (isFinished)
        {
            finishIcon.gameObject.SetActive(true);
            //if (isSkipAnim)
            //    finishIcon.fillAmount = isFinished ? 1 : 0;
            //else
            //    finishIcon.DOFillAmount(1, 0.4f);
        }
        else
        {
            //finishIcon.fillAmount = 0;
            finishIcon.gameObject.SetActive(false);
        }

        if (isFinished)
        {
            GameManager.Localization.GetPresetMaterialAsync("Btn_Green", stageText.font.name, mat =>
            {
                stageText.fontSharedMaterial = mat;
            });
        }
        else
        {
            GameManager.Localization.GetPresetMaterialAsync("Btn_Yellow", stageText.font.name, mat =>
            {
                stageText.fontSharedMaterial = mat;
            });
        }

        // 添加data为空的判断防止通过控制台一次性增加大量道具导致领取奖励报错
        if (isFinished && data != null && data.ChestType > 0)
        {
            if (stage > ClimbBeanstalkManager.Instance.HighestClaimRewardNum)
            {
                if (!isSkipAnim)
                {
                    GameManager.Task.AddDelayTriggerTask(claimRewardDelayTime, () =>
                    {
                        ClaimReward();
                    });
                }
            }
        }
    }



    public bool CheckInitComplete()
    {
        //for (int i = 0; i < loadAssetList.Count; i++)
        //{
        //    if (!loadAssetList[i].IsDone)
        //        return false;
        //}

        return true;
    }

    private void OnChestButtonClick()
    {
        if (climbBeanstalkMenu != null)
        {
            climbBeanstalkMenu.ShowRewardTip(stage, data.RewardTypeList, data.RewardNumList, chestButton.transform.position);
        }
    }


    private void ClaimReward()
    {
        //chestSpine.AnimationState.SetAnimation(0, chestAnimName, false);

        //��������
        ClimbBeanstalkManager.Instance.HighestClaimRewardNum = stage;

        //����
        List<TotalItemData> rewardIDs = data.RewardTypeList;
        List<int> rewardNums = data.RewardNumList;

        for (int i = 0; i < rewardIDs.Count; ++i)
        {
            RewardManager.Instance.AddNeedGetReward(rewardIDs[i], rewardNums[i]);
        }

        ShowNeedGetReward();
    }

    public virtual void ShowNeedGetReward()
    {
        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.ClimbBeanstalkChestRewardPanel, false, () =>
        {
            //if (ClimbBeanstalkManager.Instance.CheckActivityState())
            //    return;

            GameManager.Process.EndProcess(ProcessType.CheckClimbBeanstalk);
        }, () =>
        {
        }, () =>
        {
            GameManager.UI.HideUIForm("GlobalMaskPanel");
            ClimbBeanstalkChestRewardPanel panel = RewardManager.Instance.RewardPanel as ClimbBeanstalkChestRewardPanel;
            panel.SetChestTypeAndPosition(chestOpenAnimName, chestSpine.transform.position);
            panel.SetOnShowCallback(() =>
            {
                chestSpine.gameObject.SetActive(false);
                if (chestShadow != null)
                    chestShadow.gameObject.SetActive(false);
            });//��ȷ�� ClimbBeanstalkChestRewardPanel ��סʱ �Űѽ����ϵ����Ӻͼ�Ӱ������

            GameManager.Task.AddDelayTriggerTask(1.0f, () =>
            {
                climbBeanstalkMenu?.OnClose();
            });
        });
    }

    private void OnDestroy()
    {
        chestShadow = null;
        stageText.fontSharedMaterial = null;
    }
}
