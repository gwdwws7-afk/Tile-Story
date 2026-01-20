using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class DaySlot : MonoBehaviour
{
    public int m_Day;
    public GameObject m_NormalBg, m_TodayBg;
    public Image[] m_RewardImage;
    public TextMeshProUGUI[] m_NumText;
    public GameObject m_ClaimedIcon;
    public GameObject m_Lock;
    public Button m_ClickButton, m_TodayClickButton;

    private LoginGiftData m_Data;
    private int m_LoginDay;
    private List<AsyncOperationHandle> handleList = new List<AsyncOperationHandle>();

    public void Initialize(LoginGiftData data, int loginDay, bool isGet)
    {
        m_Data = data;
        m_LoginDay = loginDay;

        bool isDouble = PlayerPrefs.GetInt("LoginGiftIsGetDouble_" + m_Day, 0) == 1;

        List<ItemData> datas = m_Data.GetRewardDatas();
        for (int i = 0; i < datas.Count; i++)
        {
            int index = i;
            AsyncOperationHandle asyncHandle = UnityUtility.LoadSpriteAsync(UnityUtility.GetRewardSpriteKey(datas[index].type, datas[index].num), "TotalItemAtlas", sp =>
            {
                m_RewardImage[index].sprite = sp;
            });
            handleList.Add(asyncHandle);

            if (!isDouble)
                m_NumText[index].SetItemText(datas[index].num, datas[index].type, true);
            else
                m_NumText[index].SetItemText(datas[index].num * 2, datas[index].type, true);
        }

        bool isClaimed = loginDay > m_Day || (loginDay == m_Day && isGet);
        m_ClaimedIcon.SetActive(isClaimed);
        m_Lock.SetActive(loginDay < m_Day);

        if (m_TodayBg != null)
        {
            m_NormalBg.SetActive(m_Day != loginDay && !isClaimed);
            m_TodayBg.SetActive(m_Day == loginDay || isClaimed);
        }

        m_ClickButton.SetBtnEvent(OnButtonClick);
        if (m_TodayClickButton != null) 
            m_TodayClickButton.SetBtnEvent(OnButtonClick);
    }

    public void Release()
    {
        for (int i = 0; i < handleList.Count; i++)
        {
            UnityUtility.UnloadAssetAsync(handleList[i]);
        }
        handleList.Clear();
    }

    private void OnButtonClick()
    {
        if (m_LoginDay < m_Day)
        {
            GameManager.UI.ShowWeakHint("DailyBonus.Login for 1 more day to get this reward!", Vector3.zero, (m_Day - m_LoginDay).ToString());
            m_Lock.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
        }
        else if (m_LoginDay == m_Day) 
        {
            LoginGiftPanel loginGiftPanel = GameManager.UI.GetUIForm("LoginGiftPanel") as LoginGiftPanel;
            loginGiftPanel.OnClaimButtonClick();
        }
    }
}
