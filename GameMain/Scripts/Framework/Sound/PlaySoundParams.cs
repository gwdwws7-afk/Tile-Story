
using GameFramework;
/// <summary>
/// 播放声音参数
/// </summary>
public class PlaySoundParams:IReference
{
    private bool referenced;
    private float time;
    private bool muteInSoundGroup;
    private bool loop;
    private int priority;
    private float volumeInSoundGroup;
    private float fadeInSeconds;
    private float fadeOutSeconds;
    private float pitch;
    private float panStereo;
    private float spatialBlend;
    private float maxDistance;
    private float dopplerLevel;

    /// <summary>
    /// 初始化播放声音参数的新实例。
    /// </summary>
    public PlaySoundParams()
    {
        referenced = false;
        time = Constant.SoundConfig.DefaultTime;
        muteInSoundGroup = Constant.SoundConfig.DefaultMute;
        loop = Constant.SoundConfig.DefaultLoop;
        priority = Constant.SoundConfig.DefaultPriority;
        volumeInSoundGroup = Constant.SoundConfig.DefaultVolume;
        fadeInSeconds = Constant.SoundConfig.DefaultFadeInSeconds;
        fadeOutSeconds = Constant.SoundConfig.DefaultFadeOutSeconds;
        pitch = Constant.SoundConfig.DefaultPitch;
        panStereo = Constant.SoundConfig.DefaultPanStereo;
        spatialBlend = Constant.SoundConfig.DefaultSpatialBlend;
        maxDistance = Constant.SoundConfig.DefaultMaxDistance;
        dopplerLevel = Constant.SoundConfig.DefaultDopplerLevel;
    }

    /// <summary>
    /// 获取或设置播放位置。
    /// </summary>
    public float Time
    {
        get
        {
            return time;
        }
        set
        {
            time = value;
        }
    }

    /// <summary>
    /// 获取或设置在声音组内是否静音。
    /// </summary>
    public bool MuteInSoundGroup
    {
        get
        {
            return muteInSoundGroup;
        }
        set
        {
            muteInSoundGroup = value;
        }
    }

    /// <summary>
    /// 获取或设置是否循环播放。
    /// </summary>
    public bool Loop
    {
        get
        {
            return loop;
        }
        set
        {
            loop = value;
        }
    }

    /// <summary>
    /// 获取或设置声音优先级。
    /// </summary>
    public int Priority
    {
        get
        {
            return priority;
        }
        set
        {
            priority = value;
        }
    }

    /// <summary>
    /// 获取或设置在声音组内音量大小。
    /// </summary>
    public float VolumeInSoundGroup
    {
        get
        {
            return volumeInSoundGroup;
        }
        set
        {
            volumeInSoundGroup = value;
        }
    }

    /// <summary>
    /// 获取或设置声音淡入时间，以秒为单位。
    /// </summary>
    public float FadeInSeconds
    {
        get
        {
            return fadeInSeconds;
        }
        set
        {
            fadeInSeconds = value;
        }
    }

    /// <summary>
    /// 获取或设置声音淡出时间，以秒为单位。
    /// </summary>
    public float FadeOutSeconds
    {
        get
        {
            return fadeOutSeconds;
        }
        set
        {
            fadeOutSeconds = value;
        }
    }

    /// <summary>
    /// 获取或设置声音音调。
    /// </summary>
    public float Pitch
    {
        get
        {
            return pitch;
        }
        set
        {
            pitch = value;
        }
    }

    /// <summary>
    /// 获取或设置声音立体声声相。
    /// </summary>
    public float PanStereo
    {
        get
        {
            return panStereo;
        }
        set
        {
            panStereo = value;
        }
    }

    /// <summary>
    /// 获取或设置声音空间混合量。
    /// </summary>
    public float SpatialBlend
    {
        get
        {
            return spatialBlend;
        }
        set
        {
            spatialBlend = value;
        }
    }

    /// <summary>
    /// 获取或设置声音最大距离。
    /// </summary>
    public float MaxDistance
    {
        get
        {
            return maxDistance;
        }
        set
        {
            maxDistance = value;
        }
    }

    /// <summary>
    /// 获取或设置声音多普勒等级。
    /// </summary>
    public float DopplerLevel
    {
        get
        {
            return dopplerLevel;
        }
        set
        {
            dopplerLevel = value;
        }
    }

    internal bool Referenced
    {
        get
        {
            return referenced;
        }
    }

    /// <summary>
    /// 创建播放声音参数。
    /// </summary>
    /// <returns>创建的播放声音参数。</returns>
    public static PlaySoundParams Create()
    {
        PlaySoundParams playSoundParams = ReferencePool.Acquire<PlaySoundParams>();
        //PlaySoundParams playSoundParams = new PlaySoundParams();
        playSoundParams.referenced = true;
        return playSoundParams;
    }

    /// <summary>
    /// 清理播放声音参数。
    /// </summary>
    public void Clear()
    {
        referenced = false;
        time = Constant.SoundConfig.DefaultTime;
        muteInSoundGroup = Constant.SoundConfig.DefaultMute;
        loop = Constant.SoundConfig.DefaultLoop;
        priority = Constant.SoundConfig.DefaultPriority;
        volumeInSoundGroup = Constant.SoundConfig.DefaultVolume;
        fadeInSeconds = Constant.SoundConfig.DefaultFadeInSeconds;
        pitch = Constant.SoundConfig.DefaultPitch;
        panStereo = Constant.SoundConfig.DefaultPanStereo;
        spatialBlend = Constant.SoundConfig.DefaultSpatialBlend;
        maxDistance = Constant.SoundConfig.DefaultMaxDistance;
        dopplerLevel = Constant.SoundConfig.DefaultDopplerLevel;
    }
}
