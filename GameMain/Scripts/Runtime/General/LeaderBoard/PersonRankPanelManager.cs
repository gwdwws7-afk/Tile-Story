using System;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PersonRankPanelManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText, scoreText, coinText;
    [SerializeField] private NamePanelManager nameText;

    [SerializeField] private Image head, bg, rankImg, chest, cup;

    [SerializeField] private DelayButton rewardButton, arrowButton;
    [SerializeField] private GameObject upImg, coinImg;

    private AsyncOperationHandle _assetHandle1, _assetHandle2, _assetHandle3, _assetHandle4, _assetHandle5;

    public void InitPanel(PersonRankData data, int index = -1)
    {
        transform.localScale = Vector3.one;
        rankText.text = index != -1 ? index.ToString() : data.Rank.ToString();
        nameText.OnInit(data.Name);
        scoreText.text = data.Score.ToString();
        SetHead(data.HeadId, data.IsSelf());
        SetRankImg(data.Rank);
        SetChest(data.Rank);
        SetCup();
        upImg.SetActive(data.Rank <= GameManager.Task.PersonRankManager.UpRange);
        rewardButton.SetBtnEvent(() =>
        {
            GameManager.Event.Fire(this,
                CommonEventArgs.Create(CommonEventType.ShowPersonRankRewardPromptBox, data.Rank,
                    PromptBoxShowDirection.Up, rewardButton.transform.position - new Vector3(0, 0.05f, 0)));
        });
        arrowButton.SetBtnEvent(() =>
        {
            GameManager.Event.Fire(this,
                CommonEventArgs.Create(CommonEventType.ShowPersonRankTextPromptBox, PromptBoxShowDirection.Up,
                    arrowButton.transform.position - new Vector3(0, 0.05f, 0)));
        });
    }

    public void UpdatePanel(int rank)
    {
        rankImg.gameObject.SetActive(rank <= 3 && rank > 0);
        rankText.text = rank.ToString();
    }

    private void SetRankImg(int rank)
    {
        if (rank <= 0 || rank > 3)
        {
            rankImg.gameObject.SetActive(false);
            return;
        }

        var bgName = $"LeaderBoard[Rank{rank}]";
        //rankImg.sprite = null;
        UnityUtility.UnloadAssetAsync(_assetHandle3);
        _assetHandle3 = UnityUtility.LoadAssetAsync<Sprite>(bgName, s =>
        {
            rankImg.sprite = s;
            rankImg.gameObject.SetActive(true);
        });
    }

    private void SetHead(int headPortrait = 0, bool isSelf = false)
    {
        var headPortraitName = isSelf ? $"HeadPortrait_{headPortrait}_{headPortrait}" : $"HeadPortrait_{headPortrait}";
        if (headPortrait == 0)
        {
            headPortraitName = "HeadPortrait_0_0";
        }

        //head.sprite = null;
        UnityUtility.UnloadAssetAsync(_assetHandle1);
        _assetHandle1 = UnityUtility.LoadAssetAsync<Sprite>(headPortraitName, s => { head.sprite = s; });
    }

    private void SetChest(int rank)
    {
        if (rank <= 0 || rank > 10)
        {
            chest.gameObject.SetActive(false);
            var rewardDic =
                GameManager.Task.PersonRankManager.TaskData.GetRewardsByLevel(
                    GameManager.Task.PersonRankManager.RankLevel,
                    rank);
            coinImg.SetActive(true);
            coinText.text = rewardDic[TotalItemData.Coin].ToString();
            return;
        }

        coinImg.SetActive(false);

        var rankLevel = rank switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            _ => 4
        };

        var bgName = $"LeaderBoard[Chest{rankLevel}]";

        //chest.sprite = null;
        UnityUtility.UnloadAssetAsync(_assetHandle4);
        _assetHandle4 = UnityUtility.LoadAssetAsync<Sprite>(bgName, s =>
        {
            chest.sprite = s;
            chest.gameObject.SetActive(true);
        });
    }

    private void SetCup()
    {
        //cup.sprite = null;
        UnityUtility.UnloadAssetAsync(_assetHandle5);
        var cupName = $"rankCup{(int)GameManager.Task.PersonRankManager.RankLevel + 1}";
        _assetHandle5 = UnityUtility.LoadSpriteAsync(cupName, "LeaderBoard", s =>
        {
            cup.sprite = s;
            cup.gameObject.SetActive(true);
        });
    }

    public void OnReset()
    {
        UnityUtility.UnloadAssetAsync(_assetHandle1);
        UnityUtility.UnloadAssetAsync(_assetHandle2);
        UnityUtility.UnloadAssetAsync(_assetHandle3);
        UnityUtility.UnloadAssetAsync(_assetHandle4);
        UnityUtility.UnloadAssetAsync(_assetHandle5);
    }

    private void OnDestroy()
    {
        try
        {
            OnReset();
            head.sprite = null;
            bg.sprite = null;
            rankImg.sprite = null;
            chest.sprite = null;
            cup.sprite = null;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}