using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class LocalDataPanelManager:MonoBehaviour
{
    public DelayButton useButton;
    public TextMeshProUGUI levelNum;
    public TextMeshProUGUI coinNum;

    public void OnInit(bool flag)
    {
        if (flag)
        {
            levelNum.text = GameManager.PlayerData.NowLevel.ToString();
            coinNum.text = GameManager.PlayerData.CoinNum.ToString();
            AutoLevelFontSize(GameManager.PlayerData.CoinNum);

        }
    }

    private void AutoLevelFontSize(int num)
    {
        if (num >= 100000 && num < 1000000)
        {
            coinNum.fontSize = 50;
        }else if(num >=1000000 && num < 10000000)
        {
            coinNum.fontSize = 42;
        }
        else if(num >= 10000000)
        {
            coinNum.fontSize = 36;
        }
        else
        {
            coinNum.fontSize = 60;
        }
    }

    public void OnReset()
    {
        useButton.OnReset();
    }

}
