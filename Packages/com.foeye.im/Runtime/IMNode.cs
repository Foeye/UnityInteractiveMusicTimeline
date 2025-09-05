using System;
using System.Collections.Generic;
using UnityEngine;

namespace UInteractiveMusic.Runtime.Node {
    public abstract class IMNode : ScriptableObject {
        [SerializeField] private string guid;
        [SerializeField] private string displayName;
        [SerializeField] private IMNode parent;
        [SerializeField] private List<IMNode> children = new List<IMNode>();
        [SerializeField] private bool expanded = true;

        public string Guid => guid;
        
        [SerializeField] private float voiceVolume = 0f;    // 0..100  
        [SerializeField] private float voiceLowpass = 0f;   // 0..100  
        [SerializeField] private string notes = "";  
        [SerializeField] private bool mute = false;  

        public float VoiceVolume { get => voiceVolume; set => voiceVolume = Mathf.Clamp(value, 0f, 100f); }  
        public float VoiceLowpass { get => voiceLowpass; set => voiceLowpass = Mathf.Clamp(value, 0f, 100f); }  
        public string Notes { get => notes; set => notes = value; }  
        public bool Mute { get => mute; set => mute = value; } 

        public string DisplayName {
            get => string.IsNullOrEmpty(displayName) ? name : displayName;
            set => displayName = value;
        }

        public IMNode Parent {
            get => parent;
            set => parent = value;
        }

        public List<IMNode> Children => children;

        public bool Expanded {
            get => expanded;
            set => expanded = value;
        }

        public abstract MusicNodeType NodeType { get; }

        public void EnsureGuid() {
            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString("N");
        }

        public virtual bool CanCreateChild(MusicNodeType childType) {
            switch (NodeType) {
                case MusicNodeType.Root:
                case MusicNodeType.MusicSwitchContainer:
                    return childType == MusicNodeType.MusicSwitchContainer ||
                           childType == MusicNodeType.MusicPlaylistContainer ||
                           childType == MusicNodeType.MusicSegment;
                case MusicNodeType.MusicPlaylistContainer:
                    return childType == MusicNodeType.MusicSegment;
                case MusicNodeType.MusicSegment:
                default:
                    return false;
            }
        }
    }
}