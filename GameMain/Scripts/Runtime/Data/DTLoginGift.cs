using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DTLoginGift
{
    public List<LoginGiftData> LoginGiftDatas;

    public LoginGiftData GetLoginGift(int day)
    {
        for (int i = 0; i < LoginGiftDatas.Count; i++)
        {
            if (LoginGiftDatas[i].Day == day)
                return LoginGiftDatas[i];
        }

        return null;
    }
}
