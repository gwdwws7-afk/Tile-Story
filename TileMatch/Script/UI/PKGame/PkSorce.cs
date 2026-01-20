using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using DG.Tweening;
using MySelf.Model;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PkSorce : MonoBehaviour,IItemFlyReceiver
{
   [SerializeField] private Image SelfHead_Image,TargetHead_Image;
   [SerializeField] private TextMeshProUGUI SelfSorce_Text,TargetSorce_Text;
   [SerializeField] private Image SelfSorcePro_Image,TargetSorcePro_Image;
   [SerializeField] private TextMeshProUGUI SelfName_Text,TargetName_Text;
   [SerializeField] private Text SelfName_Text1, TargetName_Text1;
   [SerializeField] private GameObject ReceiverObj;

   [SerializeField] private RectTransform AnimRoot;
   [SerializeField] private Transform[] HeadItems;
   [SerializeField] private ParticleSystem ParticleSystem;

   public bool IsShowPercent = true;

   private int recordSelfSorce = 0;
   private int recordTargetSorce = 0;
   public void Init(PkGameOverStatus overStatus=PkGameOverStatus.Lose,bool isAutoProgressAnim=true,bool isSetTargetName=true)
   {
      TargetHead_Image.enabled = true;
      RewardManager.Instance.RegisterItemFlyReceiver(this);
      //初始化 获取展示数据
      PkGamePlayerData self = PkGameModel.Instance.Data.SelfPlayerPkData;
      PkGamePlayerData target = PkGameModel.Instance.Data.TargetPlayerPkData;
      //展示当前UI  头像、名称、分数
      recordSelfSorce = PkGameModel.Instance.Data.SelfOldSorce;
      recordTargetSorce = PkGameModel.Instance.Data.TargetOldSorce;
      
      SetHeadPortrait(SelfHead_Image,GameManager.PlayerData.HeadPortrait,overStatus);
      SetHeadPortrait(TargetHead_Image,target.PlayerHeadPortrait,overStatus);
      SetSelfNameText(GameManager.PlayerData.PlayerName);
      if(isSetTargetName)SetTargetNameText(target.PlayerName);
      float selfProgress = GetPercent(recordSelfSorce,recordTargetSorce);
      SetSorceText(SelfSorce_Text,SelfSorcePro_Image,recordSelfSorce,selfProgress);
      SetSorceText(TargetSorce_Text,TargetSorcePro_Image,recordTargetSorce,1-selfProgress);
      
      //展示新的
      if(isAutoProgressAnim)SetProgress();
   }

   private void OnDisable()
   {
      // if(SelfHead_Image.sprite!=null&&(SelfHead_Image.sprite.name!="0"&&SelfHead_Image.sprite.name!="0_0"))
      //    AddressableUtils.ReleaseAsset(SelfHead_Image.sprite);
      // if(TargetHead_Image.sprite!=null&&(TargetHead_Image.sprite.name!="0"&&TargetHead_Image.sprite.name!="0_0"))
      //    AddressableUtils.ReleaseAsset(TargetHead_Image.sprite);
      
      // Resources.UnloadAsset(SelfHead_Image.sprite);
      // Resources.UnloadAsset(TargetHead_Image.sprite);
      finishAction = null;
      if(ParticleSystem!=null)ParticleSystem.gameObject.SetActive(false);
      RewardManager.Instance.UnregisterItemFlyReceiver(this);
   }

   //头像
   private void SetHeadPortrait(Image image,int headPortrait,PkGameOverStatus overStatus)
   {
      string headPortraitName = $"HeadPortrait_{headPortrait}";
      //if (image.sprite != null && image.sprite.name == headPortrait.ToString()) return;

      // if(image.sprite!=null)AddressableUtils.ReleaseAsset(image.sprite);
      image.sprite=AddressableUtils.LoadAsset<Sprite>(headPortraitName);
   }

   //名称
   public void SetSelfNameText(string name)
   {
      if(SelfName_Text)SelfName_Text.text = name;
      if(SelfName_Text1)SelfName_Text1.text = name;
   }
   public void SetTargetNameText(string name)
   {
      if(TargetName_Text)TargetName_Text.text = name;
      if(TargetName_Text1)TargetName_Text1.text = name;
   }

   //分数
   private void SetSorceText(TextMeshProUGUI sorceText,Image image,int sorce,float progress)
   {
      if(IsShowPercent && image)image.rectTransform.sizeDelta = new Vector2(160 * progress + 250,97);
      sorceText.text = sorce.ToString();
   }

   //进度条
   public void SetProgress(bool isForceAnim=false)
   {
      int newSelfSorce = PkGameModel.Instance.Data.SelfPlayerPkData.ItemNum;
      int newTargetSorce = PkGameModel.Instance.Data.TargetPlayerPkData.ItemNum;
      
      if(!isForceAnim&&newSelfSorce==recordSelfSorce&&
         newTargetSorce==recordTargetSorce)return;

      TargetSorce_Text.text = newTargetSorce.ToString();
      SelfSorce_Text.text = newSelfSorce.ToString();
      float percent = GetPercent(newSelfSorce,newTargetSorce);
      DOTween.To(() => SelfSorcePro_Image.rectTransform.sizeDelta,
         (d) => SelfSorcePro_Image.rectTransform.sizeDelta = d,
         new Vector2(160 * percent + 250,97),
         0.8f).onComplete+= () =>
      {
         //进度条走完之后更新分数信息
         PkGameModel.Instance.RecordOldSorce();
      };
      DOTween.To(() => TargetSorcePro_Image.rectTransform.sizeDelta,
         (d) => TargetSorcePro_Image.rectTransform.sizeDelta = d,
         new Vector2(160 * (1 - percent) + 250, 97),
         0.8f);
   }

   private bool isStartAnim = false;
   private float delayTime = 2f;
   private float curSpeed = 0f;
   private float maxSpeed = 1000;
   private Action finishAction;
   
   public void ShowMatchTargetPlayerStartAnim()
   {
      if(AnimRoot==null||HeadItems==null)return;
      //设置动画初始状态
      curSpeed = 0f;
      AnimRoot.gameObject.SetActive(true);
      TargetHead_Image.enabled = false;
      AnimRoot.localPosition = new Vector3(0, 780, 0);
      //匹配对手开始 
      //动画
      isStartAnim = true;
      delayTime = 3f;
   }

   public void ShowMatchTargetPlayerOverAnim(Action finishAction)
   {
      if(AnimRoot==null||HeadItems==null)return;
      this.finishAction = finishAction;
      //匹配对手结束  
      GameManager.Task.AddDelayTriggerTask(delayTime, () =>
      {
         isStartAnim = false;
         var curTargetHeadId = PkGameModel.Instance.Data.TargetPlayerPkData.PlayerHeadPortrait;
         var movePos = HeadItems[curTargetHeadId].localPosition.y + AnimRoot.localPosition.y;
         AnimRoot.DOLocalMoveY(AnimRoot.localPosition.y-movePos,0.01f).SetEase(Ease.Linear).onComplete+= () =>
         {
            //刷新对方名字
            PkGamePlayerData target = PkGameModel.Instance.Data.TargetPlayerPkData;
            SetTargetNameText(PkGameModel.Instance.Data.TargetPlayerPkData.PlayerName);
            SetHeadPortrait(TargetHead_Image,target.PlayerHeadPortrait,PkGameOverStatus.Win);
            TargetHead_Image.enabled = true;
            finishAction?.Invoke();
            finishAction = null;
         };
      });
   }

   private int recordNum = 800;
   private void Update()
   {
      if(!isStartAnim)return;
      if(AnimRoot==null||HeadItems==null)return;
      //加速
      if (isStartAnim)
      {
         curSpeed +=Time.deltaTime*recordNum;
         curSpeed = UnityEngine.Mathf.Min(curSpeed, maxSpeed);
      }

      AnimRoot.localPosition -= new Vector3(0,curSpeed * Time.deltaTime,0);
      //底下的物体换到上方去
      for (int i = 0; i < HeadItems.Length; i++)
      {
         var head = HeadItems[i];
         var yPos= head.transform.localPosition.y + AnimRoot.localPosition.y;
         if (yPos <= -160)
         {
            GameManager.Sound.PlayAudio(SoundType.FPX_Champion_Stage_1.ToString());
            head.transform.localPosition += new Vector3(0, 170* 10, 0);
         }
      }
   }

   private float GetPercent(int selfScore,int targetScore)
   {
      if (selfScore == 0 && targetScore == 0) return 0.5f;
      return selfScore / (float)(selfScore + targetScore);
   }

   #region ItemFly
   public ReceiverType ReceiverType => ReceiverType.Common;

   public GameObject GetReceiverGameObject() => gameObject;

   public void OnFlyHit(TotalItemData type)
   {
      if (type == TotalItemData.Pk)
      {
         //受击动画
         if (ReceiverObj != null)
         {
            GameManager.Sound.PlayAudio("SFX_League_List_Insert");
            ReceiverObj.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
            {
               ReceiverObj.transform.localScale = Vector3.one;
               recordSelfSorce++;
      
               SelfSorce_Text.text = recordSelfSorce.ToString();
               float percent = GetPercent(recordSelfSorce,recordTargetSorce);
               DOTween.To(() => SelfSorcePro_Image.rectTransform.sizeDelta,
                  (d) => SelfSorcePro_Image.rectTransform.sizeDelta = d,
                  new Vector2(160 * percent + 250,97),
                  0.2f).onComplete+= () =>
               {
                  //进度条走完之后更新分数信息
                  PkGameModel.Instance.RecordOldSorce();
               };
               if (ParticleSystem != null)
               {
                  ParticleSystem.gameObject.SetActive(true);
                  ParticleSystem.Play();
               }
            };
         }
      }
   }

   public void OnFlyEnd(TotalItemData type)
   {
      if (type == TotalItemData.Pk)
      {
         //受击动画
         if (ReceiverObj != null)
         {
            ReceiverObj.transform.DOPunchScale(new Vector3(-0.1f, -0.1f), 0.1f, 1).onComplete = () =>
            {
               ReceiverObj.transform.localScale = Vector3.one;
               
               //分数增加、进度条移动
               SetProgress();
            };
         }
      }
   }

   public Vector3 GetItemTargetPos(TotalItemData type)
   {
      if(type==TotalItemData.Pk) return ReceiverObj.transform.position;
      return Vector3.down*2;
   }
   #endregion
}
