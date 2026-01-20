using System;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public sealed class OilDrum : AttachItem
{
    public Button button;

    public GameObject image;
    public SkeletonGraphic spine;
    public ParticleSystem collectEffect;
    public GameObject trailEffect;
    
    public bool isBeCover;
    private bool isCollected = false;
    private Action overAction; 
    
    public override void Init(AttachLogic logic)
    {
        image.SetActive(false);
        spine.gameObject.SetActive(false); 
        
        button.onClick.AddListener(() =>
        {
            if (!isBeCover)
            {
                BtnEvent();
            }
        });

        base.Init(logic);
    }
    
    public override void SetColor(bool isBeCover)
    {
        this.isBeCover = isBeCover;
        button.enabled = !isBeCover;
        try
        {
            if (!isBeCover)
            {
                image.SetActive(false);
                spine.gameObject.SetActive(true);
                GameManager.Sound.PlayAudio(SoundType.SFX_Collection_OilDrum_Appear.ToString());
                spine.AnimationState.TimeScale = 1.3f;
                spine.AnimationState.SetAnimation(0,"appear",false).Complete += (s) =>
                {
                    spine.AnimationState.TimeScale = 1f;
                    spine.AnimationState.SetAnimation(0, "idle", true);
                };
            }
            else
            {
                image.SetActive(true);
                spine.gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    
    public override void Release()
    {
        isBeCover = false;
        isCollected = false;
        overAction = null;
        button.enabled = true;
        base.Release();
    }
    
    public override void OnClick()
    {
        BtnEvent();
    }
    
    private void BtnEvent()
    {
        if (isCollected)
        {
            return;
        }
        
        //收集
        int count = GameManager.DataNode.GetData("OilDrumCollectNum", 0);
        GameManager.DataNode.SetData("OilDrumCollectNum",count+1);
        isCollected = true;
        button.enabled = false;
        GameManager.Sound.PlayAudio(SoundType.SFX_Collection_OilDrum_Fly.ToString()); 
        
        image.SetActive(false);
        spine.AnimationState.TimeScale = 0;
        // TileMatchPanel tileMatchPanel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        // GameGasolineBar oilDrumBar = tileMatchPanel.Gasoline_Bar;
        // PlayAnim(oilDrumBar.transform.position, () =>
        // {
        //     GameManager.Sound.PlayAudio(SoundType.SFX_Collection_OilDrum.ToString());
        //     //飞行结束
        //     oilDrumBar.RefreshGasolineText();
        //     oilDrumBar.PunchGasolineBar();
        //     
        //     gameObject.SetActive(false);
        //     overAction?.InvokeSafely();
        //     overAction = null;
        // });
    }

    public override void SetDisappearAction(Action action)
    {
        overAction = action;
        base.SetDisappearAction(action);
    }

    public void PlayAnim(Vector3 targetPos, Action completeAction)
    {
        float offsetY = targetPos.y - transform.position.y;
        transform.DOScale(1.4f, 0.2f).onComplete = () =>
        {
            collectEffect.gameObject.SetActive(true);
            collectEffect.Play();
            transform.DOScale(1.3f, 0.05f).onComplete = () =>
            {
                trailEffect.SetActive(true);
                transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.25f).SetEase(Ease.InQuad);
                //transform.DOLocalRotate(new Vector3(0, 0, -100), 0.25f).SetEase(Ease.InQuad);
                transform.DOMoveY(transform.position.y + 0.07f, 0.15f).onComplete = () =>
                {
                    transform.DOMoveY(transform.position.y - 0.07f, 0.1f).onComplete = () =>
                    {
                        transform.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 0.3f).SetEase(Ease.InOutQuad);
                        transform.DOLocalRotate(new Vector3(0, 0, -170), 0.3f).SetEase(Ease.InOutQuad);
                        transform.DOJump(targetPos, -0.5f * offsetY, 1, 0.3f).SetEase(Ease.InOutQuad).onComplete = () =>
                        {
                            trailEffect.SetActive(false);
                            transform.DOScale(0, 0.1f).onComplete = () =>
                            {
                                collectEffect.gameObject.SetActive(false);
                                completeAction?.Invoke();
                            };
                        };
                    };
                };
            };
        };
    }
}
