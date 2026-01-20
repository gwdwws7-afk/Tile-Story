using GameFramework;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 声音管理器
/// </summary>
public sealed partial class SoundManager : GameFrameworkModule, ISoundManager
{
    private readonly Dictionary<string, SoundGroup> soundGroups;
    private readonly List<int> soundsBeingLoaded;
    private readonly HashSet<int> soundsToReleaseOnLoad;
    private readonly List<string> forbidSoundsList;
    private Dictionary<string, AudioClip> audioDic;
    private int serialId;


    private static List<string> noUnloadSoundNames = null;

    public static List<string> NoUnloadSoundNames
    {
        get
        {
            if (noUnloadSoundNames == null)
            {
                noUnloadSoundNames = new List<string>();
                var textAsset =AddressableUtils.LoadAsset<TextAsset>("NoUnloadSounds");
                if (textAsset != null&&!string.IsNullOrEmpty(textAsset.text))
                {
                    Log.Debug($"NoUnloadSounds:{textAsset.text}");
                    noUnloadSoundNames = JsonConvert.DeserializeObject<List<string>>(textAsset.text);
                }
            }
            return noUnloadSoundNames;
        }
    }

    public SoundManager()
    {
        soundGroups = new Dictionary<string, SoundGroup>();
        soundsBeingLoaded = new List<int>();
        soundsToReleaseOnLoad = new HashSet<int>();
        forbidSoundsList = new List<string>();
        audioDic = new Dictionary<string, AudioClip>();
        serialId = 0;
    }

    /// <summary>
    /// 获取声音组数量。
    /// </summary>
    public int SoundGroupCount
    {
        get
        {
            return soundGroups.Count;
        }
    }

    /// <summary>
    /// 声音管理器轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    public override void Update(float elapseSeconds, float realElapseSeconds)
    {
    }

    /// <summary>
    /// 关闭并清理声音管理器。
    /// </summary>
    public override void Shutdown()
    {
        StopAllLoadedSound();
        soundGroups.Clear();
        soundsBeingLoaded.Clear();
        soundsToReleaseOnLoad.Clear();
        forbidSoundsList.Clear();

        UnloadAllSoundAsset();
    }

    /// <summary>
    /// 卸除所有音效资源
    /// </summary>
    public void UnloadAllSoundAsset()
    {
        audioDic.Clear();
    }

    /// <summary>
    /// 是否存在指定声音组。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <returns>指定声音组是否存在。</returns>
    public bool HasSoundGroup(string soundGroupName)
    {
        if (string.IsNullOrEmpty(soundGroupName))
        {
            throw new System.Exception("Sound group name is invalid.");
        }

        return soundGroups.ContainsKey(soundGroupName);
    }

    /// <summary>
    /// 获取指定声音组。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <returns>要获取的声音组。</returns>
    public ISoundGroup GetSoundGroup(string soundGroupName)
    {
        if (string.IsNullOrEmpty(soundGroupName))
        {
            throw new System.Exception("Sound group name is invalid.");
        }

        if (soundGroups.TryGetValue(soundGroupName, out SoundGroup soundGroup))
        {
            return soundGroup;
        }

        return null;
    }

    /// <summary>
    /// 获取所有声音组。
    /// </summary>
    /// <returns>所有声音组。</returns>
    public ISoundGroup[] GetAllSoundGroups()
    {
        int index = 0;
        ISoundGroup[] result = new ISoundGroup[soundGroups.Count];
        foreach (var soundGroup in soundGroups)
        {
            result[index++] = soundGroup.Value;
        }
        return result;
    }

    /// <summary>
    /// 增加声音组。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <returns>是否增加声音组成功。</returns>
    public bool AddSoundGroup(string soundGroupName)
    {
        return AddSoundGroup(soundGroupName, false, Constant.SoundConfig.DefaultMute, Constant.SoundConfig.DefaultVolume);
    }

