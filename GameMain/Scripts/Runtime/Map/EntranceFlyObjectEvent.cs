using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntranceFlyObjectEvent
{
    private string spriteKey;
    private int objectNum;
    private int priority;
    private Vector3 startPos;
    private Vector3 endPos;
    private GameObject targetEntance;
    private Action startAction;
    private Action endAction;
    private bool isDouble;

    public EntranceFlyObjectEvent(string spriteKey, int objectNum, int priority, Vector3 startPos, Vector3 endPos, GameObject targetEntance, Action startAction, Action endAction, bool isDouble=false)
    {
        this.spriteKey = spriteKey;
        this.objectNum = objectNum;
        this.priority = priority;
        this.startPos = startPos;
        this.endPos = endPos;
        this.targetEntance = targetEntance;
        this.startAction = startAction;
        this.endAction = endAction;
        this.isDouble = isDouble;
    }

    public string SpriteKey { get => spriteKey; }
    public int ObjectNum { get => objectNum; }
    public int Priority { get => priority; }
    public Vector3 StartPos { get => startPos; }
    public Vector3 EndPos { get => endPos; }
    public GameObject TargetEntance { get => targetEntance; }
    public Action StartAction { get => startAction; }
    public Action EndAction { get => endAction; }
    public bool IsDouble { get => isDouble; }
}
