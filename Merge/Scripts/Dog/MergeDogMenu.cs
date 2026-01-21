using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Merge
{
    public class MergeDogMenu : UIForm
    {
        public SkeletonGraphic m_TreeDecorationAnim;
        public DelayButton m_FinalRewardButton;
        public DelayButton m_CloseButton;
        public DelayButton m_BackButton;
        public DelayButton m_DecorateButton;
        public ChestPromptBox m_FinalChestPromptBox;
        public DogRewardBubble[] m_Bubbles;
        public Transform m_CoinsRoot;
        public MergeCoinBar m_CoinBar;
        public Image m_DecorateImg;
        public Image m_Black;
        public GameObject m_Finger;
        public SkeletonAnimation m_FingerAnim;
        public GameObject m_DogTitle;

        private bool m_CanDecorate;
        private bool m_DecorateAll;
        private int m_PromptDelayHideTaskId;
        private bool m_IsShowingGuide;
        private string decorateAssetName;
        private AsyncOperationHandle spriteHandle;
        private bool m_IsStartRVTime;
        private float m_CurrentRVTimeInterval;
        private DateTime getRewardRVTime;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);
            
            GameManager.Event.Subscribe(CommonEventArgs.EventId, OnDogBubbleGetRewardRVTimeUpdate);

            m_FinalRewardButton.OnInit(OnFinalRewardButtonClick);
            m_CloseButton.OnInit(OnCloseButtonClick);
            m_BackButton.OnInit(OnBackButtonClick);
            m_DecorateButton.OnInit(OnDecorateButtonClick);

            for (int i = 0; i < m_Bubbles.Length; i++)
            {
                m_Bubbles[i].Initialize(i, this);
            }

            RefreshButton(false);
            RefreshTreeDecoration(false);
            RefreshBubble(false);
            RefreshDogBubbleGetRewardRVTime();
        }

        public override void OnReset()
        {
            for (int i = 0; i < m_Bubbles.Length; i++)
            {
                m_Bubbles[i].Release();
            }

            m_FinalRewardButton.OnReset();
            m_CloseButton.OnReset();
            m_BackButton.OnReset();
            m_DecorateButton.OnReset();

            HideFinalChestRewardTipBox();

            base.OnReset();
        }

        public override void OnRelease()
        {
            OnReset();

            decorateAssetName = null;
            UnityUtility.UnloadAssetAsync(spriteHandle);
            m_FinalChestPromptBox.Release();

            base.OnRelease();
        }

        public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
        {
            base.OnShow(showSuccessAction, userData);

            if (m_CanDecorate && PlayerPrefs.GetInt("HasShowedDogDecorateGuide", 0) == 0) 
            {
                PlayerPrefs.SetInt("HasShowedDogDecorateGuide", 1);

                m_IsShowingGuide = true;
                m_Black.color = new Color(0, 0, 0, 0);
                m_Black.gameObject.SetActive(true);
                m_Black.DOFade(0.7f, 0.2f).onComplete = () =>
                {
                    StartCoroutine(ShowFingerAnimCor(m_DecorateButton.transform.position, m_DecorateButton.transform.position));
                };
            }
            else
            {
                if (!m_FinalChestPromptBox.IsShowing)
                {
                    ShowFinalChestRewardTipBox(m_FinalRewardButton.transform.position);
                }
            }

            GameManager.Sound.PlayMusic("SFX_Merge_Xmas_Tree_Bgm", 0.556f);
        }

        public override void OnHide(Action hideSuccessAction = null, object userData = null)
        {
            if (GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) != null) 
                GameManager.Sound.PlayMusic("SFX_Merge_Xmas_Bgm", 1f);

            if (MergeManager.PlayerData.GetAllStorePropIds().Count > 0)
            {
                MergeMainMenu mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu;
                if (mainMenu != null) 
                    mainMenu.m_GuideMenu.TriggerGuide(GuideTriggerType.Guide_BoardFull);
            }

            base.OnHide(hideSuccessAction, userData);
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (m_IsStartRVTime)
            {
                if (m_CurrentRVTimeInterval >= 1)
                {
                    m_CurrentRVTimeInterval = 0;
                    if ((DateTime.Now - getRewardRVTime).TotalSeconds >= 0)
                    {
                        // 看广告获取气泡奖励冷却结束，刷新气泡显示
                        for (int i = 0; i < m_Bubbles.Length; i++)
                        {
                            m_Bubbles[i].RefreshTimerBarIcon();
                        }

                        m_IsStartRVTime = false;
                    }
                }
                else
                {
                    m_CurrentRVTimeInterval += realElapseSeconds;
                }
            }
        }

        private int CheckCanDecorateMergeStage(int stage)
        {
            switch (stage)
            {
                case 0:
                    return 4;
                case 1:
                    return 6;
                case 2:
                    return 8;
                case 3:
                    return 10;
                case 4:
                    return 11;
                case 5:
                    return 12;
            }

            return 100;
        }

        private void RefreshButton(bool showAnim)
        {
            int stage = MergeManager.PlayerData.GetDogDecorationStage();
            m_DecorateAll = stage >= 6;
            m_CanDecorate = !m_DecorateAll && MergeManager.PlayerData.GetCurrentMaxMergeStage() + 1 >= CheckCanDecorateMergeStage(stage);

            if (m_CanDecorate)
            {
                string dogName = "Dog_" + (stage + 1).ToString();
                if (decorateAssetName != dogName)
                {
                    decorateAssetName = dogName;
                    spriteHandle = UnityUtility.LoadAssetAsync<Sprite>(UnityUtility.GetAltasSpriteName(dogName, "MergePropAtlas_Dog"), sp =>
                    {
                        m_DecorateImg.sprite = sp;
                        m_DecorateImg.SetNativeSize();
                        m_DecorateImg.DOFade(1, 0.2f);
                    });
                }
            }

            if (!m_DecorateAll)
            {
                if (showAnim)
                {
                    if (m_CanDecorate)
                    {
                        m_BackButton.gameObject.SetActive(!m_CanDecorate);
                        m_DecorateButton.transform.localScale = Vector3.zero;
                        m_DecorateButton.gameObject.SetActive(m_CanDecorate);
                        m_DecorateButton.transform.DOScale(0.9f, 0.2f).onComplete = () =>
                        {
                            m_DecorateButton.transform.DOScale(0.8f, 0.2f);
                        };
                    }
                    else
                    {
                        m_BackButton.transform.localScale = Vector3.zero;
                        m_BackButton.gameObject.SetActive(!m_CanDecorate);
                        m_DecorateButton.gameObject.SetActive(m_CanDecorate);
                        m_BackButton.transform.DOScale(0.9f, 0.2f).onComplete = () =>
                        {
                            m_BackButton.transform.DOScale(0.8f, 0.2f);
                        };
                    }
                }
                else
                {
                    m_BackButton.gameObject.SetActive(!m_CanDecorate);
                    m_DecorateButton.gameObject.SetActive(m_CanDecorate);
                }
            }
            else
            {
                m_BackButton.gameObject.SetActive(false);
                m_DecorateButton.gameObject.SetActive(false);
            }

            m_CloseButton.gameObject.SetActive(true);
        }

        private void RefreshTreeDecoration(bool showAnim, Action callback = null)
        {
            int stage = MergeManager.PlayerData.GetDogDecorationStage();
            if (stage > 6)
                stage = 6;

            string animName = string.Empty;
            switch (stage)
            {
                case 1:
                    animName = "01_all";
                    break;
                case 2:
                    animName = "02_all";
                    break;
                case 3:
                    animName = "03_all";
                    break;
                case 4:
                    animName = "04_all";
                    break;
                case 5:
                    animName = "05_all";
                    break;
                case 6:
                    animName = "06";
                    break;
            }

            if (!string.IsNullOrEmpty(animName))
            {
                if (showAnim)
                {
                    m_TreeDecorationAnim.AnimationState.SetAnimation(0, animName, false).Complete += t =>
                    {
                        if (stage == 6)
                        {
                            m_DogTitle.transform.localScale = Vector3.zero;
                            m_DogTitle.gameObject.SetActive(true);
                            m_DogTitle.transform.DOScale(1.1f, 0.2f).onComplete = () =>
                            {
                                m_DogTitle.transform.DOScale(1f, 0.2f);
                            };
                        }

                        GameManager.Task.AddDelayTriggerTask(0.4f, () =>
                        {
                            callback?.Invoke();
                        });
                    };
                }
                else
                {
                    if (stage == 6)
                    {
                        m_TreeDecorationAnim.AnimationState.SetAnimation(0, "07", true);
                        m_DogTitle.gameObject.SetActive(true);
                    }
                    else
                    {
                        var track = m_TreeDecorationAnim.AnimationState.SetAnimation(0, animName, false);
                        track.TrackTime = track.AnimationEnd;
                        m_DogTitle.gameObject.SetActive(false);
                    }

                    callback?.Invoke();
                }
                m_TreeDecorationAnim.gameObject.SetActive(true);
            }
            else
            {
                m_TreeDecorationAnim.gameObject.SetActive(false);

                callback?.Invoke();
            }
        }

        public void RefreshBubble(bool allCanGet)
        {
            int bubbleMaxNum = MergeManager.Instance.GetMaxBubbleNum();
            for (int i = 0; i < m_Bubbles.Length; i++)
            {
                if (i < bubbleMaxNum) 
                {
                    if (MergeManager.PlayerData.GetDogBubbleRewardId(i) > 0)
                    {
                        if(allCanGet)
                            m_Bubbles[i].Refresh(allCanGet);
                        m_Bubbles[i].Show(m_FinalRewardButton.transform.position, false, 0.1f * i);
                    }
                    else
                    {
                        m_Bubbles[i].Refresh(allCanGet);
                        m_Bubbles[i].Show(m_FinalRewardButton.transform.position, true);
                    }
                }
                else
                {
                    m_Bubbles[i].Hide();
                }
            }
        }

        public void RefreshDogBubbleGetRewardRVTime()
        {
            getRewardRVTime = MergeManager.PlayerData.GetDogBubbleGetRewardRVTime();
            m_IsStartRVTime = getRewardRVTime != Constant.GameConfig.DateTimeMin;
            m_CurrentRVTimeInterval = 0;
        }

        public void OnDogBubbleGetRewardRVTimeUpdate(object sender, GameEventArgs e)
        {
            var ne = e as CommonEventArgs;
            if (ne == null) return;
            if (ne.Type == CommonEventType.DogBubbleGetRewardRVTime)
            {
                RefreshDogBubbleGetRewardRVTime();
            }
        }

        private void ShowFinalChestRewardTipBox(Vector3 position)
        {
            if (GameManager.PlayerData.IsOwnTileID(MergeManager.Instance.TileId))
                return;

            m_FinalChestPromptBox.Show(position);

            if (m_PromptDelayHideTaskId != 0)
            {
                GameManager.Task.RemoveDelayTriggerTask(m_PromptDelayHideTaskId);
                m_PromptDelayHideTaskId = 0;
            }
            m_PromptDelayHideTaskId = GameManager.Task.AddDelayTriggerTask(3f, HideFinalChestRewardTipBox);
        }

        public void HideFinalChestRewardTipBox()
        {
            m_FinalChestPromptBox.Hide();

            if (m_PromptDelayHideTaskId != 0)
            {
                GameManager.Task.RemoveDelayTriggerTask(m_PromptDelayHideTaskId);
                m_PromptDelayHideTaskId = 0;
            }
        }

        public void ShowGetRewardAnim(int index, TotalItemData rewardData, int rewardNum, Vector3 pos, Action rewardReadyCallback)
        {
            if (rewardData.TotalItemType == TotalItemType.Coin)
                StartCoroutine(ShowGetCoinAnimCor(index, rewardNum, pos, rewardReadyCallback));
            else
                StartCoroutine(ShowGetRewardAnimCor(index, rewardData, rewardNum, pos, rewardReadyCallback));
        }

        private void OnFinalRewardButtonClick()
        {
            if (!m_FinalChestPromptBox.IsShowing)
                ShowFinalChestRewardTipBox(m_FinalRewardButton.transform.position);
            else
                HideFinalChestRewardTipBox();
        }

        private void OnCloseButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            GameManager.Process.EndProcess(ProcessType.AutoShowDogProcess);
        }

        private void OnBackButtonClick()
        {
            GameManager.UI.HideUIForm(this);

            if (GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) == null) 
                GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu"));
        }

        private void OnDecorateButtonClick()
        {
            int stage = MergeManager.PlayerData.GetDogDecorationStage() + 1;
            if (stage > 6)
                return;

            HideFinalChestRewardTipBox();

            if (m_IsShowingGuide)
            {
                m_IsShowingGuide = false;
                StopAllCoroutines();
                m_Black.gameObject.SetActive(false);
                m_Finger.SetActive(false);
            }

            m_DecorateButton.gameObject.SetActive(false);
            m_CloseButton.gameObject.SetActive(false);

            MergeManager.PlayerData.SetDogDecorationStage(stage);
            if (stage == 6)
            {
                GameManager.PlayerData.BuyTileID(TotalItemData.FromInt(MergeManager.Instance.TileRewardId).RefID);
            }

            RefreshTreeDecoration(true, () =>
            {
                if (stage == 6)
                {
                    TotalItemData reward = TotalItemData.FromInt(MergeManager.Instance.TileRewardId);

                    UnityUtility.InstantiateAsync("BgOrTileFlyReward", transform, obj =>
                    {
                        GameObject flyObject = obj;
                        FlyReward flyReward = flyObject.GetComponent<FlyReward>();
                        flyReward.OnInit(reward, 1, 1, false, null);

                        GameManager.UI.ShowUIForm("GetBGOrTileItemPanel", (u) =>
                        {
                            GameManager.Sound.PlayAudio(SoundType.SFX_ShowBGPanel.ToString());
                        }, userData: flyReward);
                    });
                }

                RefreshBubble(true);
                RefreshButton(true);
            });

            GameManager.Event.Fire(this, DogDecorateEndEventArgs.Create());

            if (stage != 6)
            {
                GameManager.Sound.PlayAudio("SFX_Xmas_Tree_Dressup");
            }
            else
            {
                GameManager.Sound.StopMusic();
                GameManager.Sound.PlayAudio("SFX_Xmas_Tree_Compelete");

                GameManager.Task.AddDelayTriggerTask(10f, () =>
                {
                    GameManager.Sound.PlayMusic("SFX_Merge_Xmas_Tree_Bgm", 0.556f);
                });
            }

            GameManager.Firebase.RecordMessageByEvent("Merge_Xmas_Tree_Decorate_Stage", new Firebase.Analytics.Parameter("Stage", stage));
        }

        IEnumerator ShowGetCoinAnimCor(int index, int coinNum, Vector3 pos, Action coinReadyCallback)
        {
            int subCoinCount = 3;
            List<RotateCoin> rotateCoins = new List<RotateCoin>();
            if (rotateCoins.Count < subCoinCount)
            {
                int delta = subCoinCount - rotateCoins.Count;
                for (int i = 0; i < delta; i++)
                {
                    GameManager.ObjectPool.Spawn<RotateCoinObject>("RotateCoin", "RotateCoin", pos, Quaternion.identity, m_CoinsRoot, (obj) =>
                    {
                        if (obj != null && obj.Target != null)
                        {
                            GameObject coinObject = (GameObject)obj.Target;
                            RotateCoin rotateCoin = coinObject.GetComponent<RotateCoin>();
                            if (rotateCoin != null)
                            {
                                rotateCoin.OnHide();
                                rotateCoins.Add(rotateCoin);
                            }
                            else
                            {
                                subCoinCount--;
                            }
                        }
                        else
                        {
                            subCoinCount--;
                        }
                    });
                }

                while (rotateCoins.Count < subCoinCount)
                {
                    yield return null;
                }

                coinReadyCallback?.Invoke();
                m_CoinBar.Show();
                m_CoinBar.OnCoinFlyEnd();

                for (int i = 0; i < rotateCoins.Count; i++)
                {
                    RotateCoin rotateCoin = rotateCoins[i];
                    Transform rotateCoinTrans = rotateCoins[i].transform;

                    var graphic = rotateCoin.skeletonGraphic;
                    rotateCoin.OnShow();
                    graphic.transform.DOScale(0.35f, 0.4f);

                    rotateCoinTrans.DOMove(m_CoinBar.CoinFlyTargetPos, 0.6f).SetEase(Ease.InCubic).OnComplete(() =>
                    {
                        rotateCoin.OnHide();
                        GameManager.ObjectPool.Unspawn<RotateCoinObject>("RotateCoin", rotateCoin.gameObject);
                        m_CoinBar.OnCoinFlyHit();
                    });
                    yield return new WaitForSeconds(0.1f);
                }

                yield return new WaitForSeconds(0.3f);

                GameManager.Event.Fire(this, CoinNumChangeEventArgs.Create(coinNum, null));

                yield return new WaitForSeconds(0.3f);

                m_Bubbles[index].Hide();
                m_Bubbles[index].Refresh(false);
                m_Bubbles[index].Show(m_FinalRewardButton.transform.position, true);
            }
        }

        IEnumerator ShowGetRewardAnimCor(int index, TotalItemData rewardData, int rewardNum, Vector3 pos, Action rewardReadyCallback)
        {
            FlyReward flyReward = null;
            UnityUtility.InstantiateAsync("DefaultFlyReward", m_CoinsRoot, obj =>
            {
                flyReward = obj.GetComponent<FlyReward>();
                flyReward.OnInit(rewardData, rewardNum, 1, false, null);
                flyReward.transform.position = pos;
                flyReward.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            });

            while (flyReward == null)
            {
                yield return null;
            }

            yield return null;

            flyReward.gameObject.SetActive(true);
            rewardReadyCallback?.Invoke();

            void FinishAction()
            {
                if (flyReward != null)
                {
                    flyReward.gameObject.SetActive(false);
                    UnityUtility.UnloadInstance(flyReward.gameObject);
                    flyReward = null;
                }
            }

            Transform cachedTransform = flyReward.transform;
            Vector3 startPos = new Vector3(cachedTransform.position.x, cachedTransform.position.y, 0);
            Vector3 targetPos = m_DecorateButton.transform.position;
            Vector3 backPos = startPos + (startPos - targetPos).normalized * 0.2f;
            Vector3 startScale = cachedTransform.localScale;

            cachedTransform.DOMove(backPos, 0.2f).SetEase(Ease.OutSine).onComplete = () =>
            {
                cachedTransform.DOMove(targetPos, 0.36f).SetEase(Ease.InCubic);
                cachedTransform.DOScale(0.25f, 0.34f).SetEase(Ease.InCubic).onComplete = () =>
                {
                    try
                    {
                        FinishAction();

                        Transform punchBtnTrans = null;
                        if (m_DecorateButton != null && m_DecorateButton.gameObject.activeSelf) 
                        {
                            punchBtnTrans = m_DecorateButton.transform;
                        }
                        else if(m_BackButton != null && m_BackButton.gameObject.activeSelf)
                        {
                            punchBtnTrans = m_BackButton.transform;
                        }

                        punchBtnTrans.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
                        {
                            punchBtnTrans.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                        };
                        GameManager.Sound.PlayAudio("SFX_itemget");
                    }
                    catch (Exception e)
                    {
                        FinishAction();
                        Log.Error("ShowGetRewardAnimCor error:" + e.Message);
                    }
                };
            };

            yield return new WaitForSeconds(1f);

            m_Bubbles[index].Hide();
            m_Bubbles[index].Refresh(false);
            m_Bubbles[index].Show(m_FinalRewardButton.transform.position, true);
        }

        IEnumerator ShowFingerAnimCor(Vector3 startPos, Vector3 endPos)
        {
            m_FingerAnim.Initialize(false);

            m_Finger.transform.DOKill();
            m_Finger.transform.position = startPos;
            m_Finger.gameObject.SetActive(true);

            MeshRenderer meshRender = m_FingerAnim.GetComponent<MeshRenderer>();

            bool isClickAnim = startPos == endPos;

            WaitForSeconds waitForSecond1 = new WaitForSeconds(1f);
            WaitForSeconds waitForSecond2 = new WaitForSeconds(0.35f);

            while (true)
            {
                m_Finger.transform.position = startPos;
                m_FingerAnim.AnimationState.SetAnimation(0, "02", false);

                yield return null;

                meshRender.material.DOKill();
                meshRender.material.SetColor("_Color", Color.white);

                if (!isClickAnim)
                {
                    yield return new WaitForSeconds(0.18f);

                    m_Finger.transform.DOMove(endPos, 1f);
                }

                yield return waitForSecond1;

                meshRender.material.DOFade(0, 0.3f);

                yield return waitForSecond2;
            }
        }
    }
}
