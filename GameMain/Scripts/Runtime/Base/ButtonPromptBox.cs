using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonPromptBox : PromptBox
{
    public DelayButton kickButton;


    public void Init(UnityAction action)
    {
        kickButton.onClick.RemoveAllListeners();
        kickButton.onClick.AddListener(action);
    }

    public override void HidePromptBox()
    {
        //kickButton.onClick.RemoveAllListeners();
        base.HidePromptBox();
    }
}
