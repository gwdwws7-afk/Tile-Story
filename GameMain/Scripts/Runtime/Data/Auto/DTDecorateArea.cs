using System;
using System.Collections.Generic;

[Serializable]
public class DTDecorateArea
{
    public List<DecorateArea> decorateArea;
    public DecorateArea GetDecorateArea(Predicate<DecorateArea> condition)
    {
        for (int i = 0; i < decorateArea.Count; i++)
        {
            if (condition(decorateArea[i]))
            {
                return decorateArea[i];
            }
        }
        return null;
    }
}
