using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Ice : AttachItem
{
    public Image iceImg;

    public override void Show()
    {
        if (!gameObject.activeSelf)
        {
            iceImg.color = new Color(iceImg.color.r, iceImg.color.g, iceImg.color.b, 0);
            iceImg.DOFade(1, 0.2f);
        }

        base.Show();
    }

    public override void SetColor(bool isBeCover)
    {
		if (!isBeCover)
		{
			iceImg.DOColor(Color.white, 0.2f);
		}
		else
		{
            iceImg.DOKill();
            iceImg.color = isBeCover ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
		}
	}
}
