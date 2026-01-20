using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasketBar : MonoBehaviour
{
    public TextMeshProUGUI basketBarNumText;
    public ParticleSystem punchEffect;
    
    public void Init()
    {
        Refresh();
    }

    public void Refresh()
    {
        basketBarNumText.text = HarvestKitchenManager.Instance.BasketNum.ToString();
    }
}
