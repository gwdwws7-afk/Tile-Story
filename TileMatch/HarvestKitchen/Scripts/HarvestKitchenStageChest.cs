using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarvestKitchenStageChest : MonoBehaviour
{
    public Image chestCloseImg, chestOpenImg;
    public Image sliderFill;
    public TextMeshProUGUI sliderText;
    public GameObject chest, slider, tick, key, sliderBorder;
    public DelayButton chestBtn;

    private DTHarvestKitchenChestDatas chestData;

    public void Init(DTHarvestKitchenChestDatas data, int curChestId)
    {
        chestData = data;

        if (chestData.ID < curChestId)
        {
            chestCloseImg.gameObject.SetActive(false);
            chestOpenImg.gameObject.SetActive(true);
            chest.transform.localScale=new Vector3(0.8f,0.8f,0.8f);
            slider.SetActive(false);
            tick.SetActive(true);
        }
        else if (chestData.ID >= curChestId)
        {
            chestCloseImg.gameObject.SetActive(true);
            chestOpenImg.gameObject.SetActive(false);
            if (chestData.ID == curChestId)
            {
                int progress = HarvestKitchenManager.Instance.GetCurrentChestProgress(curChestId);
                sliderFill.fillAmount = progress / (float)chestData.TargetDishNum;
                sliderText.text = progress.ToString() + "/" + chestData.TargetDishNum.ToString();
                chest.transform.localScale=new Vector3(1f,1f,1f);
                slider.transform.localScale=new Vector3(1f,1f,1f);
            }
            else
            {
                sliderFill.fillAmount = 0;
                sliderText.text = "0/" + chestData.TargetDishNum.ToString();
                chest.transform.localScale=new Vector3(0.8f,0.8f,0.8f);
                slider.transform.localScale=new Vector3(0.8f,0.8f,0.8f);
            }
            slider.SetActive(true);
            tick.SetActive(false);
        }
        
        chestBtn.OnInit(OnChestButtonClick);
    }

    public void ShowOpenChestAnim()
    {
        //float scale = 0.52f;
        // chestCloseImg.transform.DOScale(scale * 0.9f, 0.1f).onComplete = () =>
        // {
        //     chestCloseImg.gameObject.SetActive(false);
        //     chestOpenImg.transform.localScale = new Vector3(scale * 0.9f, scale * 0.9f, scale * 0.9f);
        //     chestOpenImg.gameObject.SetActive(true);
        //
        //     chestOpenImg.transform.DOScale(scale * 1.05f, 0.1f).onComplete = () =>
        //     {
        //         chestOpenImg.transform.DOScale(scale, 0.1f);
        //     };
        // };
        chestCloseImg.gameObject.SetActive(false);
        chestOpenImg.gameObject.SetActive(true);
        slider.gameObject.SetActive(false);
        tick.transform.localScale = Vector3.zero;
        tick.gameObject.SetActive(true);
        tick.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
    }

    public void ShowIncreaseProgressAnim()
    {
        // chestCloseImg.transform.DOScale(0.6f, 0.15f).SetEase(Ease.OutCubic).onComplete = () =>
        // {
        //     chestCloseImg.transform.DOScale(0.5f, 0.15f).SetEase(Ease.OutCubic);
        // };
        
        int progress = HarvestKitchenManager.Instance.GetCurrentChestProgress(chestData.ID);
        if (progress > chestData.TargetDishNum) 
            progress = chestData.TargetDishNum;
        float targetValue = progress / (float)chestData.TargetDishNum;
        if (targetValue > sliderFill.fillAmount)
        {
            sliderFill.DOFillAmount(targetValue, 0.1f).SetDelay(0.1f).SetEase(Ease.Linear).onComplete = () =>
            {
                sliderText.text = progress.ToString() + "/" + chestData.TargetDishNum.ToString();
            };
        }
    }

    private void OnChestButtonClick()
    {
        if (chestData == null)
            return;
        
        HarvestKitchenMainMenu mainMenu = GameManager.UI.GetUIForm("HarvestKitchenMainMenu") as HarvestKitchenMainMenu;
        if (mainMenu != null)
        {
            mainMenu.ShowItemPromptBox(chestData.RewardsList, chestData.RewardsNumList,
                chest.transform.position + new Vector3(0, 0.02f, 0));
        }
    }
}
