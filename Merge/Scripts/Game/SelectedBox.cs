using DG.Tweening;
using UnityEngine;

namespace Merge
{
    /// <summary>
    /// 道具选中框
    /// </summary>
    public class SelectedBox : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);
            transform.localScale = new Vector3(0.8f, 0.8f);
            transform.DOScale(1.05f, 0.15f).onComplete = () =>
            {
                transform.DOScale(1f, 0.15f);
            };
        }

        public void Hide()
        {
            transform.DOKill();
            gameObject.SetActive(false);
        }
    }
}