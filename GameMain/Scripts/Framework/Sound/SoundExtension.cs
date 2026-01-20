
/// <summary>
/// 声音扩展类
/// </summary>
public static class SoundExtension
{
    private static int? musicSerialId = null;

    private static string currentBgMusicName;

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="soundAssetName">音乐资源名称</param>
    public static int? PlayBgMusic(this SoundComponent soundComponent, string soundAssetName)
    {
        if (GameManager.PlayerData.CheckTargetAreaIsComplete(GameManager.PlayerData.DecorationAreaID))
            soundAssetName = GameManager.PlayerData.HappyBgMusicName;

        return PlayMusic(soundComponent, soundAssetName);
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="soundAssetName">音乐资源名称</param>
    public static int? PlayMusic(this SoundComponent soundComponent, string soundAssetName)
    {
        return PlayMusic(soundComponent, soundAssetName, 1f);
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="soundAssetName">音乐资源名称</param>
    /// <param name="volumeInSoundGroup">组内音量大小</param>
    /// <returns></returns>
    public static int? PlayMusic(this SoundComponent soundComponent, string soundAssetName, float volumeInSoundGroup)
    {
        if (soundComponent.GetSoundGroup("Music").Mute)
        {
            return 0;
        }

        if (soundAssetName == currentBgMusicName)
            return 0;

        soundComponent.StopMusic(fadeOutSeconds: 0);
        currentBgMusicName = soundAssetName;

        PlaySoundParams playSoundParams = PlaySoundParams.Create();
        playSoundParams.FadeInSeconds = 1f;
        playSoundParams.Loop = true;
        playSoundParams.Priority = 64;
        playSoundParams.MuteInSoundGroup = false;
        //playSoundParams.VolumeInSoundGroup = volumeInSoundGroup;
        playSoundParams.VolumeInSoundGroup = GameManager.PlayerData.BgMusicVolume/100f;//设置音量大小
        musicSerialId = soundComponent.PlaySound(soundAssetName, "Music", playSoundParams);
        return musicSerialId;
    }

    /// <summary>
    /// 停止播放音乐
    /// </summary>
    public static void StopMusic(this SoundComponent soundComponent, float fadeOutSeconds = 1f)
    {
        if (!musicSerialId.HasValue)
        {
            return;
        }

        soundComponent.StopSound(musicSerialId.Value, fadeOutSeconds);
        musicSerialId = null;
        currentBgMusicName = null;
    }

    /// <summary>
    /// 静音音乐
    /// </summary>
    public static void MuteMusic(this SoundComponent soundComponent, bool isMute)
    {
        soundComponent.MuteSoundGroup("Music", isMute);
    }

    /// <summary>
    /// 静音音效
    /// </summary>
    public static void MuteAudio(this SoundComponent soundComponent,bool isMute)
    {
        soundComponent.MuteSoundGroup("UISound", isMute);
        soundComponent.MuteSoundGroup("Sound", isMute);
        soundComponent.MuteSoundGroup("NormalBallSound", isMute);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public static int? PlayAudio(this SoundComponent soundComponent, string soundAssetName)
    {
        if (soundComponent.GetSoundGroup("Sound").Mute)
        {
            return 0;
        }

        return soundComponent.PlaySound(soundAssetName, "Sound");
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="priority">音效优先级</param>
    public static int? PlayAudio(this SoundComponent soundComponent, string soundAssetName, int priority)
    {
        if (soundComponent.GetSoundGroup("Sound").Mute)
        {
            return 0;
        }

        PlaySoundParams soundParams = PlaySoundParams.Create();
        soundParams.Priority = priority;
        return soundComponent.PlaySound(soundAssetName, "Sound", soundParams);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="priority">音效优先级</param>
    /// <param name="loop">是否循环</param>
    public static int? PlayAudio(this SoundComponent soundComponent, string soundAssetName, int priority, bool loop)
    {
        if (soundComponent.GetSoundGroup("Sound").Mute)
        {
            return 0;
        }

        PlaySoundParams soundParams = PlaySoundParams.Create();
        soundParams.Priority = priority;
        soundParams.Loop = loop;
        return soundComponent.PlaySound(soundAssetName, "Sound", soundParams);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="playSoundParams">全量参数</param>
    /// <returns></returns>
    public static int? PlayAudio(this SoundComponent soundComponent, string soundAssetName, PlaySoundParams soundParams)
    {
        if (soundComponent.GetSoundGroup("Sound").Mute)
        {
            return 0;
        }

        return soundComponent.PlaySound(soundAssetName, "Sound", soundParams);
    }

    /// <summary>
    /// 暂停音效
    /// </summary>
    public static void StopAudio(this SoundComponent soundComponent, int serialId)
    {
        soundComponent.StopSound(serialId);
    }

    /// <summary>
    /// 播放UI音效
    /// </summary>
    /// <param name="soundAssetName">音效资源名称</param>
    public static int? PlayUISound(this SoundComponent soundComponent, string soundAssetName)
    {
        if (soundComponent.GetSoundGroup("UISound").Mute)
        {
            return 0;
        }

        return soundComponent.PlaySound(soundAssetName, "UISound");
    }

    /// <summary>
    /// 播放UI开音效
    /// </summary>
    public static int? PlayUIOpenSound(this SoundComponent soundComponent)
    {
        return 0;
        if (soundComponent.GetSoundGroup("UISound").Mute)
        {
            return 0;
        }

        return soundComponent.PlaySound("SFX_UI_open", "UISound");
    }

    /// <summary>
    /// 播放UI关音效
    /// </summary>
    public static int? PlayUICloseSound(this SoundComponent soundComponent)
    {
        return 0;
        if (soundComponent.GetSoundGroup("UISound").Mute)
        {
            return 0;
        }

        return soundComponent.PlaySound("SFX_UI_close", "UISound");
    }
}
