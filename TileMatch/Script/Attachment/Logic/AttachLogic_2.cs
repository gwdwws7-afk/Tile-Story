using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ½ºË®
/// </summary>
public sealed class AttachLogic_2 : AttachLogic
{
    public override int AttachId => 2;

    public override string AttachAssetName => "Glue";

    private AttachLogic_2 bindAttachment;
    private bool isLeftGlue;

    public AttachLogic_2 BindAttachment => bindAttachment;

    protected override void OnInit(object userData)
    {
        TileItem.State |= TileItemState.DisableClick;
        TileItem.GetComponent<TileDelayButton>().DisableAnim = true;
    }

    protected override void OnRelease(bool showEffect)
    {
        TileItem.State &= ~TileItemState.DisableClick;
        TileItem.GetComponent<TileDelayButton>().DisableAnim = false;

        if (bindAttachment != null)
        {
            AttachLogic_2 bind = bindAttachment;
            bindAttachment = null;
            bind.Release(false);
        }
    }

    public override void Show()
    {
        if (TileItem == null)
            return;

        if (bindAttachment == null || bindAttachment.TileItem == null) 
        {
            if (!SearchBindAttachment())
            {
                Log.Error("Glue bind fail!");
                Release(false);
                return;
            }

            TileItem.SetColor();
        }

        bool isShowMainGlue = TileItem.Data.MapID > bindAttachment.TileItem.Data.MapID;

        Glue glue = (Glue)AttachItem;
        System.Action callback = () =>
          {
              if (glue != null && AttachItem != null) 
              {
                  glue.isLeftGlue = isLeftGlue;
                  glue.m_GlueMain.gameObject.SetActive(isShowMainGlue);
                  SetColor(false);
                  AttachItem.Show();
              }
          };

        if (AttachItem != null)
        {
            callback();
        }
        else
        {
            SpawnComplete += res =>
            {
                callback();
            };
        }
    }

    public override void SetColor(bool isBeCover)
    {
        SetGlueColor();
        bindAttachment?.SetGlueColor();
    }

    public void SetGlueColor()
    {
        //bool isBeCover = false;
        //if (TileItem != null && bindAttachment != null && bindAttachment.TileItem != null)
        //{
        //    isBeCover = TileItem.IsBeCover && bindAttachment.TileItem.IsBeCover;
        //}

        //if (AttachItem != null)
        //{
        //    AttachItem.SetColor(isBeCover);
        //}
        //else
        //{
        //    spawnInstanceCompleteAction += res =>
        //    {
        //        res.SetColor(isBeCover);
        //    };
        //}

        bool isBeCover = false;
        if (TileItem != null && bindAttachment != null && bindAttachment.TileItem != null)
        {
            isBeCover = TileItem.IsBeCover || bindAttachment.TileItem.IsBeCover;
        }

        if (AttachItem != null)
        {
            AttachItem.SetColor(isBeCover);
        }
        else
        {
            SpawnComplete += res =>
            {
                res.SetColor(isBeCover);
            };
        }
    }

