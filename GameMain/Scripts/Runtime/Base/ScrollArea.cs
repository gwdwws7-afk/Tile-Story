using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Linq;

public enum AnchorDir
{
    Top,
    Middle,
}

/// <summary>
/// 滑动区域基类
/// </summary>
public class ScrollArea : MonoBehaviour
{
    public RectTransform viewport;
    public RectTransform content;
    public ScrollRect scrollRect;

    public float AreaPaddingTop = 0;
    public float AreaPaddingBottom = 0;

    public float ContentPaddingTop = 0;
    public float ContentPaddingBottom = 0;

    public AnchorDir AnchorDir = AnchorDir.Top;

    public LinkedList<ScrollColumn> scrollColumnList = new LinkedList<ScrollColumn>();

    private bool m_IsInit;
    private float m_RecycleValue;
    private float m_LastVerticalNormalizedPosition;
    private int m_MinIndex;
    private int m_MaxIndex;
    private int m_MinColumnCount;
    private int m_MaxColumnCount;
    private Action<ScrollColumn> m_OnSpawnAction;
    private Action m_OnUnspawnAction;

    public float recycleWidth = 0;
    public int currentIndex = 0;

    private int columnCountDelta = 4;
    private float centerVerticalDelta = 0;

    /// <summary>
    /// 滚动栏的数量
    /// </summary>
    public int Count
    {
        get { return scrollColumnList.Count; }
    }

    /// <summary>
    /// 滚动栏生成完毕事件
    /// </summary>
    public event Action<ScrollColumn> OnSpawnAction
    {
        add { m_OnSpawnAction += value; }
        remove { m_OnSpawnAction -= value; }
    }

    /// <summary>
    /// 滚动栏回收完毕事件
    /// </summary>
    public event Action OnUnspawnAction
    {
        add { m_OnUnspawnAction += value; }
        remove { m_OnUnspawnAction -= value; }
    }

    public void OnInit(RectTransform parent)
    {
        if (recycleWidth < 0)
        {
            recycleWidth = 0;
        }

        AdjustScrollArea(parent);
        CheckContentHeight();

        //将目标栏居中
        m_LastVerticalNormalizedPosition = CenterTheTargetColumn(currentIndex, 0);

        scrollRect.enabled = true;
        scrollRect.vertical = true;

        m_IsInit = true;
    }

    public void OnReset()
    {
        m_IsInit = false;

        scrollRect.onValueChanged.RemoveAllListeners();
        scrollRect.enabled = false;
        m_RecycleValue = 0;
        m_LastVerticalNormalizedPosition = 0;
        m_OnSpawnAction = null;
        m_OnUnspawnAction = null;

        foreach (ScrollColumn column in scrollColumnList)
        {
            column.Unspawn();
        }

        scrollColumnList.Clear();
    }

    public void OnRelease()
    {
        m_IsInit = false;

        foreach (ScrollColumn column in scrollColumnList)
        {
            column.Release();
        }

        scrollColumnList.Clear();
    }

    public void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        if (!m_IsInit || recycleWidth <= 0)
        {
            return;
        }

        float delta = m_LastVerticalNormalizedPosition - scrollRect.verticalNormalizedPosition;

