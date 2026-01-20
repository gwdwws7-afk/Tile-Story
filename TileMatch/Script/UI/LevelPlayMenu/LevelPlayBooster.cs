using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class LevelPlayBooster : MonoBehaviour, IItemFlyReceiver
{
    public GameObject m_NormalBg, m_SelectedBg;
    public GameObject m_Banner;
    public GameObject m_AddIcon, m_NumIcon, m_SelectedIcon;
    public TextMeshProUGUI m_NumText;
    public SkeletonGraphic m_LockAnim;
    public Image m_BoosterImg;
    public CountdownTimer m_InfiniteTimer;
    public Button m_ClickButton;

    private TotalItemType m_BoostType;

    public string BoostUnlockSavedKey => "IsBoostUnlockAnimShowed_" + m_BoostType.ToString();

    public ReceiverType m_ReceiverType;
    public ReceiverType ReceiverType => m_ReceiverType;

    public void Initialize(TotalItemType boostType)
    {
        m_BoostType = boostType;
        RewardManager.Instance.RegisterItemFlyReceiver(this);

        m_ClickButton.SetBtnEvent(OnButtonClick);

        m_InfiniteTimer.OnReset();
        Refresh();
    }

    public void Recycle()
    {
        RewardManager.Instance.UnregisterItemFlyReceiver(this);
        m_InfiniteTimer.OnReset();
    }

    private void Update()
    {
        m_InfiniteTimer.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
    }

    public void Refresh()
    {
        bool isLock = GameManager.PlayerData.NowLevel < GetUnlockLevel() || (GameManager.PlayerData.NowLevel == GetUnlockLevel() && PlayerPrefs.GetInt(BoostUnlockSavedKey, 0) == 0);
        if (isLock)
        {
            m_NormalBg.SetActive(true);
            m_SelectedBg.SetActive(false);
            m_Banner.SetActive(false);
            m_LockAnim.gameObject.SetActive(true);
            m_BoosterImg.gameObject.SetActive(false);
            m_AddIcon.SetActive(false);
            m_NumIcon.SetActive(false);
            m_SelectedIcon.SetActive(false);
        }
        else
        {
            float infiniteTime = GameManager.PlayerData.GetInfiniteBoostTime(m_BoostType);
            if (infiniteTime > 0)
            {
                GameManager.DataNode.SetData<bool>("BoostIsSelected_" + m_BoostType.ToString(), true);
                m_InfiniteTimer.OnReset();
                m_InfiniteTimer.CountDownTextUseDay = false;
                m_InfiniteTimer.CountdownOver += OnCountdownOver;
                m_InfiniteTimer.StartCountdown(DateTime.Now.AddMinutes(infiniteTime));

                if (infiniteTime > 60)
                    m_InfiniteTimer.timeText.fontSize = 32;
                else
                    m_InfiniteTimer.timeText.fontSize = 45;
                GameManager.DataNode.SetData<bool>("InfiniteSetBoostSelected_" + m_BoostType.ToString(), true);
            }
            else if (GameManager.DataNode.GetData<bool>("InfiniteSetBoostSelected_" + m_BoostType.ToString(), false))
            {
                GameManager.DataNode.SetData<bool>("InfiniteSetBoostSelected_" + m_BoostType.ToString(), false);
                GameManager.DataNode.SetData<bool>("BoostIsSelected_" + m_BoostType.ToString(), false);
            }

            bool isSelected = GameManager.DataNode.GetData<bool>("BoostIsSelected_" + m_BoostType.ToString(), false);
            int itemNum = GetItemNum();
            m_NumText.text = itemNum.ToString();
            m_NormalBg.SetActive(!isSelected);
            m_SelectedBg.SetActive(isSelected);
            m_Banner.SetActive(infiniteTime > 0);
            m_LockAnim.gameObject.SetActive(false);
            m_BoosterImg.gameObject.SetActive(true);
            m_AddIcon.SetActive(!isSelected && infiniteTime <= 0 && itemNum <= 0);
            m_NumIcon.SetActive(!isSelected && infiniteTime <= 0 && itemNum > 0);
            m_SelectedIcon.SetActive(isSelected && infiniteTime <= 0);
        }
    }
    private void OnCountdownOver(object sender, CountdownOverEventArgs e)
    {
        GameManager.DataNode.SetData<bool>("BoostIsSelected_" + m_BoostType.ToString(), false);
        Refresh();
    }

    public void OnButtonClick()
    {
        bool isLock = GameManager.PlayerData.NowLevel < GetUnlockLevel();
        if (!isLock)
        {
            bool isSelected = GameManager.DataNode.GetData<bool>("BoostIsSelected_" + m_BoostType.ToString(), false);
            int itemNum = GetItemNum();

            if (!isSelected && itemNum <= 0) 
            {
                GameManager.UI.ShowUIForm("BoostPurchaseMenu",null, null, TotalItemData.FromInt((int)m_BoostType));
                return;
            }

            GameManager.DataNode.SetData<bool>("BoostIsSelected_" + m_BoostType.ToString(), !isSelected);
            Refresh();
        }
        else
        {
            m_LockAnim.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
            GameManager.UI.ShowWeakHint("Theme.Unlock at Level {0}", new Vector3(0, -0.2f), GetUnlockLevel().ToString());
        }
    }

    private int GetUnlockLevel()
    {
        switch (m_BoostType)
        {
            case TotalItemType.MagnifierBoost:
                return Constant.GameConfig.UnlockMagnifierBoostLevel;
            case TotalItemType.Prop_AddOneStep:
                return Constant.GameConfig.UnlockAddOneStepBoostLevel;
            case TotalItemType.FireworkBoost:
                return Constant.GameConfig.UnlockFireworkBoost;
        }

        return 0;
    }

    private int GetItemNum()
    {
        switch (m_BoostType)
        {
            case TotalItemType.MagnifierBoost:
                return GameManager.PlayerData.GetItemNum(TotalItemData.MagnifierBoost);
            case TotalItemType.Prop_AddOneStep:
                return GameManager.PlayerData.GetItemNum(TotalItemData.Prop_AddOneStep);
            case TotalItemType.FireworkBoost:
                return GameManager.PlayerData.GetItemNum(TotalItemData.FireworkBoost);
        }

        return 0;
    }

    public GameObject GetReceiverGameObject()
    {
        return gameObject;
    }

    public void OnFlyHit(TotalItemData type)
    {
        transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
        {
            transform.localScale = Vector3.one;
        };
    }

    public void OnFlyEnd(TotalItemData type)
    {
        OnFlyHit(type);
    }

    public Vector3 GetItemTargetPos(TotalItemData type)
    {
        return transform.position;
    }
}
