using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class WellDoneTaskControl : MonoBehaviour
{
    [SerializeField] private UICommonController TaskController;

    private List<int> daliyTaskList = new List<int>();
    private List<int> allTimeTaskList = new List<int>();
   public void ShowAnim()
   {
       var data = GetData();
       daliyTaskList.Clear();
       allTimeTaskList.Clear();

       if (data.Item2.Count > 0)
       {
           var list = data.Item2.OrderByDescending(id=>id).ToList();
           foreach (var id in list)
           {
               allTimeTaskList.Add(id);
               break;
           }
       }
       else
       {
           var list = data.Item1.OrderByDescending(id=>id).ToList();
           foreach (var id in list)
           {
               daliyTaskList.Add(id);
               break;
           }
       }
       //
       // foreach (var id in data.Item1)
       // {
       //     daliyTaskList.Add(id);
       // }
       // foreach (var id in data.Item2)
       // {
       //     allTimeTaskList.Add(id);
       // }
       PlayAnim();
   }

   public void ClearAnim()
   {
       daliyTaskList.Clear();
       allTimeTaskList.Clear();
       TaskController.DOKill();
       TaskController.transform.localPosition = new Vector2(0, 600);
   }

   private void PlayAnim()
   {
       if(daliyTaskList.Count<=0&&allTimeTaskList.Count<=0)return;

       ObjectiveData m_Data = null;
       if (daliyTaskList.Count > 0)
       {
           var objectiveId=daliyTaskList[0];
           daliyTaskList.RemoveAt(0);
           DTDailyObjective dtObjective = GameManager.DataTable.GetDataTable<DTDailyObjective>().Data;
           m_Data = dtObjective.GetData(objectiveId);
       }
       else
       {
           var objectiveId=allTimeTaskList[0];
           allTimeTaskList.RemoveAt(0);
           var dtObjective = GameManager.DataTable.GetDataTable<DTAllTimeObjective>().Data;
           m_Data = dtObjective.GetData(objectiveId);
       }

       bool isLast = daliyTaskList.Count <= 0 && allTimeTaskList.Count <= 0;

       TaskController.DOKill();
       TaskController.Images[0].sprite=GetTargetSprite(m_Data.Type.ToString(), "Objective");
       TaskController.TextMeshProUGUILocalizes[0].SetTerm(ObjectiveColumn.GetTitleTerm(m_Data.Type));
       TaskController.TextMeshProUGUILocalizes[0].SetParameterValue("Num", m_Data.TargetNum.ToString());

       TaskController.transform.localPosition = new Vector2(0, 600);
       TaskController.transform.DOLocalMoveY(-130f,0.2f).SetEase(Ease.InSine).OnComplete(() =>
       {
           if (!isLast)
           {
               TaskController.transform.DOLocalMoveX(-1400, 0.2f).SetEase(Ease.InOutSine).SetDelay(1f).OnComplete(() =>
               {
                   PlayAnim();
               });
           }
       });
   }

   private (List<int>,List<int>) GetData()
   {
      //收集已经完成的关卡ID 然后与进入关卡时的ID对比，做展示预备
      var lastNoCompleteIds = GameManager.DataNode.GetData<(List<int>,List<int>)>("NoCompleteObjectiveIds",(new List<int>(),new List<int>()));
      var objectiveIds =GameManager.Objective.GetObjectiveCompleteIds(lastNoCompleteIds);
      return objectiveIds;
   }
   
   private Dictionary<string, Sprite> m_SpriteDic = new Dictionary<string, Sprite>();
   private void OnDestroy()
   {
       try
       {
           foreach (KeyValuePair<string, Sprite> pair in m_SpriteDic)
           {
               try
               {
                   Addressables.Release(pair.Value);
               }
               catch (Exception e)
               {
                   Log.Debug(e);
               }
           }
           m_SpriteDic.Clear();
       }
       catch
       {
           m_SpriteDic.Clear();
       }
   }

   public Sprite GetTargetSprite(string spriteName, string atlasName)
   {
       if (m_SpriteDic.TryGetValue(spriteName, out Sprite sp)) 
       {
           return sp;
       }
       else
       {
           string atlasedSpriteAddress = $"{atlasName}[{spriteName}]";
           Sprite sprite = AddressableUtils.LoadAsset<Sprite>(atlasedSpriteAddress);
           m_SpriteDic.Add(spriteName, sprite);
           return sprite;
       }
   }
}
