using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class PageSwiper : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("页面设置")]
    public ScrollRect scrollRect;          // ScrollRect 组件
    public RectTransform[] pages;          // 所有页面的 RectTransform
    public float pageWidth = 1080f;        // 单个页面的宽度
    
    [Header("滑动设置")]
    [Range(0.1f, 0.5f)]
    public float swipeThreshold = 0.2f;    // 滑动阈值（比例）
    public float swipeDuration = 0.3f;     // 切换动画时长
    
    [Header("页面指示器")]
    public Transform indicatorContainer;   // 指示器容器
    public Image indicatorPrefab;          // 指示器预制体
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    // public Color activeColor = Color.white;// 活动页面指示器颜色
    // public Color inactiveColor = Color.gray;// 非活动页面指示器颜色
    // public float indicatorSpacing = 20f;   // 指示器间距
    
    private int _currentPage = 0;           // 当前页面索引
    private bool _isSwiping = false;        // 是否正在滑动
    private Image[] _indicators;            // 指示器数组
    private float _dragStartPosition;       // 拖拽开始位置

    void Start()
    {
        // 确保 ScrollRect 设置正确
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.inertia = false;        // 禁用惯性
        
        // 初始化页面位置
        UpdatePagePositions();
        
        // 创建页面指示器
        CreatePageIndicators();
        
        // 更新指示器状态
        UpdateIndicator();
    }

    // 更新所有页面位置（自动排列）
    private void UpdatePagePositions()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].anchoredPosition = new Vector2(i * pageWidth, 0);
        }
    }

    // 创建页面指示器
    private void CreatePageIndicators()
    {
        // 清除现有指示器
        foreach (Transform child in indicatorContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 创建新指示器
        _indicators = new Image[pages.Length];
        // float totalWidth = (pages.Length - 1) * indicatorSpacing;
        // float startX = -totalWidth / 2f;
        
        for (int i = 0; i < pages.Length; i++)
        {
            Image indicator = Instantiate(indicatorPrefab, indicatorContainer);
            // indicator.rectTransform.anchoredPosition = new Vector2(startX + i * indicatorSpacing, 0);
            _indicators[i] = indicator;
        }
    }

    // 更新页面指示器状态
    private void UpdateIndicator()
    {
        for (int i = 0; i < _indicators.Length; i++)
        {
            // // 添加平滑的颜色过渡效果
            // indicators[i].DOColor(i == currentPage ? activeColor : inactiveColor, 0.2f);
            //
            // // 添加缩放效果（可选）
            // indicators[i].transform.DOScale(i == currentPage ? 1.2f : 1f, 0.2f);

            _indicators[i].sprite = i == _currentPage ? activeSprite : inactiveSprite;
            _indicators[i].SetNativeSize();
        }
    }

    // 实现 IBeginDragHandler 接口 - 拖拽开始时调用
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSwiping) return;
        
        // 记录拖拽开始位置
        _dragStartPosition = scrollRect.horizontalNormalizedPosition;
    }

    // 实现 IEndDragHandler 接口 - 拖拽结束时调用
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isSwiping) return;
        
        // 计算滑动距离比例
        float dragEndPosition = scrollRect.horizontalNormalizedPosition;
        float dragDelta = dragEndPosition - _dragStartPosition;
        
        // 判断滑动方向
        if (Mathf.Abs(dragDelta) > swipeThreshold)
        {
            if (dragDelta > 0 && _currentPage < pages.Length - 1)
            {
                SwipeToPage(_currentPage + 1);
            }
            else if (dragDelta < 0 && _currentPage > 0)
            {
                SwipeToPage(_currentPage - 1);
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

    // 滑动到指定页面
    public void SwipeToPage(int pageIndex)
    {
        if (_isSwiping || pageIndex < 0 || pageIndex >= pages.Length) 
            return;
        
        _isSwiping = true;
        _currentPage = pageIndex;
        
        // 使用 DOTween 实现平滑滑动
        DOTween.To(
            () => scrollRect.horizontalNormalizedPosition,
            x => scrollRect.horizontalNormalizedPosition = x,
            (float)pageIndex / (pages.Length - 1),
            swipeDuration
        ).OnComplete(() => 
        {
            _isSwiping = false;
            UpdateIndicator(); // 更新指示器状态
        });
    }

    // 返回当前页面
    private void ReturnToCurrentPage()
    {
        _isSwiping = true;
        DOTween.To(
            () => scrollRect.horizontalNormalizedPosition,
            x => scrollRect.horizontalNormalizedPosition = x,
            (float)_currentPage / (pages.Length - 1),
            swipeDuration / 2
        ).OnComplete(() => _isSwiping = false);
    }
    
    // 上一页按钮
    public void PreviousPage()
    {
        if (_currentPage > 0)
        {
            SwipeToPage(_currentPage - 1);
        }
    }
    
    // 下一页按钮
    public void NextPage()
    {
        if (_currentPage < pages.Length - 1)
        {
            SwipeToPage(_currentPage + 1);
        }
    }
}