        if (delta >= m_RecycleValue)
        {
            int value = Mathf.RoundToInt(delta / m_RecycleValue);
            m_LastVerticalNormalizedPosition = m_LastVerticalNormalizedPosition - m_RecycleValue * value;

            int centerIndex = GetViewportCenterIndex() - 1;
            int num = m_MaxColumnCount / 2;
            m_MinIndex = centerIndex - num < 0 ? 0 : centerIndex - num;
            m_MaxIndex = centerIndex + num > scrollColumnList.Count ? scrollColumnList.Count : centerIndex + num;

            RefreshScrollColumnList();
        }
        else if (delta < -m_RecycleValue)
        {
            int value = Mathf.RoundToInt(-delta / m_RecycleValue);
            m_LastVerticalNormalizedPosition = m_LastVerticalNormalizedPosition + m_RecycleValue * value;

            int centerIndex = GetViewportCenterIndex() - 1;
            int num = m_MaxColumnCount / 2;
            m_MinIndex = centerIndex - num < 0 ? 0 : centerIndex - num;
            m_MaxIndex = centerIndex + num > scrollColumnList.Count ? scrollColumnList.Count : centerIndex + num;

            RefreshScrollColumnList();
        }
    }

    /// <summary>
    /// 设置最大滚动栏实例和最小滚动栏实例的差值
    /// </summary>
    public void SetColumnCountDelta(int columnCountDelta)
    {
        this.columnCountDelta = columnCountDelta;
    }

    /// <summary>
    /// 设置当前滚动栏居中的偏移量
    /// </summary>
    /// <param name="centerVerticalDelta">偏移量</param>
    public void SetCenterDelta(float centerVerticalDelta)
    {
        this.centerVerticalDelta = centerVerticalDelta;
    }

    /// <summary>
    /// 调整滚动区域大小
    /// </summary>
    /// <param name="parent">父物体</param>
    public void AdjustScrollArea(RectTransform parent)
    {
        RectTransform rectTrans = GetComponent<RectTransform>();
        float rectHeight = parent.rect.size.y;

        if (AnchorDir == AnchorDir.Top)
        {
            rectTrans.anchorMax = new Vector2(1, ((rectHeight - AreaPaddingTop) / (float)rectHeight));
            rectTrans.anchorMin = new Vector2(0, (AreaPaddingBottom / (float)rectHeight));
        }
        else if(AnchorDir == AnchorDir.Middle)
        {
            rectTrans.anchorMax = new Vector2(1, ((rectHeight / 2f + AreaPaddingTop) / (float)rectHeight));
            rectTrans.anchorMin = new Vector2(0, (AreaPaddingBottom / (float)rectHeight));
        }
    }

    /// <summary>
    /// 获取第一个目标滚动栏
    /// </summary>
    /// <param name="columnName">目标滚动栏名称</param>
    /// <returns>目标滚动栏</returns>
    public ScrollColumn GetColumn(string columnName)
    {
        foreach (var column in scrollColumnList)
        {
            if (column.ColumnName == columnName)
            {
                return column;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取所有目标滚动栏
    /// </summary>
    /// <param name="columnName">目标滚动栏名称</param>
    /// <returns>目标滚动栏集合</returns>
    public ScrollColumn[] GetColumns(string columnName)
    {
        List<ScrollColumn> result = new List<ScrollColumn>();
        foreach (var column in scrollColumnList)
        {
            if (column.ColumnName == columnName)
            {
                result.Add(column);
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// 添加滚动栏到最前
    /// </summary>
    /// <param name="column">滚动栏</param>
    public void AddColumnFirst(ScrollColumn column, bool refreshLayout = false)
    {
        scrollColumnList.AddFirst(column);

        if (refreshLayout)
        {
            CheckContentHeight();
        }
    }

    /// <summary>
    /// 添加滚动栏到最后
    /// </summary>
    /// <param name="column">滚动栏</param>
    public void AddColumnLast(ScrollColumn column, bool refreshLayout = false)
    {
        scrollColumnList.AddLast(column);

        if (refreshLayout)
        {
            CheckContentHeight();
            AutoMoveScrollRect(scrollColumnList.Count - 1, column.Height, 0.2f);
        }
    }

    /// <summary>
    /// 添加滚动栏到序号节点之后
    /// </summary>
    /// <param name="index">序号</param>
    /// <param name="column">滚动栏</param>
    public bool AddColumnAfter(int index, ScrollColumn column, bool refreshLayout = false)
    {
        int i = 0;
        LinkedListNode<ScrollColumn> node = scrollColumnList.First;
        while (node != null)
        {
            if (i == index)
            {
                scrollColumnList.AddAfter(node, column);

                if (refreshLayout)
                {
                    CheckContentHeight();
                    AutoMoveScrollRect(index + 1, column.Height, 0.2f);
                }

                return true;
            }

            node = node.Next;
            i++;
        }

        return false;
    }

    /// <summary>
    /// 移除滚动栏
    /// </summary>
    /// <param name="index">滚动栏序号</param>
    public bool RemoveColumnByIndex(int index, bool refreshLayout = false, float lerpTime = 0f)
    {
        int i = 0;
        foreach (ScrollColumn scrollColumn in scrollColumnList)
        {
            if (i == index)
            {
                scrollColumnList.Remove(scrollColumn);
                scrollColumn.Release();

                if (refreshLayout)
                {
                    CheckContentHeight(lerpTime);
                }

                return true;
            }

            i++;
        }

        return false;
    }

    /// <summary>
    /// 移除滚动栏
    /// </summary>
    /// <param name="id">滚动栏ID</param>
    public bool RemoveColumnByID(string id, bool refreshLayout = false)
    {
        foreach (ScrollColumn scrollColumn in scrollColumnList)
        {
            if (scrollColumn.ID == id)
            {
                scrollColumnList.Remove(scrollColumn);
                scrollColumn.Release();

                if (refreshLayout)
                {
                    CheckContentHeight();
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 刷新所有滚动栏
    /// </summary>
    public void Refresh()
    {
        foreach (ScrollColumn scrollColumn in scrollColumnList)
        {
            scrollColumn.Refresh();
        }
    }

    /// <summary>
    /// 刷新滚动列表
    /// </summary>
    public void RefreshScrollColumnList()
    {
        float totalHeight = ContentPaddingTop;
        int index = 0;
        foreach (ScrollColumn scrollColumn in scrollColumnList)
        {
            float scrollWidth = totalHeight + scrollColumn.Height / 2f;
            if (index >= m_MinIndex && index <= m_MaxIndex)
            {
                scrollColumn.Spawn((flag) =>
                {
                    if (!isShowingAnim || flag)
                        scrollColumn.Instance.transform.localPosition = new Vector3(0, -scrollWidth);
                    scrollColumn.Instance.SetActive(true);
                    OnSpawn(scrollColumn);
                });
            }
            else
            {
                scrollColumn.Unspawn();
                OnUnspawn();
            }

            totalHeight = scrollWidth + scrollColumn.Height / 2f;
            index++;
        }
    }

    /// <summary>
    /// 获取滚动栏的本地坐标
    /// </summary>
    /// <param name="index">滚动栏序号</param>
    public Vector3 GetColumnLocalPosition(int index)
    {
        if (index < 0)
            return new Vector3(0, -ContentPaddingTop, 0);

        if (index >= scrollColumnList.Count)
            return new Vector3(0, -content.sizeDelta.y + ContentPaddingBottom, 0);

        float totalHeight = ContentPaddingTop;
        float lastColumnHeight = 0;
        int i = 0;
        foreach (ScrollColumn scrollColumn in scrollColumnList)
        {
            float scrollWidth = totalHeight + (lastColumnHeight + scrollColumn.Height) / 2f;
            if (i == index)
            {
                return new Vector3(0, -scrollWidth);
            }

            lastColumnHeight = scrollColumn.Height;
            totalHeight = scrollWidth;

            i++;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 获取当前滚动值时处于viewport中心的滚动栏序号
    /// </summary>
    /// <returns></returns>
    public int GetViewportCenterIndex()
    {
        float value = ((1 - scrollRect.verticalNormalizedPosition) * (content.rect.height - viewport.rect.height)) +
            viewport.rect.height / 2f - ContentPaddingTop;
        int result = Mathf.RoundToInt(value / recycleWidth + 0.5f);
        return result;
    }

    /// <summary>
    /// 将目标栏居中
    /// </summary>
    public float CenterTheTargetColumn(int targetIndex, float duration, Ease ease = Ease.OutQuad)
    {
        float verticalPosition =
            1 - (ContentPaddingTop + (targetIndex + 0.5f) * recycleWidth - viewport.rect.height / 2f) /
            (content.rect.height - viewport.rect.height);
        verticalPosition += centerVerticalDelta;
        verticalPosition = verticalPosition > 1 ? 1 : verticalPosition;
        verticalPosition = verticalPosition < 0 ? 0 : verticalPosition;

        if (duration <= 0)
        {
            scrollRect.verticalNormalizedPosition = verticalPosition;
        }
        else
        {
            scrollRect.vertical = false;

            scrollRect.DOVerticalNormalizedPos(verticalPosition, duration).SetEase(ease).onComplete = () =>
            {
                scrollRect.vertical = true;
            };
        }

        return verticalPosition;
    }
    
    /// <summary>
    /// 将目标栏居中
    /// </summary>
    public float CenterTheTargetColumn(float targetIndex, float duration, Ease ease = Ease.OutQuad)
    {
        float verticalPosition =
            1 - (ContentPaddingTop + (targetIndex + 0.5f) * recycleWidth - viewport.rect.height / 2f) /
            (content.rect.height - viewport.rect.height);
        verticalPosition += centerVerticalDelta;
        verticalPosition = verticalPosition > 1 ? 1 : verticalPosition;
        verticalPosition = verticalPosition < 0 ? 0 : verticalPosition;

        if (duration <= 0)
        {
            scrollRect.verticalNormalizedPosition = verticalPosition;
        }
        else
        {
            scrollRect.vertical = false;

            scrollRect.DOVerticalNormalizedPos(verticalPosition, duration).SetEase(ease).onComplete = () =>
            {
                scrollRect.vertical = true;
            };
        }

        return verticalPosition;
    }

    private void AutoMoveScrollRect(int index, float columnHeight, float duration)
    {
        float centerIndex = GetViewportCenterIndex();

        float verticalPosition = scrollRect.verticalNormalizedPosition;
        if (index > centerIndex)
        {
            if (scrollRect.verticalNormalizedPosition <= 0)
            {
                return;
            }

            verticalPosition -= columnHeight / (content.rect.height - viewport.rect.height);
        }
        else
        {
            return;
        }

        if (duration <= 0)
        {
            scrollRect.verticalNormalizedPosition = verticalPosition;
        }
        else
        {
            scrollRect.vertical = false;

            scrollRect.DOVerticalNormalizedPos(verticalPosition, duration).onComplete = () =>
            {
                scrollRect.vertical = true;
            };
        }
    }

    /// <summary>
    /// 检测实例化成功
    /// </summary>
    public bool CheckSpawnComplete()
    {
        foreach (ScrollColumn column in scrollColumnList)
        {
            if (!column.CheckSpawnComplete())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 确定content的高度
    /// </summary>
    private void CheckContentHeight(float lerpTime = 0f)
    {
        float totalHeight = ContentPaddingTop;
        foreach (ScrollColumn scrollColumn in scrollColumnList)
        {
            if (scrollColumn == null)
                continue;

            totalHeight += scrollColumn.Height;
        }

        totalHeight += ContentPaddingBottom;
        m_RecycleValue = recycleWidth / (totalHeight - viewport.rect.height);
        m_LastVerticalNormalizedPosition = scrollRect.verticalNormalizedPosition;

        if (lerpTime > 0) 
        {
            float height = content.sizeDelta.y;
            var tween = DOTween.To(() => height, (t) => height = t, totalHeight, lerpTime);
            tween.onUpdate = () =>
            {
                content.sizeDelta = new Vector2(content.sizeDelta.x, height);
            };
            tween.onComplete = () =>
            {
                CheckContentHeight(0);
            };
        }
        else
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);

            RefreshMinMaxIndex();
            RefreshScrollColumnList();
        }
    }

    /// <summary>
    /// 刷新可视范围index
    /// </summary>
    private void RefreshMinMaxIndex()
    {
        m_MinIndex = 0;
        m_MaxIndex = scrollColumnList.Count - 1;

        if (recycleWidth <= 0)
        {
            m_MinColumnCount = scrollColumnList.Count;
            m_MaxColumnCount = scrollColumnList.Count;
        }
        else
        {
            m_MinColumnCount = Mathf.CeilToInt(viewport.rect.height / recycleWidth);
            m_MaxColumnCount = m_MinColumnCount + columnCountDelta;

            if (currentIndex <= m_MinColumnCount / 2)
            {
                m_MaxIndex = m_MinColumnCount + (m_MaxColumnCount - m_MinColumnCount) / 2;
            }
            else if (currentIndex + m_MinColumnCount / 2 >= scrollColumnList.Count)
            {
                m_MinIndex = scrollColumnList.Count - m_MinColumnCount - (m_MaxColumnCount - m_MinColumnCount) / 2;
            }
            else
            {
                int num = m_MaxColumnCount / 2;
                m_MinIndex = currentIndex - num;
                m_MaxIndex = currentIndex + num;
            }
        }
    }

    private void RefreshScrollColumnIndex()
    {
        for (var i = 0; i < scrollColumnList.Count; i++)
        {
            scrollColumnList.ElementAt(i).index = i;
        }
    }

    public void DoPanelMove(int startIndex, int targetIndex, Action callback)
    {
        if (startIndex == targetIndex)
        {
            Log.Error("Can not move to the same index");
            return;
        }

        if (targetIndex < 0 || targetIndex >= scrollColumnList.Count)
        {
            Log.Error("Target index out of range");
            return;
        }

        if (startIndex == targetIndex)
        {
            Log.Error("Can not move to the same index");
            return;
        }

        StartCoroutine(DoPanelMoveAnim(startIndex, targetIndex, callback));
    }
    [NonSerialized]
    public bool isShowingAnim = false;

    IEnumerator DoPanelMoveAnim(int startIndex, int targetIndex, Action callback)
    {
        isShowingAnim = true;
        scrollRect.vertical = false;
        var targetColumn = scrollColumnList.ElementAt(targetIndex);
        var startColumn = scrollColumnList.ElementAt(startIndex);
        var curIndex = startIndex;

        var canvas = startColumn.Instance.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10;
        canvas.sortingLayerName = "UI";

        int index = 0;
        LinkedListNode<ScrollColumn> startNode = null;
        LinkedListNode<ScrollColumn> targetNode = null;
        var tmpNode =
            new LinkedListNode<ScrollColumn>(new LeaderBoardRankScrollColumn(this,
                GameManager.Task.PersonRankManager.LocalData, curIndex, recycleWidth, true));
        for (var node = scrollColumnList.First; node != null; node = node.Next, index++)
        {
            if (index == startIndex) startNode = node;
            if (index == targetIndex) targetNode = node;
        }

        scrollColumnList.AddBefore(startNode, tmpNode);
        scrollColumnList.Remove(startNode);
        var endPos = GetColumnLocalPosition(targetIndex);
        var startPos = startColumn.Instance.transform.localPosition;
        RefreshScrollColumnIndex();

        var duration = 0.15f;
        var ease = Ease.OutQuad;
        var colDiff = Mathf.Abs(targetIndex - startIndex);
        if (colDiff <= 3)
        {
            duration = 0.3f;
            ease = Ease.Linear;
        }
        else if (colDiff <= 7)
        {
            duration = 0.25f;
            ease = Ease.Linear;
        }

        yield return new WaitForSeconds(0.5f);
        // if (colDiff >= 3)
        // {
        //     CenterTheTargetColumn(targetIndex, duration * colDiff, ease);
        // }
        CenterTheTargetColumn(targetIndex, duration * colDiff, ease);
        var a = startColumn.Instance.transform.DOLocalMove(endPos, duration * colDiff).SetEase(ease);
        if (curIndex > targetIndex)
        {
            curIndex--;
            while (curIndex >= targetIndex)
            {
                var curColumn = scrollColumnList.ElementAt(curIndex);
                if (ReferenceEquals(curColumn.Instance, null))
                {
                    yield return null;
                    continue;
                }

                var nextPos = curColumn.Instance.transform.localPosition;
                var diff = 1 - (curIndex - targetIndex + 1f) / (colDiff + 1f);
                var t = ease == Ease.Linear ? duration : duration * diff + 0.07f * (1 - diff);
                var bAnim = curColumn.Instance.transform.DOLocalMove(startPos, t)
                    .SetEase(Ease.Linear);
                startPos = nextPos;
                startColumn.index = curIndex;
                startColumn.Refresh();
                curIndex--;
                GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                yield return bAnim.WaitForCompletion();
            }

            yield return a.WaitForCompletion();
            scrollColumnList.Remove(tmpNode);
            scrollColumnList.AddBefore(targetNode, startNode);
        }
        else
        {
            curIndex++;
            while (curIndex <= targetIndex)
            {
                var curColumn = scrollColumnList.ElementAt(curIndex);
                if (ReferenceEquals(curColumn.Instance, null))
                {
                    yield return null;
                    continue;
                }

                var nextPos = curColumn.Instance.transform.localPosition;
                var diff = 1 - (targetIndex - curIndex + 1f) / (colDiff + 1f);
                var t = ease == Ease.Linear ? duration : duration * diff + 0.07f * (1 - diff);
                var bAnim = curColumn.Instance.transform.DOLocalMove(startPos, t).SetEase(Ease.Linear);
                startPos = nextPos;
                startColumn.index = curIndex;
                startColumn.Refresh();
                curIndex++;
                GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
                yield return bAnim.WaitForCompletion();
            }
            yield return a.WaitForCompletion();
            scrollColumnList.Remove(tmpNode);
            scrollColumnList.AddAfter(targetNode, startNode);
        }

        RefreshScrollColumnIndex();
        Refresh();
        callback?.Invoke();
        isShowingAnim = false;
        scrollRect.vertical = true;

        canvas.overrideSorting = false;
    }

    private void ExchangeTwoColumn(int startIndex, int targetIndex, float duration, Ease ease = Ease.Linear)
    {
        var startColumn = scrollColumnList.ElementAt(startIndex);
        var targetColumn = scrollColumnList.ElementAt(targetIndex);
        int index = 0;
        LinkedListNode<ScrollColumn> startNode = null;
        LinkedListNode<ScrollColumn> targetNode = null;
        for (var node = scrollColumnList.First; node != null; node = node.Next, index++)
        {
            if (index == startIndex) startNode = node;
            if (index == targetIndex) targetNode = node;
        }

        var startPos = startColumn.Instance.transform.localPosition;
        var endPos = targetColumn.Instance.transform.localPosition;
        scrollColumnList.Remove(startNode);
        scrollColumnList.AddBefore(targetNode, startNode);
        startColumn.index = targetIndex;
        targetColumn.index = startIndex;
        // scrollRect.vertical = false;
        startColumn.Instance.transform.DOLocalMove(endPos, duration).SetEase(ease);
        targetColumn.Instance.transform.DOLocalMove(startPos, duration).SetEase(ease).onComplete = () =>
        {
            startColumn.Refresh();
            targetColumn.Refresh();
            // scrollRect.vertical = true;
        };
    }


    private void OnSpawn(ScrollColumn column)
    {
        m_OnSpawnAction?.Invoke(column);
    }

    private void OnUnspawn()
    {
        m_OnUnspawnAction?.Invoke();
    }
}