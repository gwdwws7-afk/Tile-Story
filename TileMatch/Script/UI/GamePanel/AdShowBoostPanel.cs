using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AdShowBoostPanel : PopupMenuForm
{
    public BlackBgManager blackBg;
    public Transform m_Parent;
    public GameObject[] m_Boosts;
    public GameObject[] m_FireworkBoostEffects;

    private TotalItemType boost = TotalItemType.None;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        if (userData != null)
            boost = (TotalItemType)userData;
    }

    public override void OnReset()
    {
        for (int i = 0; i < m_FireworkBoostEffects.Length; i++)
        {
            m_FireworkBoostEffects[i].SetActive(false);
        }

        base.OnReset();
    }

    public override void OnShow(Action<UIForm> showSuccessAction = null, object userData = null)
    {
        base.OnShow(showSuccessAction, userData);

        if (boost != TotalItemType.None)
        {
            StartCoroutine(ShowBoostAnim());
        }
        else
        {
            Log.Error("Boost is invalid");
            GameManager.Task.AddDelayTriggerTask(0.1f, () =>
            {
                GameManager.UI.HideUIForm(this);
            });
        }
    }
    
    public override void OnReveal()
    {
    }
    
    IEnumerator ShowBoostAnim()
    {
        blackBg.OnShow(0.3f);
        
        int changeNum = boost == TotalItemType.Prop_AddOneStep ? 6 : 7;
        for (int i = 0; i <= changeNum; i++)
        {
            m_Boosts[0].SetActive(i % 2 == 0);
            m_Boosts[1].SetActive(i % 2 == 1);
            yield return new WaitForSeconds(0.1f);
        }

        m_Parent.DOScale(new Vector3(1.8f, 1.8f, 1f), 0.2f).onComplete = () =>
        {
            m_Parent.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InQuad);
        };
        yield return new WaitForSeconds(0.3f);
        
        blackBg.OnHide(0.3f);
        
        if (boost == TotalItemType.Prop_AddOneStep)
        {
            TriggerAddOneStepBoost(m_Parent.gameObject);
            GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Use_ExtraSlot, 1);
        }
        else
        {
            m_Parent.gameObject.SetActive(false);
            TriggerFireworkBoost();
        }
        
        yield return new WaitForSeconds(1.1f);
        GameManager.UI.HideUIForm(this);
    }

    private void TriggerAddOneStepBoost(GameObject target)
    {
        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null)
        {
            target.transform.DOScale(new Vector3(0.4f, 0.4f, 1f), 0.25f);
            target.transform.DOJump(panel.BarPos, 0.3f, 1, 0.25f).onComplete = () =>
            {
                target.SetActive(false);

                panel.IsAddOneStepState = true;
                panel.RefreshChosenBar(true);
            };
        }
    }

    private void TriggerFireworkBoost()
    {
        TileMatchPanel panel = GameManager.UI.GetUIForm("TileMatchPanel") as TileMatchPanel;
        if (panel != null)
        {
            List<TileItem> list = panel.GetFireworkTargetTile(false, false);
            if (list != null && list.Count == 3) 
            {
                for (int i = 0; i < list.Count; i++)
                {
                    TileItem tileItem = list[i];
                    int tileId = 20;
                    int layer = tileItem.Data.Layer;
                    int mapIndex = tileItem.Data.MapID;
                    TileMoveDirectionType moveIndex = tileItem.Data.MoveIndex;
                    Vector3 targetPos = tileItem.transform.localPosition;
                    var coverIndexs = tileItem.Data.CoverIndexs;
                    var beCoverIndexs = tileItem.Data.BeCoverIndexs;
                    var clickEvent= tileItem.ClickEvent;
                    int siblingIndex = tileItem.transform.GetSiblingIndex();
                    int index = i;
                    if (tileItem.AttachLogic != null)
                        tileItem.AttachLogic.Release(false);
                    if (tileItem.Data.AttachID != 0)
                        tileItem.Data.AttachID = 0;

                    panel.SpawnTile(tileId, item =>
                    {
                        item.Init(layer, mapIndex, tileId, 0, 1, moveIndex, targetPos, coverIndexs, beCoverIndexs, clickEvent, false);
                        panel.tileMapDict[layer][mapIndex] = (tileId, item);

                        panel.UnSpawnTile(tileItem.gameObject);
                        item.transform.SetSiblingIndex(siblingIndex);

                        item.transform.localScale = Vector3.zero;
                        GameManager.Task.AddDelayTriggerTask(index * 0.2f, () =>
                        {
                            item.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).onComplete = () =>
                            {
                                m_FireworkBoostEffects[index].transform.position = item.transform.position;
                                m_FireworkBoostEffects[index].SetActive(true);
                                item.transform.DOScale(Vector3.one, 0.2f);
                            };

                            GameManager.Sound.PlayAudio(SoundType.SFX_Booster_Rocket_Appear.ToString());
                        });
                    });
                }
            }
        }
    }
}
