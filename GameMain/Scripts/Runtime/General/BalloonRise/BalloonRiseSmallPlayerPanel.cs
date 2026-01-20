using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class BalloonRiseSmallPlayerPanel : MonoBehaviour
{
    public GameObject other;
    public GameObject self;
    public NamePanelManager otherName;
    public NamePanelManager selfName;
    public Image avatar;
    public Image rankImg;
    public TextMeshProUGUI rank;
    public TextMeshProUGUI score;

    private AsyncOperationHandle m_AssetHandle1, m_AssetHandle2;

    public void OnInit(BalloonRisePlayerBase data)
    {
        if (data.IsSelf)
        {
            other.SetActive(false);
            self.SetActive(true);
            otherName.gameObject.SetActive(false);
            selfName.gameObject.SetActive(true);
            selfName.OnInit(data.Name);
            score.color = new Color(0, 0.3f, 0);
        }
        else
        {
            other.SetActive(true);
            self.SetActive(false);
            otherName.gameObject.SetActive(true);
            selfName.gameObject.SetActive(false);
            otherName.OnInit(data.Name);
            score.color = new Color(0.53f, 0.3f, 0.18f);
        }
        SetAvatar(data.Avatar);
        // SetRank(data.Rank);
        // rank.text = data.Rank.ToString();
        score.text = $"{data.Score}/{GameManager.Task.BalloonRiseManager.StageTarget}";
    }

    private void SetAvatar(int headId)
    {
        string avatarSp = headId == 0 ? "HeadPortrait_0_0" : $"HeadPortrait_{headId}";
        avatar.sprite = null;
        UnityUtility.UnloadAssetAsync(m_AssetHandle1);
        m_AssetHandle1 = UnityUtility.LoadAssetAsync<Sprite>(avatarSp, sp =>
        {
            avatar.sprite = sp;
        });
    }

    private void SetRank(int rank)
    {
        string rankSp;
        switch (rank)
        {
            case 1:
                rankSp = "名次1";
                break;
            case 2:
                rankSp = "名次2";
                break;
            case 3:
                rankSp = "名次3";
                break;
            default:
                rankSp = "名次4";
                break;
        }
        rankImg.sprite = null;
        UnityUtility.UnloadAssetAsync(m_AssetHandle2);
        m_AssetHandle2 = UnityUtility.LoadAssetAsync<Sprite>($"LevelBanner[{rankSp}]", sp =>
        {
            rankImg.sprite = sp;
        });
    }

    public void OnReset()
    {
        UnityUtility.UnloadAssetAsync(m_AssetHandle1);
        UnityUtility.UnloadAssetAsync(m_AssetHandle2);
        m_AssetHandle1 = default;
        m_AssetHandle2 = default;
    }
}
