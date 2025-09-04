using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Editor.Core;
using UInteractiveMusic.Runtime.Node;

namespace UInteractiveMusic.Editor.Views {
    public static class IMSwitchContainerTable {
        private const float ColVolWidth = 120f;
        private const float ColLPFWidth = 150f;
        private const float ColNotesMinWidth = 200f;

        private struct Row {
            public IMNode node;
            public int depth;
        }

        private static void CollectDescendants(IMNode root, List<Row> buffer, int depth) {
            if (root.Children == null) return;
            foreach (var c in root.Children) {
                buffer.Add(new Row { node = c, depth = depth });
                CollectDescendants(c, buffer, depth + 1);
            }
        }

        public static void Draw(IMEditorState state, MusicSwitchContainerNode root) {
            // 工具条：标题 + 搜索
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                GUILayout.Label("Children Overview", EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();
                state.SwitchSearch = GUILayout.TextField(
                    state.SwitchSearch,
                    EditorStyles.toolbarSearchField,
                    GUILayout.MaxWidth(240));
                if (GUILayout.Button("x", EditorStyles.toolbarButton, GUILayout.Width(20))) {
                    state.SwitchSearch = "";
                    GUI.FocusControl(null);
                }
            }

            // 表头
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
                GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                GUILayout.Label("Voice Volume", EditorStyles.boldLabel, GUILayout.Width(ColVolWidth));
                GUILayout.Label("Voice Low-pass Filter", EditorStyles.boldLabel, GUILayout.Width(ColLPFWidth));
                GUILayout.Label("Notes", EditorStyles.boldLabel, GUILayout.MinWidth(ColNotesMinWidth));
            }

            // 内容
            using (var sv = new EditorGUILayout.ScrollViewScope(state.RightScroll)) {
                state.RightScroll = sv.scrollPosition;

                var rows = new List<Row>(64);
                CollectDescendants(root, rows, 0);

                for (int i = 0; i < rows.Count; i++) {
                    var r = rows[i];
                    var n = r.node;

                    if (!string.IsNullOrEmpty(state.SwitchSearch) &&
                        !n.DisplayName.ToLowerInvariant().Contains(state.SwitchSearch.ToLowerInvariant()))
                        continue;

                    Rect rowRect = GUILayoutUtility.GetRect(0, 20f, GUILayout.ExpandWidth(true));
                    if (Event.current.type == EventType.Repaint && (i & 1) == 1)
                        EditorGUI.DrawRect(rowRect, new Color(1, 1, 1, EditorGUIUtility.isProSkin ? 0.02f : 0.04f));

                    float x = rowRect.x + 6 + r.depth * 14f;

                    // 图标 + 名称
                    Texture icon = IMNodeOps.GetIcon(n.NodeType);
                    var iconRect = new Rect(x, rowRect.y + 2, 16, 16);
                    if (icon != null) GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    x += 16 + 4;

                    var nameRect = new Rect(x, rowRect.y + 2,
                        rowRect.xMax - (ColVolWidth + ColLPFWidth + ColNotesMinWidth + 24) - x, 16);
                    EditorGUI.LabelField(nameRect, n.DisplayName);

                    // Volume
                    var volRect = new Rect(rowRect.xMax - (ColVolWidth + ColLPFWidth + ColNotesMinWidth) - 8,
                        rowRect.y + 2, ColVolWidth, 16);
                    float newVol = EditorGUI.Slider(volRect, GUIContent.none, n.VoiceVolume, 0f, 100f);

                    // LPF
                    var lpfRect = new Rect(rowRect.xMax - (ColLPFWidth + ColNotesMinWidth) - 8, rowRect.y + 2,
                        ColLPFWidth, 16);
                    float newLPF = EditorGUI.Slider(lpfRect, GUIContent.none, n.VoiceLowpass, 0f, 100f);

                    // Notes
                    var notesRect = new Rect(rowRect.xMax - ColNotesMinWidth - 8, rowRect.y + 2, ColNotesMinWidth, 16);
                    string newNotes = EditorGUI.TextField(notesRect, GUIContent.none, n.Notes);

                    if (!Mathf.Approximately(newVol, n.VoiceVolume) ||
                        !Mathf.Approximately(newLPF, n.VoiceLowpass) ||
                        !string.Equals(newNotes, n.Notes)) {
                        Undo.RecordObject(n, "Edit Node Params");
                        n.VoiceVolume = newVol;
                        n.VoiceLowpass = newLPF;
                        n.Notes = newNotes;
                        EditorUtility.SetDirty(n);
                    }

                    // 点击行，同步选中
                    if (rowRect.Contains(Event.current.mousePosition) &&
                        Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                        state.Selected = n;
                        GUI.changed = true;
                        Event.current.Use();
                    }
                }
            }
        }
    }
}