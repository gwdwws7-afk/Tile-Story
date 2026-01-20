using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggerTrigger : MonoBehaviour
{
    int m_DebugClickTime;

    public void TriggerClick()
    {
        m_DebugClickTime++;
        if (m_DebugClickTime < 11)
        {
            return;
        }
        GameManager.PlayerData.HasOpenDebug = true;
        GameManager.GetGameComponent<DebuggerComponent>().ActiveWindow = true;
    }
}
