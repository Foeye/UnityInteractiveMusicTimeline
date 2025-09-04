using UInteractiveMusic.Runtime.Node;
using UnityEngine;
using UnityEditor;

namespace UInteractiveMusic.Editor.Core {
    public class IMEditorState {
        // 面板与分隔条
        public float LeftWidth = 320f;
        public const float MinLeftWidth = 220f;
        public const float MaxLeftWidth = 600f;
        public const float SplitterWidth = 4f;
        public bool DraggingSplitter = false;

        // 滚动
        public Vector2 LeftScroll;
        public Vector2 RightScroll;

        // 选择与重命名
        public IMNode Selected;
        public IMNode RenameTarget;
        public string RenameBuffer;

        // SwitchContainer 表格状态
        public string SwitchSearch = "";

        // EditorPrefs
        private const string PrefLeftWidthKey = "UInteractiveMusic.Hierarchy.LeftWidth";

        public void LoadPrefs() {
            LeftWidth = EditorPrefs.GetFloat(PrefLeftWidthKey, LeftWidth);
        }

        public void SavePrefs() {
            EditorPrefs.SetFloat(PrefLeftWidthKey, LeftWidth);
        }
    }
}