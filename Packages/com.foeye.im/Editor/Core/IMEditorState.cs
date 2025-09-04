using UnityEngine;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Runtime.Node;
using UnityEditor;

namespace UInteractiveMusic.Editor.Core {
    public class IMEditorState {
        // 面板与分隔条
        public float LeftWidth = 320f;
        public const float MinLeftWidth = 220f;
        public const float MaxLeftWidth = 600f;
        public const float SplitterWidth = 4f;
        public bool DraggingSplitter = false;

        // 滚动
        public Vector2 LeftScroll;
        public Vector2 RightScroll;

        // 选择与重命名
        public IMNode Selected;
        public IMNode RenameTarget;
        public string RenameBuffer;

        // SwitchContainer 表格状态
        public string SwitchSearch = "";

        // 列宽（可拖拽）
        public float ColVolWidth = 120f; // 初始值
        public float ColLPFWidth = 150f; // 初始值
        public float ColNotesWidth = 200f; // 初始值
        public float ColNameMin = 100f; // Name 最小宽度

        // 列拖拽状态：-1 不拖拽；0 Name|Vol；1 Vol|LPF；2 LPF|Notes
        public int ColDragging = -1;
        public float DragStartMouseX;
        public float DragStartNameWidth;
        public float DragStartVolWidth;
        public float DragStartLPFWidth;
        public float DragStartNotesWidth;

        // EditorPrefs
        private const string PrefLeftWidthKey = "UInteractiveMusic.Hierarchy.LeftWidth";
        private const string PrefColVolKey = "UInteractiveMusic.Hierarchy.ColVolWidth";
        private const string PrefColLPFKey = "UInteractiveMusic.Hierarchy.ColLPFWidth";
        private const string PrefColNotesKey = "UInteractiveMusic.Hierarchy.ColNotesWidth";
        private const string PrefColNameMinKey = "UInteractiveMusic.Hierarchy.ColNameMin";

        
        // CONTENT 标题展开/折叠  
        public bool ContentExpanded = true;  
        // EditorPrefs  
        private const string PrefContentExpanded = "UInteractiveMusic.ContentExpanded";  
        public void LoadPrefs() {
            LeftWidth = EditorPrefs.GetFloat(PrefLeftWidthKey, LeftWidth);
            ColVolWidth = EditorPrefs.GetFloat(PrefColVolKey, ColVolWidth);
            ColLPFWidth = EditorPrefs.GetFloat(PrefColLPFKey, ColLPFWidth);
            ColNotesWidth = EditorPrefs.GetFloat(PrefColNotesKey, ColNotesWidth);
            ColNameMin = EditorPrefs.GetFloat(PrefColNameMinKey, ColNameMin);
            ContentExpanded = EditorPrefs.GetBool(PrefContentExpanded, true);  
        }

        public void SavePrefs() {
            EditorPrefs.SetFloat(PrefLeftWidthKey, LeftWidth);
            EditorPrefs.SetFloat(PrefColVolKey, ColVolWidth);
            EditorPrefs.SetFloat(PrefColLPFKey, ColLPFWidth);
            EditorPrefs.SetFloat(PrefColNotesKey, ColNotesWidth);
            EditorPrefs.SetFloat(PrefColNameMinKey, ColNameMin);
            EditorPrefs.SetBool(PrefContentExpanded, ContentExpanded);  
        }
    }
}