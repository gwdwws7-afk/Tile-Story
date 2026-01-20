using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LevelPlayMenu_HiddenTempleArea : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_PickaxeCollectNumText;
    [SerializeField] private Button m_TipButton;
    [SerializeField] private GameObject m_ShineImg;
    [SerializeField] private List<CanvasGroup> m_LoseBarSlotList;
    [SerializeField] private List<CanvasGroup> m_CurBarSlotList;

    public void Initialize(LevelPlayType type)
    {
        int stage = HiddenTemple.HiddenTempleManager.PlayerData.GetPickaxeWinStreakStage();

        if(stage > 0) m_PickaxeCollectNumText.text = stage.ToString();
        m_PickaxeCollectNumText.gameObject.SetActive(stage > 0);
        m_ShineImg.SetActive(stage > 0);

        for (int i = 0; i < m_CurBarSlotList.Count; i++)
        {
            m_LoseBarSlotList[i].gameObject.SetActive(false);
            m_CurBarSlotList[i].gameObject.SetActive(i == stage - 1);
            m_CurBarSlotList[i].alpha = 1;
        }

        m_TipButton.SetBtnEvent(() =>
        {
            GameManager.UI.ShowUIForm("HiddenTempleHowToPlayMenu");
        });

        gameObject.SetActive(true);

        if (type == LevelPlayType.Retry && stage > 1) 
        {
            HiddenTemple.HiddenTempleManager.PlayerData.ClearPickaxeWinStreakStage();

            ShowLoseAnim(stage);
        }
    }

    public void Release()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    private void ShowLoseAnim(int stage)
    {
        float fadeTime = 0.12f;
        m_CurBarSlotList[stage - 1].DOFade(0, fadeTime).SetDelay(0.1f + fadeTime);

        for (int i = stage - 2; i > 0; i--) 
        {
            int index = i;
            GameManager.Task.AddDelayTriggerTask(0.1f + fadeTime * 2 * (stage - 1 - index), () =>
                   {
                       m_CurBarSlotList[index].alpha = 0;
                       m_CurBarSlotList[index].gameObject.SetActive(true);
                       m_CurBarSlotList[index].DOFade(1, fadeTime).onComplete = () =>
                       {
                           m_CurBarSlotList[index].DOFade(0, fadeTime);
                       };
                   });
        }

        GameManager.Task.AddDelayTriggerTask(0.1f + fadeTime * 2 * (stage - 1), () =>
           {
               m_CurBarSlotList[0].alpha = 0;
               m_CurBarSlotList[0].gameObject.SetActive(true);
               m_CurBarSlotList[0].DOFade(1, fadeTime);
               m_PickaxeCollectNumText.text = "1";
           });
    }
}
