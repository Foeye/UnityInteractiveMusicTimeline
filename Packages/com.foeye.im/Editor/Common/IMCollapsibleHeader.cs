using UnityEditor;
using UnityEngine;

namespace UInteractiveMusic.Editor.Views {
    public static class IMCollapsibleHeader {
        private static readonly GUIContent FoldOn = EditorGUIUtility.IconContent("IN foldout on");
        private static readonly GUIContent FoldOff = EditorGUIUtility.IconContent("IN foldout");

        // 绘制一个可点击的标题头，返回是否展开
        public static bool Draw(string title, bool expanded, System.Action rightCorner = null) {
            var r = GUILayoutUtility.GetRect(0, 24f, GUILayout.ExpandWidth(true));
            // 背景
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.25f));

            // 折叠图标
            var iconRect = new Rect(r.x + 6, r.y + 4, 16, 16);
            GUI.Label(iconRect, expanded ? FoldOn : FoldOff);

            // 标题文本（可点击）
            var labelRect = new Rect(iconRect.xMax + 4, r.y + 2, r.width - 80, 20);
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

            // 右侧插槽（可选按钮/帮助等）
            rightCorner?.Invoke();

            // 点击切换
            var e = Event.current;
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition)) {
                expanded = !expanded;
                GUI.changed = true;
                e.Use();
            }

            return expanded;
        }
    }
}