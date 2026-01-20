using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MySelf
{
    [Serializable]
    public class Reward
    {
        public int itemId;
        public int number;

        //1:300
        public Reward(string inputString)
        {
            string[] splitString = inputString.Split(':');
            if (splitString.Length == 2)
            {
                itemId = int.Parse(splitString[0]);
                number = int.Parse(splitString[1]);
            }
            else
            {
                Debug.LogError("Unexpected splitString.Length = " + splitString.Length);
            }
        }
    }
}