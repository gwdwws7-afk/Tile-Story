using DG.Tweening;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public sealed class Gold : AttachItem
{
    public Image goldImage;
    private AsyncOperationHandle m_SpriteHandle;

    public override void SetSprite(int state)
    {
        if (state >= 5)
        {
            Hide();
            Release();
            return;
        }
        m_SpriteHandle = UnityUtility.LoadSpriteAsync(state.ToString(), "GoldAttachment", sp =>
        {
            goldImage.sprite = sp;
            //goldImage.SetNativeSize();
        });
    }

    public override void SetColor(bool isBeCover)
    {
        if (!isBeCover)
        {
            goldImage.DOColor(Color.white, 0.2f);
        }
        else
        {
            goldImage.DOKill();
            goldImage.color = isBeCover ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
        }
    }

    public override void Init(AttachLogic logic)
    {
        m_SpriteHandle = UnityUtility.LoadSpriteAsync("1", "GoldAttachment", sp =>
        {
            goldImage.sprite = sp;
        });

        base.Init(logic);
    }

    public override void Release()
    {
        UnityUtility.UnloadAssetAsync(m_SpriteHandle);
        m_SpriteHandle = default;

        base.Release();
    }
}
