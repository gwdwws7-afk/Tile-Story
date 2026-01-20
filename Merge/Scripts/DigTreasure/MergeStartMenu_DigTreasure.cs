using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merge
{
    public class MergeStartMenu_DigTreasure : MergeStartMenu
    {
        public GameObject m_PreviewText, m_StartText;

        public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
        {
            base.OnInit(uiGroup, completeAction, userData);

            bool isUnlock = GameManager.PlayerData.NowLevel >= MergeManager.PlayerData.GetActivityUnlockLevel();

            m_PreviewText.gameObject.SetActive(!isUnlock);
            m_StartText.gameObject.SetActive(isUnlock);
        }
    }
}
