using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntranceFlyObjectManager : MonoBehaviour
{
    public static EntranceFlyObjectManager Instance { get; private set; }

    private LinkedList<EntranceFlyObjectEvent> flyEvents = new LinkedList<EntranceFlyObjectEvent>();
    private List<NormalEntranceFlyObject> flyObjects = new List<NormalEntranceFlyObject>();

    private int finishedFlyCount;
    private bool isClear;
    private bool isShowingFlyObjectsAnim;
    private int flyCount;

    public int Count
    {
        get
        {
            return flyEvents.Count;
        }
    }

    private void Awake()
    {
        Instance = this;
        isClear = false;
        isShowingFlyObjectsAnim = false;
        flyCount = 0;
    }

    public void Init()
    {
        isClear = false;
        isShowingFlyObjectsAnim = false;
        flyCount = 0;
    }

    private void Update()
    {
        if (finishedFlyCount >= flyEvents.Count && flyEvents.Count != 0) 
        {
            finishedFlyCount = 0;
            flyEvents.Clear();
            isShowingFlyObjectsAnim = false;
            GameManager.Process.EndProcess(ProcessType.EntranceFlyObjects);
        }
    }

    public void Clear()
    {
        isClear = true;
        finishedFlyCount = 0;
        isShowingFlyObjectsAnim = false;
        flyEvents.Clear();

        for (int i = 0; i < flyObjects.Count; i++)
        {
            flyObjects[i].OnRelease();
        }
        flyObjects.Clear();

        GameManager.ObjectPool.DestroyObjectPool("EntranceFlyObjectPool");
        GameManager.Process.EndProcess(ProcessType.EntranceFlyObjects);
    }

    public void RegisterEntranceFlyEvent(string spriteKey, int objectNum, int priority, Vector3 startPos, Vector3 endPos, GameObject targetEntance, Action startAction, Action endAction,bool isDouble = false)
    {
        EntranceFlyObjectEvent flyEvent = new EntranceFlyObjectEvent(spriteKey, objectNum, priority, startPos, endPos, targetEntance, startAction, endAction, isDouble);

        //注册时如果正在播放按钮道具动画，接入队尾
        if (isShowingFlyObjectsAnim)
        {
            GameManager.Task.AddDelayTriggerTask(flyCount * 0.3f, () =>
            {
                if (!isClear)
                {
                    EntranceFlyObjectAnim(flyEvent);
                }
            });
            flyCount++;

            return;
        }

        if (flyEvents.Count == 0)
        {
            flyEvents.AddLast(flyEvent);
        }
        else
        {
            var flyEventNode = flyEvents.First;
            while (flyEventNode != null)
            {
                if (flyEventNode.Value.Priority < priority)
                {
                    flyEvents.AddBefore(flyEventNode, flyEvent);
                    return;
                }

                flyEventNode = flyEventNode.Next;
            }

            flyEvents.AddLast(flyEvent);
        }
    }

    public void EndEntranceFlyEvent()
    {
        finishedFlyCount++;
    }

    public void ShowEntranceFlyObjectsAnim()
    {
        if (Count <= 0)
        {
            GameManager.Process.EndProcess(ProcessType.EntranceFlyObjects);
            return;
        }

        isShowingFlyObjectsAnim = true;

        var flyEvent = flyEvents.First;
        flyCount = 0;

        while (flyEvent != null)
        {
            EntranceFlyObjectEvent flyObj = flyEvent.Value;
            GameManager.Task.AddDelayTriggerTask(flyCount * 0.3f, () =>
            {
                if (!isClear)
                {
                    EntranceFlyObjectAnim(flyObj);
                }
            });

            flyEvent = flyEvent.Next;
            flyCount++;
        }
    }

    private void EntranceFlyObjectAnim(EntranceFlyObjectEvent flyEvent)
    {
        if (flyEvent == null || flyEvent.TargetEntance == null)
        {
            Log.Error("Fly Event is invalid");
            EndEntranceFlyEvent();
            return;
        }
        GameManager.ObjectPool.Spawn<ElfObject>("NormalEntranceFlyObject", "EntranceFlyObjectPool", flyEvent.StartPos, Quaternion.identity, flyEvent.TargetEntance.transform, obj =>
        {
            GameObject flyObject = (GameObject)obj.Target;
            flyObject.transform.position = flyEvent.StartPos;

            NormalEntranceFlyObject normalEntranceFlyObject = flyObject.GetComponent<NormalEntranceFlyObject>();
            flyObjects.Add(normalEntranceFlyObject);
            if (flyEvent.IsDouble)
            {
                normalEntranceFlyObject.OnInit(flyEvent.SpriteKey, flyEvent.ObjectNum/2);
                FlyDoubleTagObject(flyEvent, normalEntranceFlyObject);
                normalEntranceFlyObject.onAllAnimComplete = () =>
                {
                    flyObjects.Remove(normalEntranceFlyObject);

                    GameManager.Sound.PlayAudio("SFX_itemget");

                    flyEvent.EndAction?.Invoke();

                    normalEntranceFlyObject.OnRelease();
                    GameManager.ObjectPool.Unspawn<ElfObject>("EntranceFlyObjectPool", flyObject);
                };

                flyEvent.StartAction?.Invoke();
                normalEntranceFlyObject.gameObject.SetActive(true);
            }
            else
            {
                normalEntranceFlyObject.OnInit(flyEvent.SpriteKey, flyEvent.ObjectNum);
                normalEntranceFlyObject.onFlyObjectReach = () =>
                {
                    
                };

                normalEntranceFlyObject.onAllAnimComplete = () =>
                {
                    flyObjects.Remove(normalEntranceFlyObject);

                    GameManager.Sound.PlayAudio("SFX_itemget");

                    flyEvent.EndAction?.Invoke();

                    normalEntranceFlyObject.OnRelease();
                    GameManager.ObjectPool.Unspawn<ElfObject>("EntranceFlyObjectPool", flyObject);
                };

                flyEvent.StartAction?.Invoke();

                normalEntranceFlyObject.ShowFlyToTargetAnim(flyEvent.StartPos, flyEvent.EndPos, null);
            }
        });
    }

    public void FlyDoubleTagObject(EntranceFlyObjectEvent flyEvent, NormalEntranceFlyObject normalEntranceFlyObject)
    {
        Vector3 startPos;
        if (flyEvent.StartPos.x-flyEvent.EndPos.x < 0)
        {
            startPos = normalEntranceFlyObject.flyImg.transform.localPosition - new Vector3(120, 0);
            startPos += new Vector3(0, -40.5f);
        }
        else
        {
            startPos = normalEntranceFlyObject.flyImg.transform.localPosition + new Vector3(120, 0);
            startPos += new Vector3(0, -40.5f);
        }
    }
}
