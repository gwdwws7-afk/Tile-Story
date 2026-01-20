using DG.Tweening;
using UnityEngine;

/// <summary>
/// 提示框基类
/// </summary>
public abstract class PromptBox : MonoBehaviour
{
    public RectTransform body;
    public RectTransform box;
    public RectTransform triangleUp;
    public RectTransform triangleDown;

    public PromptBoxShowDirection showDirection;
    public bool forbidShow;

    [Header("Body Setting")]
    public float centerOffset = 220;
    public float triangelOffset = 0;
    public float boxPivot = 0.5f;
    public float triangleUpY = 8;
    public float triangleDownY = 5;
    public float triangleRightX = -4;
    public float triangleLeftX = 5;

    [Header("Box Setting")]
    public float boxMinWidth = 600;
    public float boxMinHeight = 190;
    public float boxMaxWidth = 600;
    public float boxMaxHeight = 1000;

    //方框的内部填充大小
    public float boxHorizontalPadding = 10;
    public float boxVerticalPadding = 50;

    protected float boxPreferredWidth;
    protected float boxPreferredHeight;

    protected int audoHideEventId = 0;

    public virtual void ShowPromptBox(PromptBoxShowDirection direction, Vector3 position, float autoHideTime = 0)
    {
        HidePromptBox();

        if (forbidShow)
        {
            return;
        }

        showDirection = direction;

        Transform cachedTransform = transform;
        cachedTransform.DOKill();
        cachedTransform.position = position;
        cachedTransform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        cachedTransform.DOScale(1.1f, 0.15f).onComplete = () =>
        {
            cachedTransform.DOScale(1, 0.15f);
        };

        Refresh();
        GameManager.Task.AddDelayTriggerTask(0.02f, Refresh);

        if (autoHideTime > 0)
        {
            audoHideEventId = GameManager.Task.AddDelayTriggerTask(autoHideTime, HidePromptBox);
        }
    }

    public virtual void HidePromptBox()
    {
        if (audoHideEventId > 0)
        {
            GameManager.Task.RemoveDelayTriggerTask(audoHideEventId);
            audoHideEventId = 0;
        }

        transform.DOKill();
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Refresh();
    }
#endif

    public virtual void Refresh()
    {
        boxPreferredWidth = boxPreferredWidth <= 0 ? boxMinWidth : boxPreferredWidth;
        boxPreferredHeight = boxPreferredHeight <= 0 ? boxMinHeight : boxPreferredHeight;

        if (showDirection == PromptBoxShowDirection.Up)
        {
            body.localPosition = new Vector3(0, centerOffset);

            box.pivot = new Vector2(boxPivot, 0);
            box.localPosition = new Vector3(triangelOffset, 0);

            triangleUp.gameObject.SetActive(false);
            triangleDown.gameObject.SetActive(true);
            triangleDown.localPosition = new Vector3(0, triangleUpY);
            triangleDown.eulerAngles = Vector3.zero;
            triangleDown.localScale = new Vector3(1, 1, 1);
        }
        else if (showDirection == PromptBoxShowDirection.Down)
        {
            body.localPosition = new Vector3(0, -centerOffset);

            box.pivot = new Vector2(boxPivot, 1);
            box.localPosition = new Vector3(triangelOffset, 0);

            triangleUp.gameObject.SetActive(true);
            triangleDown.gameObject.SetActive(false);
            triangleUp.localPosition = new Vector3(0, triangleDownY);
            triangleUp.eulerAngles = Vector3.zero;
            triangleUp.localScale = Vector3.one;
        }
        else if (showDirection == PromptBoxShowDirection.Left)
        {
            body.localPosition = new Vector3(-centerOffset, 0);

            box.pivot = new Vector2(1, boxPivot);
            box.localPosition = new Vector3(0, triangelOffset);

            triangleUp.gameObject.SetActive(true);
            triangleDown.gameObject.SetActive(false);
            triangleUp.localPosition = new Vector3(triangleLeftX, 0);
            triangleUp.eulerAngles = new Vector3(0, 0, -90);
            triangleUp.localScale = new Vector3(-1, 1, 1);
        }
        else if (showDirection == PromptBoxShowDirection.Right)
        {
            body.localPosition = new Vector3(centerOffset, 0);

            box.pivot = new Vector2(0, boxPivot);
            box.localPosition = new Vector3(0, triangelOffset);

            triangleUp.gameObject.SetActive(true);
            triangleDown.gameObject.SetActive(false);
            triangleUp.localPosition = new Vector3(triangleRightX, 0);
            triangleUp.eulerAngles = new Vector3(0, 0, 90);
            triangleUp.localScale = Vector3.one;
        }
    }

    public virtual void OnRelease()
    {
    }
}
