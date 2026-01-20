using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 声音组件
/// </summary>
public sealed partial class SoundComponent : GameFrameworkComponent
{
    private ISoundManager soundManager = null;
    private AudioListener audioListener = null;

    [SerializeField]
    private Transform instanceRoot = null;

    [SerializeField]
    private SoundGroup[] soundGroups = null;

    private bool isMuteSound = false;
    private bool isMuteMusic=false;
    
    /// <summary>
    /// 获取声音组的数量
    /// </summary>
    public int SoundGroupCount
    {
        get
        {
            return soundManager.SoundGroupCount;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        soundManager = GameFrameworkEntry.GetModule<SoundManager>();
        if (soundManager == null)
        {
            Log.Fatal("Sound manger is invalid");
            return;
        }

        if (audioListener == null) audioListener = FindObjectOfType<AudioListener>();

        if(audioListener==null) audioListener = gameObject.GetOrAddComponent<AudioListener>();
    }

    private void Start()
    {
        if (instanceRoot == null)
        {
            instanceRoot = new GameObject("Sound Instances").transform;
            instanceRoot.SetParent(gameObject.transform);
            instanceRoot.localScale = Vector3.one;
        }

        for (int i = 0; i < soundGroups.Length; i++)
        {
            bool soundGroupMute = soundGroups[i].Mute;
            if (soundGroups[i].Name.Equals("Music", System.StringComparison.Ordinal))
            {
                soundGroupMute = GameManager.PlayerData.MusicMuted;
            }
            else
            {
                soundGroupMute = GameManager.PlayerData.AudioMuted;
            }

            if(!AddSoundGroup(soundGroups[i].Name, soundGroups[i].AvoidBeingReplacedBySamePriority, soundGroupMute, soundGroups[i].Volume, soundGroups[i].AgentHelperCount))
            {
                Log.Warning("Add sound group '{0}' failure.", soundGroups[i].Name);
                continue;
            }
        }
    }

    /// <summary>
    /// 是否存在指定声音组。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <returns>指定声音组是否存在。</returns>
    public bool HasSoundGroup(string soundGroupName)
    {
        return soundManager.HasSoundGroup(soundGroupName);
    }

    /// <summary>
    /// 获取指定声音组。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <returns>要获取的声音组。</returns>
    public ISoundGroup GetSoundGroup(string soundGroupName)
    {
        return soundManager.GetSoundGroup(soundGroupName);
    }

    /// <summary>
    /// 获取所有声音组。
    /// </summary>
    /// <returns>所有声音组。</returns>
    public ISoundGroup[] GetAllSoundGroups()
    {
        return soundManager.GetAllSoundGroups();
    }

    /// <summary>
    /// 增加声音组。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
    /// <returns>是否增加声音组成功。</returns>
    public bool AddSoundGroup(string soundGroupName,int soundAgentHelperCount)
    {
        return AddSoundGroup(soundGroupName, false, false, 1f, soundAgentHelperCount);
    }

    /// <summary>
    /// 增加声音组。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
    /// <param name="soundGroupMute">声音组是否静音。</param>
    /// <param name="soundGroupVolume">声音组音量。</param>
    /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
    /// <returns>是否增加声音组成功。</returns>
    public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute, float soundGroupVolume, int soundAgentHelperCount)
    {
        if (soundManager.HasSoundGroup(soundGroupName))
        {
            Log.Warning("Already have soundGroup {0}", soundGroupName);
            return false;
        }

        GameObject groupHelper = new GameObject($"Sound Group - {soundGroupName}");

        Transform cachedTransfrom = groupHelper.transform;
        cachedTransfrom.SetParent(instanceRoot);
        cachedTransfrom.localScale = Vector3.one;

        if (!soundManager.AddSoundGroup(soundGroupName, soundGroupAvoidBeingReplacedBySamePriority, soundGroupMute, soundGroupVolume)) 
        {
            return false;
        }

        for (int i = 0; i < soundAgentHelperCount; i++)
        {
            if (!AddSoundAgentHelper(soundGroupName, cachedTransfrom, i)) 
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 增加声音代理辅助器。
    /// </summary>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <param name="soundGroupRoot">声音组。</param>
    /// <param name="index">声音代理辅助器索引。</param>
    /// <returns>是否增加声音代理辅助器成功。</returns>
    private bool AddSoundAgentHelper(string soundGroupName, Transform soundGroupRoot, int index)
    {
        SoundAgentHelperBase agentHelper = new GameObject($"Sound Agent Helper - {soundGroupName} - {index}").AddComponent<DefaultSoundAgentHelper>();
        if (agentHelper == null)
        {
            Log.Error("Can not create sound agent helper.");
            return false;
        }

        Transform cachedTransform = agentHelper.transform;
        cachedTransform.SetParent(soundGroupRoot);
        cachedTransform.localScale = Vector3.one;

        soundManager.AddSoundAgentHelper(soundGroupName, agentHelper);

        return true;
    }

    /// <summary>
    /// 获取所有正在加载声音的序列编号。
    /// </summary>
    /// <returns>所有正在加载声音的序列编号。</returns>
    public List<int> GetAllLoadingSoundSerialIds()
    {
        return soundManager.GetAllLoadingSoundSerialIds();
    }

    /// <summary>
    /// 是否正在加载声音。
    /// </summary>
    /// <param name="serialId">声音序列编号。</param>
    /// <returns>是否正在加载声音。</returns>
    public bool IsLoadingSound(int serialId)
    {
        return soundManager.IsLoadingSound(serialId);
    }

    /// <summary>
    /// 播放声音。
    /// </summary>
    /// <param name="soundAssetName">声音资源名称。</param>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <returns>声音的序列编号。</returns>
    public int PlaySound(string soundAssetName, string soundGroupName)
    {
        if (IsMute(soundGroupName)) return 1;
        return soundManager.PlaySound(soundAssetName, soundGroupName);
    }

    /// <summary>
    /// 播放声音。
    /// </summary>
    /// <param name="soundAssetName">声音资源名称。</param>
    /// <param name="soundGroupName">声音组名称。</param>
    /// <param name="playSoundParams">播放声音参数。</param>
    /// <returns>声音的序列编号。</returns>
    public int PlaySound(string soundAssetName, string soundGroupName, PlaySoundParams playSoundParams)
    {
        if (IsMute(soundGroupName)) return 1;
        return soundManager.PlaySound(soundAssetName, soundGroupName, playSoundParams);
    }

    /// <summary>
    /// 停止播放声音。
    /// </summary>
    /// <param name="serialId">要停止播放声音的序列编号。</param>
    /// <returns>是否停止播放声音成功。</returns>
    public bool StopSound(int serialId)
    {
        return soundManager.StopSound(serialId);
    }

    /// <summary>
    /// 停止播放声音。
    /// </summary>
    /// <param name="serialId">要停止播放声音的序列编号。</param>
    /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
    /// <returns>是否停止播放声音成功。</returns>
    public bool StopSound(int serialId, float fadeOutSeconds)
    {
        return soundManager.StopSound(serialId, fadeOutSeconds);
    }

    /// <summary>
    /// 停止所有已加载的声音。
    /// </summary>
    public void StopAllLoadedSound()
    {
        soundManager.StopAllLoadedSound();
    }

    /// <summary>
    /// 停止所有已加载的声音。
    /// </summary>
    /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
    public void StopAllLoadedSound(float fadeOutSeconds)
    {
        soundManager.StopAllLoadedSound(fadeOutSeconds);
    }

    /// <summary>
    /// 停止所有正在加载的声音。
    /// </summary>
    public void StopAllLoadingSounds()
    {
        soundManager.StopAllLoadingSounds();
    }

    /// <summary>
    /// 暂停播放声音。
    /// </summary>
    /// <param name="serialId">要暂停播放声音的序列编号。</param>
    public void PauseSound(int serialId)
    {
        soundManager.PauseSound(serialId);
    }

    /// <summary>
    /// 暂停播放声音。
    /// </summary>
    /// <param name="serialId">要暂停播放声音的序列编号。</param>
    /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
    public void PauseSound(int serialId, float fadeOutSeconds)
    {
        soundManager.PauseSound(serialId, fadeOutSeconds);
    }

    /// <summary>
    /// 恢复播放声音。
    /// </summary>
    /// <param name="serialId">要恢复播放声音的序列编号。</param>
    public void ResumeSound(int serialId)
    {
        soundManager.ResumeSound(serialId);
    }

    /// <summary>
    /// 恢复播放声音。
    /// </summary>
    /// <param name="serialId">要恢复播放声音的序列编号。</param>
    /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
    public void ResumeSound(int serialId, float fadeInSeconds)
    {
        soundManager.ResumeSound(serialId, fadeInSeconds);
    }

    /// <summary>
    /// 静音或取消静音声音组
    /// </summary>
    /// <param name="soundGroupName">声音组名称</param>
    /// <param name="isMute">是否静音</param>
    /// <returns>是否静音成功</returns>
    public bool MuteSoundGroup(string soundGroupName, bool isMute)
    {
        if (soundGroupName == "Music")
        {
            isMuteMusic = isMute;
        }
        else
        {
            isMuteSound = isMute;
        }
        
        ISoundGroup soundGroup = soundManager.GetSoundGroup(soundGroupName);
        if (soundGroup != null)
        {
            soundGroup.Mute = isMute;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// <summary>
    /// 声音是否被静止播放
    /// </summary>
    /// <param name="soundAssetName">音效名称</param>
    /// <returns>是否被静止播放</returns>
    public bool IsSoundForbidden(string soundAssetName)
    {
        return soundManager.IsSoundForbidden(soundAssetName);
    }
    /// 静止声音播放
    /// </summary>
    /// <param name="soundAssetName">音效名称</param>
    /// <param name="isForbid">是否禁止</param>
    public void ForbidSound(string soundAssetName, bool isForbid)
    {
        soundManager.ForbidSound(soundAssetName, isForbid);
    }
    
    private bool IsMute(string soundGroupName)
    {
        if (soundGroupName == "Music") return isMuteMusic;
        return isMuteSound;
    }
}
