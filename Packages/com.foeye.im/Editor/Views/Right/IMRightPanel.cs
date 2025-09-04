using UnityEditor;
using UnityEngine;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Editor.Core;
using UInteractiveMusic.Runtime.Node;

namespace UInteractiveMusic.Editor.Views {
    public class IMRightPanel {
        private readonly IMEditorState _state;

        public IMRightPanel(IMEditorState state) {
            _state = state;
        }

        public void Draw() {
            // SwitchContainer 特殊表格
            if (_state.Selected is MusicSwitchContainerNode sc) {
                IMSwitchContainerTable.Draw(_state, sc);
                return;
            }

            // 默认信息
            GUILayout.Label("Editor / Inspector Area", IMEditorStyles.RightTitle);

            if (_state.Selected == null) {
                EditorGUILayout.HelpBox("选择左侧层级中的一个节点，在这里显示其详细属性与编辑控件。", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope(IMEditorStyles.RightBox)) {
                GUILayout.Label("选中节点", IMEditorStyles.RightSectionTitle);
                EditorGUILayout.LabelField("名称", _state.Selected.DisplayName);
                EditorGUILayout.LabelField("类型", _state.Selected.NodeType.ToString());
                EditorGUILayout.LabelField("子节点数", _state.Selected.Children?.Count.ToString() ?? "0");

                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("重命名", EditorStyles.boldLabel);
                string newName = EditorGUILayout.TextField(_state.Selected.DisplayName);
                if (newName != _state.Selected.DisplayName) {
                    if (GUILayout.Button("应用重命名", GUILayout.Width(100))) {
                        IMNodeOps.CommitRename(_state.Selected, newName);
                    }
                }
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("右侧区域预留给更完善的编辑器：参数、过渡、时间线、预听等。", MessageType.None);
        }
    }
}