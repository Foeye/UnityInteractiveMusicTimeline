using UnityEditor;
using UnityEngine;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Editor.Core;
using UInteractiveMusic.Editor.Views;

namespace UInteractiveMusic.Editor {
    public class InteractiveMusicHierarchyWindow : EditorWindow {
        private IMEditorState _state;
        private IMHierarchyTreeView _treeView;
        private IMRightPanel _rightPanel;

        private InteractiveMusicHierarchyAsset _asset;

        [MenuItem("Window/Interactive Music/Hierarchy")]
        public static void Open() {
            var w = GetWindow<InteractiveMusicHierarchyWindow>();
            w.titleContent = new GUIContent("Interactive Music Hierarchy");
            w.Show();
        }

        private void OnEnable() {
            _state = new IMEditorState();
            _state.LoadPrefs();

            _treeView = new IMHierarchyTreeView(_state);
            _rightPanel = new IMRightPanel(_state);

            if (_asset == null) {
                string[] guids = AssetDatabase.FindAssets("t:InteractiveMusicHierarchyAsset");
                if (guids.Length > 0) {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _asset = AssetDatabase.LoadAssetAtPath<InteractiveMusicHierarchyAsset>(path);
                }
            }
        }

        private void OnDisable() {
            _state.SavePrefs();
        }

        private void OnGUI() {
            DrawToolbar();
            var toolbarRect = GUILayoutUtility.GetLastRect();

            var contentRect = new Rect(0f, toolbarRect.yMax, position.width, position.height - toolbarRect.yMax);

            _state.LeftWidth = Mathf.Clamp(_state.LeftWidth, IMEditorState.MinLeftWidth,
                Mathf.Min(IMEditorState.MaxLeftWidth, contentRect.width - 200f));
            var leftRect = new Rect(contentRect.x, contentRect.y, _state.LeftWidth, contentRect.height);
            var splitterRect = new Rect(leftRect.xMax, contentRect.y, IMEditorState.SplitterWidth, contentRect.height);
            var rightRect = new Rect(splitterRect.xMax, contentRect.y,
                contentRect.width - leftRect.width - IMEditorState.SplitterWidth, contentRect.height);

            IMEditorStyles.DrawPaneBackground(leftRect, true);
            IMEditorStyles.DrawPaneBackground(rightRect, false);

            // 左侧（BeginArea + 无样式滚动，消除顶部空白）
            GUILayout.BeginArea(leftRect);
            {
                using (new EditorGUILayout.VerticalScope(GUIStyle.none)) {
                    GUILayout.Space(EditorStyles.toolbar.fixedHeight);
                    _state.LeftScroll = EditorGUILayout.BeginScrollView(_state.LeftScroll, false, false, GUIStyle.none,
                        GUIStyle.none, GUIStyle.none);
                    {
                        _treeView.Draw(_asset);
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            GUILayout.EndArea();

            HandleSplitter(contentRect, splitterRect);

            // 右侧
            GUILayout.BeginArea(rightRect);
            {
                using (new EditorGUILayout.VerticalScope()) {
                    GUILayout.Space(EditorStyles.toolbar.fixedHeight);
                    _rightPanel.Draw();
                }
            }
            GUILayout.EndArea();
        }

        private void DrawToolbar() {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
                _asset = (InteractiveMusicHierarchyAsset)EditorGUILayout.ObjectField(
                    _asset, typeof(InteractiveMusicHierarchyAsset), false, GUILayout.MaxWidth(420));

                if (GUILayout.Button("新建", EditorStyles.toolbarButton, GUILayout.Width(60))) {
                    var path = EditorUtility.SaveFilePanelInProject(
                        "Create Interactive Music Hierarchy",
                        "InteractiveMusicHierarchy",
                        "asset",
                        "选择保存路径");
                    if (!string.IsNullOrEmpty(path)) {
                        _asset = IMNodeOps.CreateNewHierarchyAsset(path);
                        EditorGUIUtility.PingObject(_asset);
                    }
                }

                if (_asset != null && GUILayout.Button("定位资产", EditorStyles.toolbarButton, GUILayout.Width(70))) {
                    EditorGUIUtility.PingObject(_asset);
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void HandleSplitter(Rect contentRect, Rect splitterRect) {
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

            var e = Event.current;
            if (e.type == EventType.MouseDown && splitterRect.Contains(e.mousePosition)) {
                _state.DraggingSplitter = true;
                e.Use();
            }

            if (_state.DraggingSplitter) {
                if (e.type == EventType.MouseDrag) {
                    float mouseX = Mathf.Clamp(e.mousePosition.x, IMEditorState.MinLeftWidth, contentRect.width - 200f);
                    _state.LeftWidth = Mathf.Clamp(mouseX, IMEditorState.MinLeftWidth, IMEditorState.MaxLeftWidth);
                    Repaint();
                    e.Use();
                }
                else if (e.type == EventType.MouseUp || e.rawType == EventType.MouseUp) {
                    _state.DraggingSplitter = false;
                    e.Use();
                }
            }

            var line = splitterRect;
            line.width = 1f;
            EditorGUI.DrawRect(line, IMEditorStyles.SplitLine);
        }
    }
}