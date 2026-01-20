using System;
using DG.Tweening;
using GameFramework.Event;
using TMPro;
using UnityEngine;

public class MapLevelBtn : MonoBehaviour
{
    [SerializeField] private GameObject[] Hard_Objs;
    [SerializeField] private TextMeshProUGUI[] LevelTexts;
    [SerializeField] private ParticleSystem UILevel_Effect;
    [SerializeField] private DelayButton Level_Btn;
    [SerializeField] private GameObject BodyObj;
    [SerializeField] private GameObject Tag;
    //[SerializeField] private GameObject DecorateObj;
    [SerializeField] private TextMeshProUGUI TagText;

    public void Init()
    {
        SetLevelText();
        //DecorateObj.SetActive(ClimbBeanstalkManager.Instance.CheckActivityHasStarted());
    }

    // private void OnEnable()
    // {
    //     GameManager.Event.Subscribe(CommonEventArgs.EventId, OnEventReceived);
    //     //DecorateObj.SetActive(ClimbBeanstalkManager.Instance.CheckActivityHasStarted());
    // }
    
    // private void OnDisable()
    // {
    //     GameManager.Event.Unsubscribe(CommonEventArgs.EventId, OnEventReceived);
    // }

    // private void OnEventReceived(object sender, GameEventArgs e)
    // {
    //     var ne = e as CommonEventArgs;
    //     if (ne.Type == CommonEventType.ClimbBeanstalkInfoChanged)
    //     {
    //         DecorateObj.SetActive(ClimbBeanstalkManager.Instance.CheckActivityHasStarted());
    //     }
    // }
    
    public void PlayLevelAnim(Action finishAction)
    {
        BodyObj.transform.DOScale(new Vector3(1.26f, 0.85f, 1f), 0.25f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            PlayLevelBtnEffect();
            BodyObj.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                finishAction?.Invoke();
            });
        });
        GameManager.Task.AddDelayTriggerTask(0.28f, () =>
        {
            GameManager.DataNode.SetData("NowLevel", GameManager.PlayerData.NowLevel);
            SetLevelText();
        });
    }

    public void PlayLevelBtnEffect()
    {
        if (UILevel_Effect) UILevel_Effect.Play();
    }

    public void SetLevelBtnEvent(Action finishAction)
    {
        Level_Btn.SetBtnEvent(() => { finishAction?.Invoke(); });
    }

    private void SetLevelText()
    {
        int nowLevel = GameManager.PlayerData.NowLevel;
        var levelNum = GameManager.DataNode.GetData<int>("NowLevel", nowLevel);
        int hardIndex = DTLevelUtil.GetLevelHard(GameManager.PlayerData.RealLevel(levelNum));

        for (int i = 0; i < Hard_Objs.Length; i++)
        {
            Hard_Objs[i].gameObject.SetActive(hardIndex == i);
        }
        foreach (var text in LevelTexts)
        {
            text.text = levelNum.ToString();
            if (levelNum < 10)
                text.fontSize = 75;
            else if (levelNum < 100)
                text.fontSize = 68;
            else if (levelNum < 1000)
                text.fontSize = 50;
            else
                text.fontSize = 40;
        }

        Tag.SetActive(hardIndex > 0 /*&& PlayerPrefs.GetInt(Constant.PlayerData.FirstPassFail + nowLevel, 0) == 0*/);
        TagText.text = hardIndex == 1 ? "x2" : "x3";
    }
}
