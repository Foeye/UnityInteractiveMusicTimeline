using UnityEditor;
using UnityEngine;

namespace UInteractiveMusic.Editor.Core {
    public static class IMEditorStyles {
        public static readonly GUIStyle RightTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

        public static readonly GUIStyle RightSectionTitle = new GUIStyle(EditorStyles.boldLabel) {
            margin = new RectOffset(4, 4, 4, 4)
        };

        public static readonly GUIStyle RightBox = new GUIStyle("FrameBox") {
            padding = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(4, 4, 4, 4)
        };

        public static readonly Color LeftBgLight = new Color(0.90f, 0.90f, 0.90f, 1f);
        public static readonly Color LeftBgDark = new Color(0.18f, 0.18f, 0.18f, 1f);
        public static readonly Color RightBgLight = new Color(0.97f, 0.97f, 0.97f, 1f);
        public static readonly Color RightBgDark = new Color(0.13f, 0.13f, 0.13f, 1f);

        public static readonly Color RowSelected = new Color(0.24f, 0.48f, 0.90f, 0.25f);
        public static readonly Color RowNormal = new Color(0, 0, 0, 0);
        public static readonly Color SplitLine = new Color(0, 0, 0, 0.35f);

        public static void DrawPaneBackground(Rect r, bool left) {
            var isPro = EditorGUIUtility.isProSkin;
            var col = left
                ? (isPro ? LeftBgDark : LeftBgLight)
                : (isPro ? RightBgDark : RightBgLight);
            EditorGUI.DrawRect(r, col);
        }
    }
}