using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogJumpScrollPanel : MonoBehaviour
{
    [SerializeField] private FrogLeafManager[] LeafManagers;
    private int partIndex;
    public void Init(int index)
    {
        partIndex = index;
        if (index == 0)
        {
            LeafManagers[0].Init(0);
            LeafManagers[1].Init(1);
        }
        else
        {
            for(int i=0;i<LeafManagers.Length;i++)
            {
                LeafManagers[i].Init(index * 3 + i - 1);
            }
        }
    }
    
    public void OnReset()
    {
        gameObject.SetActive(false);
        for(int i=0;i<LeafManagers.Length;i++)
        {
            LeafManagers[i].OnReset();
        }
    }
}
