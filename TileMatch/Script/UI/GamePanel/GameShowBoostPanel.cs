using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameShowBoostPanel : PopupMenuForm
{
    public GameObject[] m_Boosts;
    public BlackBgManager blackBg;
    public GameObject[] m_FireworkBoostEffects;

    private List<TotalItemType> m_BoostList;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        m_BoostList = userData as List<TotalItemType>;
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

        if (m_BoostList != null)
        {
            blackBg.OnShow(0.3f);

            float delayHideTime = 1f;
            Vector3[] posList = UnityUtility.GetAveragePosition(Vector3.zero, new Vector3(210, 0, 0), m_BoostList.Count);
            for (int i = 0; i < m_BoostList.Count; i++)
            {
                float delayTime = 0.5f * i;
                float time = ShowBoostAnim(m_BoostList[i], posList[i], delayTime);
                if (delayHideTime < time)
                    delayHideTime = time;
            }

            GameManager.Task.AddDelayTriggerTask(0.9f, () =>
            {
                blackBg.OnHide(0.3f);

                GameManager.Task.AddDelayTriggerTask(delayHideTime, () =>
                {
                    GameManager.UI.HideUIForm(this); 
                });
            });
        }
        else
        {
            Log.Error("Boost list is invalid");
            GameManager.Task.AddDelayTriggerTask(0.1f, () =>
            {
                GameManager.UI.HideUIForm(this);
            });
        }
    }

    public override void OnReveal()
    {
    }

    private float ShowBoostAnim(TotalItemType boostType, Vector3 targetPos, float delayTime)
    {
        GameObject target = null;
        switch (boostType)
        {
            case TotalItemType.MagnifierBoost:
                target = m_Boosts[0];
                break;
            case TotalItemType.Prop_AddOneStep:
                target = m_Boosts[1];
                break;
            case TotalItemType.FireworkBoost:
                target = m_Boosts[2];
                break;
        }

        target.transform.localPosition = new Vector3(-566, -356, 0);
        target.SetActive(true);
        if (boostType == TotalItemType.Prop_AddOneStep) 
        {
            return ShowAddOneStepBoostAnim(target, targetPos, delayTime);
        }
        else if(boostType == TotalItemType.FireworkBoost)
        {
            return ShowFireworkBoostAnim(target, targetPos, delayTime);
        }

        return 0f;
    }

    private float ShowAddOneStepBoostAnim(GameObject target, Vector3 targetPos, float delayTime)
    {
        target.transform.localScale = Vector3.one;
        target.transform.DOLocalJump(targetPos, -350f, 1, 0.5f).onComplete = () =>
        {
            target.transform.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.2f).SetDelay(delayTime).onComplete = () =>
            {
                target.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InQuad);
            };
        };

        GameManager.Task.AddDelayTriggerTask(1f + delayTime, () =>
         {
             TriggerAddOneStepBoost(target);
         });

        GameManager.Objective.ChangeObjectiveProgress(ObjectiveType.Use_ExtraSlot, 1);

        return 1f + delayTime;
    }

    private float ShowFireworkBoostAnim(GameObject target, Vector3 targetPos, float delayTime)
    {
        target.transform.DOLocalJump(targetPos, -350f, 1, 0.5f).onComplete = () =>
        {
            target.transform.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.2f).SetDelay(delayTime).onComplete = () =>
            {
                target.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InQuad);
            };
        };

        GameManager.Task.AddDelayTriggerTask(0.9f + delayTime, () =>
         {
             target.SetActive(false);
             TriggerFireworkBoost();
         });

        return 1.1f + delayTime;
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
