using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 任务奖励提示框
/// </summary>
public sealed class TaskRewardPromptBox : MonoBehaviour
{
    public TextMeshProUGUI promptText;

    public void OnInit(bool isGet)
    {
        if (isGet)
        {
            promptText.text = "This reward is already been collected!"; 
        }
        else
        {
            promptText.text = "Destroy more balls to unlock this stage and get this reward!";
        }
    }

    public void OnReset()
    {
        transform.DOKill();
    }

    public void OnShow(Vector3 position)
    {
        Transform cachedTransform = transform;
        cachedTransform.position = position;
        cachedTransform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        cachedTransform.DOScale(1.1f, 0.2f).onComplete = () =>
        {
            cachedTransform.DOScale(1, 0.2f);
        };
    }

    public void OnHide()
    {
        gameObject.SetActive(false);
    }
}
