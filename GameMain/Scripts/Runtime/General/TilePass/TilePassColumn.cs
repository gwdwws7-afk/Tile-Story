using DG.Tweening;
using MySelf.Model;
using Spine.Unity;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TilePassColumn : MonoBehaviour
{
    public GameObject blackBG;

    public Slider slider;
    public GameObject taskStageUnlock;
    public GameObject taskStageLock;
    public TextMeshProUGUI taskStageText;
    public GameObject taskStageStar;
    public Transform indexLight;

    public Image freeRewardImage;
    public TextMeshProUGUI freeRewardText;
    public DelayButton freeClaimButton;
    public GameObject freeTick;
    public Button freeButton;
    public Transform freeLight;

    public Image vipRewardImage;
    public TextMeshProUGUI vipRewardText;
    public DelayButton vipClaimButton;
    public GameObject vipTick;
    public Button vipButton;
    public SkeletonGraphic lockSpine;
    public Transform vipLight;

    private TilePassMainMenu m_TilePassMainMenu;
    private TilePassData m_TilePassData;
    private int m_Index;
    private List<AsyncOperationHandle> m_LoadAssetHandleList = new List<AsyncOperationHandle>();

    public void OnInit(TilePassMainMenu mainMenu, TilePassData data)
    {
        m_TilePassMainMenu = mainMenu;
        m_TilePassData = data;
        m_Index = data.Index;

        freeButton.onClick.AddListener(OnFreeButtonClicked);
        vipButton.onClick.AddListener(OnVIPButtonClicked);
        freeClaimButton.onClick.AddListener(OnFreeClaimButtonClicked);
        vipClaimButton.onClick.AddListener(OnVIPClaimButtonClicked);

        if (m_Index == 0)
        {
            taskStageStar.SetActive(true);
            taskStageText.gameObject.SetActive(false);
        }
        else
        {
            taskStageStar.SetActive(false);
            taskStageText.gameObject.SetActive(true);
            taskStageText.SetText(m_Index.ToString());
        }
        indexLight.DOKill();
        freeLight.localEulerAngles = Vector3.zero;
        freeLight.localScale = Vector3.one;
        freeLight.GetComponent<Image>().color = new Color(1, 1, 1, 1);

        freeTick.transform.DOKill();
        freeTick.transform.localScale = Vector3.one;
        freeClaimButton.transform.DOKill();
        freeClaimButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        freeLight.DOKill();
        freeLight.localEulerAngles = Vector3.zero;
        freeLight.localScale = Vector3.one;
        freeLight.GetComponent<Image>().color = new Color(1, 1, 1, 1);

        vipTick.transform.DOKill();
        vipTick.transform.localScale = Vector3.one;
        vipClaimButton.transform.DOKill();
        vipClaimButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        vipLight.DOKill();
        vipLight.localEulerAngles = Vector3.zero;
        vipLight.localScale = Vector3.one;
        vipLight.GetComponent<Image>().color = new Color(1, 1, 1, 1);

        SetReward();
        SetStatus();
        lockSpine.gameObject.SetActive(!TilePassModel.Instance.IsVIP);
    }

    public void SetReward()
    {
        string freeRewardSpriteKey = "";
        string vipRewardSpriteKey = "";
        if (m_TilePassData.FreeRewardList.Count == 1)
        {
            freeRewardSpriteKey = UnityUtility.GetRewardSpriteKey(m_TilePassData.FreeRewardList[0], m_TilePassData.FreeRewardNumList[0]);
            freeRewardText.SetItemText(m_TilePassData.FreeRewardNumList[0], m_TilePassData.FreeRewardList[0], false);
            freeRewardText.gameObject.SetActive(true);
            freeRewardImage.transform.localPosition = new Vector3(-70, 20);
            freeRewardImage.transform.localScale = Vector3.one;
        }
        else
        {
            freeRewardSpriteKey = "Chest3";
            freeRewardText.gameObject.SetActive(false);
            freeRewardImage.transform.localPosition = new Vector3(-10, 10);
            freeRewardImage.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }
        if (m_TilePassData.VIPRewardList.Count == 1)
        {
            vipRewardSpriteKey = UnityUtility.GetRewardSpriteKey(m_TilePassData.VIPRewardList[0], m_TilePassData.VIPRewardNumList[0]);
            vipRewardText.SetItemText(m_TilePassData.VIPRewardNumList[0], m_TilePassData.VIPRewardList[0], false);
            vipRewardText.gameObject.SetActive(true);
            vipRewardImage.transform.localPosition = new Vector3(-42, 20);
            vipRewardImage.transform.localScale = Vector3.one;
        }
        else
        {
            vipRewardSpriteKey = "decorationChestSmall";
            vipRewardText.gameObject.SetActive(false);
            vipRewardImage.transform.localPosition = new Vector3(5, 10);
            vipRewardImage.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
        }
        LoadSpriteAsync(freeRewardSpriteKey, "TotalItemAtlas", sp =>
        {
            freeRewardImage.sprite = sp;
        });
        LoadSpriteAsync(vipRewardSpriteKey, "TotalItemAtlas", sp =>
        {
            vipRewardImage.sprite = sp;
        });
    }

    public void SetStatus()
    {
        if (m_Index <= m_TilePassMainMenu.lastRecordIndex)
        {
            blackBG.SetActive(false);
            slider.value = 1;
            taskStageUnlock.SetActive(true);
            taskStageLock.SetActive(false);
            GameManager.Localization.GetPresetMaterialAsync("Text_Pink", taskStageText.font.name, mat =>
            {
                taskStageText.fontSharedMaterial = mat;
            });

            bool isFreeClaimed = TilePassModel.Instance.CheckRewardGetStatus("TilePassFreeRewardGet" + m_Index.ToString());
            freeTick.SetActive(isFreeClaimed);
            freeClaimButton.gameObject.SetActive(!isFreeClaimed);
            //freeLight.gameObject.SetActive(!isFreeClaimed);
            //if (freeLight.gameObject.activeSelf)
            //{
            //    freeLight.DOFade(1f, 0.9f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);
            //}

            bool isVIPClaimed = TilePassModel.Instance.CheckRewardGetStatus("TilePassVIPRewardGet" + m_Index.ToString());
            vipTick.SetActive(isVIPClaimed);
            vipClaimButton.gameObject.SetActive(TilePassModel.Instance.IsVIP && !isVIPClaimed);
            //vipLight.gameObject.SetActive(TilePassModel.Instance.IsVIP && !isVIPClaimed);
            //if (vipLight.gameObject.activeSelf)
            //{
            //    vipLight.DOFade(1f, 0.9f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);
            //}
        }
        else
        {
            blackBG.SetActive(true);
            slider.value = 0;
            taskStageUnlock.SetActive(false);
            taskStageLock.SetActive(true);
            GameManager.Localization.GetPresetMaterialAsync("Text_Blue", taskStageText.font.name, mat =>
            {
                taskStageText.fontSharedMaterial = mat;
            });
            freeTick.SetActive(false);
            freeClaimButton.gameObject.SetActive(false);
            //freeLight.gameObject.SetActive(false);
            vipTick.SetActive(false);
            vipClaimButton.gameObject.SetActive(false);
            //vipLight.gameObject.SetActive(false);
        }
    }

    public float RefreshLayout()
    {
        float delay = 0f;
        if (m_Index < TilePassModel.Instance.CurrentIndex ||
            (m_Index == TilePassModel.Instance.CurrentIndex
            && TilePassModel.Instance.TotalTargetNum >= m_TilePassMainMenu.TilePassData.GetTotalTargetNum(m_Index)))
        {
            if (slider.value < 1)
            {
                delay = 0.5f;
                slider.DOValue(1, delay).onComplete = () =>
                {
                    taskStageUnlock.SetActive(true);
                    taskStageLock.SetActive(false);
                    GameManager.Localization.GetPresetMaterialAsync("Text_Pink", taskStageText.font.name, mat =>
                    {
                        taskStageText.fontSharedMaterial = mat;
                    });
                };
                blackBG.GetComponent<Image>().DOFade(0, delay).onComplete = () =>
                {
                    blackBG.SetActive(false);
                    blackBG.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
                };
            }
            else
            {
                blackBG.SetActive(false);
            }
        }

        return delay;
    }

    public void Unlock()
    {
        if (lockSpine.gameObject.activeSelf && TilePassModel.Instance.IsVIP)
        {
            lockSpine.AnimationState.SetAnimation(0, "idle", false).Complete += t =>
            {
                lockSpine.gameObject.SetActive(false);
            };
        }
    }

    public void RefreshRewardStatus(float delay = 0)
    {
        if (m_Index > TilePassModel.Instance.CurrentIndex ||
            (m_Index == TilePassModel.Instance.CurrentIndex
            && TilePassModel.Instance.TotalTargetNum < m_TilePassMainMenu.TilePassData.GetTotalTargetNum(m_Index)))
        {
            return;
        }

        bool isFreeClaimed = TilePassModel.Instance.CheckRewardGetStatus("TilePassFreeRewardGet" + m_Index.ToString());
        bool isVIPClaimed = TilePassModel.Instance.CheckRewardGetStatus("TilePassVIPRewardGet" + m_Index.ToString());

        if (isFreeClaimed && !freeTick.activeSelf)
        {
            freeClaimButton.gameObject.SetActive(false);
            freeLight.gameObject.SetActive(false);
            indexLight.gameObject.SetActive(false);

            freeTick.transform.localScale = Vector3.zero;
            freeTick.SetActive(true);
            freeTick.transform.DOScale(1.05f, 0.2f).onComplete = () =>
            {
                freeTick.transform.DOScale(1, 0.2f);
            };
        }
        if (!isFreeClaimed && !freeClaimButton.gameObject.activeSelf)
        {
            freeTick.SetActive(false);

            freeClaimButton.transform.localScale = Vector3.zero;
            freeClaimButton.gameObject.SetActive(true);
            freeClaimButton.transform.DOScale(0.42f, 0.2f).SetDelay(delay).onComplete = () =>
            {
                freeClaimButton.transform.DOScale(0.4f, 0.2f);
            };
            //左和中特效
            freeLight.gameObject.SetActive(true);
            freeLight.DOScale(2, 0.2f).onComplete = () =>
            {
                freeLight.DOScale(4, 0.4f).SetEase(Ease.OutCubic);
                freeLight.DOLocalRotate(new Vector3(0, 0, 50), 0.5f).SetEase(Ease.OutCubic);
                freeLight.GetComponent<Image>().DOFade(0f, 0.5f).SetEase(Ease.OutCubic).onComplete = () =>
                {
                    freeLight.gameObject.SetActive(false);
                    freeLight.localEulerAngles = Vector3.zero;
                    freeLight.localScale = Vector3.one;
                    freeLight.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                };
            };
            indexLight.gameObject.SetActive(true);
            indexLight.DOScale(2, 0.2f).onComplete = () =>
            {
                indexLight.DOScale(4, 0.4f).SetEase(Ease.OutCubic);
                indexLight.DOLocalRotate(new Vector3(0, 0, 50), 0.5f).SetEase(Ease.OutCubic);
                indexLight.GetComponent<Image>().DOFade(0f, 0.5f).SetEase(Ease.OutCubic).onComplete = () =>
                {
                    indexLight.gameObject.SetActive(false);
                    indexLight.localEulerAngles = Vector3.zero;
                    indexLight.localScale = Vector3.one;
                    indexLight.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                };
            };
        }
        if (isVIPClaimed && !vipTick.activeSelf)
        {
            vipClaimButton.gameObject.SetActive(false);
            vipLight.gameObject.SetActive(false);

            vipTick.transform.localScale = Vector3.zero;
            vipTick.SetActive(true);
            vipTick.transform.DOScale(1.05f, 0.2f).onComplete = () =>
            {
                vipTick.transform.DOScale(1, 0.2f);
            };
        }
        if (TilePassModel.Instance.IsVIP && !isVIPClaimed && !vipClaimButton.gameObject.activeSelf)
        {
            vipTick.SetActive(false);

            vipClaimButton.transform.localScale = Vector3.zero;
            vipClaimButton.gameObject.SetActive(true);
            vipClaimButton.transform.DOScale(0.42f, 0.2f).SetDelay(delay).onComplete = () =>
            {
                vipClaimButton.transform.DOScale(0.4f, 0.2f);
            };
            //右特效
            vipLight.gameObject.SetActive(true);
            vipLight.DOScale(2, 0.2f).onComplete = () =>
            {
                vipLight.DOScale(4, 0.4f).SetEase(Ease.OutCubic);
                vipLight.DOLocalRotate(new Vector3(0, 0, 50), 0.5f).SetEase(Ease.OutCubic);
                vipLight.GetComponent<Image>().DOFade(0f, 0.5f).SetEase(Ease.OutCubic).onComplete = () =>
                {
                    vipLight.gameObject.SetActive(false);
                    vipLight.localEulerAngles = Vector3.zero;
                    vipLight.localScale = Vector3.one;
                    vipLight.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                };
            };
        }
    }

    private void OnFreeButtonClicked()
    {
        GetComponentInParent<ScrollRect>().StopMovement();

        int centerIndex = m_TilePassMainMenu.scrollArea.GetViewportCenterIndex();
        PromptBoxShowDirection direction = m_Index >= centerIndex ? PromptBoxShowDirection.Up : PromptBoxShowDirection.Down;

        //已领取的奖励
        if (freeTick.activeSelf)
        {
            m_TilePassMainMenu.itemPromptBox.HidePromptBox();
            m_TilePassMainMenu.textPromptBox.SetText("TilePass.FreeClaim");
            m_TilePassMainMenu.textPromptBox.triangelOffset = 85;
            if (m_TilePassMainMenu.textPromptBox.gameObject.activeInHierarchy
                    && m_TilePassMainMenu.textPromptBox.transform.position == freeButton.transform.position)
            {
                m_TilePassMainMenu.textPromptBox.HidePromptBox();
            }
            else
            {
                m_TilePassMainMenu.clickObj = freeButton.gameObject;
                m_TilePassMainMenu.textPromptBox.ShowPromptBox(direction, freeButton.transform.position);
            }
            return;
        }

        //未领取的宝箱
        if (m_TilePassData.FreeRewardList.Count > 1)
        {
            float offset;
            if (m_TilePassData.FreeRewardList.Count == 4)
            {
                offset = 40;
            }
            else if (m_TilePassData.FreeRewardList.Count == 5)
            {
                offset = 95;
            }
            else
            {
                offset = 0;
            }
            m_TilePassMainMenu.itemPromptBox.boxMaxWidth = 600;

            m_TilePassMainMenu.textPromptBox.HidePromptBox();
            m_TilePassMainMenu.itemPromptBox.Init(m_TilePassData.FreeRewardList, m_TilePassData.FreeRewardNumList);
            m_TilePassMainMenu.itemPromptBox.triangelOffset = offset;
            if (m_TilePassMainMenu.itemPromptBox.gameObject.activeInHierarchy
                && m_TilePassMainMenu.itemPromptBox.transform.position == freeButton.transform.position)
            {
                m_TilePassMainMenu.itemPromptBox.HidePromptBox();
            }
            else
            {
                m_TilePassMainMenu.clickObj = freeButton.gameObject;
                m_TilePassMainMenu.itemPromptBox.ShowPromptBox(direction, freeButton.transform.position);
            }
            return;
        }

        //可领取的奖励
        if (freeClaimButton.gameObject.activeSelf)
        {
            OnFreeClaimButtonClicked();
            return;
        }

        //不可领取的奖励
        m_TilePassMainMenu.itemPromptBox.HidePromptBox();
        m_TilePassMainMenu.textPromptBox.SetText("TilePass.FreeLock");
        m_TilePassMainMenu.textPromptBox.triangelOffset = 85;
        if (m_TilePassMainMenu.textPromptBox.gameObject.activeInHierarchy
                && m_TilePassMainMenu.textPromptBox.transform.position == freeButton.transform.position)
        {
            m_TilePassMainMenu.textPromptBox.HidePromptBox();
        }
        else
        {
            m_TilePassMainMenu.clickObj = freeButton.gameObject;
            m_TilePassMainMenu.textPromptBox.ShowPromptBox(direction, freeButton.transform.position);
        }
    }

    private void OnVIPButtonClicked()
    {
        GetComponentInParent<ScrollRect>().StopMovement();

        int centerIndex = m_TilePassMainMenu.scrollArea.GetViewportCenterIndex();
        PromptBoxShowDirection direction = m_Index >= centerIndex ? PromptBoxShowDirection.Up : PromptBoxShowDirection.Down;

        //已领取的奖励
        if (vipTick.activeSelf)
        {
            m_TilePassMainMenu.itemPromptBox.HidePromptBox();
            m_TilePassMainMenu.textPromptBox.SetText("TilePass.FreeClaim");
            m_TilePassMainMenu.textPromptBox.triangelOffset = -85;
            if (m_TilePassMainMenu.textPromptBox.gameObject.activeInHierarchy
                    && m_TilePassMainMenu.textPromptBox.transform.position == vipButton.transform.position)
            {
                m_TilePassMainMenu.textPromptBox.HidePromptBox();
            }
            else
            {
                m_TilePassMainMenu.clickObj = vipButton.gameObject;
                m_TilePassMainMenu.textPromptBox.ShowPromptBox(direction, vipButton.transform.position);
            }
            return;
        }

        //未领取的宝箱
        if (m_TilePassData.VIPRewardList.Count > 1)
        {
            float offset;
            if (m_TilePassData.VIPRewardList.Count == 4)
            {
                offset = -40;
            }
            else if (m_TilePassData.VIPRewardList.Count == 5)
            {
                offset = -95;
            }
            else
            {
                offset = 0;
            }
            m_TilePassMainMenu.itemPromptBox.boxMaxWidth = 600;
            if (m_TilePassData.VIPRewardList.Count == 6)
            {
                m_TilePassMainMenu.itemPromptBox.boxMaxWidth = 400;
            }

            m_TilePassMainMenu.textPromptBox.HidePromptBox();
            m_TilePassMainMenu.itemPromptBox.Init(m_TilePassData.VIPRewardList, m_TilePassData.VIPRewardNumList);
            m_TilePassMainMenu.itemPromptBox.triangelOffset = offset;
            if (m_TilePassMainMenu.itemPromptBox.gameObject.activeInHierarchy
                && m_TilePassMainMenu.itemPromptBox.transform.position == vipButton.transform.position)
            {
                m_TilePassMainMenu.itemPromptBox.HidePromptBox();
            }
            else
            {
                m_TilePassMainMenu.clickObj = vipButton.gameObject;
                m_TilePassMainMenu.itemPromptBox.ShowPromptBox(direction, vipButton.transform.position);
            }
            return;
        }

        //可领取的奖励
        if (vipClaimButton.gameObject.activeSelf)
        {
            OnVIPClaimButtonClicked();
            return;
        }
        
        //未购买通行证
        if (lockSpine.gameObject.activeSelf)
        {
            lockSpine.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);

            m_TilePassMainMenu.activateButton.gameObject.SetActive(false);
            m_TilePassMainMenu.upgradeButton.gameObject.SetActive(false);
            GameManager.UI.ShowUIForm("TilePassActivateMenu");
            return;
        }

        //不可领取的奖励
        m_TilePassMainMenu.itemPromptBox.HidePromptBox();
        m_TilePassMainMenu.textPromptBox.SetText("TilePass.FreeLock");
        m_TilePassMainMenu.textPromptBox.triangelOffset = -85;
        if (m_TilePassMainMenu.textPromptBox.gameObject.activeInHierarchy
                && m_TilePassMainMenu.textPromptBox.transform.position == vipButton.transform.position)
        {
            m_TilePassMainMenu.textPromptBox.HidePromptBox();
        }
        else
        {
            m_TilePassMainMenu.clickObj = vipButton.gameObject;
            m_TilePassMainMenu.textPromptBox.ShowPromptBox(direction, vipButton.transform.position);
        }
    }

    public void OnFreeClaimButtonClicked()
    {
        if (TilePassModel.Instance.CheckRewardGetStatus("TilePassFreeRewardGet" + m_Index.ToString())) return;

        int canClaimIndex = TilePassModel.Instance.CurrentIndex >= m_TilePassMainMenu.TilePassData.CurrentTilePassDatas.Count ?
            m_TilePassMainMenu.TilePassData.CurrentTilePassDatas.Count - 1 : TilePassModel.Instance.CurrentIndex - 1;
        if (m_Index > canClaimIndex) return;

        freeClaimButton.interactable = false;
        GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());

        List<TotalItemData> rewardList = new List<TotalItemData>(m_TilePassData.FreeRewardList);
        List<int> numList = new List<int>(m_TilePassData.FreeRewardNumList);
        ShowGetReward(rewardList, numList, freeButton.transform.position);

        TilePassModel.Instance.AddRewardGetStatus("TilePassFreeRewardGet" + m_Index.ToString());
        RefreshRewardStatus();
        m_TilePassMainMenu.RefreshClaimAll();
    }

    public void OnVIPClaimButtonClicked()
    {
        if (!TilePassModel.Instance.IsVIP) return;

        if (TilePassModel.Instance.CheckRewardGetStatus("TilePassVIPRewardGet" + m_Index.ToString())) return;

        int canClaimIndex = TilePassModel.Instance.CurrentIndex >= m_TilePassMainMenu.TilePassData.CurrentTilePassDatas.Count ?
            m_TilePassMainMenu.TilePassData.CurrentTilePassDatas.Count - 1 : TilePassModel.Instance.CurrentIndex - 1;
        if (m_Index > canClaimIndex) return;

        vipClaimButton.interactable = false;
        GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());

        List<TotalItemData> rewardList = new List<TotalItemData>(m_TilePassData.VIPRewardList);
        List<int> numList = new List<int>(m_TilePassData.VIPRewardNumList);
        ShowGetReward(rewardList, numList, vipButton.transform.position);

        TilePassModel.Instance.AddRewardGetStatus("TilePassVIPRewardGet" + m_Index.ToString());
        RefreshRewardStatus();
        m_TilePassMainMenu.RefreshClaimAll();
    }

    private void ShowGetReward(List<TotalItemData> rewardList, List<int> numList, Vector3 pos)
    {
        bool isCard = false;
        List<TotalItemData> temp1 = new List<TotalItemData>(rewardList);
        List<int> temp2 = new List<int>(numList);
        for (int i = 0; i < rewardList.Count; i++)
        {
            if (rewardList[i].TotalItemType == TotalItemType.CardPack1 ||
                rewardList[i].TotalItemType == TotalItemType.CardPack2 ||
                rewardList[i].TotalItemType == TotalItemType.CardPack3 ||
                rewardList[i].TotalItemType == TotalItemType.CardPack4 ||
                rewardList[i].TotalItemType == TotalItemType.CardPack5)
            {
                isCard = true;
                RewardManager.Instance.AddNeedGetReward(rewardList[i], numList[i]);
                temp1.Remove(rewardList[i]);
                temp2.Remove(numList[i]);
            }
            else
            {
                RewardManager.Instance.SaveRewardData(rewardList[i], numList[i], true);
            }
        }

        int totalCount = temp1.Count > 4 ? 4 : temp1.Count;
        Vector3[] positions = UnityUtility.GetAveragePosition(pos + new Vector3(0, 0.05f), new Vector3(0.12f, 0), totalCount);
        for (int i = 0; i < temp1.Count; i++)
        {
            m_TilePassMainMenu.ShowRewardGetTip(temp1[i], temp2[i], positions[i % 4], i * 0.15f);
        }
        
        if (isCard)
        {
            m_TilePassMainMenu.OnPause();
            GameManager.Task.AddDelayTriggerTask(totalCount > 0 ? 0.5f : 0f, () =>
            {
                RewardManager.Instance.AutoGetRewardDelayTime = 0f;
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true, () =>
                {
                    RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                    m_TilePassMainMenu.OnResume();
                });
            });
        }
    }
    
    public void OnReset()
    {
        m_TilePassData = null;
        m_Index = 0;

        freeButton.onClick.RemoveAllListeners();
        vipButton.onClick.RemoveAllListeners();
        freeClaimButton.OnReset();
        vipClaimButton.OnReset();

        freeClaimButton.transform.DOKill();
        vipClaimButton.transform.DOKill();
        freeTick.transform.DOKill();
        vipTick.transform.DOKill();
        //freeLight.transform.DOKill();
        //vipLight.transform.DOKill();
        slider.DOKill();
    }

    public void OnRelease()
    {
        OnReset();

        for (int i = 0; i < m_LoadAssetHandleList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(m_LoadAssetHandleList[i]);
        }
        m_LoadAssetHandleList.Clear();
    }

    /// <summary>
    /// 异步加载图片
    /// </summary>
    private void LoadSpriteAsync(string spriteKey, string atlasName, Action<Sprite> completeAction)
    {
        m_LoadAssetHandleList.Add(UnityUtility.LoadSpriteAsync(spriteKey, atlasName, sp =>
        {
            completeAction?.Invoke(sp);
        }));
    }
}
