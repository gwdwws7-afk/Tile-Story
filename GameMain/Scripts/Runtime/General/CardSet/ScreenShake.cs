using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    // 震动持续时间
    public float shakeDuration = 0.5f;
    
    // 震动强度
    public float shakeMagnitude = 0.1f;
    
    // 平滑过渡速度
    public float dampingSpeed = 1.0f;
    
    // 初始位置
    private Vector3 initialPosition;
    
    void Awake()
    {
        // 保存相机初始位置
        if (Camera.main != null)
        {
            initialPosition = Camera.main.transform.localPosition;
        }
    }
    
    void Update()
    {
        // 按下空格键触发震动
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(Shake());
        }
    }
    
    // 震动协程
    IEnumerator Shake()
    {
        float elapsed = 0.0f;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            
            // 计算震动强度（随时间衰减）
            float strength = shakeMagnitude * (1 - (elapsed / shakeDuration));
            
            // 在X和Y轴上生成随机偏移
            Vector3 randomOffset = Random.insideUnitSphere * strength;
            randomOffset.z = 0; // 保持Z轴不变
            
            // 应用偏移到相机位置
            Camera.main.transform.localPosition = initialPosition + randomOffset;
            
            // 等待下一帧
            yield return null;
        }
        
        // 恢复相机初始位置
        Camera.main.transform.localPosition = initialPosition;
    }
}