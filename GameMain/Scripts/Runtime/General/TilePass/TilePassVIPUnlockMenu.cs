using DG.Tweening;
using MySelf.Model;
using System;
using UnityEngine;

public class TilePassVIPUnlockMenu : PopupMenuForm
{
    public GameObject BG;
    public GameObject effect;
    public GameObject vipImage;
    public GameObject vipText;
    public GameObject superVIPImage;
    public GameObject superVIPText;
    public GameObject[] guides;

    private bool m_IsAnimFinish;

    public override void OnInit(UIGroup uiGroup, Action initCompleteAction = null, object userData = null)
    {
        base.OnInit(uiGroup, initCompleteAction, userData);

        if (!TilePassModel.Instance.IsSuperVIP)
        {
            vipText.SetActive(true);
            vipImage.SetActive(true);
            superVIPImage.SetActive(false);
            superVIPText.SetActive(false);
        }
        else
        {
            vipText.SetActive(false);
            vipImage.SetActive(false);
            superVIPImage.SetActive(true);
            superVIPText.SetActive(true);
        }
    }

    public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
    {
        for (int i = 0; i < guides.Length; i++)
        {
            guides[i].transform.localScale = Vector3.zero;
        }
        base.OnShowInit(showInitSuccessAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(true);

        float delayTime = -0.2f;
        for (int i = 0; i < guides.Length; i++)
        {
            var index = i;
            const float fadeTime = 0.25f;
            const float showTime = 0.25f;
            delayTime += 0.2f;
            guides[i].transform.DOScale(1.1f, showTime).SetDelay(delayTime).onComplete = () =>
            {
                if (index == guides.Length - 1)
                {
                    guides[index].transform.DOScale(1f, fadeTime).onComplete = () =>
                    {
                        m_IsAnimFinish = true;
                    };
                }
                else
                {
                    guides[index].transform.DOScale(1f, fadeTime);
                }
            };
        }

        GameManager.Sound.PlayAudio("SFX_Champion_Level_Up_TilePass");
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        OnReset();
        // base.OnHide(hideSuccessAction, userData);
    }

    public override void OnReset()
    {
        m_IsAnimFinish = false;

        effect.SetActive(true);
        BG.SetActive(true);

        guides[0].transform.localScale = Vector3.one;
        guides[0].transform.localPosition = new Vector3(0, 233);
        for (int i = 0; i < guides.Length; i++)
        {
            guides[i].SetActive(true);
        }

        base.OnReset();
    }

    public override void OnClose()
    {
        if (!m_IsAnimFinish)
        {
            return;
        }
        effect.SetActive(false);
        BG.SetActive(false);
        for (int i = 1; i < guides.Length; i++)
        {
            guides[i].SetActive(false);
        }
        guides[0].transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f).onComplete = () =>
        {
            guides[0].transform.DOScale(new Vector3(0.67f, 0.67f, 0.67f), 0.2f);
        };
        TilePassMainMenu mainMenu = GameManager.UI.GetUIForm("TilePassMainMenu") as TilePassMainMenu;
        Vector3 pos = mainMenu.VIP.transform.position;
        guides[0].transform.DOJump(pos, 0.3f, 1, 0.4f).onComplete = () =>
        {
            GameManager.UI.HideUIForm(this);

            mainMenu.RefreshVIPState();
            mainMenu.RefreshClaimAll();

            //解锁
            //if (mainMenu.scrollArea.GetColumn("TilePassColumn").Instance.GetComponent<TilePassColumn>().lockSpine.gameObject.activeSelf)
            //{
            GameManager.Sound.PlayAudio("SFX_tilepass_presentlocked");
            //}
            foreach (ScrollColumn scrollColumn in mainMenu.scrollArea.scrollColumnList)
            {
                scrollColumn.Unlock();
            }

            //飞油桶
            if (TilePassModel.Instance.IsSuperVIP)
            {
                RewardManager.Instance.AddNeedGetReward(TotalItemData.Gasoline, 7);
                RewardManager.Instance.AutoGetRewardDelayTime = 0;
                RewardManager.Instance.ShowNeedGetRewards(RewardPanelType.TransparentRewardPanel, true, () =>
                {
                    RewardManager.Instance.AutoGetRewardDelayTime = 0.2f;
                });
            }
            //不飞油桶
            else
            {
                GameManager.Task.AddDelayTriggerTask(1f, () =>
                {
                    foreach (ScrollColumn scrollColumn in mainMenu.scrollArea.scrollColumnList)
                    {
                        scrollColumn.RefreshRewardStatus();
                    }
                    mainMenu.RefreshClaimAll();
                });
            }
        };

        base.OnClose();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0))
#else
            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
        {
            OnClose();
        }
    }
}
