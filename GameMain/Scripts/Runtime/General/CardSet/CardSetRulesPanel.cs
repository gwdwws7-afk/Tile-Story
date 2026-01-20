using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSetRulesPanel : PopupMenuForm, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Transform[] guides;
    public ScrollRect scrollRect;
    private bool _isAnimFinish;

    public override void OnShowInit(Action<UIForm> showInitSuccessAction = null, object userData = null)
    {
        _isAnimFinish = false;
        scrollRect.horizontal = false;
        
        foreach (var obj in guides)
        {
            obj.localScale = Vector3.zero;
        }

        base.OnShowInit(showInitSuccessAction, userData);
    }
    
    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        showSuccessAction?.Invoke(null);
        float delayTime = -0.2f;
        for (int i = 0; i < guides.Length; i++)
        {
            var index = i;
            delayTime += 0.2f;
            guides[i].DOScale(1.1f, 0.2f).SetDelay(delayTime).onComplete = () =>
            {
                guides[index].DOScale(1f, 0.2f).onComplete = () =>
                {
                    if (index == guides.Length - 1)
                    {
                        _isAnimFinish = true;
                        scrollRect.horizontal = true;
                    }
                };
            };
        }
    }
    
    #region ClickEvent
    private Vector2 _pointerDownPos;
    private bool _isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDownPos = eventData.position;   // 记录初始位置
        _isDragging = false;                    // 重置拖动状态
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 计算移动距离
        if (Vector2.Distance(_pointerDownPos, eventData.position) > 0.1f * Screen.dpi)
        {
            _isDragging = true;  // 超过阈值视为拖动
            // OnStartDrag?.Invoke(); // 自定义拖动开始事件
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDragging) 
        {
            // OnEndDrag?.Invoke(); // 自定义拖动结束事件
            _isDragging = false;  // 重置状态
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isDragging)  // 非拖动状态时触发点击
        {
            if (_isAnimFinish)
            {
                UnityUtil.EVibatorType.VeryShort.PlayerVibrator();
                GameManager.Sound.PlaySound("SFX_UI_close", "UISound");
                GameManager.UI.HideUIForm(this);
            }
        }
    }
    #endregion
}
