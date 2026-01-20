using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLifeBar : LifeBar
{
    public GameObject body;
    public GameCoinBar coinBar;
    public GameGoldBar goldBar;

    private int delayTaskId;
    private bool showGoldBar = false;

    public override void Show()
    {
        var panel= GameManager.UI.GetUIForm("ShopBuyItemPanel");
        if (panel != null)
        {
            body.gameObject.SetActive(false);
            return;
        }

        if (coinBar == null || !coinBar.body.activeSelf)
        {
            if (delayTaskId != 0)
                GameManager.Task.RemoveDelayTriggerTask(delayTaskId);

            body.gameObject.SetActive(true);

            if (goldBar != null && goldBar.body.activeSelf)
            {
                showGoldBar = true;
                goldBar.body.SetActive(false);
            }
        }
    }

    public override void OnLifeFlyEnd()
    {
        delayTaskId = GameManager.Task.AddDelayTriggerTask(1.5f, () =>
        {
            if (body != null)
            {
                body.gameObject.SetActive(false);
                if (showGoldBar)
                    goldBar.body.SetActive(true);
            }
        });
    }
}
