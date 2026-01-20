using GameFramework.Event;
using MySelf.Model;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveEntrance : EntranceUIForm
{
    public GameObject m_WarningIcon;
    public TextMeshProUGUI m_CanGetRewardNumText;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        GameManager.Event.Subscribe(CommonEventArgs.EventId, CommonHandle);

        gameObject.SetActive(GameManager.Objective.CheckObjectiveUnlock());

        RefreshWarningIcon();
    }

    public override void OnRelease()
    {
        GameManager.Event.Unsubscribe(CommonEventArgs.EventId, CommonHandle);

        base.OnRelease();
    }

    public bool IsShowGuide()
    {
        return GameManager.Objective.CheckObjectiveUnlock() && !PlayerBehaviorModel.Instance.HasShownObjectiveGuide();
    }

    public void ShowGuide()
    {
        if (IsShowGuide())
        {
            entranceBtn.interactable = false;
            GameManager.UI.ShowUIForm("CommonGuideMenu",form =>
            {
                entranceBtn.interactable = true;
                PlayerBehaviorModel.Instance.RecordObjectiveGuide();
                form.gameObject.SetActive(false);
                var guideMenu = form.GetComponent<CommonGuideMenu>();
                GameObject btnObj = Instantiate(gameObject, transform.position, Quaternion.identity, form.transform);
                btnObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(125f, btnObj.GetComponent<RectTransform>().anchoredPosition.y);
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(OnButtonClick);
                guideMenu.SetText("Objective.Tap here to the Task system");
                guideMenu.tipBox.SetOkButton(false);
                var position = btnObj.transform.position + new Vector3(0.3f, 0, 0);
                guideMenu.ShowGuideArrow(position, position + new Vector3(0.15f, 0, 0), PromptBoxShowDirection.Left);
                guideMenu.tipBox.transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)).x, transform.position.y - 0.3f);
                guideMenu.OnShow(null, null);

                void ClickAction()
                {
                    if (btnObj != null)
                        Destroy(btnObj);
                    GameManager.UI.HideUIForm(form);
                    guideMenu.guideImage.onTargetAreaClick = null;
                }
                btn.onClick.AddListener(ClickAction);
            }, () =>
            {
                GameManager.Process.EndProcess(ProcessType.ShowObjectiveGuide);
            });
        }
        else
        {
            GameManager.Process.EndProcess(ProcessType.ShowObjectiveGuide);
        }
    }

    public void RefreshWarningIcon()
    {
        if (!GameManager.Objective.CheckObjectiveUnlock())
            return;

        int num = 0;
        for (int i = 0; i < GameManager.Objective.CurDailyObjectiveIds.Count; i++)
        {
            if (GameManager.Objective.CheckObjectiveCompleted(GameManager.Objective.CurDailyObjectiveIds[i], false))
            {
                num++;
            }
        }

        for (int i = 0; i < GameManager.Objective.CurAllTimeObjectiveIds.Count; i++)
        {
            if (GameManager.Objective.CheckObjectiveCompleted(GameManager.Objective.CurAllTimeObjectiveIds[i], true))
            {
                num++;
            }
        }

        m_WarningIcon.SetActive(num > 0);
        m_CanGetRewardNumText.text = num.ToString();
    }

    public void CommonHandle(object sender, GameEventArgs e)
    {
        CommonEventArgs ne = (CommonEventArgs)e;
        switch (ne.Type)
        {
            case CommonEventType.Objective:
                RefreshWarningIcon();
                break;
        }
    }

    public override void OnButtonClick()
    {
        GameManager.UI.ShowUIForm("ObjectivePanel");
    }
}
