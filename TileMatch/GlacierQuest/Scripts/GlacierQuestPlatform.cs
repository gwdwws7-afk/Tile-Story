using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class GlacierQuestPlatform : MonoBehaviour
{
    public Transform success, fail;

    public SkeletonGraphic platformAnim;
    public GameObject effect_back, effect2_font;
    public Transform fire_back, fire_font, fire_bottom;
    public Transform smoke_back, smoke_font;
    public float value_fire = 0f;
    public float value_smoke = -0.01f;
    public float value_bottom = 0f;
    public float time_fire = 1.9f;
    public float time_smoke = 1.9f;
    public float time_bottom = 1.9f;

    public int animIndex = 1;

    public void Init()
    {
        if (platformAnim)
            platformAnim.AnimationState.SetAnimation(0, $"idle0{animIndex}", true);
        if (effect_back)
        {
            effect_back.SetActive(false);
            effect2_font.SetActive(false);
        }
    }

    private int delayTaskId = -1;

    public void PlayAnim()
    {
        if (platformAnim)
        {
            platformAnim.AnimationState.SetAnimation(0, $"active0{animIndex}", false);
        }

        if (effect_back)
        {
            effect_back.SetActive(true);
            effect2_font.SetActive(true);
            GameManager.Task.AddDelayTriggerTask(0.3f, () =>
            {
                fire_back.DOScale(value_fire, time_fire).SetEase(Ease.InCubic);
                fire_font.DOScale(value_fire, time_fire).SetEase(Ease.InCubic);
                smoke_back.DOLocalMoveY(value_smoke, time_smoke);
                smoke_font.DOLocalMoveY(value_smoke, time_smoke);
                fire_bottom.DOScale(value_bottom, time_bottom).SetEase(Ease.InCubic);
            });
        }
        // 延时6s是等待特效播放完毕
        delayTaskId = GameManager.Task.AddDelayTriggerTask(6f, () =>
        {
            delayTaskId = -1;
            gameObject.SetActive(false);
        });
    }

    private void OnDisable()
    {
        GameManager.Task.RemoveDelayTriggerTask(delayTaskId);
    }

#if UNITY_EDITOR
    public bool canSink = true;
    Vector3 scale_back, scale_font, scale_bottom;

    void OnEnable()
    {
        if (fire_back)
        {
            //初始化值
            scale_back = fire_back.localScale;
            scale_font = fire_font.localScale;
            scale_bottom = fire_bottom.localScale;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (platformAnim != null)
            {
                platformAnim.AnimationState.SetAnimation(0, $"idle0{animIndex}", true);
            }

            if (fire_back)
            {
                fire_back.localScale = scale_back;
                fire_font.localScale = scale_font;
                fire_bottom.localScale = scale_bottom;
                effect_back.SetActive(false);
                effect2_font.SetActive(false);
                smoke_back.localPosition = Vector3.zero;
                smoke_font.localPosition = Vector3.zero;
            }
            PlayAnim();
        }
    }
#endif
}
