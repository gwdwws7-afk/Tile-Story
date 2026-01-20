using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Merge
{
    public class MaxProp : Prop
    {
        public SpriteRenderer m_Sprite;
        public SpriteRenderer m_MaxSprite;

        public override void SetLayer(string layerName, int sortOrder)
        {
            base.SetLayer(layerName, sortOrder);

            m_Sprite.sortingLayerName = layerName;
            m_Sprite.sortingOrder = sortOrder;

            if (m_MaxSprite != null)
            {
                m_MaxSprite.sortingLayerName = layerName;
                m_MaxSprite.sortingOrder = sortOrder + 1;
            }
        }

        public override void OnGeneratedByMerge()
        {
            base.OnGeneratedByMerge();

            if (PropLogic != null)
            {
                IDataTable<DRMergeGenerateBubble> dataTable = MergeManager.DataTable.GetDataTable<DRMergeGenerateBubble>(MergeManager.Instance.GetMergeDataTableName());
                var data = dataTable.GetDataRow(PropLogic.PropId);
                if (data != null)
                {
                    int randomNum = Random.Range(1, 101);
                    if (randomNum <= data.GenerateBubbleProbability)
                    {
                        Square randomSquare = MergeManager.Merge.GetNearestEmptySquare(PropLogic.Square);
                        if (randomSquare != null)
                        {
                            MergeManager.Merge.GenerateProp(data.GenerateBubble, 1, transform.position, randomSquare, PropMovementState.Bouncing);
                        }
                    }
                }
            }
        }
    }
}
