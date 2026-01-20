using System;
using UnityEngine;

public sealed partial class SoundComponent : GameFrameworkComponent
{
    [Serializable]
    private sealed class SoundGroup
    {
        [SerializeField]
        private string name = null;
        [SerializeField]
        private bool avoidBeingReplacedBySamePriority = false;
        [SerializeField]
        private bool mute = false;
        [SerializeField, Range(0, 1)]
        private float volume = 1;
        [SerializeField]
        private int agentHelperCount = 1;

        public string Name { get => name; }
        public bool AvoidBeingReplacedBySamePriority { get => avoidBeingReplacedBySamePriority; }
        public bool Mute { get => mute; }
        public float Volume { get => volume; }
        public int AgentHelperCount { get => agentHelperCount; }
    }
}
