using UnityEditor;
using UnityEngine;

namespace UInteractiveMusic.Editor.Views {
    public static class IMClickableHeader {
        // 纯文字可点击标题：返回 expanded 状态；不绘制下拉箭头
        public static bool DrawLabel(string title, bool expanded, float height = 24f, GUIStyle labelStyle = null,
            System.Action rightCorner = null) {
            var r = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));

            // 背景（轻微高亮）
            if (Event.current.type == EventType.Repaint) {
                var bg = EditorGUIUtility.isProSkin
                    ? new Color(1, 1, 1, 0.06f)
                    : new Color(0, 0, 0, 0.06f);
                EditorGUI.DrawRect(r, bg);

                // 底部分隔线
                var line = new Rect(r.x, r.yMax - 1, r.width, 1);
                EditorGUI.DrawRect(line, new Color(0, 0, 0, 0.25f));
            }

            // 标题文本（整行可点击）
            var labelRect = new Rect(r.x + 10, r.y + (r.height - 18) * 0.5f, r.width - 20, 18);
            EditorGUI.LabelField(labelRect, title, labelStyle ?? EditorStyles.boldLabel);

            // 右上角插槽（可放按钮/帮助/搜索等）
            rightCorner?.Invoke();

            // 悬停反馈
            var e = Event.current;
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            if (e.type == EventType.Repaint && r.Contains(e.mousePosition)) {
                var hover = new Rect(r.x, r.y, r.width, r.height);
                EditorGUI.DrawRect(hover, new Color(0.24f, 0.48f, 0.90f, 0.08f));
            }

            // 点击切换 expanded（或交互）
            if (e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition)) {
                expanded = !expanded;
                GUI.changed = true;
                e.Use();
            }

            return expanded;
        }

        // 若只想作为按钮使用（不维护展开状态），用这个
        public static void DrawLabelButton(string title, System.Action onClick, float height = 24f,
            GUIStyle labelStyle = null, System.Action rightCorner = null) {
            var r = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint) {
                var bg = EditorGUIUtility.isProSkin
                    ? new Color(1, 1, 1, 0.06f)
                    : new Color(0, 0, 0, 0.06f);
                EditorGUI.DrawRect(r, bg);
                var line = new Rect(r.x, r.yMax - 1, r.width, 1);
                EditorGUI.DrawRect(line, new Color(0, 0, 0, 0.25f));
            }

            var labelRect = new Rect(r.x + 10, r.y + (r.height - 18) * 0.5f, r.width - 20, 18);
            EditorGUI.LabelField(labelRect, title, labelStyle ?? EditorStyles.boldLabel);

            rightCorner?.Invoke();

            var e = Event.current;
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            if (e.type == EventType.Repaint && r.Contains(e.mousePosition)) {
                EditorGUI.DrawRect(r, new Color(0.24f, 0.48f, 0.90f, 0.08f));
            }

            if (e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition)) {
                onClick?.Invoke();
                e.Use();
            }
        }
    }
}