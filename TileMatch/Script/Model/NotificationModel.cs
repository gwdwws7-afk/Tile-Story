
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json;

namespace MySelf.Model
{
    public class NotificationModelData
    {
        public bool IsNotificationForbidden;
        public DateTime NotificationScheduledWeek = Constant.GameConfig.DateTimeMin;
        public int LastNotificationRandomNum;
    }
    public class NotificationModel : BaseModel<NotificationModel, NotificationModelData>
    {
        public void SetNotificationMuted(bool isForbidden)
        {
            if (Data.IsNotificationForbidden == isForbidden) return;

            Data.IsNotificationForbidden = isForbidden;
            SaveToLocal();
        }

        public void SetNotificationScheduledWeek(DateTime inputDateTime)
        {
            if (Data.NotificationScheduledWeek == inputDateTime) return;

            Data.NotificationScheduledWeek = inputDateTime;
            SaveToLocal();
        }

        public void SetLastNotificationRandomNum(int inputLastNotificationRandomNum)
        {
            if (Data.LastNotificationRandomNum == inputLastNotificationRandomNum) return;

            Data.LastNotificationRandomNum = inputLastNotificationRandomNum;
            //SaveToLocal();
        }
    }
}
