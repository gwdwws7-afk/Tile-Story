using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SkillItem : MonoBehaviour
{
    [SerializeField]
    private Sprite[] SkillIconSprites;
    [SerializeField]
    private Image HardMask_Image,SkillIcon_Image;
    [SerializeField]
    private GameObject LockObj,ItemObj, AddSkillItemObj,CurItemNumObj;
    [SerializeField]
    private TextMeshProUGUI ItemNum_Text,BuyNeedCoinNum_Text;
    [SerializeField]
    private DelayButton DelayButton;
    [SerializeField]
    private Graphic Finger;
    [SerializeField]
    private GameObject[] HideObjs;
    [SerializeField]
    private GameObject Black_Mask;
    [SerializeField]
    private GameObject RedPoint_Mask;
    [SerializeField] private SkeletonGraphic LockAnim;
    [SerializeField] private CanvasGroup LockCanvasGroup, ImageCanvasGroup;
    [SerializeField] private GameObject Free_Forward, Free_Back;

    [SerializeField] private Image SkillBGImage;

    public Button GuideButton
    {
        get
        {
            return DelayButton;
        }
    }

    private TotalItemData m_ItemType;
    private bool m_IsUnLock;
    private int m_BuyNeedCoinNum = 0;
    private bool m_IsCanUse = false;
    private bool m_IsCanClick = false;
    private AsyncOperationHandle m_RecordAssetHandle;
    private Action unlockAction = null;

    public void Init(int curNum, TotalItemData type, bool isUnLock, TileLevelHardType hardType, int skillIndex, int buyNeedCoinNum, CallBack callBack)
    {
        m_IsUnLock = isUnLock;
        m_BuyNeedCoinNum = buyNeedCoinNum;
        m_ItemType = type;
        
        bool isHaveItem = curNum > 0;
        //设置技能icon
        SkillIcon_Image.sprite= SkillIconSprites[Mathf.Min(SkillIconSprites.Length - 1, skillIndex)];

        //设置解锁与否
        LockObj.SetActive(!isUnLock);
        ItemObj.SetActive(isUnLock);
        if (!isUnLock)
        {
            ShowSkillLockAnim("idle_lock");
        }

        //设置技能道具状态
        AddSkillItemObj.SetActive(!isHaveItem);
        CurItemNumObj.SetActive(isHaveItem);
        BuyNeedCoinNum_Text.text = $"{buyNeedCoinNum}";
        ItemNum_Text.text = $"{curNum}";

        if (Finger != null) Finger.gameObject.SetActive(false);

        SetSkillBGImage();
        DelayButton.SetBtnEvent(() =>
        {
            callBack.Invoke();
            unlockAction?.Invoke();
        });
    }

    public void SetSkillBGImage()
    {
        try
        {
            var imageName = DTLevelUtil.GetSkillBGImageName(GameManager.PlayerData.RealLevel());
            if(SkillBGImage.mainTexture.name==imageName)return;

            UnityUtility.UnloadAssetAsync(m_RecordAssetHandle);
            string atlasedSpriteAddress = $"UICommon[{imageName}]";
            m_RecordAssetHandle = UnityUtility.LoadAssetAsync<Sprite>(atlasedSpriteAddress, s =>
            {
                SkillBGImage.sprite = s;
            });
        }
        catch (Exception e)
        {
           Log.Debug($"SetSkillBGImage:{e.Message}");
        }
    }

    public void SetState(bool isCanUse)
    {
        this.m_IsCanUse = isCanUse;
        this.m_IsCanClick = !m_IsUnLock || (m_IsUnLock && isCanUse);
        
        DelayButton.interactable =!m_IsUnLock ||(m_IsUnLock&&isCanUse);

        if(Black_Mask) Black_Mask.gameObject.SetActive(m_IsUnLock&&!this.m_IsCanUse);
        if (RedPoint_Mask) RedPoint_Mask.gameObject.SetActive(m_IsUnLock&&!this.m_IsCanUse);

        foreach (var obj in HideObjs)
        {
            obj.SetActive(m_IsCanClick);
        }
    }
    
    public bool IsCanShowFingerAnim=> m_IsUnLock && DelayButton.interactable;

    Sequence Sequence = null;
    public void PlayFingerAnim(bool isPlay=true)
    {
        if (Finger == null) return;
        if (!IsCanShowFingerAnim) return;
        if (isPlay)
        {
            if (Sequence!=null&&Sequence.IsPlaying() && Finger.gameObject.activeInHierarchy) return;

            Finger.gameObject.SetActive(true);
            Finger.color = new Color(1,1,1,0f);
            Sequence = DOTween.Sequence()
                .Append(Finger.DOFade(1, 0.5f))
                .AppendInterval(3f)
                .Append(Finger.DOFade(0, 0.5f))
                .AppendInterval(2f)
                .SetLoops(-1, LoopType.Restart).OnKill(()=>Sequence=null);
        } else
        {
            Sequence.Kill(true);
            Finger.gameObject.SetActive(false);
        }
    }

    public TotalItemData GetTotalItemType()
    {
        return m_ItemType;
    }

    public bool IsCanUse()
    {
        return m_IsUnLock && DelayButton.interactable;
    }

    private void OnDisable()
    {
        Sequence.Kill(true);
        Sequence = null;
    }

    private void OnDestroy()
    {
        UnityUtility.UnloadAssetAsync(m_RecordAssetHandle);
    }

    public void ShowSkillUnlockAnim(Action finishAction=null)
    {
        //展示技能解锁动画
        GameManager.Sound.PlayAudio(SoundType.SFX_Help_Chapter_Unlock.ToString());
        ShowSkillLockAnim("active_lock",() =>
        {
            this.m_IsUnLock = true;

            bool isHaveItem = true;
            AddSkillItemObj.SetActive(!isHaveItem);
            CurItemNumObj.SetActive(isHaveItem);
            
            ItemNum_Text.text = $"{GameManager.PlayerData.GetItemNum(this.m_ItemType)}";
            BuyNeedCoinNum_Text.text = $"{m_BuyNeedCoinNum}";
            
            LockObj.SetActive(false);
            LockCanvasGroup.DOFade(0, 0.2f);
            ImageCanvasGroup.alpha = 0;
            ImageCanvasGroup.DOFade(1, 0.2f);
            ItemObj.SetActive(true);

            SetState(this.m_IsCanUse);
            
            finishAction?.Invoke();
        });
    }

    public void SetUnlockEvent()
    {
        CurItemNumObj.gameObject.SetActive(false);
        Free_Forward.gameObject.SetActive(true);
        Free_Back.gameObject.SetActive(true);
        unlockAction = () =>
        {
            Free_Forward.gameObject.SetActive(false);
            Free_Back.gameObject.SetActive(false);
            CurItemNumObj.gameObject.SetActive(true);
            unlockAction = null;
        };
    }

    public void ShowLockAnim()
    {
        //点击时执行震动动画
        ShowSkillLockAnim("shake_lock",finishAction: () =>
        {
            ShowSkillLockAnim("idle_lock");
        });
    }

    private void ShowSkillLockAnim(string animName,Action finishAction=null)
    {
        LockAnim.gameObject.SetActive(true);
        LockCanvasGroup.alpha = 1;
        LockAnim.Skeleton.SetToSetupPose();
        LockAnim.AnimationState.ClearTracks();
        var track = LockAnim.AnimationState.SetAnimation(0, animName, false);
        if (track != null)
        {
            void CompleteAction(TrackEntry entry)
            {
                finishAction?.Invoke();
            }

            track.Complete -= CompleteAction;
            track.Complete += CompleteAction;
        }
    }
}
