using GameFramework.Event;
using UnityEngine;

namespace Merge
{
    public class MergeOfferEntrance : MonoBehaviour
    {
        [SerializeField] private GameObject body;
        [SerializeField] private CountdownTimer timer;
        [SerializeField] private DelayButton button;

        private void OnEnable()
        {
            GameManager.Event.Subscribe(Merge.MergeStartEventArgs.EventId, OnMergeStateChange);
            GameManager.Event.Subscribe(Merge.MergeEndEventArgs.EventId, OnMergeStateChange);
            GameManager.Event.Subscribe(Merge.MergeOfferCompleteEventArgs.EventId, OnMergeStateChange);

            Refresh();
        }

        private void OnDisable()
        {
            GameManager.Event.Unsubscribe(Merge.MergeStartEventArgs.EventId, OnMergeStateChange);
            GameManager.Event.Unsubscribe(Merge.MergeEndEventArgs.EventId, OnMergeStateChange);
            GameManager.Event.Unsubscribe(Merge.MergeOfferCompleteEventArgs.EventId, OnMergeStateChange);

            timer.OnReset();
        }

        private void Update()
        {
            timer.OnUpdate(Time.deltaTime, Time.fixedDeltaTime);
        }

        private void Refresh()
        {
            if (MergeManager.Instance.CheckActivityHasStarted())
            {
                int curLevel = MergeManager.PlayerData.GetCurMergeOfferLevel();
                IDataTable<DRMergeOffer> dataTable = MergeManager.DataTable.GetDataTable<DRMergeOffer>(MergeManager.Instance.GetMergeDataTableName());
                if (curLevel > dataTable.Count)
                {
                    gameObject.SetActive(false);
                    return;
                }

                gameObject.SetActive(true);
                button.SetBtnEvent(OnButtonClick);
                timer.OnReset();
                timer.CountdownOver += OnCountdownOver;
                timer.StartCountdown(MergeManager.Instance.GetActivityEndTime());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnCountdownOver(object sender, CountdownOverEventArgs e)
        {
            gameObject.SetActive(false);
        }

        private void OnButtonClick()
        {
            GameManager.UI.ShowUIForm(MergeManager.Instance.GetMergeMenuName("MergeOfferMenu"));
        }

        private void OnMergeStateChange(object sender, GameEventArgs e)
        {
            Refresh();
        }
    }
}
