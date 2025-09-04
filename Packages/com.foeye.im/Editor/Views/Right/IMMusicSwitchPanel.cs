using UnityEditor;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Editor.Core;
using UInteractiveMusic.Runtime.Node;

namespace UInteractiveMusic.Editor.Views {
    public static class IMMusicSwitchPanel {
        public static void Draw(IMEditorState state, MusicSwitchContainerNode sc) {
            // TODO: 在这里实现“Music Switch”右侧面板的具体内容
            EditorGUILayout.HelpBox("MUSIC SWITCH 面板内容待实现。请在此绘制你的开关、规则或路由设置。", MessageType.Info);
        }
    }
}