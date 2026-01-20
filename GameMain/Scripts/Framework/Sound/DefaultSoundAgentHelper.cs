using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 默认声音代理辅助器。
/// </summary>
public class DefaultSoundAgentHelper : SoundAgentHelperBase
{
    private Transform cachedTransform = null;
    private AudioSource audioSource = null;
    private float volumeWhenPause = 0f;
    private bool applicationPauseFlag = false;
    private EventHandler<GameEventMessage> resetSoundAgentEventHandler = null;

    /// <summary>
    /// 获取当前是否正在播放。
    /// </summary>
    public override bool IsPlaying
    {
        get
        {
            return audioSource.isPlaying;
        }
    }

    /// <summary>
    /// 获取声音长度。
    /// </summary>
    public override float Length
    {
        get
        {
            return audioSource.clip != null ? audioSource.clip.length : 0;
        }
    }

    /// <summary>
    /// 获取或设置播放位置。
    /// </summary>
    public override float Time
    {
        get
        {
            if (audioSource.clip != null) 
                return audioSource.time;
            else
                return 0;
        }
        set
        {
            if (audioSource.clip != null) 
                audioSource.time = value;
        }
    }

    /// <summary>
    /// 获取或设置是否静音。
    /// </summary>
    public override bool Mute
    {
        get
        {
            return audioSource.mute;
        }
        set
        {
            audioSource.mute = value;
        }
    }

    /// <summary>
    /// 获取或设置是否循环播放。
    /// </summary>
    public override bool Loop
    {
        get
        {
            return audioSource.loop;
        }
        set
        {
            audioSource.loop = value;
        }
    }

    /// <summary>
    /// 获取或设置声音优先级。
    /// </summary>
    public override int Priority
    {
        get
        {
            return 128 - audioSource.priority;
        }
        set
        {
            audioSource.priority = 128 - value;
        }
    }

    /// <summary>
    /// 获取或设置音量大小。
    /// </summary>
    public override float Volume
    {
        get
        {
            return audioSource.volume;
        }
        set
        {
            audioSource.volume = value;
        }
    }

    /// <summary>
    /// 获取或设置声音音调。
    /// </summary>
    public override float Pitch
    {
        get
        {
            return audioSource.pitch;
        }
        set
        {
            audioSource.pitch = value;
        }
    }

    /// <summary>
    /// 获取或设置声音立体声声相。
    /// </summary>
    public override float PanStereo
    {
        get
        {
            return audioSource.panStereo;
        }
        set
        {
            audioSource.panStereo = value;
        }
    }

    /// <summary>
    /// 获取或设置声音空间混合量。
    /// </summary>
    public override float SpatialBlend
    {
        get
        {
            return audioSource.spatialBlend;
        }
        set
        {
            audioSource.spatialBlend = value;
        }
    }

    /// <summary>
    /// 获取或设置声音最大距离。
    /// </summary>
    public override float MaxDistance
    {
        get
        {
            return audioSource.maxDistance;
        }

        set
        {
            audioSource.maxDistance = value;
        }
    }

    /// <summary>
    /// 获取或设置声音多普勒等级。
    /// </summary>
    public override float DopplerLevel
    {
        get
        {
            return audioSource.dopplerLevel;
        }
        set
        {
            audioSource.dopplerLevel = value;
        }
    }

    /// <summary>
    /// 获取或设置声音代理辅助器所在的混音组。
    /// </summary>
    public override AudioMixerGroup AudioMixerGroup
    {
        get
        {
            return audioSource.outputAudioMixerGroup;
        }
        set
        {
            audioSource.outputAudioMixerGroup = value;
        }
    }

    /// <summary>
    /// 重置声音代理事件。
    /// </summary>
    public override event EventHandler<GameEventMessage> ResetSoundAgent
    {
        add
        {
            resetSoundAgentEventHandler -= value;
            resetSoundAgentEventHandler += value;
        }
        remove
        {
            resetSoundAgentEventHandler -= value;
        }
    }

    private void Awake()
    {
        cachedTransform = transform;
        audioSource = gameObject.GetOrAddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
    }

    private void Update()
    {
        if (!applicationPauseFlag && !IsPlaying && audioSource.clip != null && resetSoundAgentEventHandler != null)
        {
            resetSoundAgentEventHandler(this, null);
        }
    }

    private void OnApplicationPause(bool pause)
    {
        applicationPauseFlag = pause;
    }

    /// <summary>
    /// 播放声音。
    /// </summary>
    /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
    public override void Play(float fadeInSeconds)
    {
        StopAllCoroutines();

        audioSource.Play();
        if (fadeInSeconds > 0)
        {
            float volume = audioSource.volume;
            audioSource.volume = 0;
            StartCoroutine(FadeToVolume(audioSource, volume, fadeInSeconds));
        }
    }

    /// <summary>
    /// 停止播放声音。
    /// </summary>
    /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
    public override void Stop(float fadeOutSeconds)
    {
        StopAllCoroutines();

        if (fadeOutSeconds > 0 && gameObject.activeInHierarchy)
        {
            StartCoroutine(StopCo(fadeOutSeconds));
        }
        else
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// 暂停播放声音。
    /// </summary>
    /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
    public override void Pause(float fadeOutSeconds)
    {
        StopAllCoroutines();

        volumeWhenPause = audioSource.volume;
        if (fadeOutSeconds > 0 && gameObject.activeInHierarchy)
        {
            StartCoroutine(PauseCo(fadeOutSeconds));
        }
        else
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// 恢复播放声音。
    /// </summary>
    /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
    public override void Resume(float fadeInSeconds)
    {
        StopAllCoroutines();

        audioSource.UnPause();
        if (fadeInSeconds > 0)
        {
            StartCoroutine(FadeToVolume(audioSource, volumeWhenPause, fadeInSeconds));
        }
        else
        {
            audioSource.volume = volumeWhenPause;
        }
    }

    /// <summary>
    /// 重置声音代理辅助器。
    /// </summary>
    public override void Reset()
    {
        cachedTransform.localPosition = Vector3.zero;
        audioSource.clip = null;
        volumeWhenPause = 0f;
    }

    /// <summary>
    /// 设置声音资源。
    /// </summary>
    /// <param name="soundAsset">声音资源。</param>
    /// <returns>是否设置声音资源成功。</returns>
    public override bool SetSoundAsset(object soundAsset)
    {
        AudioClip audioClip = soundAsset as AudioClip;

        if (audioClip == null)
        {
            return false;
        }

        audioSource.clip = audioClip;
        return true;
    }

    /// <summary>
    /// 设置声音所在的世界坐标。
    /// </summary>
    /// <param name="worldPosition">声音所在的世界坐标。</param>
    public override void SetWorldPosition(Vector3 worldPosition)
    {
        cachedTransform.position = worldPosition;
    }

    private IEnumerator StopCo(float fadeToSeconds)
    {
        yield return FadeToVolume(audioSource, 0f, fadeToSeconds);
        audioSource.Stop();
    }

    private IEnumerator PauseCo(float fadeToSeconds)
    {
        yield return FadeToVolume(audioSource, 0f, fadeToSeconds);
        audioSource.Pause();
    }

    private IEnumerator FadeToVolume(AudioSource audioSource, float volume, float duration)
    {
        float time = 0;
        float originalVolume = audioSource.volume;
        while (time < duration)
        {
            time += UnityEngine.Time.deltaTime;
            audioSource.volume = Mathf.Lerp(originalVolume, volume, time / duration);
            yield return new WaitForEndOfFrame();
        }
        audioSource.volume = volume;
    }
}
