

using System.Collections.Generic;

public sealed partial class SoundManager : GameFrameworkModule, ISoundManager
{
    /// <summary>
    /// 声音组
    /// </summary>
    private sealed class SoundGroup : ISoundGroup
    {
        private readonly string name;
        private readonly List<SoundAgent> soundAgents;
        private bool avoidBeingReplacedBySamePriority;
        private bool mute;
        private float volume;

        /// <summary>
        /// 初始化声音组的新实例。
        /// </summary>
        /// <param name="name">声音组名称。</param>
        public SoundGroup(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.Exception("Sound group name is invalid.");
            }

            this.name = name;
            soundAgents = new List<SoundAgent>();
        }

        /// <summary>
        /// 获取声音组名称。
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// 获取声音代理数。
        /// </summary>
        public int SoundAgentCount
        {
            get
            {
                return soundAgents.Count;
            }
        }

        /// <summary>
        /// 获取或设置声音组中的声音是否避免被同优先级声音替换。
        /// </summary>
        public bool AvoidBeingReplacedBySamePriority
        {
            get
            {
                return avoidBeingReplacedBySamePriority;
            }
            set
            {
                avoidBeingReplacedBySamePriority = value;
            }
        }

        /// <summary>
        /// 获取或设置声音组静音。
        /// </summary>
        public bool Mute
        {
            get
            {
                return mute;
            }
            set
            {
                mute = value;
                foreach (SoundAgent soundAgent in soundAgents)
                {
                    soundAgent.RefreshMute();
                }
            }
        }

        /// <summary>
        /// 获取或设置声音组音量。
        /// </summary>
        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                foreach (SoundAgent soundAgent in soundAgents)
                {
                    soundAgent.RefreshVolume();
                }
            }
        }

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        /// <param name="soundAgentHelper">要增加的声音代理辅助器。</param>
        public void AddSoundAgentHelper(ISoundAgentHelper soundAgentHelper)
        {
            soundAgents.Add(new SoundAgent(this, soundAgentHelper));
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="serialId">声音的序列编号。</param>
        /// <param name="soundAsset">声音资源。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <returns>用于播放的声音代理。</returns>
        public ISoundAgent PlaySound(string soundName,int serialId, object soundAsset, PlaySoundParams playSoundParams)
        {
            SoundAgent candidateAgent = null;
            foreach (SoundAgent soundAgent in soundAgents)
            {
                if (!soundAgent.IsPlaying)
                {
                    candidateAgent = soundAgent;
                    break;
                }

                if (soundAgent.Priority < playSoundParams.Priority)
                {
                    if (candidateAgent == null || soundAgent.Priority < candidateAgent.Priority)
                    {
                        candidateAgent = soundAgent;
                    }
                }
                else if (!avoidBeingReplacedBySamePriority && soundAgent.Priority == playSoundParams.Priority)
                {
                    if (candidateAgent == null || soundAgent.SetSoundAssetTime < candidateAgent.SetSoundAssetTime)
                    {
                        candidateAgent = soundAgent;
                    }
                }
            }

            if (candidateAgent == null)
            {
                Log.Warning("Sound Ignored Due To Low Priority");
                return null;
            }

            candidateAgent.ReleaseSound(soundName);
            
            if (!candidateAgent.SetSoundAsset(soundAsset))
            {
                Log.Warning("Set SoundAsset Failure");
                return null;
            }

            candidateAgent.SoundName = soundName;
            candidateAgent.SerialId = serialId;
            candidateAgent.Time = playSoundParams.Time;
            candidateAgent.MuteInSoundGroup = playSoundParams.MuteInSoundGroup;
            candidateAgent.Loop = playSoundParams.Loop;
            candidateAgent.Priority = playSoundParams.Priority;
            candidateAgent.VolumeInSoundGroup = playSoundParams.VolumeInSoundGroup;
            candidateAgent.Pitch = playSoundParams.Pitch;
            candidateAgent.PanStereo = playSoundParams.PanStereo;
            candidateAgent.SpatialBlend = playSoundParams.SpatialBlend;
            candidateAgent.MaxDistance = playSoundParams.MaxDistance;
            candidateAgent.DopplerLevel = playSoundParams.DopplerLevel;
            candidateAgent.Play(playSoundParams.FadeInSeconds);
            return candidateAgent;
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            foreach (SoundAgent soundAgent in soundAgents)
            {
                if (soundAgent.SerialId != serialId)
                {
                    continue;
                }
                soundAgent.Stop(fadeOutSeconds);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否暂停播放声音成功。</returns>
        public bool PauseSound(int serialId, float fadeOutSeconds)
        {
            foreach (SoundAgent soundAgent in soundAgents)
            {
                if (soundAgent.SerialId != serialId)
                {
                    continue;
                }
                soundAgent.Pause(fadeOutSeconds);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        /// <returns>是否恢复播放声音成功。</returns>
        public bool ResumeSound(int serialId, float fadeInSeconds)
        {
            foreach (SoundAgent soundAgent in soundAgents)
            {
                if (soundAgent.SerialId != serialId)
                {
                    continue;
                }
                soundAgent.Resume(fadeInSeconds);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedSounds()
        {
            foreach (SoundAgent soundAgent in soundAgents)
            {
                if (soundAgent.IsPlaying)
                {
                    soundAgent.Stop();
                }
            }
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            foreach (SoundAgent soundAgent in soundAgents)
            {
                if (soundAgent.IsPlaying)
                {
                    soundAgent.Stop(fadeOutSeconds);
                }
            }
        }

        /// <summary>
        /// 是否有空闲的声音代理
        /// </summary>
        public bool HasAvailableSoundAgent()
        {
            foreach (SoundAgent soundAgent in soundAgents)
            {
                if (!soundAgent.IsPlaying)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
