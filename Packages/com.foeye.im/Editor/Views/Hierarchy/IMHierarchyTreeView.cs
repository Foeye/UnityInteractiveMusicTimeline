using UnityEditor;
using UnityEngine;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Editor.Core;
using UInteractiveMusic.Runtime.Node;

namespace UInteractiveMusic.Editor.Views {
    public class IMHierarchyTreeView {
        // 手动行布局常量
        private const float RowHeight = 20f;
        private const float IndentWidth = 16f;
        private const float FoldoutWidth = 12f;
        private const float IconSize = 16f;
        private const float RowPaddingX = 4f;
        private const float PlusButtonWidth = 22f;

        private readonly IMEditorState _state;

        public IMHierarchyTreeView(IMEditorState state) {
            _state = state;
        }

        public void Draw(InteractiveMusicHierarchyAsset asset) {
            if (asset == null) {
                EditorGUILayout.HelpBox("尚未选择或创建层级资产。", MessageType.Info);
                if (GUILayout.Button("在默认路径创建新层级资产", GUILayout.Height(24))) {
                    var a = IMNodeOps.CreateNewHierarchyAsset(IMNodeOps.DefaultAssetPath);
                    EditorGUIUtility.PingObject(a);
                }

                return;
            }

            if (asset.Root == null)
                IMNodeOps.EnsureRootNode(asset);

            DrawNodeRecursive(asset.Root, 0, asset);
        }

        private void DrawNodeRecursive(IMNode node, int indent, InteractiveMusicHierarchyAsset asset) {
            if (node == null) return;

            DrawRow(node, indent, asset);

            if (node.Expanded && node.Children != null) {
                foreach (var child in node.Children)
                    DrawNodeRecursive(child, indent + 1, asset);
            }
        }

        private void DrawRow(IMNode node, int indent, InteractiveMusicHierarchyAsset asset) {
            Rect rowRect = GUILayoutUtility.GetRect(0, RowHeight, GUILayout.ExpandWidth(true));

            bool isSelected = _state.Selected == node;
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rowRect, isSelected ? IMEditorStyles.RowSelected : IMEditorStyles.RowNormal);

            float x = rowRect.x + RowPaddingX + indent * IndentWidth;

            bool hasChildrenSlot = node.NodeType != MusicNodeType.MusicSegment;
            if (hasChildrenSlot) {
                var foldRect = new Rect(x, rowRect.y + 2f, FoldoutWidth, RowHeight - 4f);
                node.Expanded = EditorGUI.Foldout(foldRect, node.Expanded, GUIContent.none, true);
                x += FoldoutWidth;
            }

            Texture icon = IMNodeOps.GetIcon(node.NodeType);
            var iconRect = new Rect(x, rowRect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
            if (icon != null) GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            x += IconSize + 4f;

            var plusRect = new Rect(rowRect.xMax - PlusButtonWidth - 2f, rowRect.y + 2f, PlusButtonWidth,
                RowHeight - 4f);
            var labelRect = new Rect(x, rowRect.y, Mathf.Max(0, plusRect.xMin - 4f - x), rowRect.height);

            if (_state.RenameTarget == node) {
                GUI.SetNextControlName("RenameField");
                string result = EditorGUI.DelayedTextField(labelRect, _state.RenameBuffer);
                if (!string.Equals(result, _state.RenameBuffer)) {
                    IMNodeOps.CommitRename(node, result);
                    _state.RenameTarget = null;
                }
            }
            else {
                EditorGUI.LabelField(labelRect, node.DisplayName);
            }

            using (new EditorGUI.DisabledScope(node.NodeType == MusicNodeType.MusicSegment)) {
                if (GUI.Button(plusRect, "+")) {
                    ShowCreateMenuFor(asset, node);
                }
            }

            var e = Event.current;
            if (rowRect.Contains(e.mousePosition)) {
                if (e.type == EventType.MouseDown && e.button == 0) {
                    _state.Selected = node;
                    if (_state.RenameTarget != null && _state.RenameTarget != node)
                        _state.RenameTarget = null;
                    GUI.changed = true;
                    e.Use();
                }
                else if (e.type == EventType.ContextClick) {
                    _state.Selected = node;
                    ShowContextMenu(asset, node);
                    e.Use();
                }
            }
        }

        private void ShowCreateMenuFor(InteractiveMusicHierarchyAsset asset, IMNode parent) {
            var menu = new GenericMenu();
            AddCreateMenuItems(menu, asset, parent);
            menu.ShowAsContext();
        }

        private void ShowContextMenu(InteractiveMusicHierarchyAsset asset, IMNode node) {
            var menu = new GenericMenu();

            AddCreateMenuItems(menu, asset, node);
            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Rename"), false, () => {
                _state.RenameTarget = node;
                _state.RenameBuffer = node.DisplayName;
            });

            if (node.NodeType != MusicNodeType.Root)
                menu.AddItem(new GUIContent("Delete"), false, () => IMNodeOps.DeleteNode(asset, node));
            else
                menu.AddDisabledItem(new GUIContent("Delete"));

            menu.ShowAsContext();
        }

        private void AddCreateMenuItems(GenericMenu menu, InteractiveMusicHierarchyAsset asset, IMNode parent) {
            AddCreateItem(menu, asset, parent, MusicNodeType.MusicSwitchContainer, "Create/Music Switch Container");
            AddCreateItem(menu, asset, parent, MusicNodeType.MusicPlaylistContainer, "Create/Music Playlist Container");
            AddCreateItem(menu, asset, parent, MusicNodeType.MusicSegment, "Create/Music Segment");
        }

        private void AddCreateItem(GenericMenu menu, InteractiveMusicHierarchyAsset asset, IMNode parent,
            MusicNodeType type, string path) {
            if (parent.CanCreateChild(type))
                menu.AddItem(new GUIContent(path), false, () => {
                    IMNodeOps.CreateChildNode(asset, parent, type, out var child);
                    _state.Selected = child;
                });
            else
                menu.AddDisabledItem(new GUIContent(path));
        }
    }
}