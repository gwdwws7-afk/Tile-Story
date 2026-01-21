using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GoldCollectionColumn : MonoBehaviour
{
    public Image barBg;
    public Image indexBg;
    public TextMeshProUGUI indexText;
    public GameObject currentLight;
    public Image rewardBg;
    public Image rewardImage;
    public TextMeshProUGUI rewardText;
    public DelayButton rewardButton;
    public GameObject tickImage;
    public GameObject lockImage;

    private GoldCollectionMenu goldCollectionMenu;

    private int index;
    private List<TotalItemData> rewardType;
    private List<int> rewardNum;

    private List<AsyncOperationHandle> loadAssetHandleList = new List<AsyncOperationHandle>();

    public void OnInit(GoldCollectionStage data, GoldCollectionMenu goldCollectionMenu)
    {
        index = data.Index;
        rewardType = data.RewardTypeList;
        rewardNum = data.RewardNumList;
        this.goldCollectionMenu = goldCollectionMenu;

        InitIndexArea();
        InitRewardArea();
        rewardButton.OnInit(OnButtonClick);
    }

    public void OnRelease()
    {
        for (int i = 0; i < loadAssetHandleList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(loadAssetHandleList[i]);
        }
        loadAssetHandleList.Clear();
        rewardButton.OnReset();
    }

    /// <summary>
    /// 初始化序号条区域
    /// </summary>
    private void InitIndexArea()
    {
        indexText.SetText(index.ToString());
        if (index == 1)
        {
            LoadSpriteAsync("开始", "GoldCollection", sp =>
            {
                barBg.sprite = sp;
                barBg.SetNativeSize();
                barBg.transform.localPosition = new Vector3(-279.5f, 0);
            });
        }
        else if (index == GameManager.Task.GoldCollectionTaskManager.DataTable.GoldCollectionStages.Count)
        {
            LoadSpriteAsync("末端", "GoldCollection", sp =>
            {
                barBg.sprite = sp;
                barBg.SetNativeSize();
                barBg.transform.localPosition = new Vector3(-280.5f, -39);
            });
        }
        else
        {
            LoadSpriteAsync("中间", "GoldCollection", sp =>
            {
                barBg.sprite = sp;
                barBg.SetNativeSize();
                barBg.transform.localPosition = new Vector3(-280.5f, 0);
            });
        }

        int currentIndex = GameManager.Task.GoldCollectionTaskManager.CurrentIndex;
        if (index == currentIndex)
        {
            LoadSpriteAsync("任务界面当前", "GoldCollection", sp =>
            {
                indexBg.sprite = sp;
            });
            GameManager.Localization.GetPresetMaterialAsync("Btn_Yellow", "BANGOPRO SDF", mat =>
            {
                indexText.fontSharedMaterial = mat;
            });
            currentLight.SetActive(true);
        }
        else if (index < currentIndex)
        {
            LoadSpriteAsync("任务界面已完成", "GoldCollection", sp =>
            {
                indexBg.sprite = sp;
            });
            GameManager.Localization.GetPresetMaterialAsync("Btn_Green", "BANGOPRO SDF", mat =>
            {
                indexText.fontSharedMaterial = mat;
            });
            currentLight.SetActive(false);
        }
        else if (index > currentIndex)
        {
            LoadSpriteAsync("任务界面未到达", "GoldCollection", sp =>
            {
                indexBg.sprite = sp;
            });
            GameManager.Localization.GetPresetMaterialAsync("Btn_Blue", "BANGOPRO SDF", mat =>
            {
                indexText.fontSharedMaterial = mat;
            });
            currentLight.SetActive(false);
        }
    }

    /// <summary>
    /// 初始化奖励区域
    /// </summary>
    private void InitRewardArea()
    {
        string rewardImageSp = string.Empty;
        if (rewardType.Count == 1)
        {
            rewardImageSp = UnityUtility.GetRewardSpriteKey(rewardType[0], rewardNum[0]);
            rewardText.SetItemText(rewardNum[0], rewardType[0], false);
            rewardImage.transform.localPosition = new Vector3(-90, -8);
            if (rewardImageSp == "Coin3") 
                rewardImage.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            else
                rewardImage.transform.localScale = Vector3.one;
        }
        else
        {
            rewardImageSp = "Chest3";
            rewardImage.transform.localPosition = Vector3.zero;
            rewardImage.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }

        int currentIndex = GameManager.Task.GoldCollectionTaskManager.CurrentIndex;
        if (index == currentIndex)
        {
            LoadSpriteAsync("气泡黄色", "GoldCollection", sp =>
            {
                rewardBg.sprite = sp;
            });
            LoadSpriteAsync(rewardImageSp, "TotalItemAtlas", sp =>
            {
                rewardImage.sprite = sp;
                //rewardImage.SetNativeSize();
                //rewardImage.transform.localScale = rewardType.Count == 1 ? new Vector3(0.44f, 0.44f, 0.44f) : new Vector3(1.25f, 1.25f, 1.25f);
            });
            rewardImage.gameObject.SetActive(true);
            rewardText.gameObject.SetActive(rewardType.Count == 1);
            tickImage.SetActive(false);
            lockImage.SetActive(false);
        }
        else if (index < currentIndex)
        {
            LoadSpriteAsync("气泡蓝色", "GoldCollection", sp =>
            {
                rewardBg.sprite = sp;
            });
            rewardImage.gameObject.SetActive(false);
            rewardText.gameObject.SetActive(false);
            tickImage.SetActive(true);
            lockImage.SetActive(false);
        }
        else if (index > currentIndex)
        {
            LoadSpriteAsync("气泡蓝色", "GoldCollection", sp =>
            {
                rewardBg.sprite = sp;
            });
            LoadSpriteAsync(rewardImageSp, "TotalItemAtlas", sp =>
            {
                rewardImage.sprite = sp;
                //rewardImage.SetNativeSize();
                //rewardImage.transform.localScale = rewardType.Count == 1 ? new Vector3(0.44f, 0.44f, 0.44f) : new Vector3(1.25f, 1.25f, 1.25f);
            });
            rewardImage.gameObject.SetActive(true);
            rewardText.gameObject.SetActive(rewardType.Count == 1);
            tickImage.SetActive(false);
            lockImage.SetActive(true);
        }
    }

    /// <summary>
    /// 异步加载图片
    /// </summary>
    private void LoadSpriteAsync(string spriteKey, string atlasName, Action<Sprite> completeAction)
    {
        loadAssetHandleList.Add(UnityUtility.LoadSpriteAsync(spriteKey, atlasName, sp =>
        {
            completeAction?.Invoke(sp);
        }));
    }

    private void OnButtonClick()
    {
        List<GoldCollectionStage> stageList = GameManager.Task.GoldCollectionTaskManager.DataTable.GoldCollectionStages;
        int currentIndex = stageList.Count - goldCollectionMenu.scrollArea.GetViewportCenterIndex();
        PromptBoxShowDirection direction = index >= currentIndex ? PromptBoxShowDirection.Down : PromptBoxShowDirection.Up;

        if (!tickImage.activeSelf && rewardType.Count > 1)
        {
            goldCollectionMenu.itemPromptBox.Init(rewardType, rewardNum);
            goldCollectionMenu.itemPromptBox.ShowPromptBox(direction, rewardImage.transform.parent.position);
        }
        else if (lockImage.activeSelf)
        {
            goldCollectionMenu.textPromptBox.ShowPromptBox(direction, rewardImage.transform.parent.position);
        }

        lockImage.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
    }
}
