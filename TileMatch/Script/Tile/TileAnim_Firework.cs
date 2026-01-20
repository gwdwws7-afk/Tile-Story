using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TileAnim_Firework : MonoBehaviour
{
    public Transform[] m_Fireworks;
    public GameObject[] m_FireworkBodies;
    public GameObject[] m_TrailEffects;
    public GameObject[] m_DestroyEffects;
    public GameObject[] m_HitEffects;
    public Transform[] m_Leaders;
    public Transform[] m_Aims;
    
    private LinkedList<FlyFirework> m_FlyFireworks = new LinkedList<FlyFirework>();
    private LinkedListNode<FlyFirework> m_Current = null;
    public Action m_EndAction = null;
    private float m_MoveSpeed = 0.04f;
    private float m_RotateSpeed = 15;
    private float m_UpFlyTime = 0f;

    class FlyFirework
    {
        public Transform body;
        public Vector3 pos;
        public Transform leader;
        public LinkedList<FlyFirework> m_FlyFireworks;
        public Action callback;
        
        // private bool rotateOver;
        // private float rotateDegree;
        // private Vector3 rotateArroundPos;
        // private Vector3 rotateDir;
        // private float targetAngle;
        // private float flySpeed = 1.5f;

        public FlyFirework(Transform flyBody, Vector3 targetPos, Transform angleLeader, LinkedList<FlyFirework> m_FlyFireworks, Action callback)
        {
            body = flyBody;
            pos = targetPos;
            leader = angleLeader;
            this.m_FlyFireworks = m_FlyFireworks;
            this.callback = callback;
            
            Init();
        }
        
        private void Init()
        {
            Vector3 point1 = Vector3.zero, point2 = Vector3.zero;
            if (body.position.x >= 0 && body.position.y >= 0.2)
            {
                point1 = new Vector3(0.7f, 1f, 90);
                point2 = new Vector3(-0.3f, 1f, 90);
                if (body.position.x < 0.2)
                    point1 = new Vector3(0.3f, 1f, 90);
            }
            else if (body.position.x < 0 && body.position.y >= 0.2)
            {
                point1 = new Vector3(-0.7f, 1f, 90);
                point2 = new Vector3(0.3f, 1f, 90);
                if (body.position.x > -0.2)
                    point1 = new Vector3(-0.3f, 1f, 90);
            }
            else if (body.position.x < 0 && body.position.y < 0.2)
            {
                point1 = new Vector3(-0.7f, -0.5f, 90);
                point2 = new Vector3(0.3f, -0.5f, 90);
                if (body.position.x > -0.2)
                    point1 = new Vector3(-0.3f, -0.5f, 90);
            }
            else if (body.position.x >= 0 && body.position.y <= 0.2)
            {
                point1 = new Vector3(0.7f, -0.5f, 90);
                point2 = new Vector3(-0.3f, -0.5f, 90);
                if (body.position.x < 0.2)
                    point1 = new Vector3(0.3f, -0.5f, 90);
            }
            Vector3[] waypoints = {pos, point1, point2};
            float distance = Vector3.Distance(body.position, pos);
            float duration = Math.Clamp(distance, 0.6f, 0.8f);
            leader.DOPath(waypoints, duration - 0.01f, PathType.CubicBezier).SetEase(Ease.InQuad);
            body.DOPath(waypoints, duration, PathType.CubicBezier).SetEase(Ease.InQuad).onComplete = () =>
            {
                callback?.Invoke();
                m_FlyFireworks.Remove(this);
            };

            // if (body.position.x > pos.x)
            // {
            //     rotateArroundPos = body.position + new Vector3(0.2f, 0);
            //     rotateDir = Vector3.back;
            // }
            // else
            // {
            //     rotateArroundPos = body.position - new Vector3(0.2f, 0);
            //     rotateDir = Vector3.forward;
            // }
        }

        public void Rotate()
        {
            float angle = Vector2.Angle(Vector2.up, leader.position - body.position);
            angle = leader.position.x <= body.position.x ? angle : -angle;
            body.rotation = Quaternion.Euler(0, 0, angle);
        }

        // public void ObjectFlyingProcess()
        // {
        //     if (!rotateOver)
        //     {
        //         rotateDegree += 12;
        //         if (rotateDegree < 90)
        //         {
        //             body.RotateAround(rotateArroundPos, rotateDir, 12);
        //             return;
        //         }
        //         
        //         targetAngle = Vector2.Angle(Vector2.up, pos - body.position);
        //         targetAngle = pos.x <= body.position.x ? targetAngle : -targetAngle;
        //         float angle = Quaternion.Angle(Quaternion.Euler(0, 0, targetAngle), body.rotation);
        //         float angleDelta = angle - 10;
        //         if (angleDelta > 0 && rotateDegree < 360) 
        //         {
        //             body.RotateAround(rotateArroundPos, rotateDir, 12);
        //             return;
        //         }
        //         else
        //         {
        //             body.rotation = Quaternion.Euler(0, 0, targetAngle);
        //             rotateOver = true;
        //         }
        //     }
        //
        //     body.position = Vector3.MoveTowards(body.position, pos, flySpeed * Time.deltaTime);
        //
        //     if (Vector3.Distance(body.position, pos) < 0.01f)
        //     {
        //         callback?.Invoke();
        //         m_FlyFireworks.Remove(this);
        //     }
        // }
    }

    private void Update()
    {
        foreach (var firework in m_FlyFireworks)
        {
            firework.Rotate();
        }

        if (m_FlyFireworks.Count == 0)
        {
            m_EndAction?.Invoke();
            m_EndAction = null;
        }
    }


    // private void Update()
    // {
    //     m_Current = m_FlyFireworks.First;
    //     while (m_Current != null)
    //     {
    //         LinkedListNode<FlyFirework> next = m_Current.Next;
    //         m_Current.Value.flyTime += 1f;
    //         Vector3 lastPos = m_Current.Value.body.position;
    //         m_Current.Value.body.Translate(Vector2.up * m_MoveSpeed);
    //
    //         float d = (180 / m_RotateSpeed) * m_MoveSpeed / 3.1415926f * 2;
    //
    //         if (m_Current.Value.flyTime == m_UpFlyTime)
    //         {
    //             float angle = Vector2.SignedAngle(Vector2.down, (m_Current.Value.pos - m_Current.Value.body.position + new Vector3(d, 0, 0)));
    //             m_Current.Value.angle = angle + 180;
    //         }
    //         else if (m_Current.Value.flyTime > m_UpFlyTime && m_Current.Value.angle > 0) 
    //         {
    //             float rotateDegree = m_Current.Value.angle > m_RotateSpeed ? m_RotateSpeed : m_Current.Value.angle;
    //             m_Current.Value.body.Rotate(new Vector3(0, 0, rotateDegree));
    //             m_Current.Value.angle -= rotateDegree;
    //         }
    //         else if(m_Current.Value.flyTime > m_UpFlyTime && m_Current.Value.angle <= 0)
    //         {
    //             m_MoveSpeed = Mathf.Lerp(m_MoveSpeed, 0.05f, Time.deltaTime);
    //         }
    //
    //         if (m_Current.Value.flyTime > m_UpFlyTime && Vector3.Dot((m_Current.Value.body.position - lastPos), (m_Current.Value.pos - m_Current.Value.body.position)) < 0 && m_Current.Value.angle <= 0) 
    //         {
    //             m_Current.Value.callback.Invoke();
    //             m_FlyFireworks.Remove(m_Current);
    //         }
    //
    //         m_Current = next;
    //
    //         if (m_FlyFireworks.Count == 0)
    //         {
    //             m_EndAction?.Invoke();
    //             m_EndAction = null;
    //         }
    //     }
    // }

    public void ShowAnim(List<TileItem> targetTiles, float deltaY, Action<TileItem> callback)
    {
        m_UpFlyTime = 12 + (int)(deltaY / m_MoveSpeed);
        m_UpFlyTime = Mathf.Clamp(m_UpFlyTime, 5, 15);

        gameObject.SetActive(true);
        m_FlyFireworks.Clear();
        for (int i = 0; i < m_Fireworks.Length; i++)
        {
            int index = i;
            TileItem tileItem = targetTiles[index];
            Vector3 targetPos = targetTiles[index].transform.position;
            Transform firework = m_Fireworks[index];
            GameObject body = m_FireworkBodies[index];
            GameObject trail = m_TrailEffects[index];
            GameObject effect = m_DestroyEffects[index];
            GameObject hitEffect = m_HitEffects[index];
            Transform leader = m_Leaders[index];
            Transform aim = m_Aims[index];

            float aimDelayTime = 0.1f * index;
            GameManager.Task.AddDelayTriggerTask(aimDelayTime, () =>
            {
                if (tileItem != null)
                {
                    tileItem.transform.SetAsLastSibling();
                    tileItem.SetUncoverState();

                    if (tileItem.AttachLogic != null)
                        tileItem.AttachLogic.SetColor(false);
                }

                aim.position = targetPos;
                aim.transform.localScale = new Vector3(2, 2, 2);
                aim.gameObject.SetActive(true);
                aim.DOScale(0.9f, 0.1f).onComplete = () =>
                {
                    aim.DOScale(1f, 0.05f);
                };
            });

            float delayTime = 0.25f * index;
            GameManager.Task.AddDelayTriggerTask(delayTime, () =>
            {
                firework.gameObject.SetActive(true);
                body.SetActive(true);
                trail.SetActive(true);
                
                m_FlyFireworks.AddLast(new FlyFirework(firework, targetPos, leader, m_FlyFireworks, () =>
                {
                    GameManager.Task.AddDelayTriggerTask(0.2f, () =>
                    {
                        trail.SetActive(false);
                        hitEffect.transform.position = targetPos;
                        hitEffect.SetActive(true);
                    });

                    effect.transform.SetParent(targetTiles[index].transform);
                    effect.transform.localPosition = Vector3.zero;
                    effect.transform.localScale = new Vector3(950, 950, 1);
                    effect.SetActive(true);

                    aim.gameObject.SetActive(false);
                    body.SetActive(false);
                    callback?.Invoke(targetTiles[index]);

                    GameManager.Sound.PlayAudio(SoundType.SFX_Prop_Bomb.ToString());
                }));
            });
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_DestroyEffects.Length; i++)
        {
            if (m_DestroyEffects[i] != null)
                Destroy(m_DestroyEffects[i]);
        }
    }

    public void Clear()
    {
        m_FlyFireworks.Clear();
        m_Current = null;
        m_EndAction = null;
    }
}
