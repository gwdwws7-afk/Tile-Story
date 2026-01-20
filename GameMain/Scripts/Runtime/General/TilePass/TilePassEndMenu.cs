using MySelf.Model;
using System;
using System.Collections.Generic;

public class TilePassEndMenu : PopupMenuForm, ICustomOnEscapeBtnClicked
{
    public DelayButton okButton;

    private Dictionary<TotalItemData, int> m_UnclaimedRewards = new Dictionary<TotalItemData, int>();

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        okButton.OnInit(OnOKButtonClick);

        TilePassUtil.RecordTilePassCompleted(); //打点
        TilePassUtil.RecordTilePassClaim();
    }

    public override void OnReset()
    {
        base.OnReset();

        okButton.OnReset();
    }

    private void OnOKButtonClick()
    {
        GameManager.UI.HideUIForm(this);

        //发放未领取奖励，领完直接弹新一期开始界面
        ShowUnclaimedRewards(ResetData);
    }

    public void OnEscapeBtnClicked()
    {
        OnOKButtonClick();
    }

    private void ShowUnclaimedRewards(Action callback)
    {
        CheckUnclaimedRewards();

        if (m_UnclaimedRewards.Count > 0)
        {
            foreach (var reward in m_UnclaimedRewards)
            {
                RewardManager.Instance.AddNeedGetReward(reward.Key, reward.Value);
            }

            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.TilePassRewardPanel, false, callback);
        }
        else
        {
            callback?.Invoke();
        }
    }

    private void CheckUnclaimedRewards()
    {
        List<TilePassData> datas = GameManager.DataTable.GetDataTable<DTTilePassData>().Data.CurrentTilePassDatas;

        int currentIndex = 0;
        int totalTargetNum = TilePassModel.Instance.TotalTargetNum;
        while (totalTargetNum >= 0 && currentIndex < datas.Count)
        {
            totalTargetNum -= datas[currentIndex].TargetNum;

            if (totalTargetNum >= 0)
            {
                if (!TilePassModel.Instance.CheckRewardGetStatus("TilePassFreeRewardGet" + currentIndex.ToString()))
                {
                    List<TotalItemData> freeRewardList = datas[currentIndex].FreeRewardList;
                    List<int> freeRewardNumList = datas[currentIndex].FreeRewardNumList;

                    for (int i = 0; i < freeRewardList.Count; i++)
                    {
                        if (m_UnclaimedRewards.TryGetValue(freeRewardList[i], out int num))
                        {
                            m_UnclaimedRewards[freeRewardList[i]] = freeRewardNumList[i] + num;
                        }
                        else
                        {
                            m_UnclaimedRewards.Add(freeRewardList[i], freeRewardNumList[i]);
                        }
                    }
                }

                if (TilePassModel.Instance.IsVIP
                    && !TilePassModel.Instance.CheckRewardGetStatus("TilePassVIPRewardGet" + currentIndex.ToString()))
                {
                    List<TotalItemData> vipRewardList = datas[currentIndex].VIPRewardList;
                    List<int> vipRewardNumList = datas[currentIndex].VIPRewardNumList;

                    for (int i = 0; i < vipRewardList.Count; i++)
                    {
                        if (m_UnclaimedRewards.TryGetValue(vipRewardList[i], out int num))
                        {
                            m_UnclaimedRewards[vipRewardList[i]] = vipRewardNumList[i] + num;
                        }
                        else
                        {
                            m_UnclaimedRewards.Add(vipRewardList[i], vipRewardNumList[i]);
                        }
                    }
                }
            }

            currentIndex++;
        }
    }

    private void ResetData()
    {
        // TilePassModel.Instance.ResetData();

        // ShowNextStartMenu();

        TilePassModel.Instance.ShowedEndMenu = true;
        GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
    }

    private void ShowNextStartMenu()
    {
        TilePassModel.Instance.EndTime = GameManager.DataTable.GetDataTable<DTTilePassScheduleData>().Data.GetNowActiveActivityEndTime();
        //活动开始，初始化数据表
        if (TilePassModel.Instance.EndTime != DateTime.MinValue)
        {
            GameManager.DataTable.GetDataTable<DTTilePassData>().Data.GetDataTable();
        }
        //无排期，结束进程
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
            return;
        }

        GameManager.UI.ShowUIForm("TilePassStartMenu",showSuccessAction =>
        {
            TilePassModel.Instance.ShowedStartMenu = true;
            MapTopPanelManager mapTop = GameManager.UI.GetUIForm("MapTopPanelManager") as MapTopPanelManager;
            mapTop.tilePassEntrance.OnInit(null);
        }, () =>
        {
            GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
        });
    }
}
