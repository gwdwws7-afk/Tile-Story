using System;
using UnityEngine.AddressableAssets;

public sealed partial class SoundManager : GameFrameworkModule, ISoundManager
{
    /// <summary>
    /// 声音代理
    /// </summary>
    private sealed class SoundAgent:ISoundAgent
    {
        private readonly SoundGroup soundGroup;
        private readonly ISoundAgentHelper soundAgentHelper;
        private int serialId;
        private object soundAsset;
        private DateTime setSoundAssetTime;
        private bool muteInSoundGroup;
        private float volumeInSoundGroup;
        private string soundName;

        /// <summary>
        /// 初始化声音代理的新实例。
        /// </summary>
        /// <param name="soundGroup">所在的声音组。</param>
        /// <param name="soundAgentHelper">声音代理辅助器接口。</param>
        public SoundAgent(SoundGroup soundGroup, ISoundAgentHelper soundAgentHelper)
        {
            if (soundGroup == null)
            {
                throw new Exception("Sound group is invalid.");
            }

            if (soundAgentHelper == null)
            {
                throw new Exception("Sound agent helper is invalid.");
            }

            this.soundGroup = soundGroup;
            this.soundAgentHelper = soundAgentHelper;
            soundAgentHelper.ResetSoundAgent += OnResetSoundAgent;
            serialId = 0;
            soundAsset = null;
            Reset();
        }

        /// <summary>
        /// 获取所在的声音组。
        /// </summary>
        public ISoundGroup SoundGroup
        {
            get
            {
                return soundGroup;
            }
        }

        /// <summary>
        /// 获取或设置声音的序列编号。
        /// </summary>
        public int SerialId
        {
            get
            {
                return serialId;
            }
            set
            {
                serialId = value;
            }
        }
        
        /// <summary>
        /// 名称
        /// </summary>
        public string SoundName
        {
            get
            {
                return soundName;
            }
            set
            {
                soundName = value;
            }
        }

        /// <summary>
        /// 获取当前是否正在播放。
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return soundAgentHelper.IsPlaying;
            }
        }

        /// <summary>
        /// 获取声音长度。
        /// </summary>
        public float Length
        {
            get
            {
                return soundAgentHelper.Length;
            }
        }

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public float Time
        {
            get
            {
                return soundAgentHelper.Time;
            }
            set
            {
                soundAgentHelper.Time = value;
            }
        }

        /// <summary>
        /// 获取是否静音。
        /// </summary>
        public bool Mute
        {
            get
            {
                return soundAgentHelper.Mute;
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
                RefreshMute();
            }
        }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public bool Loop
        {
            get
            {
                return soundAgentHelper.Loop;
            }
            set
            {
                soundAgentHelper.Loop = value;
            }
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public int Priority
        {
            get
            {
                return soundAgentHelper.Priority;
            }
            set
            {
                soundAgentHelper.Priority = value;
            }
        }

        /// <summary>
        /// 获取音量大小。
        /// </summary>
        public float Volume
        {
            get
            {
                return soundAgentHelper.Volume;
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
                RefreshVolume();
            }
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public float Pitch
        {
            get
            {
                return soundAgentHelper.Pitch;
            }
            set
            {
                soundAgentHelper.Pitch = value;
            }
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public float PanStereo
        {
            get
            {
                return soundAgentHelper.PanStereo;
            }
            set
            {
                soundAgentHelper.PanStereo = value;
            }
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public float SpatialBlend
        {
            get
            {
                return soundAgentHelper.SpatialBlend;
            }
            set
            {
                soundAgentHelper.SpatialBlend = value;
            }
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public float MaxDistance
        {
            get
            {
                return soundAgentHelper.MaxDistance;
            }
            set
            {
                soundAgentHelper.MaxDistance = value;
            }
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public float DopplerLevel
        {
            get
            {
                return soundAgentHelper.DopplerLevel;
            }
            set
            {
                soundAgentHelper.DopplerLevel = value;
            }
        }

        /// <summary>
        /// 获取声音代理辅助器。
        /// </summary>
        public ISoundAgentHelper Helper
        {
            get
            {
                return soundAgentHelper;
            }
        }

        /// <summary>
        /// 获取声音创建时间。
        /// </summary>
        public DateTime SetSoundAssetTime
        {
            get
            {
                return setSoundAssetTime;
            }
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        public void Play()
        {
            soundAgentHelper.Play(Constant.SoundConfig.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void Play(float fadeInSeconds)
        {
            soundAgentHelper.Play(fadeInSeconds);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        public void Stop()
        {
            soundAgentHelper.Stop(Constant.SoundConfig.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void Stop(float fadeOutSeconds)
        {
            soundAgentHelper.Stop(fadeOutSeconds);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        public void Pause()
        {
            soundAgentHelper.Pause(Constant.SoundConfig.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void Pause(float fadeOutSeconds)
        {
            soundAgentHelper.Pause(fadeOutSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        public void Resume()
        {
            soundAgentHelper.Resume(Constant.SoundConfig.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void Resume(float fadeInSeconds)
        {
            soundAgentHelper.Resume(fadeInSeconds);
        }

        /// <summary>
        /// 重置声音代理。
        /// </summary>
        public void Reset()
        {
            setSoundAssetTime = DateTime.MinValue;
            Time = Constant.SoundConfig.DefaultTime;
            MuteInSoundGroup = Constant.SoundConfig.DefaultMute;
            Loop = Constant.SoundConfig.DefaultLoop;
            Priority = Constant.SoundConfig.DefaultPriority;
            VolumeInSoundGroup = Constant.SoundConfig.DefaultVolume;
            Pitch = Constant.SoundConfig.DefaultPitch;
            PanStereo = Constant.SoundConfig.DefaultPanStereo;
            SpatialBlend = Constant.SoundConfig.DefaultSpatialBlend;
            MaxDistance = Constant.SoundConfig.DefaultMaxDistance;
            DopplerLevel = Constant.SoundConfig.DefaultDopplerLevel;

            soundAgentHelper.Reset();
        }

        /// <summary>
        /// 设置声音资源。
        /// </summary>
        /// <param name="soundAsset">声音资源。</param>
        /// <returns>是否设置声音资源成功。</returns>
        public bool SetSoundAsset(object soundAsset)
        {
            this.soundAsset = soundAsset;
            setSoundAssetTime = DateTime.UtcNow;
            return soundAgentHelper.SetSoundAsset(soundAsset);
        }

        public void RefreshMute()
        {
            soundAgentHelper.Mute = soundGroup.Mute || muteInSoundGroup;
        }

        public void RefreshVolume()
        {
            soundAgentHelper.Volume = soundGroup.Volume * VolumeInSoundGroup;
        }

        private void OnResetSoundAgent(object sender, GameEventMessage e)
        {
            Reset();
        }

        public void ReleaseSound(string newSoundName)
        {
            if (soundAsset != null)
            {
                if(!NoUnloadSoundNames.Contains(SoundName)&&soundName!=newSoundName)Addressables.Release(soundAsset);
                soundAsset = null;
            }
        }
    }
}
