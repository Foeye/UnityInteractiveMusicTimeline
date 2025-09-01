/**
  * @file :MusicPlaylistClipEditor
  * @brief :BRIEF
  * @details : Detail of this file
  * @author :六队
  * @version :0.1
  * @date :2025年8月26日
  * @copyright :Foeye
 */

using System.Collections.Generic;
using IMT.Runtime.Clip;
using IMT.Runtime.Data;
using UnityEditor;
using UnityEngine;

namespace IMT.Editor.Clip {
    [CustomEditor(typeof(MusicPlaylistClip))]
    public class MusicPlaylistClipEditor : UnityEditor.Editor {
        private MusicPlaylistClip clip;
        private MusicPlaylistNode selectedNode;
        private Vector2 scrollPosition;

        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;
        private GUIStyle headerStyle;
        private GUIStyle nodeStyleRoot;
        private GUIStyle selectedNodeStyleRoot;

        // 顶部样式字段区域新增
        private GUIStyle inspectorBox;
        private GUIStyle inspectorHeader;

        private const float INDENT_WIDTH = 25f;
        private const float LINE_THICKNESS = 1f;
        private Color lineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        // 存储绘制时的节点位置信息
        private Dictionary<MusicPlaylistNode, Rect> nodeRects = new Dictionary<MusicPlaylistNode, Rect>();

        private void OnEnable() {
            clip = target as MusicPlaylistClip;
            InitStyles();
        }

        private void InitStyles() {
            nodeStyle = new GUIStyle(EditorStyles.helpBox) {
                padding = new RectOffset(0, 0, 5, 5),
                margin = new RectOffset(0, 0, 2, 2)
            };

            selectedNodeStyle = new GUIStyle(nodeStyle) {
                normal = { background = MakeColorTexture(new Color(0.3f, 0.5f, 0.8f, 0.3f)) }
            };

            headerStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 12
            };
            // 在 InitStyles() 末尾或 nodeStyle/selectedNodeStyle 初始化后添加
            nodeStyleRoot = new GUIStyle(nodeStyle);
            nodeStyleRoot.padding.left = 2; // 根节点更靠左

            selectedNodeStyleRoot = new GUIStyle(selectedNodeStyle);
            selectedNodeStyleRoot.padding.left = 2; // 根节点选中态同样靠左

            // 在 InitStyles() 末尾或现有样式初始化之后添加
            inspectorBox = new GUIStyle(EditorStyles.helpBox) {
                padding = new RectOffset(10, 10, 8, 10),
                margin = new RectOffset(0, 0, 8, 8)
            };

