using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UInteractiveMusic.Runtime;
using UInteractiveMusic.Runtime.Node;

namespace UInteractiveMusic.Editor.Core {
    public static class IMNodeOps {
        public const string DefaultAssetPath = "Assets/UInteractiveMusic/InteractiveMusicHierarchy.asset";

        // 资产创建与根节点保证
        public static InteractiveMusicHierarchyAsset CreateNewHierarchyAsset(string assetPath) {
            var asset = ScriptableObject.CreateInstance<InteractiveMusicHierarchyAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);

            var root = ScriptableObject.CreateInstance<MusicRootNode>();
            root.name = "Interactive Music Hierarchy";
            root.DisplayName = "Interactive Music Hierarchy";
            root.EnsureGuid();

            AssetDatabase.AddObjectToAsset(root, asset);
            asset.SetRoot(root);

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        public static void EnsureRootNode(InteractiveMusicHierarchyAsset asset) {
            if (asset == null || asset.Root != null) return;

            var root = ScriptableObject.CreateInstance<MusicRootNode>();
            root.name = "Interactive Music Hierarchy";
            root.DisplayName = "Interactive Music Hierarchy";
            root.EnsureGuid();
            AssetDatabase.AddObjectToAsset(root, asset);
            asset.SetRoot(root);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        // 节点 CRUD
        public static void CreateChildNode(InteractiveMusicHierarchyAsset asset, IMNode parent, MusicNodeType type,
            out IMNode child) {
            child = null;
            if (asset == null || parent == null) return;
            if (!parent.CanCreateChild(type)) return;

            Undo.RecordObject(asset, "Create Music Node");
            Undo.RecordObject(parent, "Create Music Node");

            child = InstantiateNodeByType(type);
            child.EnsureGuid();
            child.Parent = parent;
            child.name = GetDefaultName(type);
            child.DisplayName = child.name;

            AssetDatabase.AddObjectToAsset(child, asset);
            parent.Children.Add(child);

            EditorUtility.SetDirty(child);
            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        public static void DeleteNode(InteractiveMusicHierarchyAsset asset, IMNode node) {
            if (node == null || asset == null) return;
            if (node.NodeType == MusicNodeType.Root) return;

            var parent = node.Parent;
            if (parent != null) {
                Undo.RecordObject(parent, "Delete Music Node");
                parent.Children.Remove(node);
                EditorUtility.SetDirty(parent);
            }

            var toDelete = CollectNodesDepthFirst(node);
            foreach (var n in toDelete) {
                Undo.DestroyObjectImmediate(n);
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        public static void CommitRename(IMNode node, string newName) {
            if (node == null) return;
            Undo.RecordObject(node, "Rename Music Node");
            node.DisplayName = string.IsNullOrEmpty(newName) ? node.DisplayName : newName.Trim();
            node.name = node.DisplayName;
            EditorUtility.SetDirty(node);
            AssetDatabase.SaveAssets();
        }

        public static List<IMNode> CollectNodesDepthFirst(IMNode node) {
            var list = new List<IMNode>();
            if (node == null) return list;
            if (node.Children != null) {
                foreach (var c in node.Children)
                    list.AddRange(CollectNodesDepthFirst(c));
            }

            list.Add(node);
            return list;
        }

        // 工具
        public static IMNode InstantiateNodeByType(MusicNodeType type) {
            switch (type) {
                case MusicNodeType.MusicSwitchContainer:
                    return ScriptableObject.CreateInstance<MusicSwitchContainerNode>();
                case MusicNodeType.MusicPlaylistContainer:
                    return ScriptableObject.CreateInstance<MusicPlaylistContainerNode>();
                case MusicNodeType.MusicSegment: return ScriptableObject.CreateInstance<MusicSegmentNode>();
                default: return ScriptableObject.CreateInstance<MusicSegmentNode>();
            }
        }

        public static string GetDefaultName(MusicNodeType type) {
            switch (type) {
                case MusicNodeType.MusicSwitchContainer: return "Music Switch Container";
                case MusicNodeType.MusicPlaylistContainer: return "Music Playlist Container";
                case MusicNodeType.MusicSegment: return "Music Segment";
                default: return "Node";
            }
        }

        public static Texture GetIcon(MusicNodeType type) {
            switch (type) {
                case MusicNodeType.MusicSwitchContainer:
                    return EditorGUIUtility.IconContent("d_Folder Icon").image;
                case MusicNodeType.MusicPlaylistContainer:
                    return EditorGUIUtility.IconContent("d_AudioMixerView Icon").image;
                case MusicNodeType.MusicSegment:
                    return EditorGUIUtility.IconContent("AudioClip Icon").image;
                case MusicNodeType.Root:
                default:
                    return EditorGUIUtility.IconContent("Project").image;
            }
        }
    }
}