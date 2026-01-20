using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;

public class OptimizedSwiper : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public CardSetBookMenu cardSetBookMenu;
    // 基本配置
    public Transform content;
    public Scroller scroller;
    [Range(0.1f, 0.5f)] public float swipeThreshold = 0.2f;
    public float swipeDuration = 0.6f;
    
    //页面指示器
    public Transform indicatorContainer;
    public Image indicatorPrefab;
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    
    // 分页状态
    public int currentPage = 0;
    public int totalPages = 0;
    private bool _isSwiping = false;
    private Image[] _indicators;
    private float _dragStartPosition;

    public void Init()
    {
        // 创建页面指示器
        CreatePageIndicators();
        // 更新指示器状态
        UpdateIndicator();
        // 检测发奖
        CheckPageNeedShowReward();
    }

    public void Release()
    {
        // 清除现有指示器
        foreach (var image in _indicators)
        {
            image.sprite = null;
        }
        indicatorContainer.DestroyAllChild();
    }
    
    #region 页面指示器
    // 创建页面指示器
    private void CreatePageIndicators()
    {
        _indicators = new Image[totalPages];
        for (int i = 0; i < totalPages; i++)
        {
            Image indicator = Instantiate(indicatorPrefab, indicatorContainer);
            _indicators[i] = indicator;
        }
    }
    
    // 更新页面指示器状态
    private void UpdateIndicator()
    {
        for (int i = 0; i < _indicators.Length; i++)
        {
            _indicators[i].sprite = i == currentPage ? activeSprite : inactiveSprite;
            _indicators[i].SetNativeSize();
        }

        cardSetBookMenu.leftButton.gameObject.SetActive(currentPage != 0);
        cardSetBookMenu.rightButton.gameObject.SetActive(currentPage != totalPages - 1);
    }
    #endregion
    
    #region UI交互
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSwiping) return;
        _dragStartPosition = scroller.Position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isSwiping) return;
        
        float dragEndPosition = scroller.Position;
        float dragDelta = dragEndPosition - _dragStartPosition;
        
        if (Mathf.Abs(dragDelta) > swipeThreshold)
        {
            if (dragDelta > 0 && currentPage < totalPages - 1)
            {
                SwipeToPage(currentPage + 1);
            }
            else if (dragDelta < 0 && currentPage > 0)
            {
                SwipeToPage(currentPage - 1);
            }
            else
            {
                ReturnToCurrentPage();
            }
        }
        else
        {
            ReturnToCurrentPage();
        }
    }

    private void SwipeToPage(int pageIndex)
    {
        if (_isSwiping || pageIndex < 0 || pageIndex >= totalPages) 
            return;

        GameManager.Sound.PlayAudio("Card_Collection_Switch_Pages");
        
        RefreshPageWhenLeave();
        _isSwiping = true;
        currentPage = pageIndex;
        
        DOTween.To(
            () => scroller.Position,
            x => scroller.Position = x,
            (float)pageIndex,
            swipeDuration
        ).OnComplete(() => 
        {
            _isSwiping = false;
            UpdateIndicator();          // 更新指示器状态
            CheckPageNeedShowReward();  // 检测发奖
        });
    }

    private void RefreshPageWhenLeave()
    {
        foreach (Transform child in content)
        {
            if (child.name == (currentPage + 1).ToString())
                child.GetComponent<CardSetPage>().Refresh();
        }
    }

    private void CheckPageNeedShowReward()
    {
        foreach (Transform child in content)
        {
            if (child.name == (currentPage + 1).ToString())
                child.GetComponent<CardSetPage>().ShowCardSetReward();
        }
    }
    
    // 返回当前页面
    private void ReturnToCurrentPage()
    {
        _isSwiping = true;
        DOTween.To(
            () => scroller.Position,
            x => scroller.Position = x,
            (float)currentPage,
            swipeDuration / 2
        ).OnComplete(() => _isSwiping = false);
    }

    // 上一页按钮
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            SwipeToPage(currentPage - 1);
        }
    }
    
    // 下一页按钮
    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            SwipeToPage(currentPage + 1);
        }
    }
    #endregion
}