            inspectorHeader = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 12
            };
        }

        private Texture2D MakeColorTexture(Color color) {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public override void OnInspectorGUI() {
            if (clip.playlistData == null) {
                clip.playlistData = new MusicPlaylistData();
            }

            serializedObject.Update();

            // 标题
            EditorGUILayout.LabelField("Music Playlist Structure", headerStyle);
            EditorGUILayout.Space();

            // 工具栏
            DrawToolbar();

            EditorGUILayout.Space();

            // 节点树视图
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));

            // 清空节点位置记录
            nodeRects.Clear();

            if (clip.playlistData.rootNode != null) {
                DrawNodeTree(clip.playlistData.rootNode, 0, null);
            }

            // 绘制所有连线（在节点绘制完成后）
            DrawAllConnections();

            EditorGUILayout.EndScrollView();

            // 选中节点的详细信息
            if (selectedNode != null) {
                EditorGUILayout.Space();
                DrawNodeInspector(selectedNode);
            }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed) {
                EditorUtility.SetDirty(clip);
            }
        }

        private void DrawToolbar() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New Group", EditorStyles.toolbarButton, GUILayout.Width(80))) {
                ShowAddNodeMenu();
            }

            GUILayout.FlexibleSpace();

            if (selectedNode != null && GUILayout.Button("Delete", EditorStyles.toolbarButton, GUILayout.Width(60))) {
                DeleteSelectedNode();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawNodeTree(MusicPlaylistNode node, int indent, MusicPlaylistNode parent) {
            if (node == null) return;

            EditorGUILayout.BeginHorizontal();

            // 缩进
            GUILayout.Space(indent * INDENT_WIDTH);

            // 节点内容
            var isRoot = indent == 0;
            var style = node.isSelected
                ? (isRoot ? selectedNodeStyleRoot : selectedNodeStyle)
                : (isRoot ? nodeStyleRoot : nodeStyle);

            EditorGUILayout.BeginVertical(style);

            // 节点头部
            EditorGUILayout.BeginHorizontal();

            // 展开/折叠按钮
            const float FOLDOUT_W = 14f;
            if (node.children.Count > 0) {
                // 生成一个固定宽度、高度为单行高的矩形
                Rect foldRect = GUILayoutUtility.GetRect(FOLDOUT_W, EditorGUIUtility.singleLineHeight,
                    GUILayout.Width(FOLDOUT_W));
                // 关闭 toggleOnLabelClick，避免整行都可点击
                node.isExpanded = EditorGUI.Foldout(foldRect, node.isExpanded, GUIContent.none, false);
            }
            else {
                GUILayout.Space(FOLDOUT_W);
            }

            // 节点图标和名称
            var icon = GetNodeIcon(node.nodeType);
            if (icon != null) {
                GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
            }

            if (GUILayout.Button(node.nodeName, EditorStyles.label)) {
                SelectNode(node);
            }

            GUILayout.FlexibleSpace();

            // 显示关键参数
            DrawNodeQuickInfo(node);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // 记录节点位置
            var rect = GUILayoutUtility.GetLastRect();
            node.lastRect = rect;
            nodeRects[node] = rect;
            node.parent = parent;

            EditorGUILayout.EndHorizontal();

            // 递归绘制子节点
            if (node.isExpanded) {
                foreach (var child in node.children) {
                    DrawNodeTree(child, indent + 1, node);
                }
            }
        }

        private void DrawAllConnections() {
            Handles.BeginGUI();
            Handles.color = lineColor;

            foreach (var kvp in nodeRects) {
                var node = kvp.Key;
                var nodeRect = kvp.Value;

                if (node.parent != null && nodeRects.ContainsKey(node.parent)) {
                    var parentRect = nodeRects[node.parent];
                    DrawLShapedConnection(parentRect, nodeRect);
                }
            }

            Handles.EndGUI();
        }

        private void DrawLShapedConnection(Rect parentRect, Rect childRect) {
            // 计算连接点
            float parentX = parentRect.x; // 从父节点的展开按钮位置开始
            float parentY = parentRect.y + parentRect.height;

            float childX = childRect.x - 5f; // 子节点左边缘前一点
            float childY = childRect.y + childRect.height / 2f;

            // 中间点（形成直角）
            float midX = parentX + INDENT_WIDTH / 2f;

            // 绘制三段线：
            // 1. 父节点水平延伸线
            //DrawLine(new Vector2(parentX, parentY), new Vector2(midX, parentY));

            // 2. 垂直连接线
            DrawLine(new Vector2(midX, parentY), new Vector2(midX, childY));

            // 3. 子节点水平连接线
            DrawLine(new Vector2(midX, childY), new Vector2(childX, childY));

            // 绘制连接点（可选）
            DrawConnectionDot(new Vector2(childX, childY));
        }

        private void DrawLine(Vector2 start, Vector2 end) {
            // 使用粗一点的线条
            for (int i = 0; i < LINE_THICKNESS; i++) {
                Handles.DrawLine(
                    new Vector3(start.x, start.y + i, 0),
                    new Vector3(end.x, end.y + i, 0)
                );
            }
        }

        private void DrawConnectionDot(Vector2 position) {
            // 在连接点绘制一个小圆点
            var oldColor = Handles.color;
            Handles.color = new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a * 1.5f);

            var size = 3f;
            var rect = new Rect(position.x - size / 2, position.y - size / 2, size, size);
            EditorGUI.DrawRect(rect, Handles.color);

            Handles.color = oldColor;
        }

        private void DrawNodeQuickInfo(MusicPlaylistNode node) {
            var miniStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };

            switch (node.nodeType) {
                case NodeType.RandomContinuous:
                    GUILayout.Label($"Random: {node.randomType}", miniStyle, GUILayout.Width(100));
                    if (node.avoidRepeat) {
                        GUILayout.Label("Avoid Repeat", miniStyle, GUILayout.Width(80));
                    }

                    break;

                case NodeType.SequenceContinuous:
                case NodeType.MusicSegment:
                    if (node.parent != null && node.parent.nodeType == NodeType.RandomContinuous) {
                        GUILayout.Label($"Weight: {node.weight}", miniStyle, GUILayout.Width(70));
                    }

                    break;
            }

            if (node.loopCount > 1) {
                GUILayout.Label($"Loop: {node.loopCount}", miniStyle, GUILayout.Width(50));
            }
        }

        private void DrawNodeInspector(MusicPlaylistNode node) {
            // 原先的标题/空行替换为 Box 面板
            EditorGUILayout.BeginVertical(inspectorBox);
            EditorGUILayout.LabelField("Node Properties", inspectorHeader);
            GUILayout.Space(4);

            // —— 以下保留你原有的参数绘制逻辑 —— //
            node.nodeName = EditorGUILayout.TextField("Name", node.nodeName);
            node.loopCount = EditorGUILayout.IntSlider("Loop Count", node.loopCount, 1, 10);

            switch (node.nodeType) {
                case NodeType.RandomContinuous:
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Random Settings", EditorStyles.boldLabel);
                    node.randomType = (RandomType)EditorGUILayout.EnumPopup("Random Type", node.randomType);
                    node.avoidRepeat = EditorGUILayout.Toggle("Avoid Repeat", node.avoidRepeat);
                    break;

                case NodeType.MusicSegment:
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Audio Settings", EditorStyles.boldLabel);
                    node.audioClip =
                        (AudioClip)EditorGUILayout.ObjectField("Audio Clip", node.audioClip, typeof(AudioClip), false);
                    break;
            }

            if (node.parent != null && node.parent.nodeType == NodeType.RandomContinuous) {
                EditorGUILayout.Space();
                node.weight = EditorGUILayout.Slider("Weight", node.weight, 0f, 100f);
            }

            // 添加子节点按钮（如有）
            if (node.nodeType != NodeType.MusicSegment) {
                EditorGUILayout.Space();
                if (GUILayout.Button("Add Child Node"))
                    ShowAddChildMenu(node);
            }

            EditorGUILayout.EndVertical(); // 结束 Box
        }

        private void SelectNode(MusicPlaylistNode node) {
            if (selectedNode != null)
                selectedNode.isSelected = false;

            selectedNode = node;
            node.isSelected = true;

            Repaint();
        }

        private void ShowAddNodeMenu() {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Sequence Step"), false, () => AddRootNode(NodeType.SequenceStep));
            menu.AddItem(new GUIContent("Random Continuous"), false, () => AddRootNode(NodeType.RandomContinuous));
            menu.AddItem(new GUIContent("Sequence Continuous"), false, () => AddRootNode(NodeType.SequenceContinuous));

            menu.ShowAsContext();
        }

        private void ShowAddChildMenu(MusicPlaylistNode parent) {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Sequence Step"), false, () => AddChildNode(parent, NodeType.SequenceStep));
            menu.AddItem(new GUIContent("Random Continuous"), false,
                () => AddChildNode(parent, NodeType.RandomContinuous));
            menu.AddItem(new GUIContent("Sequence Continuous"), false,
                () => AddChildNode(parent, NodeType.SequenceContinuous));
            menu.AddItem(new GUIContent("Music Segment"), false, () => AddChildNode(parent, NodeType.MusicSegment));

            menu.ShowAsContext();
        }

        private void AddRootNode(NodeType type) {
            clip.playlistData.rootNode = new MusicPlaylistNode {
                nodeType = type,
                nodeName = type.ToString(),
                loopCount = 1
            };
        }

        private void AddChildNode(MusicPlaylistNode parent, NodeType type) {
            var newNode = new MusicPlaylistNode {
                nodeType = type,
                nodeName = type.ToString(),
                parent = parent,
                loopCount = 1,
                weight = 50f
            };

            parent.children.Add(newNode);
        }

        private void DeleteSelectedNode() {
            if (selectedNode == null) return;

            if (selectedNode == clip.playlistData.rootNode) {
                clip.playlistData.rootNode = null;
            }
            else if (selectedNode.parent != null) {
                selectedNode.parent.children.Remove(selectedNode);
            }

            selectedNode = null;
        }

        private Texture GetNodeIcon(NodeType type) {
            switch (type) {
                case NodeType.SequenceStep:
                    return EditorGUIUtility.IconContent("d_UnityEditor.HierarchyWindow").image;
                case NodeType.RandomContinuous:
                    return EditorGUIUtility.IconContent("d_UnityEditor.Graphs.AnimatorControllerTool").image;
                case NodeType.SequenceContinuous:
                    return EditorGUIUtility.IconContent("d_align_horizontally_center").image;
                case NodeType.MusicSegment:
                    return EditorGUIUtility.IconContent("d_AudioSource Icon").image;
                default:
                    return null;
            }
        }
    }
}