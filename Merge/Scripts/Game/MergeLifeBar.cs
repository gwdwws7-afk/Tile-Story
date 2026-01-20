using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeLifeBar : LifeBar
{
    public GameObject body;

    private int delayTaskId;

    public override void Show()
    {
        if (delayTaskId != 0)
            GameManager.Task.RemoveDelayTriggerTask(delayTaskId);

        if (body != null)
        {
            body.gameObject.SetActive(true);
        }
    }

    public override void OnLifeFlyEnd()
    {
        delayTaskId = GameManager.Task.AddDelayTriggerTask(1.2f, () =>
        {
            if (body != null)
            {
                body.gameObject.SetActive(false);
            }
        });

        base.OnLifeFlyEnd();
    }
}
