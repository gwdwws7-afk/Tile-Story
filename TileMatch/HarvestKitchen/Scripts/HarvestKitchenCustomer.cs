using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarvestKitchenCustomer : MonoBehaviour
{
    public enum CustomerState
    {
        none,
        idle,
        angry,
        happy
    }
    
    private bool needFood = false;
    private bool isStart = false;
    private CustomerState state = CustomerState.none;

    private int customerId = 0;
    // 当前剩余情绪值
    private float emotionValue = 100;
    // 情绪削减速度
    private float emotionReduce = 0;
    public GameObject green, yellow, red;
    public Image slider;
    public Image needFrame;
    public Transform needFrameTrans;
    public SkeletonGraphic customerSpine;

    private List<bool> needFoodState = new List<bool> { true, true };
    // 需要的食物id   需要的食物数量
    private List<int> itemId, needNum;
    public List<Image> needImg;
    public List<TextMeshProUGUI> needNumText;
    public List<Transform> completeImgTrans;
    public List<Transform> failImgTrans;

    private Vector3 moveTargetPos;

    private Action leaveAction = null;
    private Action completeAction = null;
    private Action timeoutAction = null;
    private Action<CustomerState> stateFinishAction = null;

    public int CustomerId => customerId;
    
    public void Init(int id, KitchenCustomerData data, Vector3 moveTarget, Action action = null)
    {
        customerId = id;
        itemId = new List<int>();
        itemId.AddRange(data.needFoodId);
        needNum = new List<int>();
        needNum.AddRange(data.needFoodNum);
        emotionReduce = data.emotionReduce;
        moveTargetPos = moveTarget;
        emotionValue = 100;
        slider.fillAmount = 1;
        for (int i = 0; i < needFoodState.Count; i++)
        {
            needFoodState[i] = true;
        }
        
        for (int i = 0; i < itemId.Count; i++)
        {
            needImg[i].sprite = HarvestKitchenManager.Instance.GetTileSpriteById(itemId[i]);
            needImg[i].SetNativeSize();
            needNumText[i].text = needNum[i].ToString();
            if (itemId.Count == 1)
            {
                needImg[i].transform.localPosition = new Vector3(0, 8, 0);
                completeImgTrans[i].localPosition = Vector3.zero;
                failImgTrans[i].localPosition = Vector3.zero;
            }
            else
            {
                needImg[i].transform.localPosition = new Vector3(-70 + i * 140, 8, 0);
                completeImgTrans[i].localPosition = Vector3.right * (-70 + i * 140);
                failImgTrans[i].localPosition = Vector3.right * (-70 + i * 140);
            }
        }

        needImg[0].gameObject.SetActive(true);
        needImg[0].transform.localScale = Vector3.one;
        needImg[1].gameObject.SetActive(itemId.Count == 2);
        needImg[1].transform.localScale = Vector3.one;
        completeImgTrans[0].gameObject.SetActive(false);
        completeImgTrans[1].gameObject.SetActive(false);
        failImgTrans[0].gameObject.SetActive(false);
        failImgTrans[1].gameObject.SetActive(false);

        needFrame.rectTransform.sizeDelta = new Vector2(itemId.Count == 1 ? 250f : 350f, 162f);
        // 隐藏顾客需求
        needFrameTrans.gameObject.SetActive(false);
        slider.transform.parent.gameObject.SetActive(false);
        
        green.SetActive(true);
        yellow.SetActive(false);
        red.SetActive(false);

        SetCustomerState(CustomerState.idle);
        
        if (action != null)
        {
            completeAction = action;
        }
    }

    public void Update()
    {
        if (isStart)
        {
            emotionValue -= emotionReduce * emotionReduce * Time.deltaTime;
            slider.fillAmount = emotionValue / 100;
            if (slider.fillAmount < 0.66f)
            {
                //WaitTimeOut();
                emotionValue = 100;
                SetCustomerState(CustomerState.angry);
            }
            // else if (slider.fillAmount < 1 / 3f)
            // {
            //     if (!red.activeSelf)
            //     {
            //         green.SetActive(false);
            //         yellow.SetActive(false);
            //         red.SetActive(true);
            //     }
            // }
            // else if (slider.fillAmount < 2 / 3f)
            // {
            //     if (!yellow.activeSelf)
            //     {
            //         green.SetActive(false);
            //         yellow.SetActive(true);
            //         red.SetActive(false);
            //     }
            // }
        }
    }
    
    public void WaitTimeOut()
    {
        Log.Info("Kitchen:超时未完成食物提供，游戏失败");
        // 隐藏食物需求和进度条
        isStart = false;
        needFood = false;
        // 播放角色生气动画
        timeoutAction?.Invoke();
    }

    public int IsNeedFood(int id)
    {
        if (needFood)
        {
            for (int i = 0; i < itemId.Count; i++)
            {
                if (itemId[i] == id && needNum[i] > 0)
                    return i;
            }
        }

        return -1;
    }

    public List<int> GetNeededFoodsId()
    {
        List<int> result = new List<int>();
        for (int i = 0; i < itemId.Count; i++)
        {
            if (needNum[i] > 0)
                result.Add(itemId[i]);
        }

        return result;
    }

    private int flyAnimNum = 0;

    public List<int> GetFood(int id)
    {
        flyAnimNum += 1;

        for (int i = 0; i < itemId.Count; i++)
        {
            if (itemId[i] == id)
            {
                needNum[i] -= 1;
                break;
            }
        }
        
        if(needNum.Sum() <= 0)
        {
            Log.Info($"Kitchen:{transform.name}完成了食物收集");
            // 隐藏食物需求和进度条
            needFood = false;
            isStart = false;
        }

        return needNum;
    }

    public void ReduceFlyAnimNum()
    {
        flyAnimNum -= 1;
    }

    public void SetCustomerState(CustomerState newState)
    {
        if (state != newState)
        {
            state = newState;
            switch (state)
            {
                case CustomerState.idle:
                    customerSpine.AnimationState.SetAnimation(0, "idle", true);
                    break;
                case CustomerState.angry:
                    customerSpine.AnimationState.SetAnimation(0, "angry", false).Complete += t =>
                    {
                        SetCustomerState(CustomerState.idle);
                    };
                    break;
                case CustomerState.happy:
                    emotionValue = 100;
                    customerSpine.AnimationState.SetAnimation(0, "happy", false).Complete += t =>
                    {
                        SetCustomerState(CustomerState.idle);
                        
                        stateFinishAction?.Invoke(CustomerState.happy);
                        stateFinishAction = null;
                    };
                    break;
            }
        }
    }
    
    public void ResfershNeedText(List<int> num, int needIndex, int customerIndex)
    {
        bool isComplete = CheckCustomerState();
        for (int i = 0; i < num.Count; i++)
        {
            needNumText[i].text = num[i].ToString();
            if (needFoodState[i])
            {
                if (num[i] == 0)
                {
                    needFoodState[i] = false;
                    Transform complete = completeImgTrans[i];
                    Transform foodTrans = needImg[i].transform;
                    complete.localScale = Vector3.zero;
                    foodTrans.DOScale(0, 0.2f).SetEase(Ease.InBack).onComplete += () =>
                    {
                        // 食物收集完成播放音效 
                        if (isComplete)
                            GameManager.Sound.PlayAudio(SoundType.SFX_Kitchen_Match_Order_Satisfied.ToString());
                        else
                            GameManager.Sound.PlayAudio(SoundType.SFX_DecorationObjectFinished.ToString());
                        foodTrans.gameObject.SetActive(false);
                        complete.gameObject.SetActive(true);
                        complete.DOScale(1, 0.2f).onComplete += () =>
                        {
                            // 等到收集完成动画播放完毕
                            if (isComplete)
                            {
                                needFrameTrans.DOScale(0, 0.3f);
                                slider.transform.parent.gameObject.SetActive(false);
                                // 角色动画播放毕后播放角色离开的动画
                                LeaveRestaurant(customerIndex);
                            }
                        };
                    };
                }
                else if (needIndex == i) 
                {
                    Transform foodTrans = needImg[i].transform;
                    foodTrans.DOScale(1.3f, 0.1f).SetEase(Ease.InOutCubic).onComplete = () =>
                    {
                        foodTrans.DOScale(1f, 0.1f).SetEase(Ease.InCubic);
                    };
                }
            }
        }
    }
    
    public void LeaveRestaurant(int index)
    {
        void CustomerLeaveEvent(CustomerState endState)
        {
            if (endState == CustomerState.happy)
            {
                // 防止顾客在向一号位移动的过程中完成收集，导致动画出现bug
                transform.DOKill();
                transform.DOLocalMove(moveTargetPos, 0.5f + 0.5f * index).SetEase(Ease.Linear).onComplete +=
                    () =>
                    {
                        completeAction?.Invoke();
                    };

                leaveAction?.Invoke();   
            }
        };

        if (state == CustomerState.happy) 
        {
            stateFinishAction = CustomerLeaveEvent;

            GameManager.Task.AddDelayTriggerTask(1f, () =>
            {
                stateFinishAction?.Invoke(CustomerState.happy);
                stateFinishAction = null;
            });
        }
        else
        {
            CustomerLeaveEvent(CustomerState.happy);
        }
    }

    public void SetLeaveEvent(Action action)
    {
        leaveAction = action;
    }

    public void ShowNeedInfo(Action callback = null, Action StartAction = null)
    {
        for (int i = 0; i < needNum.Count; i++)
        {
            needNumText[i].text = needNum[i].ToString();
        }

        needFrameTrans.localScale = Vector3.zero;
        needFrameTrans.gameObject.SetActive(true);
        needFrameTrans.DOScale(1, 0.3f).onComplete += () =>
        {
            callback?.InvokeSafely();
            callback = null;
            isStart = true;
            needFood = true;
            
            StartAction?.InvokeSafely();
            StartAction = null;
        };
    }

    public void ShowFail(Action callback)
    {
        //customerSpine.Initialize(true);
        //customerSpine.AnimationState.SetAnimation(0, $"unhappy{customerId}", false);
        customerSpine.AnimationState.SetAnimation(0, "angry", false);
        
        for (int i = 0; i < itemId.Count; i++)
        {
            if (needFoodState[i] && needNum[i] > 0)
            {
                Transform fail = failImgTrans[i];
                Transform foodTrans = needImg[i].transform;
                fail.localScale = Vector3.zero;
                foodTrans.DOScale(0, 0.5f).onComplete += () =>
                {
                    foodTrans.gameObject.SetActive(false);
                    fail.gameObject.SetActive(true);
                    fail.DOScale(1, 0.5f).SetEase(Ease.OutBack).onComplete += () =>
                    {
                        callback?.InvokeSafely();
                        callback = null;
                    };
                };
            }
        }
    }

    public bool CheckCustomerState()
    {
        return needNum.Sum() <= 0 && flyAnimNum <= 0;
    }

    /// <summary>
    /// 顾客是否完成收集
    /// </summary>
    public bool CheckCollectComplete()
    {
        return needNum.Sum() <= 0;
    }

    public void SetTimeoutEvent(Action action)
    {
        timeoutAction = action;
    }

    public void GamePause()
    {
        isStart = false;
        needFood = false;
    }

    public void LevelContinue()
    {
        // 恢复情绪值
        emotionValue = 100;
        // 刷新情绪值显示
        slider.fillAmount = emotionValue / 100;
        
        green.SetActive(true);
        yellow.SetActive(false);
        red.SetActive(false);
        
        customerSpine.AnimationState.SetAnimation(0, "idle", true);
        
        for (int i = 0; i < itemId.Count; i++)
        {
            if (needFoodState[i])
            {
                failImgTrans[i].gameObject.SetActive(false);
                needImg[i].transform.localScale = Vector3.one;
                needImg[i].gameObject.SetActive(true);
            }
        }
    }

    public void GameContinue()
    {
        // 继续游戏
        if (needNum.Sum() > 0)
        {
            isStart = true;
            needFood = true;
        }
    }

    public void OnRelease()
    {
        if (gameObject)
            Destroy(gameObject);
    }
}
