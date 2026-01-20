using DG.Tweening;
using System;
using UnityEngine;

public class TilePassGuideMenu : UIForm
{
    public CommonGuideImage GuideBg;
    public TextPromptBox textPromptBox;
    public Transform npc;
    public Transform hand;
    public bool showBlackBg = true;

    public override void OnReset()
    {
        GameManager.DataNode.SetData("GuideBlock", 0);

        GuideBg.gameObject.SetActive(true);
        GuideBg.DOKill();
        GuideBg.color = new Color(1, 1, 1, 0);
        GuideBg.OnReset();

        textPromptBox.gameObject.SetActive(false);
        hand.gameObject.SetActive(false);
        npc.gameObject.SetActive(false);

        base.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(true);
        Input.multiTouchEnabled = true;
        if (showBlackBg)
        {
            GuideBg.DOFade(1f, 0.2f);
        }
        GameManager.DataNode.SetData("GuideBlock", 1);

        base.OnShow(showSuccessAction, userData);
    }

    public override void OnHide(Action hideSuccessAction = null, object userData = null)
    {
        gameObject.SetActive(false);
        Input.multiTouchEnabled = false;

        base.OnHide(hideSuccessAction, userData);
    }

    public virtual void HideGuideImage()
    {
        GuideBg.DOKill();
        GuideBg.raycastAll = true;
        GuideBg.color = new Color(1, 1, 1, 0);
        //GuideBg.raycastTarget = false;
    }

    public virtual void SetText(string content)
    {
        textPromptBox.SetText(content);
    }

    public virtual void ShowNpc(float posY)
    {
        npc.SetPositionY(posY);
        npc.gameObject.SetActive(true);

        textPromptBox.ShowPromptBox(PromptBoxShowDirection.Right, npc.position);
    }

    public virtual void Showhand(float posX, float posY)
    {
        hand.SetAsLastSibling();
        hand.SetPositionX(posX);
        hand.SetPositionY(posY);
        hand.gameObject.SetActive(true);
    }

    public virtual void HideHand()
    {
        hand.gameObject.SetActive(false);
    }
}
