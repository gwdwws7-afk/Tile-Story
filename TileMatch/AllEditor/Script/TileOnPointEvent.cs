using UnityEngine;
using UnityEngine.UI;

public class TileOnPointEvent : UnityOnPointEvent
{
	private Image attachmentImg;
	public Image directionImg;

    public override void SetColor(Color color)
    {
        base.SetColor(color);

		if (attachmentImg != null)
			attachmentImg.color = color;
	}

    public void SetAttachSprite(Sprite sprite)
	{
		if (attachmentImg == null)
		{
			attachmentImg = transform.GetChild(0).GetComponent<Image>();
		}
		if (attachmentImg != null)
        {
            if (sprite == null)
            {
				attachmentImg.gameObject.SetActive(false);
			}
            else
            {
				attachmentImg.sprite = sprite;
				attachmentImg.gameObject.SetActive(true);
			}
		}
	}

	public void SetDirectionSprite(Sprite sprite)
    {
		if (directionImg == null)
		{
			directionImg = transform.GetChild(1).GetComponent<Image>();
		}
		if (directionImg != null)
		{
			if (sprite == null)
			{
				directionImg.gameObject.SetActive(false);
			}
			else
			{
				directionImg.sprite = sprite;
				directionImg.gameObject.SetActive(true);
			}
		}
	}
}
