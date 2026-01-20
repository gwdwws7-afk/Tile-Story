using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BalloonRiseSettlementPanel : MonoBehaviour
{
    public Image black;
    public GameObject winRoot, failRoot;
    public Transform winText1, winText2, failText1, failText2;
    public Image chest;
    public GameObject winEffect;
    public Transform[] rankIcons;
    public Image[] playerAvatars;
    public NamePanelManager[] playerNames;

    private bool delaySetAvatar = false;
    private BalloonRisePlayerPanel[] panels;
    
    public void Init(BalloonRisePlayerPanel[] playerPanels)
    {
        panels = playerPanels;
        
        if (playerPanels[0].avatar.sprite != null)
        {
            for (int i = 0; i < playerPanels.Length; i++)
            {
                playerAvatars[i].sprite = playerPanels[i].avatar.sprite;
            }   
        }
        else
        {
            delaySetAvatar = true;
            for (int i = 0; i < playerPanels.Length; i++)
            {
                playerAvatars[i].color = new Color(1, 1, 1, 0);
            }  
        }
        
        playerNames[0].OnInit(GameManager.Task.BalloonRiseManager.RobotPlayers[0].Name);
        playerNames[1].OnInit(GameManager.Task.BalloonRiseManager.RobotPlayers[1].Name);
        playerNames[2].OnInit(GameManager.Task.BalloonRiseManager.SelfPlayer.Name);
        playerNames[3].OnInit(GameManager.Task.BalloonRiseManager.RobotPlayers[2].Name);
        playerNames[4].OnInit(GameManager.Task.BalloonRiseManager.RobotPlayers[3].Name);
        
        SetRankIcon(0, GameManager.Task.BalloonRiseManager.RobotPlayers[0].Rank);
        SetRankIcon(1, GameManager.Task.BalloonRiseManager.RobotPlayers[1].Rank);
        SetRankIcon(2, GameManager.Task.BalloonRiseManager.SelfPlayer.Rank);
        SetRankIcon(3, GameManager.Task.BalloonRiseManager.RobotPlayers[2].Rank);
        SetRankIcon(4, GameManager.Task.BalloonRiseManager.RobotPlayers[3].Rank);
    }

    private void SetRankIcon(int index, int rank)
    {
        rankIcons[rank - 1].transform.position = new Vector3(playerAvatars[index].transform.position.x,
            rankIcons[rank - 1].transform.position.y, 0);
    }

    private void Update()
    {
        if (delaySetAvatar)
        {
            if (panels[0].avatar.sprite == null)
                return;
            
            for (int i = 0; i < panels.Length; i++)
            {
                playerAvatars[i].sprite = panels[i].avatar.sprite;
                playerAvatars[i].color = Color.white;
            }

            delaySetAvatar = false;
        }
    }
}
