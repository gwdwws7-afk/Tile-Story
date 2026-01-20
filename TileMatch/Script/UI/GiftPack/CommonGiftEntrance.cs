using System;
using UnityEngine;

[System.Serializable]
public class DataTimeBySerializeField
{
    public int Year;
    public int Month;
    public int Day;
    public int Hour;
    public int Minute;

    public DateTime DateTime
    {
        get
        {
            if (Year == 0)
            {
                return System.DateTime.MinValue;
            }
            else
            {
                return new DateTime(Year, Month, Day, Hour, Minute, 0);
            }
        }
    }
}

public class CommonGiftEntrance : UIForm
{
    [SerializeField] private int OpenLevel=0;
    [SerializeField] private string PanelName;
    [SerializeField] private DataTimeBySerializeField StartDateTimeBySerializeField;
    [SerializeField] private DataTimeBySerializeField EndDateTimeBySerializeField;
    [SerializeField] private CountdownTimer CountdownTimer;
    [SerializeField] private DelayButton Btn;

    private DateTime startDateTime = DateTime.MinValue;
    private DateTime endDateTime = DateTime.MaxValue;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        startDateTime = StartDateTimeBySerializeField.DateTime;
        endDateTime = EndDateTimeBySerializeField.DateTime;
        
        Init();
        base.OnInit(uiGroup, completeAction, userData);
    }

    public override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
    {
    }
    
    public void Init(string panelName,DateTime startDateTime,DateTime endDateTime)
    {
        this.PanelName = panelName;
        this.startDateTime = startDateTime;
        this.endDateTime = endDateTime;

        Init();
    }

    public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (CountdownTimer) CountdownTimer.OnUpdate(elapseSeconds, realElapseSeconds);
        base.OnUpdate(elapseSeconds, realElapseSeconds);
    }

    private void Init()
    {
        if (!string.IsNullOrEmpty(PanelName)&&
            DateTime.Now>=startDateTime&&
            DateTime.Now<=endDateTime&&
            GameManager.PlayerData.NowLevel>=OpenLevel)
        {
            gameObject.SetActive(true);
            transform.SetSiblingIndex(5);
            //设置倒计时
            CountdownTimer.StartCountdown(endDateTime);
            
            CountdownTimer.CountdownOver -= TimeOverEvent;
            CountdownTimer.CountdownOver += TimeOverEvent;
            
            Btn.SetBtnEvent(() =>
            {
                //展示选中的panel
                GameManager.UI.ShowUIForm(PanelName,userData:endDateTime);
            });
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void TimeOverEvent(object obj, CountdownOverEventArgs args)
    {
        gameObject.SetActive(false);
    }

    public override void OnPause()
    {
        Btn.interactable = false;
        base.OnPause();
    }

    public override void OnResume()
    {
        Btn.interactable = true;
        base.OnResume();
    }
}
