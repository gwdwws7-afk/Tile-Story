using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class KeyProp : MaxProp
    {
        public override Square OnPutOn(PropLogic prop)
        {
            //lock chest put on
            if (MovementState == PropMovementState.Static && !PropLogic.IsPetrified)
            {
                if (prop.PropId / 10000 == 3 && !PropLogic.IsSilenced && !prop.IsSilenced && prop.Prop != null)  
                {
                    Square square = PropLogic.Square;
                    ChestProp_DigTreasure chestProp = prop.Prop as ChestProp_DigTreasure;

                    if (!chestProp.IsUnLock)
                    {
                        MergeManager.Merge.ReleaseProp(PropLogic);
                        chestProp.UnlockChest(false);
                    }

                    return square;
                }
            }

            return base.OnPutOn(prop);
        }
    }
}
