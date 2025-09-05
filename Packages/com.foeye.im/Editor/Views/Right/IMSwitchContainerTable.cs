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

        // 新增：与左侧一致的小三角区域宽度
        private const float FoldoutWidth = 12f;

        // 每列的最小宽度（拖拽时约束）
        private const float MinName = 100f;
        private const float MinVol = 80f;
        private const float MinLPF = 100f;
        private const float MinNotes = 120f;

        // 分隔线交互
        private const float SeparatorHit = 6f; // 命中区域宽度
        private static readonly Color SeparatorLine = new Color(0, 0, 0, 0.35f);
        private static readonly Color SeparatorHot = new Color(0.24f, 0.48f, 0.90f, 0.6f);

        // 原有 Row/CollectDescendants 可保留或删除，这里不再使用扁平化收集
        private struct Row {
            public IMNode node;
            public int depth;
        }

        // 统一列计算：根据 state 的可调列宽计算每列 Rect
        private static void CalcColumns(IMEditorState state, Rect total,
            out Rect name, out Rect vol, out Rect lpf, out Rect notes,
            out float nameWidthOut) {
            total = new Rect(total.x + LeftPadding, total.y, Mathf.Max(0, total.width - LeftPadding - RightPadding),
                total.height);

            float wVol = Mathf.Max(MinVol, state.ColVolWidth);
            float wLPF = Mathf.Max(MinLPF, state.ColLPFWidth);
            float wNotes = Mathf.Max(MinNotes, state.ColNotesWidth);

            float fixedSum = wVol + wLPF + wNotes;
            float nameWidth = total.width - fixedSum - ColumnSpacing * 3f;
            nameWidth = Mathf.Max(20f, nameWidth);
            nameWidthOut = nameWidth;

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

            // 表头
            Rect headerRect = GUILayoutUtility.GetRect(0, HeaderHeight, GUILayout.ExpandWidth(true));
            CalcColumns(state, headerRect, out var hName, out var hVol, out var hLPF, out var hNotes,
                out float headerNameWidth);

            EditorGUI.LabelField(hName, "Name", EditorStyles.boldLabel);
            EditorGUI.LabelField(hVol, "Voice Volume", EditorStyles.boldLabel);
            EditorGUI.LabelField(hLPF, "Voice Low-pass Filter", EditorStyles.boldLabel);
            EditorGUI.LabelField(hNotes, "Notes", EditorStyles.boldLabel);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1, headerRect.width, 1),
                    new Color(0, 0, 0, 0.25f));

            HandleSeparators(state, headerRect, hName, hVol, hLPF, hNotes, headerNameWidth);

            // ========= 替换：内容区域改为树形递归绘制 =========
            using (var sv = new EditorGUILayout.ScrollViewScope(state.RightScroll)) {
                state.RightScroll = sv.scrollPosition;

                int rowIndex = 0;
                string filterLower = string.IsNullOrEmpty(state.SwitchSearch)
                    ? null
                    : state.SwitchSearch.ToLowerInvariant();

                if (root.Children != null) {
                    foreach (var child in root.Children) {
                        DrawNodeRecursive(state, child, 0, filterLower, ref rowIndex);
                    }
                }
            }
        }

        // 新增：是否有“子项槽位”（决定是否显示 Foldout）
        private static bool HasChildrenSlot(IMNode n) {
            return n.NodeType != MusicNodeType.MusicSegment;
        }

        // 新增：搜索显示策略——自己匹配或任一子孙匹配则显示该分支
        private static bool ShouldShow(IMNode node, string filterLower) {
            if (string.IsNullOrEmpty(filterLower)) return true;
            if (!string.IsNullOrEmpty(node.DisplayName) &&
                node.DisplayName.ToLowerInvariant().Contains(filterLower)) {
                return true;
            }

            if (node.Children != null) {
                foreach (var c in node.Children) {
                    if (ShouldShow(c, filterLower)) return true;
                }
            }

            return false;
        }

        // 新增：树形递归绘制
        private static void DrawNodeRecursive(IMEditorState state, IMNode node, int depth, string filterLower,
            ref int rowIndex) {
            if (!ShouldShow(node, filterLower)) return;

            Rect rowRect = GUILayoutUtility.GetRect(0, RowHeight, GUILayout.ExpandWidth(true));

            // 斑马纹
            if (Event.current.type == EventType.Repaint && (rowIndex & 1) == 1) {
                EditorGUI.DrawRect(rowRect, new Color(1, 1, 1, EditorGUIUtility.isProSkin ? 0.02f : 0.04f));
            }

            // 选中高亮覆盖
            if (Event.current.type == EventType.Repaint && state.Selected == node) {
                EditorGUI.DrawRect(rowRect, IMEditorStyles.RowSelected);
            }

            // 列与单行绘制
            CalcColumns(state, rowRect, out var cName, out var cVol, out var cLPF, out var cNotes, out _);
            DrawSingleRow(state, node, depth, rowRect, cName, cVol, cLPF, cNotes);

            rowIndex++;

            // 递归子项
            if (node.Expanded && node.Children != null) {
                foreach (var c in node.Children) {
                    DrawNodeRecursive(state, c, depth + 1, filterLower, ref rowIndex);
                }
            }
        }

        // 新增：单行绘制（Name 列：Foldout + Icon + Label；其余列沿用原控件）
        private static void DrawSingleRow(
            IMEditorState state,
            IMNode n,
            int depth,
            Rect rowRect,
            Rect cName, Rect cVol, Rect cLPF, Rect cNotes
        ) {
            // Name 列内部：缩进 + 可选 Foldout + 图标 + 文本
            float x = cName.x + depth * DepthIndent;

            bool showFoldout = HasChildrenSlot(n);
            if (showFoldout) {
                var foldRect = new Rect(x, rowRect.y + 2f, FoldoutWidth, RowHeight - 4f);
                n.Expanded = EditorGUI.Foldout(foldRect, n.Expanded, GUIContent.none, true);
                x += FoldoutWidth;
            }

            var iconRect = new Rect(x, rowRect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
            Texture icon = IMNodeOps.GetIcon(n.NodeType);
            if (icon != null) GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

            var labelRect = new Rect(iconRect.xMax + 4f, rowRect.y + 2f, cName.xMax - (iconRect.xMax + 4f), 16f);
            EditorGUI.LabelField(labelRect, n.DisplayName);

            // 其余列：与表头对齐（沿用原控件）
            float newVol = EditorGUI.Slider(cVol, GUIContent.none, n.VoiceVolume, 0f, 100f);
            float newLPF = EditorGUI.Slider(cLPF, GUIContent.none, n.VoiceLowpass, 0f, 100f);
            string newNotes = EditorGUI.TextField(cNotes, GUIContent.none, n.Notes);

            if (!Mathf.Approximately(newVol, n.VoiceVolume) ||
                !Mathf.Approximately(newLPF, n.VoiceLowpass) ||
                !string.Equals(newNotes, n.Notes)) {
                Undo.RecordObject(n, "Edit Node Params");
                n.VoiceVolume = newVol;
                n.VoiceLowpass = newLPF;
                n.Notes = newNotes;
                EditorUtility.SetDirty(n);
            }

            // 点击选中（与左侧一致）
            if (rowRect.Contains(Event.current.mousePosition) &&
                Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                state.Selected = n;
                GUI.changed = true;
                Event.current.Use();
            }
        }

        // 原有 HandleSeparators/BeginDrag/DrawSeparatorLine 维持不变
        private static void HandleSeparators(
            IMEditorState state,
            Rect headerRect,
            Rect hName, Rect hVol, Rect hLPF, Rect hNotes,
            float headerNameWidth) {
            var e = Event.current;

            float x0 = hName.xMax; // Name|Vol
            float x1 = hVol.xMax; // Vol|LPF
            float x2 = hLPF.xMax; // LPF|Notes

            var sep0 = new Rect(x0 - SeparatorHit * 0.5f, headerRect.y, SeparatorHit, headerRect.height);
            var sep1 = new Rect(x1 - SeparatorHit * 0.5f, headerRect.y, SeparatorHit, headerRect.height);
            var sep2 = new Rect(x2 - SeparatorHit * 0.5f, headerRect.y, SeparatorHit, headerRect.height);

            EditorGUIUtility.AddCursorRect(sep0, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(sep1, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(sep2, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.Repaint) {
                DrawSeparatorLine(x0, headerRect);
                DrawSeparatorLine(x1, headerRect);
                DrawSeparatorLine(x2, headerRect);
            }

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

            if (state.ColDragging != -1 && e.type == EventType.MouseDrag) {
                float dx = e.mousePosition.x - state.DragStartMouseX;

                switch (state.ColDragging) {
                    case 0: {
                        float pair = state.DragStartNameWidth + state.DragStartVolWidth;
                        float newName = Mathf.Clamp(state.DragStartNameWidth + dx, MinName, pair - MinVol);
                        float newVol = pair - newName;
                        state.ColVolWidth = newVol;
                        GUI.changed = true;
                        break;
                    }
                    case 1: {
                        float pair = state.DragStartVolWidth + state.DragStartLPFWidth;
                        float newVol = Mathf.Clamp(state.DragStartVolWidth + dx, MinVol, pair - MinLPF);
                        float newLPF = pair - newVol;
                        state.ColVolWidth = newVol;
                        state.ColLPFWidth = newLPF;
                        GUI.changed = true;
                        break;
                    }
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

            if (state.ColDragging != -1 && (e.type == EventType.MouseUp || e.rawType == EventType.MouseUp)) {
                state.ColDragging = -1;
                state.SavePrefs();
                e.Use();
            }

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