using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GlacierQuestTeamMenu : UIForm
{
    public DelayButton okBtn;
    [SerializeField] private TextMeshProUGUI peopleNum;
    public Transform headIconParent;
    public GameObject[] animationGO;
    public GlacierQuestHeadIcon[] headIcons;

    GlacierQuestHeadIcon[] HeadIcons
    {
        get
        {
            if (headIcons == null || headIcons.Length <= 0)
            {
                headIcons = new GlacierQuestHeadIcon[headIconParent.childCount];
                for (int i = 0; i < headIconParent.childCount; ++i)
                    headIcons[headIconParent.childCount - i - 1] = headIconParent.GetChild(i).GetComponent<GlacierQuestHeadIcon>();
            }
            return headIcons;
        }
    }

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        okBtn.SetBtnEvent(OnClickOkBtn);
        okBtn.interactable = false;
        peopleNum.SetText("1/100");
        // 初始化头像并隐藏
        for (int i = 0; i < animationGO.Length; i++)
        {
            animationGO[i].SetActive(false);
        }

        int[] headIds = GameManager.Task.GlacierQuestTaskManager.HeadIds;
        for (int i = 0; i < HeadIcons.Length; i++)
        {
            HeadIcons[i].Init(headIds[i], i == 0);
            HeadIcons[i].gameObject.SetActive(false);
            HeadIcons[i].headIcon.gameObject.SetActive(false);
        }

        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        // 播放界面出现动画
        StartCoroutine("PlayFindAnim");
        // 防止动画出现错误，导致面板无法正常关闭游戏卡死
        GameManager.Task.AddDelayTriggerTask(5f, () => okBtn.interactable = true);
        base.OnShow(showSuccessAction, userData);
    }

    public override void OnRelease()
    {
        foreach (var headIcon in HeadIcons)
        {
            headIcon.Release();
        }
        base.OnRelease();
    }

    public void OnClickOkBtn()
    {
        GameManager.UI.HideUIForm(this);
        GameManager.UI.ShowUIForm("GlacierQuestMenu");
    }

    public float speed = 7 / 6f;
    public float waitTime = 0.1f;
    public float speedMax = 2f;
    public float speedAd = 0.05f;
    public float waitre = 0.004f;
    float num = 1;
    IEnumerator PlayFindAnim()
    {
        //显示界面UI
        int i = 0;
        for (; i < animationGO.Length - 1; i++)
        {
            animationGO[i].transform.localScale = Vector3.zero;
            animationGO[i].SetActive(true);
            animationGO[i].transform.DOScale(1f, 0.4f);
            yield return new WaitForSeconds(0.4f);
        }
        // 显示头像出现动画
        GameManager.Sound.PlayAudio("Glacier_Search_Com");
        // 显示参与人数增长动画
        float wait = waitTime;
        float spee = speed;
        int numspeed = 1;
        bool run = false;
        //人数增长和头像显示动画(人数增长速度从慢到快)
        for (int j = 0; j < HeadIcons.Length; j++)
        {
            HeadIcons[j].gameObject.SetActive(true);
            HeadIcons[j].skele.timeScale = spee;
            HeadIcons[j].skele.Initialize(true);
            HeadIcons[j].skele.AnimationState.SetAnimation(0, j != 0 ? "01" : "03", false);
            //延时一帧显示头像，防止头像提前显示导致头像出现闪现的效果
            yield return null;
            HeadIcons[j].headIcon.gameObject.SetActive(true);
            if (spee < speedMax)
            {
                spee += speedAd;
                wait = Mathf.Max(0, wait - waitre);
            }
            if (j >= 2)//延时人数的增长
            {
                num += numspeed;
                numspeed = Mathf.Min(3, numspeed + 1);
                peopleNum.text = $"{num}/100";
                if (!run)
                {
                    peopleNum.transform.DOScale(1.1f, 0.2f).SetLoops(-1, LoopType.Yoyo);
                    run = true;
                }
            }
            yield return new WaitForSeconds(wait);
        }
        //等待头像动画播放完毕，期间人数继续进行增长
        float time = 0;
        while (num < 100 && time < 0.35f)
        {
            num += 3;
            peopleNum.text = $"{num}/100";
            peopleNum.transform.localScale = Vector3.one;
            time += wait;
            yield return new WaitForSeconds(wait);
        }
        peopleNum.text = $"{100}/100";
        peopleNum.transform.DOKill();
        //TODO
        //EffectManager.Instance.PunchAnimPlayInTarget(peopleNum.transform, 100, new Vector3(-80, 0, 0));
        //显示确定按钮
        okBtn.interactable = true;
        animationGO[i].SetActive(true);
        animationGO[i].transform.localScale = Vector3.zero;
        animationGO[i].transform.DOScale(1f, 0.4f);
        okBtn.enabled = true;
    }

#if UNITY_EDITOR
    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            okBtn.SetBtnEvent(OnClickOkBtn);
            peopleNum.SetText("1/100");
            // 初始化头像并隐藏
            for (int i = 0; i < animationGO.Length; i++)
            {
                animationGO[i].SetActive(false);
            }

            int[] headIds = GameManager.Task.GlacierQuestTaskManager.HeadIds;
            for (int i = 0; i < HeadIcons.Length; i++)
            {
                HeadIcons[i].Init(headIds[i], i == 0);
                HeadIcons[i].gameObject.SetActive(false);
                HeadIcons[i].headIcon.gameObject.SetActive(false);
            }

            StopCoroutine("PlayFindAnim");
            StartCoroutine("PlayFindAnim");
        }
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }
#endif
}
