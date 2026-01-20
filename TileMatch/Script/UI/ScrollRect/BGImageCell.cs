using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Firebase.Analytics;
using Spine.Unity;

public class BGImageCell:MonoBehaviour
{
    [SerializeField]
    private Image BgImage;
    [SerializeField]
    private GameObject LockObj,NeedCoin;
    [SerializeField]
    private TextMeshProUGUI NeedCoin_Num;
    [SerializeField]
    private ExtensionsToggle Toggle;
    [SerializeField]
    private GameObject Norm_F;
    [SerializeField]
    private SkeletonGraphic Ok_Anim;

    [SerializeField] private GameObject RedPoint_Obj;

    [SerializeField]
    int recordID;
    [SerializeField]
    BGItemData data;

    bool isInit = false;

    private Action refreshAction;
    
    
    public void Init(int bgID,Action refreshAction)
    {
        RedPoint_Obj.gameObject.SetActive(GameManager.PlayerData.IsShowBGRedPointById(bgID));
        this.refreshAction = refreshAction;

        transform.localScale = Vector3.one;

        Toggle.onPointerDown = OnPointerDown;
        Toggle.onPointerUp = OnPointerUp;
        Toggle.onValueCanChanged = IsCanToggle;
        Toggle.SetBtnEvent((isActive) =>
        {
            GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
            ToggleHandle(isActive);
            GameManager.PlayerData.RemoveShowBGRedPoint(data.ID);
            RedPoint_Obj.gameObject.SetActive(false);
        });

        UpdateContent(bgID);
    }

    private void OnPointerDown()
    {
        bool isUnLock = IsUnlock();
        if (!isUnLock)
        {
            LockObj.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
            return;
        }

        transform.DOKill();
        transform.DOScale(Vector3.one * 1.04f, 0.1f);
    }

    private void OnPointerUp()
    {
        transform.DOKill();
        transform.DOScale(Vector3.one, 0.1f);
    }

    private bool IsCanToggle()
    {
        bool isCurBG = IsCurBG();

        if (isCurBG)
        {
            Toggle.IsOn = true;
            return true;
        }

        bool isUnLock = IsUnlock();
        bool isOwn = IsOwn();
        bool isOwnBgID = IsOwnBgID();

        if (!isUnLock && !isOwnBgID)
        {
            if (!string.IsNullOrEmpty(data.UnlockTips))
                GameManager.UI.ShowWeakHint(data.UnlockTips, startPos: new Vector3(0, 0.15f, 0));
            else
                GameManager.UI.ShowWeakHint("Theme.Unlock at Level {0}", startPos: new Vector3(0, 0.15f, 0), args: data.BGUnlockLevel.ToString());
        }
        else if (!isOwn)
        {
            System.Action buySuccessAction = () =>
            {
                ForceRefresh();
                ToggleHandle(true);
                refreshAction?.Invoke();
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Theme_Background_Purchase,
                    new Parameter("BgId",data.ID));
            };
            GameManager.UI.ShowUIForm("BuyBGPanel",userData: (data.ID, data.BGPrice, buySuccessAction));
        }
        else if (!isCurBG)
        {
            Norm_F.gameObject.SetActive(false);

            Ok_Anim.AnimationState.ClearTracks();
            Ok_Anim.Skeleton.SetToSetupPose();
            Ok_Anim.AnimationState.SetAnimation(0, "animation", false);
            return true;
        }

        Norm_F.gameObject.SetActive(true);
        return false;
    }

    private void ToggleHandle(bool isActive)
    {
        if (isActive)
        {
            GameManager.PlayerData.BGImageIndex = data.ID;
            Toggle.IsOn = true;
        }
        Ok_Anim.gameObject.SetActive(isActive);
        Norm_F.gameObject.SetActive(!isActive);
    }

    static Color unlockColor = Color.white;
    static Color lockColor = new Color(1,1,1,0);
    public void UpdateContent(int bgID)
    {
        //if (this.recordID == bgID) return;
        this.recordID = bgID;

        ForceRefresh();
    }

    private void ForceRefresh()
    {
        this.data = GameManager.DataTable.GetDataTable<DTBGID>().Data.BGItemDataDict[recordID];

        gameObject.name = this.recordID.ToString();

        bool isUnLock = IsUnlock();
        bool isOwn = IsOwn();
        bool isCurBG = IsCurBG();
        bool isOwnBgID = IsOwnBgID();

        Toggle.IsOn = isCurBG;

        LockObj.gameObject.SetActive(!isUnLock && !isOwnBgID);
        NeedCoin.gameObject.SetActive(isUnLock && !isOwn);
        NeedCoin_Num.text = data.BGPrice.ToString();

        BgImage.sprite = BGSmallUtil.GetSprite(this.recordID);
        BgImage.color = unlockColor;
        if (!isInit)
        {
            isInit = true;
            BgImage.color = lockColor;
            BgImage.DOBlendableColor(unlockColor, 0.2f);
        }
    }

    private bool IsCurBG()
    {
        return GameManager.PlayerData.BGImageIndex == data.ID;
    }

    private bool IsUnlock()
    {
        bool playerLevelIsEnough = GameManager.PlayerData.NowLevel >= data.BGUnlockLevel;
        bool timeIsLateEnough = DateTime.Now >= data.StartSellTimeDT;
        return playerLevelIsEnough && timeIsLateEnough;
    }

    private bool IsOwn()
    {
        return GameManager.PlayerData.IsOwnBGID(data.ID) ||!GameManager.DataTable.GetDataTable<DTBGID>().Data.IsNeedBuyBG(data.ID);
    }

    private bool IsOwnBgID()
    {
        return GameManager.PlayerData.IsOwnBGID(data.ID);
    }

    private void OnDestroy()
    {
        BgImage.sprite = null;
    }
}

