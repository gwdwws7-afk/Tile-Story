using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarFlyObject : MonoBehaviour
{
    public Canvas starCanvas;
    public ParticleSystemRenderer ps;
    public TrailRenderer tr;

    public void SetMapFlyLayer()
    {
        starCanvas.sortingOrder = 19;
        ps.sortingOrder = 18;
        tr.sortingOrder = 17;
    }

    public void SetDecorationFlyLayer()
    {
        starCanvas.sortingOrder = 24;
        ps.sortingOrder = 23;
        tr.sortingOrder = 22;
    }

    public void SetGameSettlementFlyLayer()
    {
        starCanvas.sortingOrder = 34;
        ps.sortingOrder = 33;
        tr.sortingOrder = 32;
    }
}
