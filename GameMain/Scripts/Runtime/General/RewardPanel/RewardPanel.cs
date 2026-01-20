using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 奖励界面
/// </summary>
public class RewardPanel : MonoBehaviour
{
    [SerializeField] private GameObject ClearBg;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI tipText;
    public BlackBgManager blackBg;
    public GameObject tipBg;

    public RewardPanelType RewardPanelType { get; set; }
    
    public virtual RewardArea CustomRewardArea
    {
        get => null;
    }

    protected bool autoGetReward;

    private int taskId = 0;
    public void SetClearBgActive(bool isActive)
    {
        if(ClearBg)ClearBg.gameObject.SetActive(isActive);
        
        if (taskId != 0)
        {
            GameManager.Task.RemoveDelayTriggerTask(taskId);
            taskId = 0;
        }
        if (isActive)
        {
            taskId=GameManager.Task.AddDelayTriggerTask(8f, () =>
            {
                try
                {
                    if(ClearBg)ClearBg.gameObject.SetActive(false);
                }
                catch (Exception e)
                {
                    Log.Warning($"SetClearBgActive:{e.Message}");
                }
            });
        }
    }

    public virtual void OnInit(bool autoGetReward)
    {
        this.autoGetReward = autoGetReward;
    }

    public virtual void OnReset()
    {
        autoGetReward = false;
    }

    public virtual void OnRelease()
    {
    }

    public virtual void OnShow(RewardArea rewardArea, Action onShowComplete)
    {
        transform.SetAsFirstSibling();

        blackBg.OnShow();

        if (RewardManager.Instance.ForceHideRewardPanelBg)
        {
            blackBg.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
        }

        Transform titleTrans = titleText.transform;
        Transform tipTrans = ReferenceEquals(tipBg, null) ? tipText.transform : tipBg.transform;

        rewardArea.OnShow(() =>
        {
            //if (!autoGetReward)
            {
                tipText.gameObject.SetActive(true);
                if (!ReferenceEquals(tipBg, null))
                {
                    tipBg.SetActive(true);
                }

                tipTrans.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            }

            onShowComplete?.Invoke();
        });

        titleTrans.localScale = Vector3.zero;
        tipTrans.localScale = Vector3.zero;

        titleText.gameObject.SetActive(true);
        titleTrans.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            try
            {
                titleTrans.DOScale(1f, 0.2f);
            }
            catch (Exception e)
            {
                OnHide(true,null);
                Debug.LogError(e.Message);
            }
        };
    }

    public virtual void OnHide(bool quickHide, Action onHideComplete)
    {
        blackBg.OnHide(quickHide ? 0 : 0.2f);

        if (!quickHide)
        {
            Transform titleTrans = titleText.transform;
            Transform tipTrans = ReferenceEquals(tipBg, null) ? tipText.transform : tipBg.transform;

            titleTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                titleTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    try
                    {
                        titleText.gameObject.SetActive(false);
                        titleTrans.localScale = Vector3.one;
                    }
                    catch (Exception e)
                    {
                        OnHide(true,null);
                        Debug.LogError(e.Message);
                    }
                };
            };

            tipTrans.DOScale(new Vector3(1.05f, 1.05f), 0.2f).onComplete += () =>
            {
                tipTrans.DOScale(Vector3.zero, 0.2f).onComplete += () =>
                {
                    try
                    {                    
                        tipText.gameObject.SetActive(false);
                        if (!ReferenceEquals(tipBg, null))
                        {
                            tipBg.SetActive(false);
                        }
                        tipTrans.localScale = Vector3.one;

                        onHideComplete?.Invoke();
                    }
                    catch (Exception e)
                    {
                        OnHide(true,null);
                        Debug.LogError(e.Message);
                    }
                };
            };
        }
        else
        {
            titleText.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);
            if (!ReferenceEquals(tipBg, null))
            {
                tipBg.SetActive(false);
            }

            onHideComplete?.Invoke();
        }
    }

    public virtual void SetOnClickEvent(UnityAction onClick)
    {
        blackBg.clickButton.onClick.RemoveAllListeners();
        blackBg.clickButton.onClick.AddListener(onClick);
    }

    public virtual void ClearOnClickEvent()
    {
        blackBg.clickButton.onClick.RemoveAllListeners();
    }
}