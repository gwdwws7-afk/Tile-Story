using UnityEngine;

public sealed class ShakeController : MonoBehaviour
{
    private Material m_material;
    private float m_timer;
    private bool isShaking;

    [Header("Shake Settings")]
    public float speed = 30f;      // 震动速度
    public float strength = 1f;    // 最大强度
    public float duration = 0.5f;  // 整个震动时长

    private bool isInit = false;

    public void Init()
    {
        if (isInit) return;
        isInit = true;

        if (m_material == null)
        {
            var originalMat = AddressableUtils.LoadAsset<Material>("ShakeCameraMaterial");
            m_material = new Material(originalMat);
            // 克隆材质实例
        }
    }

    void OnEnable()
    {
        m_timer = 0;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (m_material != null && isShaking)
        {
            float timeX = Time.time;
            m_material.SetFloat("_TimeX", timeX);
            m_material.SetFloat("_Value", speed);
            m_material.SetFloat("_Value2", strength * 0.025f);
            m_material.SetFloat("_Value3", strength * 0.025f);

            // Envelope: sin曲线实现渐强→渐弱
            float t = Mathf.Clamp01(m_timer / duration);
            float envelope = Mathf.Sin(t * Mathf.PI);
            m_material.SetFloat("_Envelope", envelope);

            Graphics.Blit(src, dest, m_material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    void Update()
    {
        if (!isShaking) return;

        m_timer += Time.deltaTime;

        if (m_timer >= duration)
        {
            isShaking = false;
            m_timer = 0;
            if (m_material != null)
                m_material.SetFloat("_Envelope", 0f);

            // 震动结束，自动禁用脚本
            enabled = false;
        }
    }

    /// <summary>
    /// 开始震动
    /// </summary>
    /// <param name="speed">震动速度</param>
    /// <param name="strength">震动强度</param>
    /// <param name="duration">持续时间</param>
    public void StartShake(float speed = 30f, float strength = 1f, float duration = 0.5f)
    {
        Init();

        this.speed = speed;
        this.strength = strength;
        this.duration = duration;

        m_timer = 0;
        isShaking = true;

        enabled = true;
    }

    public void Release()
    {
        if (m_material != null)
        {
            AddressableUtils.ReleaseAsset(m_material);
            m_material = null;
        }
        isInit = false;
    }
}