    public override void OnClick()
    {
        base.OnClick();

        TileMatchPanel panel = null;
        if (GameManager.UI != null)
        {
            panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
            if (panel != null && panel.gameObject.activeInHierarchy && panel.CheckIsLose())
                return;
        }

        if (TileItem != null && bindAttachment != null && bindAttachment.TileItem != null)
        {
            if (!TileItem.IsBeCover && !bindAttachment.TileItem.IsBeCover) 
            {
                TileItem item1 = TileItem;
                TileItem item2 = bindAttachment.TileItem;
                TileItem.SetClickState(false);
                bindAttachment.TileItem.SetClickState(false);

                if (TileItem.Data.MapID > bindAttachment.TileItem.Data.MapID) 
                {
                    bindAttachment.TileItem.transform.SetAsLastSibling();
                    TileItem.transform.SetAsLastSibling();
                }
                else
                {
                    TileItem.transform.SetAsLastSibling();
                    bindAttachment.TileItem.transform.SetAsLastSibling();
                }

                Glue glue = AttachItem as Glue;
                Glue glue2 = bindAttachment.AttachItem as Glue;
                if (glue != null && glue2 != null) 
                {
                    if (panel != null)
                    {
                        panel.isCheckGameLostOver = false;
                    }

                    glue.ShowSeparateAnim(item1.transform, () =>
                    {
                        Release(false);
                        if (panel != null)
                        {
                            //bool originValue = panel.isCheckGameLostOver;
                            //panel.isCheckGameLostOver = false;
                            if (isLeftGlue)
                            {
                                item1.ClickEvent?.Invoke(item1);
                                item2.ClickEvent?.Invoke(item2);
                            }
                            else
                            {
                                item2.ClickEvent?.Invoke(item2);
                                item1.ClickEvent?.Invoke(item1);
                            }
                            panel.isCheckGameLostOver = true;
                            panel.CheckLose();
                        }
                        else
                        {
                            if (isLeftGlue)
                            {
                                item1.ClickEvent?.Invoke(item1);
                                item2.ClickEvent?.Invoke(item2);
                            }
                            else
                            {
                                item2.ClickEvent?.Invoke(item2);
                                item1.ClickEvent?.Invoke(item1);
                            }
                        }
                    });
                    glue2.ShowSeparateAnim(item2.transform, null);

                    Transform cachedTrans = TileItem.transform;
                    Vector3 targetPos = glue.m_GlueMain.transform.position;

                    float startTime = Time.time;
                    GameManager.ObjectPool.Spawn<EffectObject>("Spine_Attachment_Glue", "TileItemDestroyEffectPool", targetPos, cachedTrans.rotation, cachedTrans.parent, obj =>
                    {
                        GameObject effectObj = (GameObject)obj.Target;
                        effectObj.SetActive(false);

                        GameManager.Task.AddDelayTriggerTask(0.3f - (Time.time - startTime), () =>
                        {
                            effectObj.SetActive(true);
                        });

                        GameManager.Task.AddDelayTriggerTask(1.5f, () =>
                        {
                            GameManager.ObjectPool.Unspawn<EffectObject>("TileItemDestroyEffectPool", effectObj);
                        }, true);
                    });

                    GameManager.Sound.ForbidSound(SoundType.SFX_ClickTile_new.ToString(), true);
                    GameManager.Sound.ForbidSound(SoundType.SFX_ClickTile_pop.ToString(), true);
                    GameManager.Sound.PlayAudio(SoundType.SFX_Gule_Tile_Break_Slime.ToString());

                    GameManager.Task.AddDelayTriggerTask(0f, () =>
                    {
                        GameManager.Sound.ForbidSound(SoundType.SFX_ClickTile_new.ToString(), false);
                        GameManager.Sound.ForbidSound(SoundType.SFX_ClickTile_pop.ToString(), false);
                    });
                }
                else
                {
                    Release(false);
                    if (isLeftGlue)
                    {
                        item1.ClickEvent?.Invoke(item1);
                        item2.ClickEvent?.Invoke(item2);
                    }
                    else
                    {
                        item2.ClickEvent?.Invoke(item2);
                        item1.ClickEvent?.Invoke(item1);
                    }
                }
            }
            else if (!TileItem.IsBeCover)
            {
                Vector3 shakePos = new Vector3(2, 2, 0);
                ShakeTile(shakePos);
                bindAttachment.ShakeTile(shakePos);
            }
        }
    }

    private Vector3 positionShake = Vector3.zero;
    private Vector3 startPosition = Vector3.zero;
    private Coroutine shakeCoroutine = null;

    public void ShakeTile(Vector3 posShake)
    {
        positionShake = posShake;
        if (shakeCoroutine != null)
        {
            TileItem.StopCoroutine(shakeCoroutine);
            TileItem.transform.localPosition = startPosition;
        }
        shakeCoroutine = TileItem.StartCoroutine(ShakeBetweenTime());
    }

    IEnumerator ShakeBetweenTime()
    {
        Transform myTransform = TileItem.transform;
        float currentTime = 0f;
        startPosition = myTransform.localPosition;
        float cycleTime = 0.2f;
        int cycleCount = 2;
        int curCycle = 0;

        while (true)
        {
            float deltaTime = Time.deltaTime;
            currentTime += deltaTime;

            if (curCycle >= cycleCount)
            {
                yield break;
            }

            currentTime += Time.deltaTime;
            while (currentTime >= cycleTime)
            {
                currentTime -= cycleTime;
                curCycle++;
                if (curCycle >= cycleCount)
                {
                    myTransform.localPosition = startPosition;
                    break;
                }
            }

            float offsetScale = Mathf.Sin(2 * Mathf.PI * currentTime / cycleTime);
            if (positionShake != Vector3.zero)
                myTransform.localPosition = startPosition + positionShake * offsetScale;
            yield return null;

        }
    }

    private bool SearchBindAttachment()
    {
        Dictionary<int, SortedDictionary<int, (int, TileItem)>> dict = null;
        TileMatchPanel panel = null;
        if (GameManager.UI != null)
        {
            panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;

            if (panel != null)
                dict = panel.tileMapDict;
        }

        if (dict != null)
        {
            if (dict.TryGetValue(TileItem.Data.Layer, out SortedDictionary<int, (int, TileItem)> layerData)) 
            {
                int row = TileItem.Data.MapID / 16;
                int col = TileItem.Data.MapID % 16;

                //ÏòÓÒËÑË÷
                int rightCount = 0;
                for (int i = col + 2; i < 16; i+=2) 
                {
                    int mapIndex = row * 16 + i;
                    if (layerData.TryGetValue(mapIndex, out (int, TileItem) mapData))
                    {
                        if (mapData.Item2.AttachLogic != null && mapData.Item2.AttachLogic.AttachId == AttachId)
                        {
                            rightCount++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (rightCount % 2 != 0)
                {
                    bindAttachment = layerData[row * 16 + col + 2].Item2.AttachLogic as AttachLogic_2;
                    isLeftGlue = true;
                }
                else if (col >= 2 && layerData.TryGetValue(row * 16 + col - 2, out (int, TileItem) leftData)) 
                {
                    bindAttachment = leftData.Item2.AttachLogic as AttachLogic_2;
                    isLeftGlue = false;
                }

                if (bindAttachment != null)
                    return true;
            }
        }

        return false;
    }
}
