using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LevelPlayMenu_NormalArea : MonoBehaviour
{
    [SerializeField] private GameObject m_TargetIcon, m_Cross;
    [SerializeField] private Image m_Target1Image, m_Target2Image;
    [SerializeField] private TextMeshProUGUI m_TargetNumText, m_LeftNumText;

    public void Initialize(LevelPlayType type)
    {
        m_Target1Image.sprite = TileMatchUtil.GetTileSprite(2);
        m_Target2Image.sprite = TileMatchUtil.GetTileSprite(13);

        if (type == LevelPlayType.Play)
        {
            m_TargetNumText.gameObject.SetActive(true);
            InitTileTotalCount();
            m_LeftNumText.gameObject.SetActive(false);
            m_Cross.SetActive(false);

            m_TargetIcon.transform.localPosition = new Vector3(-52.5f, -7f, 0);
        }
        else if (type == LevelPlayType.Retry)
        {
            m_TargetNumText.gameObject.SetActive(false);
            m_LeftNumText.gameObject.SetActive(true);
            var panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
            if (panel != null)
            {
                var surplusNum = panel.SurplusNum;
                m_LeftNumText.text = $"<color=#F74243>{surplusNum.Item2 - surplusNum.Item1}</color> / {surplusNum.Item2}";
            }
            m_Cross.SetActive(true);

            m_TargetIcon.transform.localPosition = new Vector3(-52.5f, 20f, 0);
        }

        gameObject.SetActive(true);
    }

    public void Release()
    {
        gameObject.SetActive(false);
    }

    private void InitTileTotalCount()
    {
        int level = GameManager.PlayerData.RealLevel();
        int savedTileTotalCount = GameManager.DataNode.GetData<int>("TileTotalCount" + level.ToString(), 0);
        if (savedTileTotalCount == 0)
        {
            savedTileTotalCount = TileMatchPanelData.GetNowLevelTileTotalCount();
            GameManager.DataNode.SetData<int>("TileTotalCount" + level.ToString(), savedTileTotalCount);
        }

        m_TargetNumText.text = savedTileTotalCount.ToString();
    }

    public bool CheckInitComplete()
    {
        return true;
    }
}
