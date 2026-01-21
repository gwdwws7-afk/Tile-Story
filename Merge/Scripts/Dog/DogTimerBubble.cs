using Merge;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DogTimerBubble : MonoBehaviour
{
    public Transform m_Body;
    public SkeletonGraphic m_BubbleAnim;
    public TextMeshProUGUI m_TimerText;

    private bool m_IsShowing;

    public bool IsShowing => m_IsShowing;

    public void Show(Vector3 pos, bool showAnim)
    {
        if (m_IsShowing)
        {
            return;
        }
        m_IsShowing = true;

        transform.position = pos;

        gameObject.SetActive(true);

        if (showAnim)
        {
            m_BubbleAnim.AnimationState.SetAnimation(0, "appear", false).Complete += t =>
            {
                m_BubbleAnim.AnimationState.SetAnimation(0, "idle2", true);
            };
        }
        else
        {
            m_BubbleAnim.AnimationState.SetAnimation(0, "idle2", true);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        m_IsShowing = false;
    }
}
