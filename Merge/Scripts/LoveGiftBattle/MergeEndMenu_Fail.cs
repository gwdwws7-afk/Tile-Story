using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MergeEndMenu_Fail : MergeEndMenu_LoveGiftBattle
    {
        protected override void OnCloseButtonClick()
        {
            base.OnCloseButtonClick();

            MergeManager.Instance.EndActivity();
        }
    }
}
