using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Editor.Core;
using UInteractiveMusic.Runtime.Node;

namespace UInteractiveMusic.Editor.Views {
    public static class IMSwitchContainerTable {
        // 布局常量
        private const float HeaderHeight = 22f;
        private const float RowHeight = 20f;
        private const float LeftPadding = 8f;
        private const float RightPadding = 8f;
        private const float ColumnSpacing = 8f;

        private const float DepthIndent = 14f;
        private const float IconSize = 16f;

        // 每列的最小宽度（拖拽时约束）
        private const float MinName = 100f;
        private const float MinVol = 80f;
        private const float MinLPF = 100f;
        private const float MinNotes = 120f;

        // 分隔线交互
        private const float SeparatorHit = 6f; // 命中区域宽度
        private static readonly Color SeparatorLine = new Color(0, 0, 0, 0.35f);
        private static readonly Color SeparatorHot = new Color(0.24f, 0.48f, 0.90f, 0.6f);

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

        // 统一列计算：根据 state 的可调列宽计算每列 Rect
        // 注意：Name 列为“剩余宽度”，在窗口过窄情况下可能小于其最小值
        private static void CalcColumns(IMEditorState state, Rect total,
            out Rect name, out Rect vol, out Rect lpf, out Rect notes,
            out float nameWidthOut) {
            // 减去左右 padding
            total = new Rect(total.x + LeftPadding, total.y, Mathf.Max(0, total.width - LeftPadding - RightPadding),
                total.height);

            // 先基于 state 中的宽度（已可拖拽）
            float wVol = Mathf.Max(MinVol, state.ColVolWidth);
            float wLPF = Mathf.Max(MinLPF, state.ColLPFWidth);
            float wNotes = Mathf.Max(MinNotes, state.ColNotesWidth);

            // 计算剩余给 Name 的宽度
            float fixedSum = wVol + wLPF + wNotes;
            float nameWidth = total.width - fixedSum - ColumnSpacing * 3f;
            nameWidth = Mathf.Max(20f, nameWidth); // 极端情况下，至少保底留一点空间
            nameWidthOut = nameWidth;

            // 实例化 Rect
            float x = total.x;
            name = new Rect(x, total.y, nameWidth, total.height);
            x += nameWidth + ColumnSpacing;
            vol = new Rect(x, total.y, wVol, total.height);
            x += wVol + ColumnSpacing;
            lpf = new Rect(x, total.y, wLPF, total.height);
            x += wLPF + ColumnSpacing;
            notes = new Rect(x, total.y, Mathf.Max(MinNotes, total.xMax - x), total.height);
        }

        public static void Draw(IMEditorState state, MusicSwitchContainerNode root) {
            // 工具条：标题 + 搜索
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                /*GUILayout.Label("CONTENTS", EditorStyles.boldLabel);*/
                GUILayout.FlexibleSpace();
                state.SwitchSearch = GUILayout.TextField(
                    state.SwitchSearch,
                    EditorStyles.toolbarSearchField,
                    GUILayout.MaxWidth(240)
                );
                if (GUILayout.Button("x", EditorStyles.toolbarButton, GUILayout.Width(20))) {
                    state.SwitchSearch = "";
                    GUI.FocusControl(null);
                }
            }

            // 表头（使用显式 Rect）
            Rect headerRect = GUILayoutUtility.GetRect(0, HeaderHeight, GUILayout.ExpandWidth(true));

            // 计算表头列矩形
            CalcColumns(state, headerRect, out var hName, out var hVol, out var hLPF, out var hNotes,
                out float headerNameWidth);

            // 绘制标题文本
            EditorGUI.LabelField(hName, "Name", EditorStyles.boldLabel);
            EditorGUI.LabelField(hVol, "Voice Volume", EditorStyles.boldLabel);
            EditorGUI.LabelField(hLPF, "Voice Low-pass Filter", EditorStyles.boldLabel);
            EditorGUI.LabelField(hNotes, "Notes", EditorStyles.boldLabel);

