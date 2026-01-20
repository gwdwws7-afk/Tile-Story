using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class FlowerProp : NormalProp
    {
        public override void OnClick()
        {
            base.OnClick();

            if (PropLogic.PropId != 10104)
                return;

            MergeMainMenu_LoveGiftBattle mainBoard = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenu_LoveGiftBattle;
            if (mainBoard == null)
                return;

            if (mainBoard.m_ThiefBoard.IsThiefDefeated)
                return;

            GameManager.Event.FireNow(this, MergeFlowerGetEventArgs.Create());

            int row = PropLogic.Square != null ? PropLogic.Square.m_Row : 5;

            Vector3 startPos = transform.position;
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                UnityUtility.InstantiateAsync("MergeFlowerGetTip", mainBoard.transform, obj =>
                {
                    Transform cachedTrans = obj.transform;
                    float randomRadius = Random.Range(0.12f, 0.16f);
                    Vector3 randomPosition = GetRandomPositionInLowerHalf(startPos, randomRadius);
                    cachedTrans.position = startPos;
                    obj.SetActive(true);

                    float delayTime = 0.2f;
                    if (index == 1)
                        delayTime += 0.3f;
                    else if (index == 2)
                        delayTime += 0.7f;

                    cachedTrans.DOMove(randomPosition, 0.2f);

                    cachedTrans.DOLocalRotate(new Vector3(0, 0, 500 + 500 / 0.3f * delayTime), 0.3f + delayTime, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetDelay(0.2f);

                    float time = 0.4f + row * 0.05f;
                    cachedTrans.DOMove(mainBoard.m_ThiefBoard.m_ThiefHitPos.position, time).SetEase(Ease.InOutQuart).SetDelay(delayTime).onComplete = () =>
                      {
                          cachedTrans.DOKill();
                          UnityUtility.UnloadInstance(obj);
                      };

                    GameManager.Task.AddDelayTriggerTask(time + delayTime - 0.1f - row * 0.02f, () =>
                         {
                             if (obj != null)
                                 obj.SetActive(false);
                         });

                    GameManager.Task.AddDelayTriggerTask(delayTime, () =>
                    {
                        GameManager.Sound.PlayAudio("SFX_Pinata_Stick_Fly");
                    });
                });
            }

            MergeManager.Merge.ReleaseProp(PropLogic);
        }

        public override void OnGeneratedByMerge()
        {
            if (PropLogic != null && PropLogic.PropId == 10104 && PropLogic.Square != null && PropLogic.Prop != null)
            {
                MergeMainMenuBase mainMenu = GameManager.UI.GetUIForm(MergeManager.Instance.GetMergeMenuName("MergeMainMenu")) as MergeMainMenuBase;
                mainMenu.m_GuideMenu.TriggerGuide(Merge.GuideTriggerType.Guide_MaxLevelBouquet, PropLogic);
            }

            base.OnGeneratedByMerge();
        }

        private Vector3 GetRandomPositionInLowerHalf(Vector3 startPos, float radius = 1.0f)
        {
            float r = Mathf.Sqrt(Random.value) * radius;
            float theta = Random.Range(Mathf.PI, 2f * Mathf.PI);
            float x = r * Mathf.Cos(theta);
            float y = r * Mathf.Sin(theta);
            return startPos + new Vector3(x, y, 0f);
        }
    }
}
