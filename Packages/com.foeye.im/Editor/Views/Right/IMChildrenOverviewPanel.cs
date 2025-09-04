// Assets/UInteractiveMusic/Editor/Views/Right/IMChildrenOverviewPanel.cs
using UnityEditor;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Editor.Core;
using UInteractiveMusic.Editor.Views;
using UInteractiveMusic.Runtime.Node;
using UnityEngine;

namespace UInteractiveMusic.Editor.Views
{
    public static class IMChildrenOverviewPanel
    {
        public static void Draw(IMEditorState state, MusicSwitchContainerNode sc)
        {
            // 1) 先获取一个固定高度的 rect
            EditorGUILayout.Space(EditorStyles.toolbar.fixedHeight);
            var tabsRect = GUILayoutUtility.GetRect(1, 30, GUILayout.ExpandWidth(true));
            int selected = (int)state.RightTab;
            // 2) 在该 rect 内绘制 tabs（不会参与额外布局，避免被裁剪/覆盖）
            selected = IMTabs.DrawUnderlineTabs(tabsRect, new[] { "CONTENTS", "MUSIC SWITCH" }, selected);
            state.RightTab = (IMRightTab)selected;
            // 4) 切换面板
            switch (state.RightTab)
            {
                case IMRightTab.Contents:
                    IMSwitchContainerTable.Draw(state, sc);
                    break;
                case IMRightTab.MusicSwitch:
                    IMMusicSwitchPanel.Draw(state, sc); // 你的 Music Switch 面板
                    break;
            }
        }
    }
}