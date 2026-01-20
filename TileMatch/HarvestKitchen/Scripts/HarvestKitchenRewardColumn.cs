using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class HarvestKitchenRewardColumn : MonoBehaviour
{
    public Image dishImage;
    public GameObject currentLight;
    public GameObject normalRewardBg, curRewardBg;
    public DelayButton rewardButton;
    public GameObject tickImage;
    public GameObject lockImage;
    public Transform line;
    public Transform rewardRoot;
    public HarvestKitchenRewardSlot[] rewardSlots;
    public GameObject chest;

    private int index;
    private HarvestKitchenRewardListMenu mainMenu;
    private List<TotalItemData> rewardType;
    private List<int> rewardNum;
    private AsyncOperationHandle dishHandle;

    public void OnInit(DTHarvestKitchenTaskDatas data, HarvestKitchenRewardListMenu menu)
    {
        index = data.ID;
        mainMenu = menu;
        rewardType = data.RewardsList;
        rewardNum = data.RewardsNumList;

        InitIndexArea();
        InitRewardArea();
        rewardButton.OnInit(OnButtonClick);
    }

    public void OnRelease()
    {
        UnityUtility.UnloadAssetAsync(dishHandle);
        dishHandle = default;
        
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            rewardSlots[i].Release();
        }
        rewardButton.OnReset();
    }

    /// <summary>
    /// 初始化序号条区域
    /// </summary>
    private void InitIndexArea()
    {
        int currentIndex = HarvestKitchenManager.Instance.TaskId;
        
        if (index == HarvestKitchenManager.Instance.TaskData.KitchenTaskDatas.Count)
        {
            line.gameObject.SetActive(false);
        }
        else if (index == currentIndex - 1)
        {
            line.localPosition = new Vector3(line.localPosition.x, 42.7f, 0);
            line.gameObject.SetActive(true);
        }
        else
        {
            line.localPosition = new Vector3(line.localPosition.x, 48.2f, 0);
            line.gameObject.SetActive(true);
        }
            
        if (index == currentIndex)
        {
            currentLight.SetActive(true);
        }
        else
        {
            currentLight.SetActive(false);
        }
        
        UnityUtility.UnloadAssetAsync(dishHandle);
        dishHandle = UnityUtility.LoadSpriteAsync("dishes_" + index.ToString(),
            "HarvestKitchenDishes",
            sp =>
            {
                dishImage.sprite = sp;
                //dishImage.SetNativeSize();
                
                if (index <= currentIndex)
                    dishImage.color = Color.white;
                else
                    dishImage.color = Color.gray;
            });
    }

    /// <summary>
    /// 初始化奖励区域
    /// </summary>
    private void InitRewardArea()
    {
        int currentIndex = HarvestKitchenManager.Instance.TaskId;

        if (index >= currentIndex)
        {
            if (rewardType.Count <= 2)
            {
                chest.SetActive(false);
                
                int deltaX = 170;
                if (rewardType.Count == 2)
                    deltaX = 200;
                else if (rewardType.Count == 3)
                    deltaX = 170;
                else if (rewardType.Count == 4)
                    deltaX = 160;
            
                // if (rewardType.Count == 4)
                //     rewardRoot.localScale=new Vector3(0.7f, 0.7f, 0.7f);
                // else
                //     rewardRoot.localScale=new Vector3(0.8f, 0.8f, 0.8f);
                rewardRoot.localScale = Vector3.one;
            
                Vector3[] posList = UnityUtility.GetAveragePosition(Vector3.zero, new Vector3(deltaX, 0, 0), rewardType.Count);
                for (int i = 0; i < rewardSlots.Length; i++)
                {
                    if (i < rewardType.Count)
                    {
                        rewardSlots[i].Init(rewardType[i], rewardNum[i]);
                        rewardSlots[i].transform.localPosition = posList[i];
                        rewardSlots[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        rewardSlots[i].gameObject.SetActive(false);
                    }
                }      
            }
            else
            {
                for (int i = 0; i < rewardSlots.Length; i++)
                {
                    rewardSlots[i].gameObject.SetActive(false);
                } 
                chest.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < rewardSlots.Length; i++)
            {
                rewardSlots[i].gameObject.SetActive(false);
            }  
            chest.SetActive(false);
        }
        
        curRewardBg.SetActive(index == currentIndex);
        normalRewardBg.SetActive(index != currentIndex);
        if (index == currentIndex)
        {
            tickImage.SetActive(false);
            lockImage.SetActive(false);
        }
        else if (index < currentIndex)
        {
            tickImage.SetActive(true);
            lockImage.SetActive(false);
        }
        else if (index > currentIndex)
        {
            tickImage.SetActive(false);
            lockImage.SetActive(true);
        }
    }

    private void OnButtonClick()
    {
        if (lockImage.activeSelf)
        {
            lockImage.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);   
        }

        if (rewardType.Count > 2)
        {
            mainMenu.itemPromptBox.HidePromptBox();
            mainMenu.itemPromptBox.Init(rewardType, rewardNum);
            mainMenu.itemPromptBox.ShowPromptBox(PromptBoxShowDirection.Up, chest.transform.position, 2f);
        }
    }
}
