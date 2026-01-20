using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class NotificationData
{
    public NotificationKey Key;
    public DayOfWeek DayOfWeek;
    public int Hours;
    public int Minutes;
}
