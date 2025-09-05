using System;
using System.Collections.Generic;
using UnityEngine;

namespace UInteractiveMusic.Runtime.Switching {
    public enum IMSwitchGroupKind {
        Switch,
        State
    }

    [Serializable]
    public class IMSwitchValue {
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        public string Id => id;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? id : displayName;

        public IMSwitchValue(string id, string name) {
            this.id = id;
            this.displayName = name;
        }
    }

    [CreateAssetMenu(fileName = "SwitchGroup", menuName = "InteractiveMusic/Switch Group", order = 10)]
    public class IMSwitchGroupAsset : ScriptableObject {
        [SerializeField] private string guid;
        [SerializeField] private string displayName;
        [SerializeField] private IMSwitchGroupKind kind = IMSwitchGroupKind.Switch;
        [SerializeField] private List<IMSwitchValue> values = new List<IMSwitchValue>();

        public string Guid => guid;

        public string DisplayName {
            get => string.IsNullOrEmpty(displayName) ? name : displayName;
            set => displayName = value;
        }

        public IMSwitchGroupKind Kind => kind;
        public IReadOnlyList<IMSwitchValue> Values => values;

        private void OnValidate() {
            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString("N");
        }
    }
}