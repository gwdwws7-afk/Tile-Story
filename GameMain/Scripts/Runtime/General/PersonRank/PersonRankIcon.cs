using GameFramework.Event;
using MySelf.Model;
using TMPro;
using UnityEngine;

public class PersonRankIcon : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    private void OnEnable()
    {
        GameManager.Event.Subscribe(TileMatchDestroyEventArgs.EventId, OnTileMatchDestroy);
        RefreshText();
    }

    private void OnDisable()
    {
        GameManager.Event.Unsubscribe(TileMatchDestroyEventArgs.EventId, OnTileMatchDestroy);
    }

    private void OnTileMatchDestroy(object sender, GameEventArgs e)
    {
    }

    private void RefreshText()
    {
        scoreText.text = $"+{PersonRankModel.Instance.ScoreInGame}";
    }
}
