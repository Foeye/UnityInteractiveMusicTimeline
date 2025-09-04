// Assets/UInteractiveMusic/Editor/Views/Common/IMTabs.cs
using UnityEditor;
using UnityEngine;

namespace UInteractiveMusic.Editor.Views
{
    public static class IMTabs
    {
        public static int DrawUnderlineTabs(
            Rect r,
            string[] titles,
            int selectedIndex,
            float underlineHeight = 2f,
            float spacing = 18f,
            GUIStyle activeStyle = null,
            GUIStyle inactiveStyle = null)
        {
            if (titles == null || titles.Length == 0) return 0;

            var bg = EditorGUIUtility.isProSkin ? new Color(1,1,1,0.05f) : new Color(0,0,0,0.05f);
            var line = new Color(0,0,0,0.2f);
            var activeUnderline = EditorGUIUtility.isProSkin ? new Color(1,1,1,0.85f) : new Color(0.15f,0.15f,0.15f);

            activeStyle ??= new GUIStyle(EditorStyles.miniBoldLabel) { fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            inactiveStyle ??= new GUIStyle(EditorStyles.miniLabel) { fontSize = 12, alignment = TextAnchor.MiddleLeft };

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(r, bg);
                EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), line);
            }

            float x = r.x + 10f;
            float yCenter = r.y + r.height * 0.5f;

            for (int i = 0; i < titles.Length; i++)
            {
                var style = i == selectedIndex ? activeStyle : inactiveStyle;
                var content = new GUIContent(titles[i]);
                var size = style.CalcSize(content);
                var labelRect = new Rect(x, yCenter - size.y * 0.5f, size.x, size.y);

                EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
                GUI.Label(labelRect, content, style);

                if (i == selectedIndex && Event.current.type == EventType.Repaint)
                {
                    var ul = new Rect(labelRect.x, r.yMax - underlineHeight - 1, labelRect.width, underlineHeight);
                    EditorGUI.DrawRect(ul, activeUnderline);
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRect.Contains(Event.current.mousePosition))
                {
                    selectedIndex = i;
                    GUI.changed = true;
                    Event.current.Use();
                }

                x += size.x + spacing;
            }

            return Mathf.Clamp(selectedIndex, 0, titles.Length - 1);
        }
    }
}