    /// <summary>
    /// 增加声音组。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
    /// <param name="soundGroupMute">声音组是否静音。</param>
    /// <param name="soundGroupVolume">声音组音量。</param>
    /// <returns>是否增加声音组成功。</returns>
    public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute, float soundGroupVolume)
    {
        if (string.IsNullOrEmpty(soundGroupName))
        {
            throw new System.Exception("Sound group name is invalid.");
        }

        if (HasSoundGroup(soundGroupName))
        {
            Log.Warning("Already have sound group: {0}", soundGroupName);
            return false;
        }

        SoundGroup soundGroup = new SoundGroup(soundGroupName)
        {
            AvoidBeingReplacedBySamePriority = soundGroupAvoidBeingReplacedBySamePriority,
            Mute = soundGroupMute,
            Volume = soundGroupVolume
        };

        soundGroups.Add(soundGroupName, soundGroup);

        return true;
    }

    /// <summary>
    /// 增加声音代理辅助器。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <param name="soundAgentHelper">要增加的声音代理辅助器。</param>
    public void AddSoundAgentHelper(string soundGroupName, ISoundAgentHelper soundAgentHelper)
    {
        if (soundAgentHelper == null)
        {
            throw new System.Exception("soundAgentHelper is null.");
        }

        SoundGroup soundGroup = (SoundGroup)GetSoundGroup(soundGroupName);
        if (soundGroup == null)
        {
            throw new System.Exception($"Sound group '{soundGroupName}' is not exist.");
        }

        soundGroup.AddSoundAgentHelper(soundAgentHelper);
    }

    /// <summary>
    /// 获取所有正在加载声音的序列编号。
    /// </summary>
    /// <returns>所有正在加载声音的序列编号。</returns>
    public List<int> GetAllLoadingSoundSerialIds()
    {
        return soundsBeingLoaded;
    }

    /// <summary>
    /// 是否正在加载声音。
    /// </summary>
    /// <param name="serialId">声音序列编号。</param>
    /// <returns>是否正在加载声音。</returns>
    public bool IsLoadingSound(int serialId)
    {
        return soundsBeingLoaded.Contains(serialId);
    }

    /// <summary>
    /// 播放声音。
    /// </summary>
    /// <param name="soundAssetName">声音资源名称。</param>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <returns>声音的序列编号。</returns>
    public int PlaySound(string soundAssetName, string soundGroupName)
    {
        return PlaySound(soundAssetName, soundGroupName, null);
    }

    /// <summary>
    /// 播放声音。
    /// </summary>
    /// <param name="soundAssetName">声音资源名称。</param>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <param name="priority">加载声音资源的优先级。</param>
    /// <param name="playSoundParams">播放声音参数。</param>
    /// <returns>声音的序列编号。</returns>
    public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams)
    {
        if (string.IsNullOrEmpty(soundAssetName)) return 0;
        
        if (forbidSoundsList.Contains(soundAssetName))
        {
            return 0;
        }

        SoundGroup soundGroup = (SoundGroup)GetSoundGroup(soundGroupName);
        if (soundGroup !=null&& !soundGroup.HasAvailableSoundAgent())
        {
            if (playSoundParams != null && playSoundParams.Referenced) 
            {
                ReferencePool.Release(playSoundParams);
            }
            return 0;
        }

        if (playSoundParams == null)
        {
            playSoundParams = PlaySoundParams.Create();
        }

        int id = ++serialId;

        soundsBeingLoaded.Add(id);

        if (soundsToReleaseOnLoad.Contains(id))
        {
            soundsToReleaseOnLoad.Remove(id);
            return id;
        }

        if (!audioDic.TryGetValue(soundAssetName, out AudioClip audioClip)||audioClip==null)
        {
            audioClip = AddressableUtils.LoadAsset<AudioClip>(soundAssetName);
            if (audioClip == null)
            {
                Log.Error("Load audioClip {0} From asset is null", soundAssetName);
                return id;
            }

            if (NoUnloadSoundNames.Count == 0 || NoUnloadSoundNames.Contains(soundAssetName))
            {
                audioDic[soundAssetName] = audioClip;
            }
        }

        soundGroup.PlaySound(soundAssetName,id, audioClip, playSoundParams);

        soundsBeingLoaded.Remove(id);

        if (playSoundParams.Referenced)
        {
            ReferencePool.Release(playSoundParams);
        }

        return id;
    }

    /// <summary>
    /// 停止播放声音。
    /// </summary>
    /// <param name="serialId">要停止播放声音的序列编号。</param>
    /// <returns>是否停止播放声音成功。</returns>
    public bool StopSound(int serialId)
    {
        return StopSound(serialId, Constant.SoundConfig.DefaultFadeOutSeconds);
    }

    /// <summary>
    /// 停止播放声音。
    /// </summary>
    /// <param name="serialId">要停止播放声音的序列编号。</param>
    /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
    /// <returns>是否停止播放声音成功。</returns>
    public bool StopSound(int serialId, float fadeOutSeconds)
    {
        if (IsLoadingSound(serialId))
        {
            soundsToReleaseOnLoad.Add(serialId);
            soundsBeingLoaded.Remove(serialId);
            return true;
        }

        foreach (var soundGroup in soundGroups)
        {
            if (soundGroup.Value.StopSound(serialId, fadeOutSeconds))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 停止所有已加载的声音。
    /// </summary>
    public void StopAllLoadedSound()
    {
        StopAllLoadedSound(Constant.SoundConfig.DefaultFadeOutSeconds);
    }

    /// <summary>
    /// 停止所有已加载的声音。
    /// </summary>
    /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
    public void StopAllLoadedSound(float fadeOutSeconds)
    {
        foreach (var soundGroup in soundGroups)
        {
            soundGroup.Value.StopAllLoadedSounds(fadeOutSeconds);
        }
    }

    /// <summary>
    /// 停止所有正在加载的声音。
    /// </summary>
    public void StopAllLoadingSounds()
    {
        foreach (int serialId in soundsBeingLoaded)
        {
            soundsToReleaseOnLoad.Add(serialId);
        }
        soundsBeingLoaded.Clear();
    }

    /// <summary>
    /// 暂停播放声音。
    /// </summary>
    /// <param name="serialId">要暂停播放声音的序列编号。</param>
    public void PauseSound(int serialId)
    {
        PauseSound(serialId, Constant.SoundConfig.DefaultFadeOutSeconds);
    }

    /// <summary>
    /// 暂停播放声音。
    /// </summary>
    /// <param name="serialId">要暂停播放声音的序列编号。</param>
    /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
    public void PauseSound(int serialId, float fadeOutSeconds)
    {
        foreach (var soundGroup in soundGroups)
        {
            if (soundGroup.Value.PauseSound(serialId, fadeOutSeconds))
            {
                return;
            }
        }

        Log.Warning("Can not find sound '{0}' to pause.", serialId);
    }

    /// <summary>
    /// 恢复播放声音。
    /// </summary>
    /// <param name="serialId">要恢复播放声音的序列编号。</param>
    public void ResumeSound(int serialId)
    {
        ResumeSound(serialId, Constant.SoundConfig.DefaultFadeInSeconds);
    }

    /// <summary>
    /// 恢复播放声音。
    /// </summary>
    /// <param name="serialId">要恢复播放声音的序列编号。</param>
    /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
    public void ResumeSound(int serialId, float fadeInSeconds)
    {
        foreach (var soundGroup in soundGroups)
        {
            if (soundGroup.Value.ResumeSound(serialId, fadeInSeconds))
            {
                return;
            }
        }

        Log.Warning("Can not find sound '{0}' to resume.", serialId);
    }
    
        
    /// <summary>
    /// 声音是否被静止播放
    /// </summary>
    /// <param name="soundAssetName">音效名称</param>
    /// <returns>是否被静止播放</returns>
    public bool IsSoundForbidden(string soundAssetName)
    {
        return forbidSoundsList.Contains(soundAssetName);
    }

    /// <summary>
    /// 静止声音播放
    /// </summary>
    /// <param name="soundAssetName">音效名称</param>
    /// <param name="isForbid">是否禁止</param>
    public void ForbidSound(string soundAssetName, bool isForbid)
    {
        if (isForbid && !forbidSoundsList.Contains(soundAssetName)) 
        {
            forbidSoundsList.Add(soundAssetName);
        }

        if (!isForbid && forbidSoundsList.Contains(soundAssetName))
        {
            forbidSoundsList.Remove(soundAssetName);
        }
    }
}
