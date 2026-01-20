using DG.Tweening;
using System;
using UnityEngine;

public class ClimbBeanstalkMenu_Easter : ClimbBeanstalkMenu
{
    [SerializeField] private Transform airship;

    [SerializeField]
    Transform[] airshipImgList;

    private float airshipRatio = 4f;
    private float airshipOriginY;

    public override void OnInit(UIGroup uiGroup, Action completeAction = null, object userData = null)
    {
        base.OnInit(uiGroup, completeAction, userData);

        bottomBoundary = 0f;

        // 重新设置头像的位置
        int currentStage = ClimbBeanstalkManager.Instance.LastWinStreakNum;
        float y = GetSliderY(currentStage) + ((currentStage == 0) ? 150 : 0);
        myPortraitTransform.localPosition = new Vector3(0, y, 0);

        //播放飞艇的动画
        for (int i = 0; i < airshipImgList.Length; i++)
        {
            float x = (i % 2 == 0) ? -800f : 800f;
            float time = 30 - i * 5;
            airshipImgList[i].DOLocalMoveX(x, time).SetLoops(-1);
        }
    }

    public override void OnRelease()
    {
        itemPromptBox.OnRelease();
        GameManager.ObjectPool.DestroyObjectPool("ClimbBeanstalkColumnPool");

        base.OnRelease();
    }

    public override void OnScrollViewChanged(Vector2 value)
    {
        backScene.localPosition = new Vector3(0, -scrollArea.scrollRect.verticalNormalizedPosition * (4320f - Screen.height - 800f) - Screen.height / 2, 0);
        airship.position = new Vector3(0, airshipOriginY - scrollArea.scrollRect.verticalNormalizedPosition * airshipRatio, 0);
    }
}
