using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class LockPromptBox : MonoBehaviour
    {
        public RectTransform box;
        public Transform slotRoot;

        private bool isShowing;

        public bool IsShowing => isShowing;

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

        }

        private bool CheckInitAllComplete()
        {
            return true;
        }
    }
}
