using UnityEditor;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Editor.Core;
using UInteractiveMusic.Runtime.Node;

namespace UInteractiveMusic.Editor.Views
{
    public static class IMChildrenOverviewPanel
    {
        public static void Draw(IMEditorState state, MusicSwitchContainerNode sc)
        {
            // 用无箭头可点击 Label 的形式展示“CONTENT”
            state.ContentExpanded = IMClickableHeader.DrawLabel(
                "CONTENT",
                state.ContentExpanded,
                24f,
                EditorStyles.boldLabel,
                rightCorner: () =>
                {
                    // 可放置右侧按钮/帮助/搜索等（可选）
                    // 例如：GUILayout.Button("Help", EditorStyles.miniButton, GUILayout.Width(50));
                }
            );

            if (!state.ContentExpanded)
                return;

            // 保持原表格功能与列宽拖拽
            IMSwitchContainerTable.Draw(state, sc);
        }
    }
}