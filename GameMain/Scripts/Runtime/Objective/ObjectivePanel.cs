using GameFramework.Event;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ObjectivePanel : CenterForm
{
    public DelayButton m_CloseButton;
    public LoopScrollView m_DailyScrollView;
    public LoopScrollView m_AllTimeScrollViews;
    //public CoinBarManager m_CoinBar;
    //public HeadPortrait m_HeadPortrait;
    public Toggle[] m_AllToggles;
    public Scroller[] m_Scrollbars;
    public VerticalLayoutGroup[] m_LayoutGroups;
    public GameObject[] m_WarningIcons;

    private int m_CurToggleIndex = 0;
    //private int m_HideCoinBarEventId = 0;
    //private int m_HideHeadPortraitEventId = 0;
    private Dictionary<string, Sprite> m_SpriteDic = new Dictionary<string, Sprite>();
    //private int m_ShowCoinBarCount = 0;
    //private int m_ShowHeadPortraitCount = 0;

    [NonSerialized]
    public List<int> m_AllTimeIdList = new List<int>();
    [NonSerialized]
    public List<int> m_DailyIdList = new List<int>();
    [NonSerialized]
    public int m_AllTimeAnimCompleteNum = 0;
    [NonSerialized]
    public int m_DailyAnimCompleteNum = 0;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonHandle);

        SetCurBtnIndex();
        BtnEvent();
        SetToggleBtns();

        base.OnInit(uiGroup, completeAction, userData);

        RefreshWarningIcon();

        //进入界面，播放音效
        GameManager.Sound.PlayAudio("SFX_League_List_Insert");
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonHandle);

        m_CurToggleIndex = 0;
        GameManager.DataNode.SetData("CurToggleIndex", 0);

        base.OnRelease();
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        m_AllTimeIdList.Clear();
        m_DailyIdList.Clear();
        m_AllTimeAnimCompleteNum = 0;
        m_DailyAnimCompleteNum = 0;
        //m_ShowCoinBarCount = 0;
        //m_ShowHeadPortraitCount = 0;
        //m_CoinBar.gameObject.SetActive(false);
        //m_HeadPortrait.gameObject.SetActive(false);

        GameManager.Process.EndProcess(ProcessType.ShowObjectiveGuide);

        base.OnHide(hideSuccessAction, userData);
    }

    private void OnDestroy()
    {
        try
        {
            foreach (KeyValuePair<string, Sprite> pair in m_SpriteDic)
            {
                try
                {
                    Addressables.Release(pair.Value);
                }
                catch (Exception e)
                {
                    Log.Debug(e);
                }
            }
            m_SpriteDic.Clear();
        }
        catch
        {
            m_SpriteDic.Clear();
        }
    }

    public Sprite GetTargetSprite(string spriteName, string atlasName)
    {
        if (m_SpriteDic.TryGetValue(spriteName, out Sprite sp))
        {
            return sp;
        }
        else
        {
            string atlasedSpriteAddress = $"{atlasName}[{spriteName}]";
            Sprite sprite = AddressableUtils.LoadAsset<Sprite>(atlasedSpriteAddress);
            m_SpriteDic.Add(spriteName, sprite);
            return sprite;
        }
    }

    private void BtnEvent()
    {
        m_CloseButton.SetBtnEvent(() =>
        {
            GameManager.UI.HideUIForm(this);
        });
    }

    private void SetToggleBtns()
    {
        SetLoopScrollViews();
        for (int i = 0; i < m_AllToggles.Length; i++)
        {
            int curIndex = i;
            bool isActive = m_CurToggleIndex == i;

            m_AllToggles[i].isOn = isActive;
            m_AllToggles[i].onValueChanged.RemoveAllListeners();
            m_AllToggles[i].onValueChanged.AddListener((b) =>
            {
                GameManager.Sound.PlayAudio("SFX_League_List_Insert");
                if (b)
                {
                    GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
                    m_CurToggleIndex = curIndex;
                    GameManager.DataNode.SetData("CurToggleIndex", curIndex);
                    if (m_CurToggleIndex == 0)
                    {
                        m_DailyIdList.Clear();
                        m_DailyAnimCompleteNum = 0;
                        GameManager.Objective.RefreshDailyObjective();
                    }
                    else
                    {
                        m_AllTimeIdList.Clear();
                        m_AllTimeAnimCompleteNum = 0;
                        GameManager.Objective.RefreshAllTimeObjective();
                    }
                    SetLoopScrollViews();
                }
            });
        }
    }

    private void SetLoopScrollViews()
    {
        switch (m_CurToggleIndex)
        {
            //Daily
            case 0:
                m_AllTimeScrollViews.gameObject.SetActive(false);
                m_DailyScrollView.gameObject.SetActive(true);
                m_DailyScrollView.OnCellClicked(null);
                List<int> dailyIds = new List<int>(GameManager.Objective.CurDailyObjectiveIds.Count + 1) { -1 };
                GameManager.Objective.CurDailyObjectiveIds.Sort(Compare);
                dailyIds.AddRange(GameManager.Objective.CurDailyObjectiveIds);
                //dailyIds.Sort(Compare);
                m_DailyScrollView.UpdateData(dailyIds);
                m_DailyScrollView.JumpTo(0);
                m_Scrollbars[0].ScrollSensitivity = 15;
                break;
            //All Time
            case 1:
                m_DailyScrollView.gameObject.SetActive(false);
                m_AllTimeScrollViews.gameObject.SetActive(true);
                m_AllTimeScrollViews.OnCellClicked(null);
                GameManager.Objective.CurAllTimeObjectiveIds.Sort(Compare);
                m_AllTimeScrollViews.UpdateData(GameManager.Objective.CurAllTimeObjectiveIds);
                m_AllTimeScrollViews.JumpTo(0);
                m_Scrollbars[1].ScrollSensitivity = 15;
                break;
        }
    }

    public void RefreshWarningIcon()
    {
        m_WarningIcons[0].SetActive(false);
        for (int i = 0; i < GameManager.Objective.CurDailyObjectiveIds.Count; i++)
        {
            if (GameManager.Objective.CheckObjectiveCompleted(GameManager.Objective.CurDailyObjectiveIds[i], false))
            {
                m_WarningIcons[0].SetActive(true);
                break;
            }
        }

        m_WarningIcons[1].SetActive(false);
        for (int i = 0; i < GameManager.Objective.CurAllTimeObjectiveIds.Count; i++)
        {
            if (GameManager.Objective.CheckObjectiveCompleted(GameManager.Objective.CurAllTimeObjectiveIds[i], true))
            {
                m_WarningIcons[1].SetActive(true);
                break;
            }
        }
    }

    //public void ShowCoinBar()
    //{
    //    if (m_HideCoinBarEventId != 0)
    //    {
    //        GameManager.Task.RemoveDelayTriggerTask(m_HideCoinBarEventId);
    //        m_HideCoinBarEventId = 0;
    //    }

    //    if (m_HideHeadPortraitEventId != 0)
    //    {
    //        GameManager.Task.RemoveDelayTriggerTask(m_HideHeadPortraitEventId);
    //        m_HideHeadPortraitEventId = 0;
    //        m_HeadPortrait.Release();
    //        m_HeadPortrait.gameObject.SetActive(false);
    //    }

    //    if (!m_CoinBar.gameObject.activeSelf)
    //    {
    //        m_CoinBar.OnInit(null, null, null);
    //        m_CoinBar.gameObject.SetActive(true);
    //    }

    //    m_ShowCoinBarCount++;
    //}

    //public void HideCoinBar(float delayTime = 0)
    //{
    //    m_ShowCoinBarCount--;

    //    if (m_ShowCoinBarCount <= 0)
    //    {
    //        m_ShowCoinBarCount = 0;
    //        m_HideCoinBarEventId = GameManager.Task.AddDelayTriggerTask(delayTime, () =>
    //        {
    //            m_HideCoinBarEventId = 0;
    //            m_CoinBar.OnReset();
    //            m_CoinBar.gameObject.SetActive(false);
    //        });
    //    }
    //}

    //public void ShowHeadPortrait()
    //{
    //    if (m_HideHeadPortraitEventId != 0)
    //    {
    //        GameManager.Task.RemoveDelayTriggerTask(m_HideHeadPortraitEventId);
    //        m_HideHeadPortraitEventId = 0;
    //    }

    //    if (m_HideCoinBarEventId != 0)
    //    {
    //        GameManager.Task.RemoveDelayTriggerTask(m_HideCoinBarEventId);
    //        m_HideCoinBarEventId = 0;
    //        m_CoinBar.OnReset();
    //        m_CoinBar.gameObject.SetActive(false);
    //    }

    //    if (!m_HeadPortrait.gameObject.activeSelf)
    //        m_HeadPortrait.Initialize();

    //    m_ShowHeadPortraitCount++;
    //}

    //public void HideHeadPortrait(float delayTime = 0)
    //{
    //    m_ShowHeadPortraitCount--;

    //    if (m_ShowHeadPortraitCount <= 0)
    //    {
    //        m_ShowHeadPortraitCount = 0;
    //        m_HideHeadPortraitEventId = GameManager.Task.AddDelayTriggerTask(delayTime, () =>
    //        {
    //            m_HideHeadPortraitEventId = 0;
    //            m_HeadPortrait.Release();
    //        });
    //    }
    //}

    public void CommonHandle(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        switch (ne.Type)
        {
            case CommonEventType.Objective:
                RefreshWarningIcon();
                break;
        }
    }

    int Compare(int a_Id, int b_Id)
    {
        var m_CurToggleIndex = GameManager.DataNode.GetData<int>("CurToggleIndex", 0);

        var a_data = GameManager.Objective.GetObjectiveStatus(a_Id, m_CurToggleIndex == 1);
        var b_data = GameManager.Objective.GetObjectiveStatus(b_Id, m_CurToggleIndex == 1);

        if (a_data.Item2 != b_data.Item2)
        {
            return b_data.Item2 ? 1 : -1;
        }
        else if (a_data.Item2 && b_data.Item2)
        {
            if (b_Id == -1) return 1;
            if (a_Id == -1) return -1;
            return a_Id.CompareTo(b_Id);
        }
        else
        {
            if (b_data.Item1 > a_data.Item1)
            {
                return 1;
            }
            else if (b_data.Item1 == a_data.Item1)
            {
                return a_Id.CompareTo(b_Id);
            }
            else
            {
                return -1;
            }
        }
    }

    private void SetCurBtnIndex()
    {
        m_CurToggleIndex = 0;
        //如果daliy没有可完成的任务  但是alltime有 那么标签就切到alltime
        bool isHaveOverTaskByDaliy = IsHaveOverTaskByDaliy();

        if (!isHaveOverTaskByDaliy)
        {
            bool isHaveOverTaskByAllTime = IsHaveOverTaskByAllTime();
            if (isHaveOverTaskByAllTime || (GameManager.Objective.CurDailyObjectiveIds.Count == 0 && GameManager.Objective.CurAllTimeObjectiveIds.Count > 0))
            {
                m_CurToggleIndex = 1;
                GameManager.DataNode.SetData("CurToggleIndex", 1);
            }
        }
    }

    private bool IsHaveOverTaskByDaliy()
    {
        List<int> dailyIds = new List<int>(GameManager.Objective.CurDailyObjectiveIds.Count + 1) { -1 };
        GameManager.Objective.CurDailyObjectiveIds.Sort(Compare);
        dailyIds.AddRange(GameManager.Objective.CurDailyObjectiveIds);
        foreach (var id in dailyIds)
        {
            if (GameManager.Objective.CheckObjectiveCompleted(id, false))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsHaveOverTaskByAllTime()
    {
        var idlist = GameManager.Objective.CurAllTimeObjectiveIds;
        foreach (var id in idlist)
        {
            if (GameManager.Objective.CheckObjectiveCompleted(id, true))
            {
                return true;
            }
        }

        return false;
    }
}
