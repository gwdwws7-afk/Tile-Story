using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Merge
{
    public class ChestPromptBox : MonoBehaviour
    {
        public RectTransform box;
        public Transform slotRoot;
        public GameObject tileImage;

        [Header("Body Setting")]
        public float centerOffset = 220;
        public float triangelOffset = 0;
        public float boxPivot = 0.5f;

        [Header("Box Setting")]
        public float boxMinWidth = 600;
        public float boxMinHeight = 190;
        public float boxMaxWidth = 600;
        public float boxMaxHeight = 1000;

        //方框的内部填充大小
        public float boxHorizontalPadding = 10;
        public float boxVerticalPadding = 50;

        public float spaceX;
        public float spaceY;
        public float itemOffsetX;
        public float itemOffsetY;
        public bool balancedArrange;

        public bool isShowSlotTight;

        protected float boxPreferredWidth;
        protected float boxPreferredHeight;

        private List<MergePromptItemSlot> itemSlots = new List<MergePromptItemSlot>();
        private List<AsyncOperationHandle> asyncHandlesList = new List<AsyncOperationHandle>();
        private int rewardCount;
        private bool isRelease;
        private bool isShowing;

        public bool IsShowing => isShowing;

        public void Init(List<ItemData> datas)
        {
            isRelease = false;

            int tileDataIndex = -1;
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i].type.ID >= MergeManager.Instance.TileRewardId)
                {
                    tileDataIndex = i;
                    break;
                }
            }

            if (!isShowSlotTight)
            {
                if (tileDataIndex != -1)
                {
                    datas.RemoveAt(tileDataIndex);
                    if (tileImage) tileImage.SetActive(true);
                    itemOffsetX = 160;
                    boxHorizontalPadding = 200;
                    spaceX = 130;
                }
                else
                {
                    if (tileImage) tileImage.SetActive(false);
                    itemOffsetX = 0;
                    boxHorizontalPadding = 140;
                    spaceX = 170;
                }
            }
            else
            {
                if (datas.Count >= 4)
                    itemOffsetY = 80;
                else
                    itemOffsetY = 0;
            }

            rewardCount = datas.Count;
            InstantiateItemSlotAsync(datas.Count, () =>
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    itemSlots[i].isShowSlotTight = isShowSlotTight;
                    itemSlots[i].OnInit(datas[i].type, datas[i].num);
                    itemSlots[i].gameObject.SetActive(true);
                }

                for (int i = datas.Count; i < itemSlots.Count; i++)
                {
                    itemSlots[i].gameObject.SetActive(false);
                }
            });
        }

        public void Release()
        {
            isRelease = true;

            rewardCount = 0;

            for (int i = 0; i < itemSlots.Count; i++)
            {
                itemSlots[i].OnRelease();
            }
            itemSlots.Clear();

            for (int i = 0; i < asyncHandlesList.Count; i++)
            {
                UnityUtility.UnloadAssetAsync(asyncHandlesList[i]);
            }
            asyncHandlesList.Clear();
        }

        public void Show(Vector3 position, float offsetX = 0f)
        {
            Transform cachedTransform = transform;
            cachedTransform.DOKill();
            cachedTransform.position = position;
            cachedTransform.localScale = Vector3.zero;
            box.localPosition = new Vector3(offsetX, box.localPosition.y, 0);
            slotRoot.localPosition = new Vector3(offsetX, slotRoot.localPosition.y, 0);
            gameObject.SetActive(true);

            StartCoroutine(ShowPromptBoxCor());
        }

        public void Hide()
        {
            isShowing = false;
            StopAllCoroutines();

            transform.DOKill();
            gameObject.SetActive(false);
        }

        IEnumerator ShowPromptBoxCor()
        {
            yield return null;

            while (!CheckInitAllComplete())
            {
                yield return null;
            }

            Refresh();

            transform.DOScale(1.1f, 0.15f).onComplete = () =>
            {
                transform.DOScale(1, 0.15f);
            };

            yield return new WaitForSeconds(0.3f);

            isShowing = true;
        }

        private void Refresh()
        {
            if (itemSlots.Count <= 0 || itemSlots.Count < rewardCount)
            {
                return;
            }

            float slotWidth = itemSlots[0].GetComponent<RectTransform>().sizeDelta.x;
            float slotHeight = itemSlots[0].GetComponent<RectTransform>().sizeDelta.y;

            int maxColumnNum = (int)(boxMaxWidth / (float)(slotWidth + spaceX));
            int columnNum = rewardCount < maxColumnNum ? rewardCount : maxColumnNum;
            int rowNum = Mathf.CeilToInt(rewardCount / (float)columnNum);

            if (balancedArrange && rowNum > 1)
            {
                columnNum = Mathf.CeilToInt(rewardCount / (float)rowNum);
                rowNum = Mathf.CeilToInt(rewardCount / (float)columnNum);
            }

            boxPreferredWidth = Mathf.Clamp(columnNum * (slotWidth + spaceX) + boxHorizontalPadding - spaceX, boxMinWidth, boxMaxWidth);

            rowNum = rowNum <= 0 ? 1 : rowNum;
            boxPreferredHeight = rowNum * (slotHeight + spaceY) + boxVerticalPadding;

            box.sizeDelta = new Vector2(boxPreferredWidth, boxPreferredHeight);
            Vector3[] posList = UnityUtility.GetAveragePosition(Vector3.zero, new Vector3(spaceX, 0, 0), columnNum);

            for (int i = 0; i < rewardCount; i++)
            {
                int index = i;
                float height = 0;

                if (rowNum > 1)
                {
                    if (rowNum % 2 == 0)
                    {
                        height = slotHeight / 2f + slotHeight * ((rowNum - 2) / 2f);
                    }
                    else
                    {
                        height = slotHeight * ((rowNum - 1) / 2f);
                    }
                }

                for (int j = rowNum; j >= 1; j--)
                {
                    if (i >= columnNum * j)
                    {
                        index = i - columnNum * j;
                        height -= j * slotHeight;
                        break;
                    }
                }

                itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(posList[i % columnNum].x + itemOffsetX, height + itemOffsetY);
            }
        }

        private void InstantiateItemSlotAsync(int targetCount, Action callback)
        {
            if (isRelease)
            {
                return;
            }

            if (itemSlots.Count < targetCount)
            {
                var asyncHandle = UnityUtility.InstantiateAsync("MergePromptItemSlot", slotRoot, obj =>
                {
                    var slot = obj.GetComponent<MergePromptItemSlot>();
                    itemSlots.Add(slot);
                    InstantiateItemSlotAsync(targetCount, callback);
                });

                asyncHandlesList.Add(asyncHandle);
            }
            else
            {
                callback?.Invoke();
            }
        }

        private bool CheckInitAllComplete()
        {
            if (itemSlots.Count < rewardCount)
            {
                return false;
            }

            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (itemSlots[i].gameObject.activeSelf && !itemSlots[i].CheckInitComplete())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