            // 底部分隔线
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1, headerRect.width, 1),
                    new Color(0, 0, 0, 0.25f));

            // 绘制列分隔线并处理拖拽
            HandleSeparators(state, headerRect, hName, hVol, hLPF, hNotes, headerNameWidth);

            // 内容区域（滚动）
            using (var sv = new EditorGUILayout.ScrollViewScope(state.RightScroll)) {
                state.RightScroll = sv.scrollPosition;

                var rows = new List<Row>(64);
                CollectDescendants(root, rows, 0);

                for (int i = 0; i < rows.Count; i++) {
                    var r = rows[i];
                    var n = r.node;

                    // 过滤
                    if (!string.IsNullOrEmpty(state.SwitchSearch) &&
                        !n.DisplayName.ToLowerInvariant().Contains(state.SwitchSearch.ToLowerInvariant()))
                        continue;

                    Rect rowRect = GUILayoutUtility.GetRect(0, RowHeight, GUILayout.ExpandWidth(true));

                    // 斑马纹
                    if (Event.current.type == EventType.Repaint && (i & 1) == 1)
                        EditorGUI.DrawRect(rowRect, new Color(1, 1, 1, EditorGUIUtility.isProSkin ? 0.02f : 0.04f));

                    // 与表头一致的列计算
                    CalcColumns(state, rowRect, out var cName, out var cVol, out var cLPF, out var cNotes, out _);

                    // 名称列内部：缩进 + 图标 + 文本
                    float innerX = cName.x + r.depth * DepthIndent;
                    var iconRect = new Rect(innerX, rowRect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
                    Texture icon = IMNodeOps.GetIcon(n.NodeType);
                    if (icon != null) GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                    var labelRect = new Rect(iconRect.xMax + 4, rowRect.y + 2, cName.xMax - (iconRect.xMax + 4), 16);
                    EditorGUI.LabelField(labelRect, n.DisplayName);

                    // 其余列：与表头对齐
                    float newVol = EditorGUI.Slider(cVol, GUIContent.none, n.VoiceVolume, 0f, 100f);
                    float newLPF = EditorGUI.Slider(cLPF, GUIContent.none, n.VoiceLowpass, 0f, 100f);
                    string newNotes = EditorGUI.TextField(cNotes, GUIContent.none, n.Notes);

                    // 变更提交
                    if (!Mathf.Approximately(newVol, n.VoiceVolume) ||
                        !Mathf.Approximately(newLPF, n.VoiceLowpass) ||
                        !string.Equals(newNotes, n.Notes)) {
                        Undo.RecordObject(n, "Edit Node Params");
                        n.VoiceVolume = newVol;
                        n.VoiceLowpass = newLPF;
                        n.Notes = newNotes;
                        EditorUtility.SetDirty(n);
                    }

                    // 点击选中
                    if (rowRect.Contains(Event.current.mousePosition) &&
                        Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                        state.Selected = n;
                        GUI.changed = true;
                        Event.current.Use();
                    }
                }
            }
        }

        private static void HandleSeparators(
            IMEditorState state,
            Rect headerRect,
            Rect hName, Rect hVol, Rect hLPF, Rect hNotes,
            float headerNameWidth) {
            var e = Event.current;

            // 计算三个分隔线的 X 坐标（列右边缘）
            float x0 = hName.xMax; // Name|Vol
            float x1 = hVol.xMax; // Vol|LPF
            float x2 = hLPF.xMax; // LPF|Notes

            // 构造命中区域（更宽），以及可见的中心线（细线）
            var sep0 = new Rect(x0 - SeparatorHit * 0.5f, headerRect.y, SeparatorHit, headerRect.height);
            var sep1 = new Rect(x1 - SeparatorHit * 0.5f, headerRect.y, SeparatorHit, headerRect.height);
            var sep2 = new Rect(x2 - SeparatorHit * 0.5f, headerRect.y, SeparatorHit, headerRect.height);

            // Hover 光标
            EditorGUIUtility.AddCursorRect(sep0, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(sep1, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(sep2, MouseCursor.ResizeHorizontal);

            // 可见分隔线
            if (Event.current.type == EventType.Repaint) {
                DrawSeparatorLine(x0, headerRect);
                DrawSeparatorLine(x1, headerRect);
                DrawSeparatorLine(x2, headerRect);
            }

            // 鼠标按下：开始拖拽
            if (e.type == EventType.MouseDown) {
                if (sep0.Contains(e.mousePosition)) {
                    BeginDrag(state, 0, e.mousePosition.x, headerNameWidth);
                    e.Use();
                }
                else if (sep1.Contains(e.mousePosition)) {
                    BeginDrag(state, 1, e.mousePosition.x, headerNameWidth);
                    e.Use();
                }
                else if (sep2.Contains(e.mousePosition)) {
                    BeginDrag(state, 2, e.mousePosition.x, headerNameWidth);
                    e.Use();
                }
            }

            // 拖拽过程中：更新列宽
            if (state.ColDragging != -1 && e.type == EventType.MouseDrag) {
                float dx = e.mousePosition.x - state.DragStartMouseX;

                switch (state.ColDragging) {
                    // Name|Vol：改 Name 和 Vol
                    case 0: {
                        float pair = state.DragStartNameWidth + state.DragStartVolWidth;
                        float newName = Mathf.Clamp(state.DragStartNameWidth + dx, MinName, pair - MinVol);
                        float newVol = pair - newName;
                        state.ColVolWidth = newVol;
                        GUI.changed = true;
                        break;
                    }
                    // Vol|LPF：改 Vol 和 LPF
                    case 1: {
                        float pair = state.DragStartVolWidth + state.DragStartLPFWidth;
                        float newVol = Mathf.Clamp(state.DragStartVolWidth + dx, MinVol, pair - MinLPF);
                        float newLPF = pair - newVol;
                        state.ColVolWidth = newVol;
                        state.ColLPFWidth = newLPF;
                        GUI.changed = true;
                        break;
                    }
                    // LPF|Notes：改 LPF 和 Notes
                    case 2: {
                        float pair = state.DragStartLPFWidth + state.DragStartNotesWidth;
                        float newLPF = Mathf.Clamp(state.DragStartLPFWidth + dx, MinLPF, pair - MinNotes);
                        float newNotes = pair - newLPF;
                        state.ColLPFWidth = newLPF;
                        state.ColNotesWidth = newNotes;
                        GUI.changed = true;
                        break;
                    }
                }

                e.Use();
            }

            // 结束拖拽
            if (state.ColDragging != -1 && (e.type == EventType.MouseUp || e.rawType == EventType.MouseUp)) {
                state.ColDragging = -1;
                state.SavePrefs(); // 立即持久化
                e.Use();
            }

            // 高亮当前拖拽分隔线
            if (state.ColDragging != -1 && Event.current.type == EventType.Repaint) {
                float hx = state.ColDragging == 0 ? x0 : state.ColDragging == 1 ? x1 : x2;
                var r = new Rect(hx - 1, headerRect.y, 2, headerRect.height);
                EditorGUI.DrawRect(r, SeparatorHot);
            }
        }

        private static void BeginDrag(IMEditorState state, int id, float mouseX, float currentNameWidth) {
            state.ColDragging = id;
            state.DragStartMouseX = mouseX;

            state.DragStartNameWidth = Mathf.Max(MinName, currentNameWidth);
            state.DragStartVolWidth = Mathf.Max(MinVol, state.ColVolWidth);
            state.DragStartLPFWidth = Mathf.Max(MinLPF, state.ColLPFWidth);
            state.DragStartNotesWidth = Mathf.Max(MinNotes, state.ColNotesWidth);
        }

        private static void DrawSeparatorLine(float x, Rect headerRect) {
            var line = new Rect(x - 0.5f, headerRect.y + 2f, 1f, headerRect.height - 4f);
            EditorGUI.DrawRect(line, SeparatorLine);
        }
    }
}