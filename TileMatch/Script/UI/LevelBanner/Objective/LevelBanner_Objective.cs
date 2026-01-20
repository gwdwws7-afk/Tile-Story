using DG.Tweening;
using UnityEngine;

/// <summary>
/// 成就关卡横幅
/// </summary>
public sealed class LevelBanner_Objective : LevelBannerBase
{
    public ObjectiveLevelColumn m_ObjectiveColumnPrefab;

    private ObjectiveLevelColumn m_CurObjectiveColumn;

    protected override void OnInitialize()
    {
        bool isAllTimeObjective = false;
        bool isCompleted = true;
        var data = GameManager.Objective.GetDailyCompletedObjectiveData();
        if (data == null)
        {
            data = GameManager.Objective.GetAllTimeCompletedObjectiveData();
            isAllTimeObjective = true;
        }

        if (data == null)
        {
            data = GameManager.Objective.GetAllTimeFirstObjectiveData();
            isCompleted = false;
        }

        if (data != null)
        {
            if (m_CurObjectiveColumn == null)
            {
                m_CurObjectiveColumn = Instantiate(m_ObjectiveColumnPrefab, transform).GetComponent<ObjectiveLevelColumn>();
            }

            m_CurObjectiveColumn.OnInitialize(data, isAllTimeObjective, isCompleted, this);
            m_CurObjectiveColumn.gameObject.SetActive(true);
        }
    }

    protected override void OnRelease()
    {
        if (m_CurObjectiveColumn != null)
        {
            m_CurObjectiveColumn.DOKill();
            m_CurObjectiveColumn.OnRelease();
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.D))
    //    {
    //        m_CurObjectiveColumn.m_ClaimButton.onClick.Invoke();
    //    }
    //}

    public void RefreshObjectiveColumn()
    {
        ObjectiveLevelColumn newColumn = null;
        bool isAllTimeObjective = false;
        bool isCompleted = true;
        var data = GameManager.Objective.GetDailyCompletedObjectiveData();
        if (data == null)
        {
            data = GameManager.Objective.GetAllTimeCompletedObjectiveData();
            isAllTimeObjective = true;
        }

        if (data == null)
        {
            data = GameManager.Objective.GetAllTimeFirstObjectiveData();
            isCompleted = false;
        }

        if (data != null)
        {
            newColumn = Instantiate(m_ObjectiveColumnPrefab, transform).GetComponent<ObjectiveLevelColumn>();
            newColumn.OnInitialize(data, isAllTimeObjective, isCompleted, this);
            newColumn.transform.localPosition = new Vector3(-1080f, newColumn.transform.localPosition.y, 0);
            newColumn.gameObject.SetActive(true);
        }

        if (m_CurObjectiveColumn != null && newColumn != null) 
        {
            var lastColumn = m_CurObjectiveColumn;

            lastColumn.transform.DOScale(new Vector3(0.96f, 1.04f, 0), 0.1f);
            lastColumn.transform.DOLocalMoveX(-20, 0.1f).onComplete = () =>
            {
                lastColumn.transform.DOScale(new Vector3(1.04f, 0.96f, 0), 0.1f);
                lastColumn.transform.DOLocalMoveX(1080, 0.3f).SetEase(Ease.InOutQuad);
            };

            newColumn.transform.localScale = new Vector3(1.04f, 0.96f, 0);
            newColumn.transform.DOScale(new Vector3(0.96f, 1.04f, 0), 0.1f).SetDelay(0.3f);
            newColumn.transform.DOLocalMoveX(20, 0.3f).SetEase(Ease.InOutQuad).SetDelay(0.1f).onComplete = () =>
            {
                newColumn.transform.DOScale(Vector3.one, 0.1f);
                newColumn.transform.DOLocalMoveX(0, 0.1f);
            };

            m_CurObjectiveColumn = newColumn;
        }
    }
}
