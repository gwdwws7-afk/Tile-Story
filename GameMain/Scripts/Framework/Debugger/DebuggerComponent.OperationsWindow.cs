using System;
using System.Collections.Generic;
using MySelf.Model;
using UnityEngine;

public sealed partial class DebuggerComponent : GameFrameworkComponent
{
    private sealed class OperationsWindow : ScrollableDebuggerWindowBase
    {
        private int m_CurrentSelectLevel = 0;
        private string m_LevelText, m_DerviceIdText;
        private Vector2 m_ScrollPosition = Vector2.zero;
        private int m_CurrentDecorationAreaNum = 0;
        private string m_CurrentDecorationAreaText;
        private int currentCardSetNum = 1;
        private string currentCardSetText;

        protected override void OnDrawScrollableWindow()
        {
            GUIStyle biggerFontLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            GUIStyle biggerFontButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 26 };
            GUILayout.Label("<b>Operations</b>");
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                {
                    if (m_CurrentSelectLevel == 0)
                    {
                        m_CurrentSelectLevel = GameManager.PlayerData.NowLevel;
                        m_LevelText = m_CurrentSelectLevel.ToString();
                    }

                    if (GUILayout.Button("<<", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        m_CurrentSelectLevel--;
                        m_LevelText = m_CurrentSelectLevel.ToString();
                    }

                    m_LevelText = GUILayout.TextField(m_LevelText, biggerFontLabelStyle, GUILayout.Width(100), GUILayout.Height(50));

                    if (GUILayout.Button(">>", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        m_CurrentSelectLevel++;
                        m_LevelText = m_CurrentSelectLevel.ToString();
                    }

                    int.TryParse(m_LevelText, out m_CurrentSelectLevel);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(20);

                if (GUILayout.Button("Open Target Level", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    GameManager.PlayerData.NowLevel = m_CurrentSelectLevel;

                    GameManager.UI.OnInit();
                }
                
                if (GUILayout.Button("Level Win", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    try
                    {
                        var panel=GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;

                        if (panel != null&&panel.gameObject.activeInHierarchy)
                        {
                            panel.CheckWin(true);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
                if (GUILayout.Button("Level Lose", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    try
                    {
                        var panel=GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;

                        if (panel != null&&panel.gameObject.activeInHierarchy)
                        {
                            panel.CheckLose(true);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
                
                if (GUILayout.Button($"{(AdsCommon.IsForceOpenNativeInterstitial?"关闭":"开启")}native插屏", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    AdsCommon.IsForceOpenNativeInterstitial = !AdsCommon.IsForceOpenNativeInterstitial;
                    Log.Info($" {(AdsCommon.IsForceOpenNativeInterstitial?"强制开启":"关闭强制开启")}");
                }
                
                if (GUILayout.Button($"{(AdsCommon.IsForceOpenNativeReward?"关闭":"开启")}native RV", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    AdsCommon.IsForceOpenNativeReward = !AdsCommon.IsForceOpenNativeReward;
                    Log.Info($" {(AdsCommon.IsForceOpenNativeReward?"强制开启":"关闭强制开启")}");
                }
                
                // if (GUILayout.Button($"{(AdsCommon.IsForceOpenNativeChangeInterstitial?"关闭":"开启")}native替换插屏", biggerFontButtonStyle, GUILayout.Height(60)))
                // {
                //     AdsCommon.IsForceOpenNativeChangeInterstitial = !AdsCommon.IsForceOpenNativeChangeInterstitial;
                //     Log.Info($" {(AdsCommon.IsForceOpenNativeChangeInterstitial?"强制开启":"关闭强制开启")}");
                // }
                //
                // if (GUILayout.Button($"{(AdsCommon.IsForceOpenNativeChangeReward?"关闭":"开启")}native替换RV", biggerFontButtonStyle, GUILayout.Height(60)))
                // {
                //     AdsCommon.IsForceOpenNativeChangeReward = !AdsCommon.IsForceOpenNativeChangeReward;
                //     Log.Info($" {(AdsCommon.IsForceOpenNativeChangeReward?"强制开启":"关闭强制开启")}");
                // }
                
                if (GUILayout.Button($"{(PlayerPrefs.GetInt("LocalOpenKV", 0)==0?"开启":"关闭")}KV", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    PlayerPrefs.SetInt("LocalOpenKV", PlayerPrefs.GetInt("LocalOpenKV", 0) == 0 ? 1 : 0);
                    PlayerPrefs.Save();
                    Log.Info($" {(PlayerPrefs.GetInt("LocalOpenKV", 0)==1?"KV开启":"KV关闭")}");
                }
                
                if (GUILayout.Button("使用基准组Level", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    PlayerPrefs.SetString("RecordLevelPathName", "Level");
                    PlayerPrefs.Save();
                    Log.Info($"RecordLevelPathName:基准Level");
                }
                
                if (GUILayout.Button("使用A组Level", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    PlayerPrefs.SetString("RecordLevelPathName", "Level_A");
                    PlayerPrefs.Save();
                    Log.Info($"RecordLevelPathName:A Level");
                }
                
                if (GUILayout.Button("使用B组Level", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    PlayerPrefs.SetString("RecordLevelPathName", "Level_B");
                    PlayerPrefs.Save();
                    Log.Info($"RecordLevelPathName:B Level");
                }

                if (GUILayout.Button("使用C组Level", biggerFontButtonStyle, GUILayout.Height(60)))
                {
                    PlayerPrefs.SetString("RecordLevelPathName", "Level_C");
                    PlayerPrefs.Save();
                    Log.Info($"RecordLevelPathName:C Level");
                }

                GUILayout.Space(60);

                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                {
                    if (GUILayout.Button("Clear all Saved Data", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        PlayerPrefs.DeleteAll();
                        GameManager.DataNode.Clear();

                        GameManager.Task.OnReset();
                        GameManager.Task.OnInit();

                        GameManager.UI.OnInit();
                    }

                    if (GUILayout.Button("Clear Remove Ads State", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        //GameManager.PlayerData.IsRemoveAds = !GameManager.PlayerData.IsRemoveAds;
                        GameManager.Ads.IsRemovePopupAds = ! GameManager.Ads.IsRemovePopupAds;
                        if(!GameManager.Ads.IsRemovePopupAds) GameManager.Ads.ReInitAds();
                        GameManager.Ads.RequestAds();
                    }
                    
#if AmazonStore || UNITY_IOS
                    if (GUILayout.Button("展示MaxSDK Debugger", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        if(MaxSdk.IsInitialized())
                            MaxSdk.ShowMediationDebugger();
                    }
#endif
                    
                    if (GUILayout.Button("强制使用俄罗斯cdn下载", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        MyDownloadUrl.DownloadUrlManager.ForceUseRussiaCDN();
                    }

                    if (GUILayout.Button("强制使用俄罗斯广告", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        AdsCommon.IsCanLocalUseYandex = !AdsCommon.IsCanLocalUseYandex;
                        Debug.Log("设置强制使用俄罗斯广告为：" + AdsCommon.IsCanLocalUseYandex.ToString());
                    }

                    if (GUILayout.Button("开启Purchase Debugger", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.Purchase.DebuggerMode = true;
                    }
                    
                    if (GUILayout.Button("打开AB下载", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                       GameManager.UI.ShowUIForm("DownloadAbBundleMenuManager");
                    }

                    if (GUILayout.Button("清空金币数量", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        ItemModel.Instance.UseItem(TotalItemData.Coin.TotalItemType,
                            ItemModel.Instance.GetItemTotalNum(TotalItemData.Coin.TotalItemType));
                    }

                    if (GUILayout.Button("清空生命数量", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        ItemModel.Instance.UseItem(TotalItemData.Life.TotalItemType,
                            ItemModel.Instance.GetItemTotalNum(TotalItemData.Life.TotalItemType));
                        PlayerPrefs.SetString(Constant.PlayerData.InfiniteLifeEndTime, DateTime.Now.ToString(Constant.GameConfig.DefaultDateTimeFormet));
                    }


                    if (GUILayout.Button("道具增加100", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.PlayerData.AddItemNum(TotalItemData.Prop_Absorb, 100);
                        GameManager.PlayerData.AddItemNum(TotalItemData.Prop_AddOneStep, 100);
                        GameManager.PlayerData.AddItemNum(TotalItemData.Prop_Back, 100);
                        GameManager.PlayerData.AddItemNum(TotalItemData.Prop_ChangePos, 100);
                        GameManager.PlayerData.AddItemNum(TotalItemData.Prop_Grab, 100);
                    }

                    if (GUILayout.Button("添加金币 100000", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.PlayerData.AddItemNum(TotalItemData.Coin, 100000);
                    }

                    if (GUILayout.Button("添加星星 50", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.PlayerData.AddItemNum(TotalItemData.Star, 50);
                        GameManager.Event.Fire(this, StarNumRefreshEventArgs.Create());
                    }

                    if (GUILayout.Button("添加金块 100", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.Task.GoldCollectionTaskManager.TotalCollectNum += 100;
                    }
                    
                    if (GUILayout.Button("发1星卡包", biggerFontButtonStyle, GUILayout.Height(50)))
                    {
                        RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack1, 1);
                        RewardManager.Instance.AutoGetRewardDelayTime = 0f;
                        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true, () =>
                        {
                            RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                        });
                    }
                    if (GUILayout.Button("发2星卡包", biggerFontButtonStyle, GUILayout.Height(50)))
                    {
                        RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack2, 1);
                        RewardManager.Instance.AutoGetRewardDelayTime = 0f;
                        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true, () =>
                        {
                            RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                        });
                    }
                    if (GUILayout.Button("发3星卡包", biggerFontButtonStyle, GUILayout.Height(50)))
                    {
                        RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack3, 1);
                        RewardManager.Instance.AutoGetRewardDelayTime = 0f;
                        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true, () =>
                        {
                            RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                        });
                    }
                    if (GUILayout.Button("发4星卡包", biggerFontButtonStyle, GUILayout.Height(50)))
                    {
                        RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack4, 1);
                        RewardManager.Instance.AutoGetRewardDelayTime = 0f;
                        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true, () =>
                        {
                            RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                        });
                    }
                    if (GUILayout.Button("发5星卡包", biggerFontButtonStyle, GUILayout.Height(50)))
                    {
                        RewardManager.Instance.AddNeedGetReward(TotalItemData.CardPack5, 1);
                        RewardManager.Instance.AutoGetRewardDelayTime = 0f;
                        RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.CardTransparentRewardPanel, true, () =>
                        {
                            RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                        });
                    }
                    
                    GUILayout.BeginHorizontal();
                    {
                        currentCardSetText = currentCardSetNum.ToString();
                        
                        if (GUILayout.Button("<<", biggerFontButtonStyle, GUILayout.Height(60)))
                        {
                            currentCardSetNum--;
                            currentCardSetNum = Mathf.Clamp(currentCardSetNum, 1, 15);
                            currentCardSetText = currentCardSetNum.ToString();
                        }

                        currentCardSetText = GUILayout.TextField(currentCardSetText, biggerFontLabelStyle,
                            GUILayout.Width(100), GUILayout.Height(50));

                        if (GUILayout.Button(">>", biggerFontButtonStyle, GUILayout.Height(60)))
                        {
                            currentCardSetNum++;
                            currentCardSetNum = Mathf.Clamp(currentCardSetNum, 1, 15);
                            currentCardSetText = currentCardSetNum.ToString();
                        }

                        int.TryParse(currentCardSetText, out currentCardSetNum);
                    }
                    GUILayout.EndHorizontal();
                
                    if (GUILayout.Button($"Complete CardSet{currentCardSetNum}", biggerFontButtonStyle, GUILayout.Height(50)))
                    {
                        CardUtil.CompleteCardSet(currentCardSetNum);
                        MapTopPanelManager mapTopPanel = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
                        mapTopPanel?.cardSetEntrance.SetClaim();
                    }
                    
                    if (GUILayout.Button("添加储蓄罐金币 1000", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        PiggyBankModel.Instance.Data.PigTotalCoins += 1000;
                        PiggyBankModel.Instance.SaveToLocal();
                    }
                    if (GUILayout.Button("添加储蓄罐金币 100", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        PiggyBankModel.Instance.Data.PigTotalCoins += 100;
                        PiggyBankModel.Instance.SaveToLocal();
                    }
                    
                    if (GUILayout.Button("添加油桶 10", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        TilePassModel.Instance.TotalTargetNum += 10;
                    }

                    if (GUILayout.Button("添加油桶 100", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        TilePassModel.Instance.TotalTargetNum += 100;
                    }

                    if (GUILayout.Button("删掉banner并重新加载", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.Ads.DestroyBannerAndReLoad();
                    }
                    
                    if (GUILayout.Button("日历时间模式", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        TileMatchUtil.OpenCalendarTimeLimitTest = !TileMatchUtil.OpenCalendarTimeLimitTest;
                        Log.Debug($"{(TileMatchUtil.OpenCalendarTimeLimitTest ? "开启日历挑战时间模式测试" : "关闭日历挑战时间模式测试")}");
                    }

                    if (GUILayout.Button("增加遗迹寻宝稿子10", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        HiddenTemple.HiddenTempleManager.PlayerData.AddPickaxeNum(10);
                    }
                    
                    if (GUILayout.Button("增加棋子上升移动速度+5", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        KitchenManager.Instance.tileItemSpeed += 5f;
                        Log.Debug($"当前棋子上升速度为： {KitchenManager.Instance.tileItemSpeed}每秒");
                    }
                    if (GUILayout.Button("降低棋子上升移动速度-5", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        KitchenManager.Instance.tileItemSpeed -= 5f;
                        Log.Debug($"当前棋子上升速度为： {KitchenManager.Instance.tileItemSpeed}每秒");
                    }
                    if (GUILayout.Button("增加厨师帽15", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        KitchenManager.Instance.AddChefHat(15);
                    }
                    if (GUILayout.Button("增加点赞数100", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        KitchenManager.Instance.ActivityLevelWin(100);
                    }
                    
                    if (GUILayout.Button("增加厨师篮子15", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        HarvestKitchenManager.Instance.AddBasket(15);
                    }
                    if (GUILayout.Button("增加厨师做菜进度100", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        HarvestKitchenManager.Instance.ActivityLevelWin(100);
                    }

                    GUILayout.BeginHorizontal();
                    {
                        if (m_CurrentDecorationAreaNum == 0)
                        {
                            m_CurrentDecorationAreaNum = DecorationModel.Instance.Data.DecorationAreaID;
                            m_CurrentDecorationAreaText = m_CurrentDecorationAreaNum.ToString();
                        }

                        if (GUILayout.Button("<<", biggerFontButtonStyle, GUILayout.Height(60)))
                        {
                            m_CurrentDecorationAreaNum--;
                            m_CurrentDecorationAreaNum = Mathf.Max(m_CurrentDecorationAreaNum, 1);
                            m_CurrentDecorationAreaText = m_CurrentDecorationAreaNum.ToString();
                        }

                        GUILayout.Label(m_CurrentDecorationAreaText, biggerFontLabelStyle, GUILayout.Width(100), GUILayout.Height(50));

                        if (GUILayout.Button(">>", biggerFontButtonStyle, GUILayout.Height(60)))
                        {
                            m_CurrentDecorationAreaNum++;
                            m_CurrentDecorationAreaNum = Mathf.Min(m_CurrentDecorationAreaNum, Constant.GameConfig.MaxDecorationArea);
                            m_CurrentDecorationAreaText = m_CurrentDecorationAreaNum.ToString();
                        }
                    }
                    GUILayout.EndHorizontal();


                    if (GUILayout.Button($"测试装修第{m_CurrentDecorationAreaNum}章", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        SetDecorationAreaToTargetID(m_CurrentDecorationAreaNum);
                    }

                    //if (GUILayout.Button($"Send Test Notification in 1min", biggerFontButtonStyle, GUILayout.Height(60)))
                    //{
                    //    GameManager.Notification.DEBUG_SendTestNotification();
                    //}

                    if (GUILayout.Button("爬藤加3分", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        ClimbBeanstalkManager.Instance.CurrentWinStreak += 3;
                    }

                    if (GUILayout.Button("爬藤加10分", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        ClimbBeanstalkManager.Instance.CurrentWinStreak += 10;
                    }
        
                    if (GUILayout.Button("爬藤加100分", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        ClimbBeanstalkManager.Instance.CurrentWinStreak += 100;
                    }
        
                    if (GUILayout.Button("爬藤加1000分", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        ClimbBeanstalkManager.Instance.CurrentWinStreak += 1000;
                    }

                    //if (GUILayout.Button("爬藤失败", biggerFontButtonStyle, GUILayout.Height(60)))
                    //{
                    //    ClimbBeanstalkManager.Instance.OnGameLose();
                    //}
                    
                    if (GUILayout.Button("完成所有日历挑战", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.Task.CalendarChallengeManager.FinishAllCalendarChallenge();
                    }
                    if (GUILayout.Button("完成当月日历挑战", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.Task.CalendarChallengeManager.FinishAllCalendarChallengeOfThisMonth();
                    }
                    if (GUILayout.Button("个人赛到最高段位", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        PersonRankModel.Instance.Data.RankLevel = PersonRankLevel.Supreme;
                        PersonRankModel.Instance.SaveToLocal();
                    }

                    if (GUILayout.Button("弹出评价界面", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.UI.ShowUIForm("EvaluationMenu");
                    }

                    if (GUILayout.Button("清空所有道具", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GameManager.PlayerData.UseItem(TotalItemData.Prop_Absorb, GameManager.PlayerData.GetItemNum(TotalItemData.Prop_Absorb));
                        GameManager.PlayerData.UseItem(TotalItemData.Prop_AddOneStep, GameManager.PlayerData.GetItemNum(TotalItemData.Prop_AddOneStep));
                        GameManager.PlayerData.UseItem(TotalItemData.Prop_Back, GameManager.PlayerData.GetItemNum(TotalItemData.Prop_Back));
                        GameManager.PlayerData.UseItem(TotalItemData.Prop_ChangePos, GameManager.PlayerData.GetItemNum(TotalItemData.Prop_ChangePos));
                        GameManager.PlayerData.UseItem(TotalItemData.Prop_Grab, GameManager.PlayerData.GetItemNum(TotalItemData.Prop_Grab));
                        GameManager.PlayerData.UseItem(TotalItemData.MagnifierBoost, GameManager.PlayerData.GetItemNum(TotalItemData.MagnifierBoost));
                        GameManager.PlayerData.UseItem(TotalItemData.FireworkBoost, GameManager.PlayerData.GetItemNum(TotalItemData.FireworkBoost));
                    }
                    
                    if (GUILayout.Button("机器人1增加10分钟", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        BalloonRiseModel.Instance.SimulateRobotTimeAdd(0, 600);
                    }

                    if (GUILayout.Button("添加合成体力 10", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        Merge.MergeManager.PlayerData.AddMergeEnergyBoxNum(10);
                        GameManager.Event.Fire(this, Merge.RefreshMergeEnergyBoxEventArgs.Create());
                    }

                    if (GUILayout.Button("添加合成道具", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        Merge.MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(Merge.MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as Merge.MergeMainMenuBase;
                        if (mainBoard != null)
                        {
                            if (Merge.MergeManager.Instance.Theme == MergeTheme.LoveGiftBattle)
                                mainBoard.StoreProp(10104);
                            else if (Merge.MergeManager.Instance.Theme == MergeTheme.DigTreasure)
                                mainBoard.StoreProp(80104);
                            else
                                mainBoard.StoreProp(10101 + Merge.MergeManager.PlayerData.GetCurrentMaxMergeStage());
                        }
                    }

                    if (GUILayout.Button("添加合成长线道具", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        Merge.MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(Merge.MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as Merge.MergeMainMenuBase;
                        if (mainBoard != null)
                        {
                            if (Merge.MergeManager.Instance.Theme == MergeTheme.LoveGiftBattle)
                                mainBoard.StoreProp(80106);
                            else if (Merge.MergeManager.Instance.Theme == MergeTheme.DigTreasure)
                                mainBoard.StoreProp(10111);
                        }
                    }

                    if (GUILayout.Button("完成合成活动", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        if (Merge.MergeManager.Instance.Theme == MergeTheme.LoveGiftBattle)
                        {
                            Merge.MergeManager.PlayerData.SetLoveGiftRewardStage(7);
                        }
                    }

                    if (GUILayout.Button("开启UMP测试", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        //GameManager.DataNode.SetData("IsOpenUMPTest", true);
                        GameManager.DataNode.SetData("IsOpenKVTest", true);
                    }
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("设备ID：", biggerFontLabelStyle, GUILayout.Width(200));
                        m_DerviceIdText = GUILayout.TextField(m_DerviceIdText, biggerFontLabelStyle, GUILayout.Height(50));
                    }

                    if (GUILayout.Button("强制回收垃圾", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        Resources.UnloadUnusedAssets();
                    }

                    if (GUILayout.Button("强制GC", biggerFontButtonStyle, GUILayout.Height(60)))
                    {
                        GC.Collect();
                    }

                    GUILayout.EndHorizontal();
                    //if (GUILayout.Button("写入设备ID", biggerFontButtonStyle, GUILayout.Height(60)))
                    //{
                    //    if(!m_DerviceIdText.IsNullOrEmpty())
                    //        PlayerPrefs.SetString("UMPTestDevericeId", m_DerviceIdText);
                    //}
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        private void SetDecorationAreaToTargetID(int targetAreaID)
        {
            GameManager.PlayerData.AddItemNum(TotalItemData.Star, 50);
            DecorationModel.Instance.Data.areaGetRewardDic.Clear();
            DecorationModel.Instance.Data.decorateIDTypeDic.Clear();
            DecorationModel.Instance.Data.areaFinishedTimeStampDic.Clear();
            DecorationModel.Instance.SetDecorationAreaID(targetAreaID);
            DecorationModel.Instance.SetDecorationOperatingAreaID(targetAreaID);
            //DecorationModel.Instance.SetTargetAreaGetReward(targetAreaID, false);

            //把所有目标章节前的章节 的各家具装修状态设为1，领奖状态置为true，避免不必要的测试情况
            for (int i = 1; i < targetAreaID; ++i)
            {
                int alternativeAreaID = DecorationModel.Instance.GetAlteredDecorationAreaID(i);
                var items = GameManager.DataTable.GetDataTable<DTDecorateItem>().Data.GetDecorateItems(alternativeAreaID);
                for (int j = 0; j < items.Count; j++)
                {
                    DecorationModel.Instance.SetDecorationType(alternativeAreaID, j, 1);
                }
                //这里记录原值
                DecorationModel.Instance.SetTargetAreaGetReward(i, true);
            }

            DecorationModel.Instance.SaveToLocal();

            //所有该章节前的对话 也标记为已读,其他都是未读
            StoryModel.Instance.Data.RecordShowStoryIDs.Clear();
            List<HelpData> helpDataList = GameManager.DataTable.GetDataTable<DTHelp>().Data.HelpDatas;
            for (int i = 0; i < helpDataList.Count; ++i)
            {
                if (helpDataList[i].Chapter < targetAreaID)
                    GameManager.PlayerData.RecordShowStoryData(helpDataList[i].Chapter, helpDataList[i].BuildSchedule);
            }

            GameManager.UI.OnInit();
        }

    }
}
