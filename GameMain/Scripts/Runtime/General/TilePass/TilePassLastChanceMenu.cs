using MySelf.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TilePassLastChanceMenu : PopupMenuForm
{
    public Transform blackBG;
    public GameObject lastChanceRoot;
    public GameObject ensureRoot;

    public DelayButton closeButton;
    public DelayButton buyButton;
    public TextMeshProUGUILocalize priceText;
    public RectTransform content;

    public DelayButton quitButton;
    public DelayButton continueButton;

    private List<ItemSlot> m_ItemSlots = new List<ItemSlot>();
    private List<AsyncOperationHandle<GameObject>> m_AsyncOperationHandles = new List<AsyncOperationHandle<GameObject>>();
    private Dictionary<TotalItemData, int> m_UnclaimedRewards;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        buyButton.OnInit(OnBuyButtonClicked);
        closeButton.OnInit(OnCloseButtonClicked);

        priceText.SetTerm(GameManager.Purchase.GetPrice(ProductNameType.VIP_Pass));

        ShowUnclaimedRewards();

        quitButton.OnInit(OnClose);
        continueButton.OnInit(OnContinueButtonClicked);
    }

    public override void OnReset()
    {
        base.OnReset();

        buyButton.OnReset();
        closeButton.OnReset();

        for (int i = 0; i < m_ItemSlots.Count; i++)
        {
            m_ItemSlots[i].OnRelease();
        }
        m_ItemSlots.Clear();

        for (int i = 0; i < m_AsyncOperationHandles.Count; i++)
        {
            UnityUtility.UnloadInstance(m_AsyncOperationHandles[i]);
        }
        m_AsyncOperationHandles.Clear();

        m_UnclaimedRewards = null;

        quitButton.OnReset();
        continueButton.OnReset();
    }

    public override void OnClose()
    {
        base.OnClose();

        GameManager.UI.HideUIForm(this);

        //如果时间结束，接通行证结束页面
        if (DateTime.Now > TilePassModel.Instance.EndTime && TilePassModel.Instance.EndTime != DateTime.MinValue)
        {
            GameManager.UI.ShowUIForm("TilePassEndMenu",showSuccessAction =>
            {

            }, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
        }
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        blackBG.SetAsFirstSibling();
        lastChanceRoot.transform.SetAsLastSibling();
        ensureRoot.SetActive(false);

        m_ItemSlots.Sort((x, y) =>
        {
            int xTypeIndex = x.ItemType.TotalItemTypeInt;
            int yTypeIndex = y.ItemType.TotalItemTypeInt;

            if (xTypeIndex < yTypeIndex)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        });

        for (int i = 0; i < m_ItemSlots.Count; i++)
        {
            int xPos = (1 - (i % 3)) * -272;
            int yPos = i / 3 * (-262) - 131;
            m_ItemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(xPos, yPos);
        }

        base.OnShow(showSuccessAction, userData);
    }

    public override bool CheckInitComplete()
    {
        if (m_ItemSlots.Count >= m_UnclaimedRewards.Count)
        {
            return true;
        }

        return false;
    }

    private void OnCloseButtonClicked()
    {
        lastChanceRoot.transform.SetAsFirstSibling();
        ensureRoot.transform.SetAsLastSibling();
        ensureRoot.SetActive(true);
    }

    private void OnContinueButtonClicked()
    {
        blackBG.SetAsFirstSibling();
        lastChanceRoot.transform.SetAsLastSibling();
        ensureRoot.SetActive(false);
    }

    private void OnBuyButtonClicked()
    {
        GameManager.Purchase.BuyProduct(ProductNameType.VIP_Pass, () =>
        {
            TilePassUtil.RecordLastChancePurchase(); //打点

            foreach (var reward in m_UnclaimedRewards)
            {
                TotalItemData type = reward.Key;
                int num = reward.Value;
                //登记奖励
                RewardManager.Instance.AddNeedGetReward(type, num);
            }
            for (int index = 0; index < GameManager.DataTable.GetDataTable<DTTilePassData>().Data.CurrentTilePassDatas.Count; index++)
            {
                TilePassModel.Instance.AddRewardGetStatus("TilePassVIPRewardGet" + index.ToString());
            }

            GameManager.UI.HideUIForm(this);

            TilePassModel.Instance.IsVIP = true;
            //发放奖励
            RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.DefaultRewardPanel, false, () =>
            {
                //如果时间结束，接通行证结束页面
                if (DateTime.Now > TilePassModel.Instance.EndTime && TilePassModel.Instance.EndTime != DateTime.MinValue)
                {
                    GameManager.UI.ShowUIForm("TilePassEndMenu",showSuccessAction =>
                    {

                    }, () =>
                    {
                        GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
                    });
                }
                else
                {
                    GameManager.Process.EndProcess(ProcessType.ShowTilePassEndProcess);
                }
            });
        });
    }

    private void ShowUnclaimedRewards()
    {
        CheckAllVIPRewards();

        if (m_UnclaimedRewards.Count > 6)
        {
            int ratio = (m_UnclaimedRewards.Count - 7) / 3;
            content.sizeDelta = new Vector2(content.sizeDelta.x, 700 + ratio * 250);
        }
        else
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, 300);
        }

        foreach (var reward in m_UnclaimedRewards)
        {
            AsyncOperationHandle<GameObject> asyncHandle = UnityUtility.InstantiateAsync("TilePassRewardSlot", content, obj =>
            {
                ItemSlot slot = obj.GetComponent<ItemSlot>();
                slot.OnInit(reward.Key, reward.Value);

                m_ItemSlots.Add(slot);
            });
            m_AsyncOperationHandles.Add(asyncHandle);
        }
    }

    private void CheckAllVIPRewards()
    {
        //添加购买VIP的奖励
        m_UnclaimedRewards = new Dictionary<TotalItemData, int> { [TotalItemData.Coin] = 1000 };

        List<TilePassData> tilePassDatas = GameManager.DataTable.GetDataTable<DTTilePassData>().Data.CurrentTilePassDatas;

        //添加所有VIP奖励
        for (int i = 0; i < tilePassDatas.Count; i++)
        {
            List<TotalItemData> vipRewardList = tilePassDatas[i].VIPRewardList;
            List<int> vipRewardNumList = tilePassDatas[i].VIPRewardNumList;

            for (int j = 0; j < vipRewardList.Count; j++)
            {
                if (m_UnclaimedRewards.TryGetValue(vipRewardList[j], out int num))
                {
                    m_UnclaimedRewards[vipRewardList[j]] = vipRewardNumList[j] + num;
                }
                else
                {
                    m_UnclaimedRewards.Add(vipRewardList[j], vipRewardNumList[j]);
                }
            }
        }
    }
}
