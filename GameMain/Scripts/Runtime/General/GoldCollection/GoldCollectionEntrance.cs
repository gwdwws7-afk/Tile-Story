using DG.Tweening;
using Firebase.Analytics;
using Spine.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GoldCollectionEntrance : EntranceUIForm
{
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private Transform mainImageTransform;
    [SerializeField] private ParticleSystem reachEffect;
    [SerializeField] private Transform sliderTransform;
    [SerializeField] private SimpleSlider slider;
    [SerializeField] private TaskFlyReward taskReward;
    [SerializeField] private BlackBgManager blackBG;
    [SerializeField] private ParticleSystem rewardGetEffect;

    [SerializeField] private GameObject banner;
    [SerializeField] private GameObject preview;
    [SerializeField] private Image mainBg;
    [SerializeField] private SkeletonGraphic spine;

    public Vector3 taskRewardOriginalPos;

    private int needFlyRewardNum;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        SetTimer();

        sliderTransform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
        sliderTransform.localPosition = new Vector3(-600, 0, 0);
        needFlyRewardNum = 0;
    }

    public override void OnRelease()
    {
        StopAllCoroutines();
        if (taskReward.transform.parent != sliderTransform)
        {
            taskReward.HideRewardShowEffect();
            taskReward.transform.SetParent(sliderTransform);
        }

        countdownTimer.OnReset();

        base.OnRelease();
    }
    
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        countdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
    }
    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }

    public void SetTimer()
    {
        DateTime endTime = GameManager.Task.GoldCollectionTaskManager.EndTime;
        if (endTime == DateTime.MinValue)
        {
            GameManager.Task.GoldCollectionTaskManager.SetEndTime();
            endTime = GameManager.Task.GoldCollectionTaskManager.EndTime;
        }
        //不处于活动开放时间内（活动无排期/上期活动结束）
        if (endTime < DateTime.Now)
        {
            gameObject.SetActive(false);
        }
        //处于活动开放时间内
        else
        {
            countdownTimer.OnReset();
            countdownTimer.StartCountdown(endTime);
            countdownTimer.CountdownOver += OnCountdownOver;

            InitButton();
        }
    }

    private void InitButton()
    {
        //已达到预告等级
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockGoldCollectionButtonLevel)
        {
            gameObject.SetActive(true);
            //未解锁
            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockGoldCollectionLevel)
            {
                OnLocked();
            }
            //已解锁
            else
            {
                //周期内提前完成所有任务，入口消失
                if (GameManager.Task.GoldCollectionTaskManager.CheckAllTaskComplete())
                {
                    gameObject.SetActive(false);
                }

                //初始化活动
                if (GameManager.Task.GoldCollectionTaskManager.CurrentIndex == 0)
                {
                    GameManager.Task.GoldCollectionTaskManager.OnInit();
                }

                OnUnlocked();
            }
        }
        //未达到预告等级
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        gameObject.SetActive(false);

        if (GameManager.Process.Count > 0)
            return;
        
        if (GameManager.Task.GoldCollectionTaskManager.CurrentIndex <= 0)
            return;

        if (!GameManager.Task.GoldCollectionTaskManager.ShowedLastMenu)
        {
            GameManager.UI.ShowUIForm("GoldCollectionEndPanel",showSuccessAction =>
            {
                GameManager.Task.GoldCollectionTaskManager.ShowedLastMenu = true;
                //如果在主界面倒计时结束，展示结束界面后，重置数据
                // GameManager.Task.GoldCollectionTaskManager.OnReset();
            });
        }
    }

    /// <summary>
    /// 奖励飞入
    /// </summary>
    public void FlyReward()
    {
        needFlyRewardNum = GameManager.Task.GoldCollectionTaskManager.LevelCollectNum;
        if (needFlyRewardNum <= 0)
            return;
        EntranceFlyObjectManager.Instance.RegisterEntranceFlyEvent("ElementGuide[3]", needFlyRewardNum, 21, new Vector3(250f, 0), Vector3.zero, gameObject,
            () =>
            {
                GameManager.Task.GoldCollectionTaskManager.LevelCollectNum = 0;
            }, () =>
            {
                EntranceFlyObjectManager.Instance.EndEntranceFlyEvent();
                //GameManager.Process.Register(ProcessType.ShowGoldCollectionReward, 99, ShowBarPopupAnim);

                mainImageTransform.DOScale(0.85f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
                {
                    mainImageTransform.DOScale(1f, 0.15f);
                };
                if (reachEffect != null)
                {
                    reachEffect.Play();
                }
            }, false);
    }

    /// <summary>
    /// 展示横条
    /// </summary>
    public void ShowBarPopupAnim()
    {
        GoldCollectionTaskManager goldCollectionTaskManager = GameManager.Task.GoldCollectionTaskManager;
        DTGoldCollectionData goldCollectionData = goldCollectionTaskManager.DataTable;
        RewardTask currentTask = goldCollectionTaskManager.CurrentTask;
        int needCollectNum = goldCollectionData.GetNeedCollectNum(currentTask.Index);
        int lastIndexNeedCollectNum = goldCollectionData.GetNeedCollectNum(currentTask.Index - 1);
        int totalCollectNum = goldCollectionTaskManager.TotalCollectNum;
        //触发奖励领取 -- 改只要有收集便展示横条
        if (totalCollectNum >= needCollectNum || needFlyRewardNum > 0)
        {
            //初始化进度条和奖励
            slider.TotalNum = goldCollectionTaskManager.CurrentTask.TargetNum;
            slider.CurrentNum = goldCollectionTaskManager.LastRecordTotalCollectNum - lastIndexNeedCollectNum > 0 ? goldCollectionTaskManager.LastRecordTotalCollectNum - lastIndexNeedCollectNum : 0;

            taskReward.OnReset();
            taskReward.Init(currentTask.Reward, currentTask.RewardNum, currentTask.Reward.Count, false);

            sliderTransform.parent.gameObject.SetActive(true);
            //展示横条
            sliderTransform.DOScaleY(1.3f, 0.2f).onComplete = () =>
            {
                sliderTransform.DOScaleY(1.6f, 0.1f);
            };
            sliderTransform.DOLocalMoveX(20f, 0.2f).onComplete = () =>
            {
                sliderTransform.DOLocalMoveX(-70f, 0.1f).onComplete = () =>
                {
                    //播放进度动画
                    StartCoroutine(ShowTaskTargetGetAnimCor(goldCollectionData, totalCollectNum));
                };
            };
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionReward);
        }
    }

    /// <summary>
    /// 横条动画
    /// </summary>
    /// <param name="goldCollectionData"></param>
    /// <param name="totalCollectNum"></param>
    /// <returns></returns>
    IEnumerator ShowTaskTargetGetAnimCor(DTGoldCollectionData goldCollectionData, int totalCollectNum)
    {
        RewardTask currentTask = GameManager.Task.GoldCollectionTaskManager.CurrentTask;
        int needCollectNum = goldCollectionData.GetNeedCollectNum(currentTask.Index);
        int stageCount = 0;
        while (totalCollectNum >= needCollectNum)
        {
            stageCount++;

            //走进度条
            float duration = ShowSliderIncreaseAnim(currentTask.TargetNum, currentTask.TargetNum);
            while (duration >= 0.1f)
            {
                duration -= 0.1f;
                GameManager.Sound.PlayAudio("SFX_Banner_Challenge_Progress");
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(duration);
            GameManager.Sound.PlayAudio("SFX_Goldenpass_ActiveReward");
            rewardGetEffect.Play();

            //登记奖励
            for (int i = 0; i < taskReward.RewardTypeList.Count; i++)
            {
                RewardManager.Instance.AddNeedGetReward(taskReward.RewardTypeList[i], taskReward.RewardNumList[i]);
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Lucky_Collect_Task, new Parameter("TaskNum", currentTask.Index));
            }

            //接收新任务
            if (!GameManager.Task.GoldCollectionTaskManager.AcceptNextTask())
                break;
            currentTask = GameManager.Task.GoldCollectionTaskManager.CurrentTask;
            needCollectNum = goldCollectionData.GetNeedCollectNum(currentTask.Index);
            if (stageCount == 1 && totalCollectNum < needCollectNum)
                break;

            //切换奖励图标
            taskReward.transform.DOScale(1.1f, 0.15f).onComplete = () =>
            {
                taskReward.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InQuad).onComplete = () =>
                {
                    taskReward.OnReset();
                    taskReward.Init(currentTask.Reward, currentTask.RewardNum, currentTask.Reward.Count, false);
                    taskReward.OnShow();

                    taskReward.transform.DOScale(1.1f, 0.15f).onComplete = () =>
                    {
                        taskReward.transform.DOScale(Vector3.one, 0.1f);
                    };
                };
            };
            yield return new WaitForSeconds(0.35f);

            //清空进度条
            slider.Value = 0;
        }

        if (stageCount == 1)
        {
            //单个奖励获取
            ShowTaskRewardGetAnim();
        }
        else
        {
            float duration = ShowSliderIncreaseAnim(currentTask.TargetNum - (needCollectNum - totalCollectNum), currentTask.TargetNum);
            while (duration >= 0.1f)
            {
                duration -= 0.1f;
                GameManager.Sound.PlayAudio("SFX_Banner_Challenge_Progress");
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(duration);
            //多层奖励获取
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                StartCoroutine(ShowBarCallinAnim());
            }, null, () =>
            {
                GameManager.UI.HideUIForm("GlobalMaskPanel");
            });
        }
    }

    /// <summary>
    /// 单层奖励获取
    /// </summary>
    private void ShowTaskRewardGetAnim()
    {
        if (!taskReward.IsChest)
        {
            RewardManager.Instance.AddRewardFlyObject(taskReward);
            StartCoroutine(TaskRewardJumpToScreenCenter(taskReward));
        }
        else
        {
            StartCoroutine(ChestRewardJumpToScreenCenter(taskReward));
        }
    }

    /// <summary>
    /// 普通奖励动画
    /// </summary>
    /// <param name="taskReward"></param>
    /// <returns></returns>
    IEnumerator TaskRewardJumpToScreenCenter(FlyReward taskReward)
    {
        yield return new WaitForSeconds(0.1f);

        Transform cachedTransform = taskReward.CachedTransform;
        taskRewardOriginalPos = cachedTransform.position;
        cachedTransform.SetParent(RewardManager.Instance.rewardArea.CachedTransform);

        cachedTransform.DOScale(4.64f, 0.5f).SetEase(Ease.InOutQuad);
        float delta = cachedTransform.position.y;
        cachedTransform.DOJump(new Vector3(0, 0, cachedTransform.position.z), 0.5f, 1, 0.65f).SetEase(Ease.InOutQuad);
        blackBG.OnShow(0.6f);

        yield return new WaitForSeconds(0.6f);

        cachedTransform.DOScale(new Vector3(5f, 4f, 4.64f), 0.13f).onComplete = () =>
        {
            cachedTransform.DOScale(new Vector3(4.4f, 4.8f, 4.64f), 0.13f).onComplete = () =>
            {
                cachedTransform.DOScale(4.64f, 0.13f);
            };
        };
        taskReward.ShowRewardBgEffect();

        RewardManager.Instance.ForceHideRewardPanelBg = true;
        RewardManager.Instance.AutoGetRewardDelayTime = 1f;
        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, true, () =>
        {
            RewardManager.Instance.ForceHideRewardPanelBg = false;
            RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
            if (GameManager.Task.GoldCollectionTaskManager.CurrentTask != null)
            {
                StartCoroutine(TaskRewardJumpBack());
            }
        }, () =>
        {
            if (GameManager.Task.GoldCollectionTaskManager.CurrentTask != null)
            {
                slider.TotalNum = GameManager.Task.GoldCollectionTaskManager.CurrentTask.TargetNum;
                slider.Value = 0;
            }
            else
            {
                StartCoroutine(ShowBarCallinAnim());
            }
            blackBG.OnHide(0.2f);
        }, () =>
        {
            //GameManager.UI.HideUIForm<GlobalMaskPanel>();
        });
    }

    /// <summary>
    /// 宝箱奖励动画
    /// </summary>
    /// <param name="taskReward"></param>
    /// <returns></returns>
    IEnumerator ChestRewardJumpToScreenCenter(FlyReward taskReward)
    {
        yield return new WaitForSeconds(0.1f);

        Transform cachedTransform = taskReward.CachedTransform;
        taskRewardOriginalPos = cachedTransform.position;
        cachedTransform.SetParent(RewardManager.Instance.rewardArea.CachedTransform);

        RewardManager.Instance.onRewardPanelStartShow = () =>
        {
            this.taskReward.OnHide();
        };

        RewardManager.Instance.AutoGetRewardDelayTime = 1f;
        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.GoldCollectionChestRewardPanel, true, () =>
        {
            RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
            if (GameManager.Task.GoldCollectionTaskManager.CurrentTask != null)
            {
                StartCoroutine(TaskRewardJumpBack());
            }
        }, () =>
        {
            if (GameManager.Task.GoldCollectionTaskManager.CurrentTask != null)
            {
                slider.TotalNum = GameManager.Task.GoldCollectionTaskManager.CurrentTask.TargetNum;
                slider.Value = 0;
            }
            else
            {
                StartCoroutine(ShowBarCallinAnim());
            }
        }, () =>
        {
            //GameManager.UI.HideUIForm<GlobalMaskPanel>();
        });
    }

    /// <summary>
    /// 下层奖励展示
    /// </summary>
    /// <returns></returns>
    IEnumerator TaskRewardJumpBack()
    {
        yield return null;

        Transform cachedTransform = taskReward.CachedTransform;
        cachedTransform.position = Vector3.zero;
        cachedTransform.localScale = Vector3.zero;

        taskReward.OnReset();
        taskReward.Init(GameManager.Task.GoldCollectionTaskManager.CurrentTask.Reward,
            GameManager.Task.GoldCollectionTaskManager.CurrentTask.RewardNum,
            GameManager.Task.GoldCollectionTaskManager.CurrentTask.Reward.Count,
            false);
        taskReward.OnShow();

        cachedTransform.DOScale(4.8f, 0.15f).onComplete = () =>
        {
            cachedTransform.DOScale(4.5f, 0.15f).onComplete = () =>
            {
                cachedTransform.DOScale(4.64f, 0.15f);
            };
        };
        taskReward.ShowRewardShowEffect();

        yield return new WaitForSeconds(0.6f);

        taskReward.HideRewardShowEffect();

        cachedTransform.DOScale(1.45f, 0.5f).SetEase(Ease.InOutQuad);
        cachedTransform.DOJump(taskRewardOriginalPos, 0.1f, 1, 0.58f).SetEase(Ease.InOutQuad).onComplete = () =>
        {
            cachedTransform.SetParent(sliderTransform);
        };

        yield return new WaitForSeconds(0.6f);
        cachedTransform.DOScale(new Vector3(1.32f, 1.18f, 1.35f), 0.13f).onComplete = () =>
        {
            cachedTransform.DOScale(new Vector3(1.2f, 1.3f, 1.35f), 0.13f).onComplete = () =>
            {
                cachedTransform.DOScale(1.25f, 0.13f);
            };
        };
        rewardGetEffect.Play();

        yield return new WaitForSeconds(0.4f);

        //进度
        GoldCollectionTaskManager goldCollectionTaskManager = GameManager.Task.GoldCollectionTaskManager;
        DTGoldCollectionData goldCollectionData = goldCollectionTaskManager.DataTable;
        int lastIndexNeedCollectNum = goldCollectionData.GetNeedCollectNum(goldCollectionTaskManager.CurrentIndex - 1);
        int totalCollectNum = goldCollectionTaskManager.TotalCollectNum;

        float endValue = (totalCollectNum - lastIndexNeedCollectNum) / (float)goldCollectionTaskManager.CurrentTask.TargetNum;

        if (endValue >= 1)
        {
            StartCoroutine(ShowTaskTargetGetAnimCor(goldCollectionData, totalCollectNum));
        }
        else
        {
            float duration = ShowSliderIncreaseAnim(totalCollectNum - lastIndexNeedCollectNum, goldCollectionTaskManager.CurrentTask.TargetNum);
            while (duration >= 0.1f)
            {
                duration -= 0.1f;
                GameManager.Sound.PlayAudio("SFX_Banner_Challenge_Progress");
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(duration + 0.2f);
        }

        //taskReward.HideRewardShowEffect();
        //cachedTransform.SetParent(sliderTransform);
        StartCoroutine(ShowBarCallinAnim());
    }

    /// <summary>
    /// 进度条动画
    /// </summary>
    /// <param name="targetNum"></param>
    /// <param name="totalNum"></param>
    /// <returns></returns>
    private float ShowSliderIncreaseAnim(float targetNum, float totalNum)
    {
        float endValue = targetNum / totalNum;
        if (endValue > 1)
        {
            endValue = 1;
        }
        float duration = Mathf.Clamp((endValue - slider.Value) * 3, 0.3f, 0.7f);
        slider.DOValue(targetNum, totalNum, duration).SetEase(Ease.InCubic);
        return duration;
    }

    /// <summary>
    /// 收回横条
    /// </summary>
    private IEnumerator ShowBarCallinAnim(bool isDelay = true)
    {
        yield return new WaitForSeconds(isDelay ? 0.5f : 0);

        sliderTransform.DOScaleY(1.3f, 0.2f).onComplete = () =>
        {
            sliderTransform.DOScaleY(1.6f, 0.2f);
        };
        sliderTransform.DOLocalMoveX(20f, 0.2f).onComplete = () =>
        {
            sliderTransform.DOLocalMoveX(-600f, 0.2f).onComplete = () =>
            {
                sliderTransform.parent.gameObject.SetActive(false);

                GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionReward);
                if (GameManager.Task.GoldCollectionTaskManager.CurrentTask == null)
                {
                    gameObject.SetActive(false);
                }
            };
        };
        GameManager.Task.GoldCollectionTaskManager.LastRecordTotalCollectNum = GameManager.Task.GoldCollectionTaskManager.TotalCollectNum;
    }

    /// <summary>
    /// 横条消失
    /// </summary>
    private void OnDisable()
    {
        if (taskReward.transform.parent != sliderTransform)
        {
            taskReward.HideRewardShowEffect();
            taskReward.transform.SetParent(sliderTransform);
        }
        sliderTransform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
        sliderTransform.localPosition = new Vector3(-600, 0, 0);
        GameManager.Process.EndProcess(ProcessType.ShowGoldCollectionReward);
    }

    public static bool IsHaveRewardByGoldCollection()
    {
        try
        {
            GoldCollectionTaskManager goldCollectionTaskManager = GameManager.Task.GoldCollectionTaskManager;
            DTGoldCollectionData goldCollectionData = goldCollectionTaskManager.DataTable;
            RewardTask currentTask = goldCollectionTaskManager.CurrentTask;
            int needCollectNum = goldCollectionData.GetNeedCollectNum(currentTask.Index);
            int lastIndexNeedCollectNum = goldCollectionData.GetNeedCollectNum(currentTask.Index - 1);
            int totalCollectNum = goldCollectionTaskManager.TotalCollectNum;

            return totalCollectNum >= needCollectNum;
        }
        catch (Exception e)
        {
            Log.Info($"IsHaveRewardByGoldCollection:{e.Message}");
            return false;
        }
    }

    public override void OnButtonClick()
    {
        //已达到预告等级
        if (GameManager.PlayerData.NowLevel >= Constant.GameConfig.UnlockGoldCollectionButtonLevel)
        {
            //未解锁
            if (GameManager.PlayerData.NowLevel < Constant.GameConfig.UnlockGoldCollectionLevel)
            {
                GameManager.UI.ShowUIForm("GoldCollectionStartPanel");
            }
            //已解锁
            else
            {
                GameManager.UI.ShowUIForm("GoldCollectionMenu");
            }
        }
    }

    public override void OnLocked()
    {
        if (IsLocked)
            return;

        banner.SetActive(false);
        preview.SetActive(true);

        mainBg.color = Color.gray;
        spine.color = Color.gray;
        spine.freeze = true;

        base.OnLocked();
    }

    public override void OnUnlocked()
    {
        if (!IsLocked)
            return;

        banner.SetActive(true);
        preview.SetActive(false);

        mainBg.color = Color.white;
        spine.color = Color.white;
        spine.freeze = false;

        base.OnUnlocked();
    }
}
