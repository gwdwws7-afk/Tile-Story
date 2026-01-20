using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlacierQuestRewardSet : UIForm
{
    [SerializeField] private Transform title;
    [SerializeField] private DelayButton bgButton;
    [SerializeField] private Transform youWin;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUILocalize rewardDesText;
    [SerializeField] private Image myHeadIcon;
    [SerializeField] private Image[] headIcon;
    [SerializeField] private SkeletonGraphic goldMountainSpine;
    [SerializeField] private Transform tapText;

    private int m_CoinNum = 0;
    private Action m_ClaimEvent;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        bgButton.SetBtnEvent(GetReward);
        bgButton.interactable = false;

        m_CoinNum = GameManager.Task.GlacierQuestTaskManager.GetRewardCoinNum();
        coinText.text = "0";
        rewardDesText.SetParameterValue("0", (GameManager.Task.GlacierQuestTaskManager.SuccessPeopleNum - 1).ToString());
        int[] iconIds = GameManager.Task.GlacierQuestTaskManager.SuccessHeadIdArray;
        if (iconIds.Length <= 0)
        {
            Log.Info("GlacierQuest：头像数据为空");
            return;
        }

        ShowHeadIcon(myHeadIcon, iconIds[0], true);

        int showPeopleNum = Mathf.Min(GameManager.Task.GlacierQuestTaskManager.SuccessPeopleNum - 1, headIcon.Length);
        Vector3[] posList = UnityUtility.GetAveragePosition(Vector3.zero, new Vector3(160, 0, 0), showPeopleNum);
        for (int i = 0; i < headIcon.Length; i++)
        {
            if (i + 1 < GameManager.Task.GlacierQuestTaskManager.SuccessPeopleNum)
            {
                ShowHeadIcon(headIcon[i], iconIds[i + 1], false);
                headIcon[i].transform.parent.localPosition = posList[i];
            }

            headIcon[i].transform.localScale = Vector3.zero;
        }

        title.localScale = Vector3.zero;
        youWin.localScale = Vector3.zero;
        rewardDesText.transform.localScale = Vector3.zero;
        tapText.localScale = Vector3.zero;

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnReset()
    {
        for (int i = 0; i < headIcon.Length; i++)
        {
            headIcon[i].transform.DOKill();
        }
        tapText.DOKill();

        foreach (var pair in headSprites)
        {
            UnityEngine.AddressableAssets.Addressables.Release(pair.Value);
        }
        headSprites.Clear();

        base.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        GameManager.Sound.PlayAudio("Glacier_Final_Success");
        //bgButton.interactable = false;

        GameManager.Task.AddDelayTriggerTask(2.3f, () =>
        {
            bgButton.interactable = true;
        });

        goldMountainSpine.Initialize(false);
        goldMountainSpine.AnimationState.SetAnimation(0, "active", false);

        title.DOScale(1.1f, 0.1f).SetEase(Ease.OutCubic).onComplete = () =>
        {
            title.DOScale(1f, 0.1f).SetEase(Ease.InQuad);
        };

        youWin.DOScale(1.1f, 0.1f).SetEase(Ease.OutCubic).SetDelay(0.2f).onComplete = () =>
        {
            youWin.DOScale(1f, 0.1f).SetEase(Ease.InQuad);
        };

        int startValue = 0;
        var anim= DOTween.To(() => startValue, t => startValue = t, m_CoinNum, 1.1f).SetDelay(0.4f);
        anim.onUpdate = () =>
        {
            coinText.text = startValue.ToString();
        };
        anim.onComplete = () =>
        {
            coinText.transform.DOScale(1.35f, 0.22f).SetEase(Ease.OutCubic).onComplete = () =>
            {
                coinText.transform.DOScale(1f, 0.15f).SetEase(Ease.InCubic);
            };
        };

        float delayTime = 1.5f;
        rewardDesText.transform.DOScale(1.1f, 0.15f).SetEase(Ease.OutCubic).SetDelay(delayTime).onComplete = () =>
        {
            rewardDesText.transform.DOScale(1f, 0.15f).SetEase(Ease.InQuad);
        };

        delayTime += 0.3f;
        for (int i = 0; i < headIcon.Length; i++)
        {
            if (i + 1 < GameManager.Task.GlacierQuestTaskManager.SuccessPeopleNum)
            {
                delayTime += 0.15f;
                var head=headIcon[i];
                head.transform.DOScale(0.85f, 0.15f).SetEase(Ease.OutCubic).SetDelay(delayTime).onComplete = () =>
                {
                    head.transform.DOScale(0.72f, 0.15f).SetEase(Ease.InQuad);
                };
            }
        }

        delayTime += 0.15f;
        tapText.DOScale(1.1f, 0.15f).SetEase(Ease.OutCubic).SetDelay(delayTime).onComplete = () =>
        {
            tapText.DOScale(1f, 0.15f).SetEase(Ease.InQuad);
        };

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        base.OnHide(hideSuccessAction, userData);
    }

    public void GetReward()
    {
        GameManager.Task.GlacierQuestTaskManager.ClearRewardData();
        // 返回主界面后发放奖励
        RewardManager.Instance.AddNeedGetReward(TotalItemData.Coin, m_CoinNum);
        GameManager.Firebase.RecordCoinGet("LavaQuest", m_CoinNum);
        GameManager.Task.GlacierQuestTaskManager.CheckIfNeedSetStateToWaitAgain();
        if (m_ClaimEvent == null)
            GameManager.UI.HideUIForm(this);
        m_ClaimEvent?.Invoke();
        m_ClaimEvent = null;
    }

    private Dictionary<int, Sprite> headSprites = new Dictionary<int, Sprite>();

    private void ShowHeadIcon(Image image, int headId, bool isSelf)
    {
        if (!headSprites.TryGetValue(headId, out Sprite sprite))
        {
            string headName = $"HeadPortrait_{headId}{(isSelf ? $"_{headId}" : "")}";
            UnityUtility.LoadAssetAsync<Sprite>(headName, s =>
            {
                if (!headSprites.Keys.Contains(headId))
                    headSprites.Add(headId, s);
                image.sprite = headSprites[headId];
            });
        }
        else
        {
            image.sprite = sprite;
        }
    }

    public void SetClaimEvent(Action action)
    {
        m_ClaimEvent = action;
    }
}
