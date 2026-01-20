using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class CanGenerateProp : GeneratorProp
    {
        public override void OnGeneratedByMerge()
        {
            base.OnGeneratedByMerge();

            if (PropLogic.PropId == 80108)
            {
                GameManager.Firebase.RecordMessageByEvent(Constant.AnalyticsEvent.Merge_Get_Final_Box);
            }
        }
    }
}
