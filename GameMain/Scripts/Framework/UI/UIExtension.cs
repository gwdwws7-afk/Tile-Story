using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI扩展类
/// </summary>
public static class UIExtension
{
    public static void ShowWeakHint(this GameUIComponent uiComponent, string content, params string[] args)
    {
        string[] temp = args;
        uiComponent.ShowUIForm("WeakHintMenu",form =>
        {
            WeakHintMenu weakHintMenu = form.GetComponent<WeakHintMenu>();
            weakHintMenu.SetHintText(content, Camera.main.ViewportToScreenPoint(new Vector3(0, 0.15f)), temp);
            weakHintMenu.OnShow();
        });
    }

    public static void ShowWeakHint(this GameUIComponent uiComponent, string content, Vector3 startPos, params string[] args)
    {
        string[] temp = args;
        uiComponent.ShowUIForm("WeakHintMenu",form =>
        {
            WeakHintMenu weakHintMenu = form.GetComponent<WeakHintMenu>();
            weakHintMenu.SetHintText(content, Camera.main.WorldToScreenPoint(startPos), temp);
            weakHintMenu.OnShow();
        });
    }

    public static void ShowWeakHint(this GameUIComponent uiComponent, string content, bool disableBG, params string[] args)
    {
        string[] temp = args;
        uiComponent.ShowUIForm("WeakHintMenu",form =>
        {
            WeakHintMenu weakHintMenu = form.GetComponent<WeakHintMenu>();
            weakHintMenu.SetHintText(content, Camera.main.ViewportToScreenPoint(new Vector3(0, 0.15f)), temp);
            weakHintMenu.OnShow();
            if (disableBG)
            {
                weakHintMenu.DisableBg();
            }
        });
    }
}
