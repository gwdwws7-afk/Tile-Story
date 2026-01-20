using DG.Tweening;
using MySelf.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecorationArea : MonoBehaviour
{
    public int areaId;
    public List<DecorationObject> decorationObjects;
    [SerializeField]
    //装修任意物件时，即调用PlayCharacterAction时 播放的音效
    //由于场景中可能有多个角色 所以也是Array
    private string[] audioForCharacterActiveAnim;

    private string lastAudioName;
    private List<string> randomNameList = new List<string>();
    public void InitArea(Action onFinished = null)
    {
        InitializeObjects(onFinished);
    }

    public void OnReset()
    {

    }

    public void OnRelease()
    {
        onAllObjectInitCallback = null;
        OnReset();
        if(decorationObjects!=null)
            for (int i = 0; i < decorationObjects.Count; i++)
            {
                decorationObjects[i].OnRelease();
            }
    }

    public Vector3[] GetAllObjectPosition()
    {
        List<Vector3> unlockBtnPositionList = new List<Vector3>();
        for (int i = 0; i < decorationObjects.Count; i++)
        {
            DecorationObject obj = decorationObjects[i];

            unlockBtnPositionList.Add(obj.unlockBtnPos.position);
        }
        return unlockBtnPositionList.ToArray();
    }

    /// <summary>
    /// 根据当前是否可以解锁 来更新各家具的Material
    /// </summary>
    public void ChangeAllDecorationObjectPrefabMaterial()
    {
        for (int i = 0; i < decorationObjects.Count; i++)
        {
            DecorationObject obj = decorationObjects[i];
            if (obj != null)
            {
                DecorateItem decorateItem = DecorationModel.Instance.GetTargetDecorationItem(areaId, i);
                if (DecorationModel.Instance.GetTargetDecorationItemIsUnlocked(decorateItem))
                    obj.SetDecorationObjectPrefabToRedShineMaterial();
                else
                    obj.SetDecorationObjectPrefabToNormalMaterial();
            }
        }
    }

    /// <summary>
    /// 把所有家具的Material都设置为无描边效果(正常效果)
    /// </summary>
    public void ChangeAllDecorationObjectPrefabToNormalMaterial()
    {
        for (int i = 0; i < decorationObjects.Count; i++)
        {
            DecorationObject obj = decorationObjects[i];
            if (obj != null)
                obj.SetDecorationObjectPrefabToNormalMaterial();
        }
    }


    private void InitializeObjects(Action onFinished = null)
    {
        totalWaitingInitObjectCount = decorationObjects.Count;
        onAllObjectInitCallback = onFinished;

        for (int i = 0; i < decorationObjects.Count; i++)
        {
            int idx = i;//位置的下标
            decorationObjects[idx].areaID = areaId;
            decorationObjects[idx].positionID = i + 1;

            int currentProgress = DecorationModel.Instance.GetTargetDecorationType(areaId, i);
            decorationObjects[idx].OnInit(currentProgress, OnSingleDecorationObjectInit);
        }
    }

    private int totalWaitingInitObjectCount = 0;
    private Action onAllObjectInitCallback;
    private void OnSingleDecorationObjectInit()
    {
        totalWaitingInitObjectCount--;
        if (totalWaitingInitObjectCount <= 0)
        {
            if (onAllObjectInitCallback != null)
            {
                onAllObjectInitCallback();
                onAllObjectInitCallback = null;
            }
        }
    }


    /// <summary>
    /// 执行消耗并播放构筑动画
    /// </summary>
    /// <param name="positionFlag"></param>
    /// <param name="isQuick"></param>
    public void FirstTimeDecorate(int positionFlag, bool isQuick = false)
    {
        decorationObjects[positionFlag].afterAnimPlay = () =>
        {
            DecorationOperationPanel decorationOperationPanel = (DecorationOperationPanel)GameManager.UI.GetUIForm("DecorationOperationPanel");
            decorationOperationPanel.PlayShinningFlyAnim(decorationObjects[positionFlag].unlockBtnPos.position, ()=>
            {
                decorationOperationPanel.RefreshDisplay();
                GameManager.Task.AddDelayTriggerTask(0.1f, () =>
                {
                    decorationOperationPanel.UnregisterInAnimReason(DecorationOperationPanel.InAnimReason.StarFlyTillShinningFly);
                });
            });
            MapDecorationBGPanel decorationBGPanel = (MapDecorationBGPanel)GameManager.UI.GetUIForm("MapDecorationBGPanel");
            decorationBGPanel.InitClickToPlayDialogScript();
            decorationBGPanel.PlayCharacterAction();
            DecorateItem decorateItem = DecorationModel.Instance.GetTargetDecorationItem(areaId, positionFlag);
            decorationBGPanel.PlayDialogueForSpecificCharacter(decorateItem.PlayDialogueCharacterIndex, decorateItem.PlayDialogueTerm);

            //1 配置了PlayAudio 就播 PlayAudio (并记录)
            //2 否则 从audioForCharacterActiveAnim里加上不播放 一共n+1种可能性中随机
            //3 优先播放(并记录)和上次记录的名字不同的音效
            if (!PlayAudioAndRecordName(decorateItem.PlayAudio))
            {
                randomNameList.Clear();
                if (!string.IsNullOrEmpty(lastAudioName))
                    randomNameList.Add(string.Empty);
                for (int i = 0; i < audioForCharacterActiveAnim.Length; ++i)
                {
                    if (audioForCharacterActiveAnim[i] != lastAudioName)
                        randomNameList.Add(audioForCharacterActiveAnim[i]);
                }

                if (randomNameList.Count > 0)
                {
                    int result = UnityEngine.Random.Range(0, randomNameList.Count);
                    string finalAudioName = randomNameList[result];
                    if (!PlayAudioAndRecordName(finalAudioName))
                    {
                        //Log.Warning("ClearRecord");
                        lastAudioName = string.Empty;
                    }
                }
            }

        };
        decorationObjects[positionFlag].ShowUpgradeAnim(1);

        GameManager.Event.Fire(this, StarNumRefreshEventArgs.Create());
    }



    //public void PlayBackDecorate(Action callback)
    //{
    //    for (int i = 0; i < decorationObjects.Count; i++)
    //    {
    //        int position = i;
    //        var target = decorationObjects[i];
    //        if (target == decorationObjects[decorationObjects.Count - 1])
    //        {
    //            target.afterAnimPlay = () =>
    //            {
    //                callback?.Invoke();
    //            };
    //        }
    //        int decorationType = DecorationModel.Instance.GetTargetDecorationType(areaId, position);
    //        target.ShowPlayBackAnim(decorationType);
    //    }
    //}

    public void PlayCharacterAction()
    {
        for (int i = 0; i < decorationObjects.Count; i++)
        {
            decorationObjects[i].PlaySpineCharacterActiveAnim();
        }
    }

    private bool PlayAudioAndRecordName(string inputName)
    {
        if (!string.IsNullOrEmpty(inputName))
        {
            //Log.Warning("PlayAudioAndRecordName " + inputName);
            GameManager.Sound.PlayAudio(inputName);
            lastAudioName = inputName;
            return true;
        }

        return false;
    }
}
