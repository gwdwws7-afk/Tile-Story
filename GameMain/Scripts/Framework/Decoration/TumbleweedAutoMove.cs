using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TumbleweedAutoMove : MonoBehaviour
{
    public Transform m_Tumbleweed;
    public Transform m_StartPos;
    public Transform m_EndPos;
    public float m_StartSize = 1;
    public float m_EndSize = 1;
    public float m_StartDelay = 0.2f;
    public float m_NextDelay = 1;
    public float m_MoveTime = 5;

    private void OnEnable()
    {
        ShowMoveAnim(m_StartDelay);
    }

    private void OnDisable()
    {
        m_Tumbleweed.DOKill();
    }

    private void ShowMoveAnim(float delayTime)
    {
        m_Tumbleweed.position = m_StartPos.position;
        m_Tumbleweed.localScale = new Vector3(m_StartSize, m_StartSize, m_StartSize);
        m_Tumbleweed.DOScale(new Vector3(m_EndSize, m_EndSize, m_EndSize), m_MoveTime).SetDelay(delayTime);
        m_Tumbleweed.DOMove(m_EndPos.position, m_MoveTime).SetDelay(delayTime).onComplete = () =>
        {
            ShowMoveAnim(m_NextDelay);
        };
    }
}
