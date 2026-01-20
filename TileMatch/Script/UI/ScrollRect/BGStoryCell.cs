using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;
using MySelf.Model;

public class BGStoryCell : MonoBehaviour
{
   [SerializeField] private Image Bg_Image;
   [SerializeField] private UIEffect Bg_UIEffect;
   [SerializeField] private GameObject Lock;
   [SerializeField] private ExtensionsToggle Toggle;
   [SerializeField]private SkeletonGraphic Ok_Anim;
   [SerializeField] private GameObject Norm_F;
   [SerializeField] private GameObject RedPoint_Obj;
   
   private int storyId = 0;
   public void Init(int storyId, bool isUnLock)
   {
      int storyBgId = 1000 + DecorationModel.Instance.GetAlteredDecorationAreaID(storyId);
      RedPoint_Obj.gameObject.SetActive(GameManager.PlayerData.IsShowBGRedPointById(storyBgId));
      this.storyId = storyId;
      Bg_Image.sprite = BGSmallUtil.GetSprite(storyBgId);
      transform.name = storyBgId.ToString();
      Lock.gameObject.SetActive(!isUnLock);
      Bg_UIEffect.effectMode = isUnLock?EffectMode.None:EffectMode.Grayscale;
      transform.localScale = Vector3.one;

      Norm_F.gameObject.SetActive(true);
      Toggle.IsOn = IsCurBG();

      Toggle.onPointerDown = OnPointerDown;
      Toggle.onPointerUp = OnPointerUp;
      Toggle.onValueCanChanged = IsCanToggle;
      Toggle.SetBtnEvent((isActive) =>
      {
         if (isActive)
         {
            if (!Toggle.IsOn)
            {
               GameManager.Sound.PlayAudio(SoundType.SFX_Click.ToString());
            }

            GameManager.PlayerData.BGImageIndex = storyBgId;
            Toggle.IsOn = true;
         }
         Norm_F.gameObject.SetActive(!isActive);
         GameManager.PlayerData.RemoveShowBGRedPoint(storyBgId);
         RedPoint_Obj.gameObject.SetActive(false);
      });
   }

    private void OnPointerDown()
    {
        bool isUnLock = IsUnlock();
        if (!isUnLock)
        {
            Lock.transform.DOShakePosition(0.2f, new Vector3(5, 0, 0), 20, 90, false, false, ShakeRandomnessMode.Harmonic);
            return;
        }

        transform.DOKill();
        transform.DOScale(Vector3.one * 1.04f, 0.1f);
    }

    private void OnPointerUp()
    {
        transform.DOKill();
        transform.DOScale(Vector3.one, 0.1f);
    }

    private bool IsCanToggle()
   {
      bool isCurBG = IsCurBG();
      
      if (isCurBG)
      {
         Toggle.IsOn = true;
         return true;
      }

      Norm_F.gameObject.SetActive(true);
      bool isUnLock = IsUnlock();
      if (!isUnLock)
      {
         GameManager.UI.ShowWeakHint("Common.Complete Chapter {0} of Story to unlock",Vector3.zero,storyId.ToString());
         return false;
      }
      else if(!isCurBG)
      {
         Norm_F.gameObject.SetActive(false);
         Ok_Anim.AnimationState.ClearTracks();
         Ok_Anim.Skeleton.SetToSetupPose();
         Ok_Anim.AnimationState.SetAnimation(0,"animation",false);
         return true;
      }
      return false;
   }
   
   private bool IsCurBG()
   {
        return GameManager.PlayerData.BGImageIndex == 1000 + DecorationModel.Instance.GetAlteredDecorationAreaID(storyId);
   }

   private bool IsUnlock()
   {
      return storyId<=GameManager.PlayerData.GetHighestFinishedDecorationAreaID();
   }
}
