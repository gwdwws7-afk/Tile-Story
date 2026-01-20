using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ObjectiveLevelColumn : MonoBehaviour
{
    public Image m_ObjectiveImage;
    public Image m_RewardImage;
    public TextMeshProUGUI m_RewardText;
    public TextMeshProUGUILocalize m_TitleText;
    public GameObject m_RewardTitle;
    public Button m_ClaimButton;
    public SimpleSlider m_Slider;
    public GameObject m_Tick;

    private ObjectiveData m_Data;
    private bool m_IsAllTimeObjective = false;
    private AsyncOperationHandle m_ObjectiveHandle;
    private AsyncOperationHandle m_RewardHandle;
    private LevelBanner_Objective m_Banner;
    private int m_DelayTaskId;

    public void OnInitialize(ObjectiveData data, bool isAllTimeObjective, bool isCompleted, LevelBanner_Objective banner)
    {
        m_Data = data;
        m_IsAllTimeObjective = isAllTimeObjective;
        m_Banner = banner;

        m_ClaimButton.SetBtnEvent(OnClaimButtonClick);
        m_ClaimButton.interactable = true;

        if (m_Data != null)
        {
            //title
            m_TitleText.SetTerm(ObjectiveColumn.GetTitleTerm(m_Data.Type));
            m_TitleText.SetParameterValue("Num", m_Data.TargetNum.ToString());

            if (GameManager.Localization.Language == Language.Russian)
                m_TitleText.Target.lineSpacing = -20;
            else
                m_TitleText.Target.lineSpacing = 0;

            //objective
            if (m_ObjectiveHandle.IsValid())
                UnityUtility.UnloadAssetAsync(m_ObjectiveHandle);
            m_ObjectiveHandle = UnityUtility.LoadSpriteAsync(m_Data.Type.ToString(), "Objective", sp =>
            {
                m_ObjectiveImage.sprite = sp;
                m_ObjectiveImage.SetNativeSize();
            });
            
            //button
            if (isCompleted)
            {
                m_ClaimButton.gameObject.SetActive(true);
                m_Tick.SetActive(false);
                m_RewardTitle.SetActive(false);
                m_RewardText.transform.parent.localPosition = new Vector3(280, 72, 0);

                //progress
                m_Slider.Value = 1;
                m_Slider.sliderText.text = $"{m_Data.TargetNum} / {m_Data.TargetNum}";
            }
            else
            {
                m_ClaimButton.gameObject.SetActive(false);
                m_Tick.SetActive(false);
                m_RewardTitle.SetActive(true);
                m_RewardText.transform.parent.localPosition = new Vector3(280, -30, 0);

                //progress
                m_Slider.Value = GameManager.Objective.GetObjectiveProgress(m_Data, isAllTimeObjective) / (float)m_Data.TargetNum;
                m_Slider.sliderText.text = $"{GameManager.Objective.GetObjectiveProgress(m_Data, isAllTimeObjective)} / {m_Data.TargetNum}";
            }

            //reward
            string rewardName = m_Data.RewardType.ToString();
            string atlasedSpriteAddress = $"TotalItemAtlas[{rewardName}]";
            if (m_RewardHandle.IsValid())
                UnityUtility.UnloadAssetAsync(m_RewardHandle);
            m_RewardHandle = UnityUtility.LoadSpriteAsync(rewardName, "TotalItemAtlas", sp =>
            {
                m_RewardImage.sprite = sp;
            });

            if (m_Data.RewardType == TotalItemData.Coin.TotalItemType)
            {
                m_RewardText.text = m_Data.RewardNum.ToString();
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
            }
        }
        else
        {
            m_ClaimButton.gameObject.SetActive(false);
            m_Tick.SetActive(true);
        }
    }

    public void OnRelease()
    {
        m_ClaimButton.onClick.RemoveAllListeners();

        m_Tick.transform.DOKill();

        if (m_ObjectiveHandle.IsValid())
        {
            UnityUtility.UnloadAssetAsync(m_ObjectiveHandle);
            m_ObjectiveHandle = default;
        }

        if (m_RewardHandle.IsValid())
        {
            UnityUtility.UnloadAssetAsync(m_RewardHandle);
            m_RewardHandle = default;
        }

        if (m_DelayTaskId != 0)
        {
            GameManager.Task.RemoveDelayTriggerTask(m_DelayTaskId);
        }
    }

    private void OnClaimButtonClick()
    {
        if (m_Data == null)
            return;

        m_ClaimButton.interactable = false;
        m_ClaimButton.gameObject.SetActive(false);

        if (!m_Tick.activeSelf)
        {
            m_Tick.transform.localScale = Vector3.one;
            m_Tick.SetActive(true);
            m_Tick.transform.DOScale(1.1f, 0.15f).onComplete = () =>
            {
                m_Tick.transform.DOScale(1f, 0.15f);
            };
        }

        //��ȡ����
        Vector3 rewardStartPos = new Vector3(m_ClaimButton.transform.position.x, m_RewardImage.transform.position.y);
        TotalItemData rewardType = TotalItemData.FromInt((int)m_Data.RewardType);
        int rewardNum = m_Data.RewardNum;
        RewardManager.Instance.SaveRewardData(rewardType, rewardNum, true);

        UnityUtility.InstantiateAsync("ObjectiveRewardGetTip", m_Banner.transform, obj =>
        {
            if (obj == null)
                return;
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
                cachedTrans.DOBlendableMoveBy(new Vector3(0, 0.22f, 0), 0.8f).SetEase(Ease.InSine);
                obj.GetComponent<CanvasGroup>().DOFade(0, 0.4f).SetDelay(0.4f).onComplete = () =>
                {
                    slot.OnRelease();
                    UnityUtility.UnloadInstance(obj);
                };
            };
        });

        GameManager.Objective.OnObjectiveClaim(m_Data, m_IsAllTimeObjective);

        if (rewardType == TotalItemData.Coin)
            GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(0, null));
        else if (rewardType == TotalItemData.Life || rewardType == TotalItemData.InfiniteLifeTime)
            GameManager.Event.Fire(this, LifeNumChangeEventArgs.Create(0, null));
        else if (rewardType == TotalItemData.MagnifierBoost
            || rewardType == TotalItemData.FireworkBoost
            || rewardType == TotalItemData.Prop_AddOneStep
            || rewardType == TotalItemData.InfiniteMagnifierBoost
            || rewardType == TotalItemData.InfiniteFireworkBoost
            || rewardType == TotalItemData.InfiniteAddOneStepBoost)
            GameManager.Event.Fire(this, CommonEventArgs.Create(CommonEventType.BoostNumChange));

        GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());

        if (!m_IsAllTimeObjective)
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Objective_Daily_Claim_Banner);
        else
            GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Objective_AllTime_Claim_Banner);

        m_DelayTaskId = GameManager.Task.AddDelayTriggerTask(0.4f, () =>
          {
              m_DelayTaskId = 0;
              m_Banner.RefreshObjectiveColumn();
          });
    }
}
