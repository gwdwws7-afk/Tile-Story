using System;
using System.Collections.Generic;

[Serializable]
public class DTDecorateItem
{
    public List<DecorateItem> decorateItem;
    public DecorateItem GetDecorateItem(Predicate<DecorateItem> condition)
    {
        for (int i = 0; i < decorateItem.Count; i++)
        {
            if (condition(decorateItem[i]))
            {
                return decorateItem[i];
            }
        }
        return null;
    }

    public List<DecorateItem> GetDecorateItems(int areaId)
    {
        List<DecorateItem> result = new List<DecorateItem>();
        for (int i = 0; i < decorateItem.Count; i++)
        {
            if (decorateItem[i].Belong == areaId)
            {
                result.Add(decorateItem[i]);
            }
        }
        return result;
    }

    public DecorateItem GetDecorateItem(int inputID)
    {
        for (int i = 0; i < decorateItem.Count; i++)
        {
            if (decorateItem[i].ID == inputID)
            {
                return decorateItem[i];
            }
        }
        return null;
    }


}
