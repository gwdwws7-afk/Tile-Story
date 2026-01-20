using UnityEngine;

public class PersonRankDoublePanelManager : MonoBehaviour
{
    public GameObject[] CurrentMultObjects, NormalMultObjects;

    private void OnEnable()
    {
        var index = GameManager.Task.PersonRankManager.ContinuousWinTime;
        for (var i = 0; i < CurrentMultObjects.Length; i++)
        {
            CurrentMultObjects[i].SetActive(i == index);
            NormalMultObjects[i].SetActive(i != index);
        }
    }
}
