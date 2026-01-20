using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCoinBar : CoinBarManager
{
    public GameObject body;
    public GameLifeBar lifeBar;
    public GameGoldBar goldBar;

    private int delayTaskId;
    private bool showGoldBar = false;

    public override void Show()
    {
        if (delayTaskId != 0)
            GameManager.Task.RemoveDelayTriggerTask(delayTaskId);

        body.gameObject.SetActive(true);

        if (lifeBar != null && lifeBar.body.activeSelf)
            lifeBar.body.SetActive(false);

        if (goldBar != null && goldBar.body.activeSelf)
        {
            showGoldBar = true;
            goldBar.body.SetActive(false);
        }
    }

    public override void OnCoinFlyEnd()
    {
        RefreshCoinText();

        delayTaskId = GameManager.Task.AddDelayTriggerTask(1f, () =>
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
