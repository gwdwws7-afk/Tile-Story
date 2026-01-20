using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MySelf.Model;
using Merge;

public sealed class ShortCutMenu : EditorWindow
{
    [MenuItem("Window/ShortcutMenu")]
    static void Init()
    {
        ShortCutMenu myShortcutMenu = (ShortCutMenu)EditorWindow.GetWindow(typeof(ShortCutMenu), false, "ShortcutMenu", true);
        myShortcutMenu.Show();
    }

    private int currentSelectLevel = 0;
    private int index = 0;
    private string levelText;
    private string indexText;

    private void OnGUI()
    {
        GUILayout.Space(20);

        if (GUILayout.Button("清空所有本地数据"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        if (GUILayout.Button("清空本地所有下载缓存"))
        {
            Caching.ClearCache();
            Log.Info($"清空所有的下载缓存成功");
        }

        if (GUILayout.Button("清空金币数量"))
        {
            ItemModel.Instance.UseItem(TotalItemData.Coin.TotalItemType,
                ItemModel.Instance.GetItemTotalNum(TotalItemData.Coin.TotalItemType));
        }

        if (GUILayout.Button("清空生命数量"))
        {
            ItemModel.Instance.UseItem(TotalItemData.Life.TotalItemType,
                ItemModel.Instance.GetItemTotalNum(TotalItemData.Life.TotalItemType));
        }

        if (GUILayout.Button((TileMatchUtil.EditorOpenEnterLevel ? "Editor关闭放过机制" : "Editor打开放过机制")))
        {
            TileMatchUtil.EditorOpenEnterLevel = !TileMatchUtil.EditorOpenEnterLevel;
        }

        if (GUILayout.Button("Level Win"))
        {
            try
            {
                var panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;

                if (panel != null && panel.gameObject.activeInHierarchy)
                {
                    panel.CheckWin(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        if (GUILayout.Button("Level Lose"))
        {
            try
            {
                var panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;

                if (panel != null && panel.gameObject.activeInHierarchy)
                {
                    panel.CheckLose(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        if (GUILayout.Button("跳到300关"))
        {
            LevelModel.Instance.SetLevelNum(300);
        }
        
        if (GUILayout.Button("添加储存罐1000金币"))
        {
            PiggyBankModel.Instance.AddCoinPigTotalCoins1000();
        }
        
        if (GUILayout.Button("添加储存罐100金币"))
        {
            PiggyBankModel.Instance.AddCoinPigTotalCoins100();
        }

        if (GUILayout.Button("添加金币 100000"))
        {
            ItemModel.Instance.AddItem(TotalItemData.Coin.TotalItemType, 100000);
        }

        if (GUILayout.Button("添加星星 10", GUILayout.Height(60)))
        {
            GameManager.PlayerData.AddItemNum(TotalItemData.Star, 10);
            GameManager.Event.Fire(this, StarNumRefreshEventArgs.Create());
        }

        if (GUILayout.Button("添加金块 10", GUILayout.Height(60)))
        {
            GameManager.Task.GoldCollectionTaskManager.TotalCollectNum += 10;
        }

        if (GUILayout.Button("减少金块 10", GUILayout.Height(60)))
        {
            GameManager.Task.GoldCollectionTaskManager.TotalCollectNum -= 10;
        }
        
        if (GUILayout.Button("添加油桶 10", GUILayout.Height(60)))
        {
            TilePassModel.Instance.TotalTargetNum += 10;
        }

        if (GUILayout.Button("添加油桶 100", GUILayout.Height(60)))
        {
            TilePassModel.Instance.TotalTargetNum += 100;
        }

        if (GUILayout.Button("去广告"))
        {
            if (GameManager.Ads) GameManager.Ads.IsRemovePopupAds = !GameManager.Ads.IsRemovePopupAds;
            else AdsModel.Instance.SetRemoveAds(true);

            Log.Debug($"{(GameManager.Ads.IsRemovePopupAds ? "当前为去广告状态" : "当前为未去广告状态")}");
        }

        if (GUILayout.Button("道具增加10"))
        {
            ItemModel.Instance.AddItem(TotalItemData.Prop_Absorb.TotalItemType, 10);
            ItemModel.Instance.AddItem(TotalItemData.Prop_AddOneStep.TotalItemType, 10);
            ItemModel.Instance.AddItem(TotalItemData.Prop_Back.TotalItemType, 10);
            ItemModel.Instance.AddItem(TotalItemData.Prop_ChangePos.TotalItemType, 10);
            ItemModel.Instance.AddItem(TotalItemData.Prop_Grab.TotalItemType, 10);
        }

        if (GUILayout.Button("日历时间模式"))
        {
            TileMatchUtil.OpenCalendarTimeLimitTest = !TileMatchUtil.OpenCalendarTimeLimitTest;
            Log.Debug($"{(TileMatchUtil.OpenCalendarTimeLimitTest ? "开启日历挑战时间模式测试" : "关闭日历挑战时间模式测试")}");
        }

        GUILayout.BeginHorizontal();
        {
            if (currentSelectLevel == 0)
            {
                currentSelectLevel = LevelModel.Instance.EditorData.Level;
                levelText = currentSelectLevel.ToString();
            }

            if (GUILayout.Button("<<", GUILayout.Height(50)))
            {
                currentSelectLevel--;
                levelText = currentSelectLevel.ToString();
            }

            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            levelText = GUILayout.TextField(levelText, style, GUILayout.Width(100), GUILayout.Height(50));

            if (GUILayout.Button(">>", GUILayout.Height(50)))
            {
                currentSelectLevel++;
                levelText = currentSelectLevel.ToString();
            }

            int.TryParse(levelText, out currentSelectLevel);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (GUILayout.Button("SetCurLevel", GUILayout.Height(50)))
        {
            LevelModel.Instance.SetLevelNum(currentSelectLevel);
        }

        GUILayout.BeginHorizontal();
        {
            if (index == 0)
            {
                index = LevelModel.Instance.EditorData.Level;
                indexText = index.ToString();
            }

            if (GUILayout.Button("<<", GUILayout.Height(50)))
            {
                index--;
                indexText = index.ToString();
            }

            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            indexText = GUILayout.TextField(indexText, style, GUILayout.Width(100), GUILayout.Height(50));

            if (GUILayout.Button(">>", GUILayout.Height(50)))
            {
                index++;
                indexText = index.ToString();
            }

            int.TryParse(indexText, out index);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("测试动画", GUILayout.Height(50)))
        {
            var personRankMenu = GameManager.UI.GetUIForm("PersonRankMenu").GetComponent<PersonRankMenu>();
            personRankMenu.ExchangePanel();
        }

        if (GUILayout.Button("设置分数高", GUILayout.Height(50)))
        {
            PersonRankModel.Instance.Score = 1500;
            PersonRankModel.Instance.GameWin();
        }

        if (GUILayout.Button("日历挑战", GUILayout.Height(50)))
        {
            GameManager.UI.ShowUIForm("CalendarChallengeWin");
        }

        if (GUILayout.Button("完成所有日历挑战", GUILayout.Height(50)))
        {
            GameManager.Task.CalendarChallengeManager.FinishAllCalendarChallenge();
        }
        
        if (GUILayout.Button("重置所有日历挑战", GUILayout.Height(50)))
        {
            GameManager.Task.CalendarChallengeManager.HasShownCalendarSwitchGuide = false;
        }

        if (GUILayout.Button("清空当前活动", GUILayout.Height(50)))
        {
            ActivityModel.Instance.CurActivityID = 0;
            ActivityModel.Instance.CurPeriodID = 0;
        }

        if (GUILayout.Button("增加遗迹寻宝稿子", GUILayout.Height(50)))
        {
            HiddenTemple.HiddenTempleManager.PlayerData.AddPickaxeNum(10);
        }

        if (GUILayout.Button("增加合成能量", GUILayout.Height(50)))
        {
            Merge.MergeManager.PlayerData.AddMergeEnergyBoxNum(10);
        }

        if (GUILayout.Button("增加合成道具", GUILayout.Height(50)))
        {
            Merge.MergeMainMenuBase mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as Merge.MergeMainMenuBase;
            if (mainBoard != null)
            {
                mainBoard.StoreProp(10104);
            }
        }
    }

}
