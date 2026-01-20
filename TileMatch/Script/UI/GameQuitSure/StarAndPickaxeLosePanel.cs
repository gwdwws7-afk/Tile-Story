using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class StarAndPickaxeLosePanel : BaseGameFailPanel
{
    public GameObject starRoot,
        starTwoLose,
        starThreeLose,
        PickaxeRoot,
        PickaxeWinStreakRoot,
        LosePickaxe_Text,
        LosePickaxeStreak_Text,
        TargetParent,
        LifeRoot,
        LoseLifeImg,
        InfiniteLifeImg,
        LoseLifeText,
        LoseLevelText;
    public TextMeshProUGUILocalize Describe_Text;
    public TextMeshProUGUI PickaxeNumText, PickaxeWinStreakText;

    private int hardIndex = -1;
    public int HardIndex
    {
        get
        {
            if (hardIndex == -1)
            {
                int nowLevel = GameManager.PlayerData.RealLevel();
                hardIndex = DTLevelUtil.GetLevelHard(nowLevel);
            }

            return hardIndex;
        }
    }

    public override bool IsShowFailPanel => !GameManager.Task.CalendarChallengeManager.IsPlayingCalendarChallenge &&
            (LifeRoot || HardIndex > 0 || (HiddenTemple.HiddenTempleManager.PlayerData.GetPickaxeWinStreakStage() > 0 && 
                               HiddenTemple.HiddenTempleManager.Instance.CheckLevelWinCanGetPickaxe()));

    public override GameFailPanelPriorityType PriorityType => GameFailPanelPriorityType.StarAndPickaxeLosePanel;

    public override void ShowFailPanel(Action finishAction)
    {
        int showCount = 0;
        
        //显示需要挖宝的相关信息
        int stage = HiddenTemple.HiddenTempleManager.PlayerData.GetPickaxeWinStreakStage();
        if (stage > 0 && HiddenTemple.HiddenTempleManager.Instance.CheckLevelWinCanGetPickaxe())
        {
            //showCount = 1;
            PickaxeNumText.text = stage.ToString();
            PickaxeNumText.gameObject.SetActive(stage > 1);
            PickaxeRoot.SetActive(true);
            PickaxeWinStreakText.text = "+" + stage.ToString();
            PickaxeWinStreakRoot.SetActive(stage > 1);
            LosePickaxe_Text.gameObject.SetActive(stage == 1);
            LosePickaxeStreak_Text.gameObject.SetActive(stage > 1);

            float delayTime = 0;
            if (stage > 1)
            {
                GameObject pickaxeStreak = PickaxeWinStreakText.transform.parent.gameObject;
                pickaxeStreak.transform.localScale = Vector3.zero;
                pickaxeStreak.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack,1);
                delayTime = 0.15f;
            }
            
            GameObject pickaxe = PickaxeNumText.transform.parent.gameObject;
            pickaxe.transform.localScale = Vector3.zero;
            pickaxe.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack,1.2f).SetDelay(delayTime);
            
            starRoot.SetActive(false);
            Describe_Text.gameObject.SetActive(false);
            
            if (LifeRoot)
            {
                LifeRoot.SetActive(false);
                LoseLifeText.SetActive(false);
                LoseLevelText.SetActive(false);
            }
        }
        else
        {
            PickaxeRoot.SetActive(false);
            PickaxeWinStreakRoot.SetActive(false);
            LosePickaxe_Text.gameObject.SetActive(false);
            LosePickaxeStreak_Text.gameObject.SetActive(false);
            
            // 显示需要失去生命
            if (LifeRoot)
            {
                showCount++;
                bool isInfiniteLife = GameManager.PlayerData.GetInfiniteLifeTime() > 0;
                LoseLifeImg.SetActive(!isInfiniteLife);
                InfiniteLifeImg.SetActive(isInfiniteLife);
                LoseLifeText.SetActive(!isInfiniteLife && HardIndex == 0);
                LoseLevelText.SetActive(isInfiniteLife && HardIndex == 0);
                LifeRoot.SetActive(true);
            
                GameObject life = isInfiniteLife? InfiniteLifeImg : LoseLifeImg;
                life.transform.localScale = Vector3.zero;
                life.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack,1.2f).SetDelay((showCount - 1) * 0.12f);
            }
        
            // 显示将要失去的星星数
            if (HardIndex > 0)
            {
                showCount++;
                starRoot.SetActive(true);
                starTwoLose.SetActive(HardIndex == 1);
                starThreeLose.SetActive(HardIndex == 2);

                GameObject star = HardIndex == 1 ? starTwoLose : starThreeLose;
                star.transform.localScale = Vector3.zero;
                star.transform.DOScale(0.55f, 0.2f).SetEase(Ease.OutBack,1.2f).SetDelay((showCount - 1) * 0.12f);

                if (HardIndex == 1)
                    Describe_Text.SetTerm("FirstTry.You'll lose double stars");
                else if (HardIndex == 2)
                    Describe_Text.SetTerm("FirstTry.You'll lose triple stars");
                Describe_Text.gameObject.SetActive(true);
                if (LifeRoot)
                    LoseLifeText.SetActive(false);
            }
            else
            {
                starRoot.SetActive(false);
            
                if(LifeRoot)
                    LoseLifeText.GetComponent<TextMeshProUGUILocalize>().SetTerm("Game.You will lose a life!");
            }
        }
        
        // if (showCount >= 3)
        //     TargetParent.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        // else
        //     TargetParent.transform.localScale = Vector3.one;
    }
}
