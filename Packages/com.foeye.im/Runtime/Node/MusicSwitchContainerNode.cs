using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UInteractiveMusic.Runtime.Switching;

namespace UInteractiveMusic.Runtime.Node {
    public enum SwitchResolveMode {
        BestMatch,
        Weighted // 与 Wwise「最好匹配 / 加权」等价
    }

    [Serializable]
    public class GroupSlot {
        public string groupGuid; // 对应 IMSwitchGroupAsset.Guid
        public IMSwitchGroupKind kind; // Switch or State
        public string groupName; // 缓存名（编辑器冗余，便于显示）
    }

    [Serializable]
    public class PathEntry {
        // 路径：groupGuid -> valueId；通配“*”代表 Any
        public List<string> groupGuids = new List<string>();
        public List<string> valueIds = new List<string>(); // 与 groupGuids 一一对应

        public float weight = 50f; // Weighted 模式下才生效
        public IMNode targetObject; // 目标子对象（Segment/Playlist/容器）
        public bool selected; // 编辑器用：多选

        public void SetValue(string groupGuid, string valueId) {
            int idx = groupGuids.IndexOf(groupGuid);
            if (idx < 0) {
                groupGuids.Add(groupGuid);
                valueIds.Add(valueId);
            }
            else valueIds[idx] = valueId;
        }

        public string GetValue(string groupGuid, string fallback = "*") {
            int idx = groupGuids.IndexOf(groupGuid);
            return idx >= 0 ? valueIds[idx] : fallback;
        }

        public PathEntry Clone() {
            return new PathEntry {
                groupGuids = new List<string>(groupGuids),
                valueIds = new List<string>(valueIds),
                weight = weight,
                targetObject = targetObject
            };
        }
    }

    public partial class MusicSwitchContainerNode : IMNode {
        public override MusicNodeType NodeType { get; } = MusicNodeType.MusicSwitchContainer;
        [SerializeField] private SwitchResolveMode resolveMode = SwitchResolveMode.BestMatch;
        [SerializeField] private List<GroupSlot> groupSlots = new List<GroupSlot>();
        [SerializeField] private List<PathEntry> paths = new List<PathEntry>();

        public SwitchResolveMode ResolveMode {
            get => resolveMode;
            set => resolveMode = value;
        }

        public List<GroupSlot> GroupSlots => groupSlots;
        public List<PathEntry> Paths => paths;

        // 组槽位管理
        public void AddGroup(IMSwitchGroupAsset g) {
            if (g == null) return;
            if (groupSlots.Any(s => s.groupGuid == g.Guid)) return;
            groupSlots.Add(new GroupSlot { groupGuid = g.Guid, kind = g.Kind, groupName = g.DisplayName });
            // 新增组时，将所有已存在 path 补上 “*”
            foreach (var p in paths)
                p.SetValue(g.Guid, "*");
        }

        public void RemoveGroup(string groupGuid) {
            if (string.IsNullOrEmpty(groupGuid)) return;
            groupSlots.RemoveAll(s => s.groupGuid == groupGuid);
            foreach (var p in paths) {
                int i = p.groupGuids.IndexOf(groupGuid);
                if (i >= 0) {
                    p.groupGuids.RemoveAt(i);
                    p.valueIds.RemoveAt(i);
                }
            }
        }

        // 生成一个全通配的「Generic Path」
        public PathEntry CreateGenericPath(float defaultWeight = 50f) {
            var pe = new PathEntry { weight = defaultWeight };
            foreach (var s in groupSlots)
                pe.SetValue(s.groupGuid, "*");
            paths.Add(pe);
            return pe;
        }

        // 按当前组的值笛卡尔积批量生成 Path
        public void CreateAllCombinationPaths(IReadOnlyDictionary<string, List<string>> groupValues,
            float defaultWeight = 50f) {
            if (groupSlots.Count == 0) return;

            var keys = groupSlots.Select(g => g.groupGuid).ToList();
            var valueLists = keys.Select(k => groupValues.TryGetValue(k, out var v) ? v : new List<string> { "*" })
                .ToList();

            void Rec(int depth, PathEntry cur) {
                if (depth == keys.Count) {
                    paths.Add(cur.Clone());
                    return;
                }

                foreach (var val in valueLists[depth]) {
                    cur.SetValue(keys[depth], val);
                    Rec(depth + 1, cur);
                }
            }

            Rec(0, new PathEntry { weight = defaultWeight });
        }

        // 规范化路径（组变化后修复缺失键，移除无用键）
        public void NormalizePaths() {
            var valid = new HashSet<string>(groupSlots.Select(s => s.groupGuid));
            foreach (var p in paths) {
                // 移除无效
                for (int i = p.groupGuids.Count - 1; i >= 0; --i)
                    if (!valid.Contains(p.groupGuids[i])) {
                        p.groupGuids.RemoveAt(i);
                        p.valueIds.RemoveAt(i);
                    }

                // 补缺
                foreach (var g in groupSlots)
                    if (!p.groupGuids.Contains(g.groupGuid))
                        p.SetValue(g.groupGuid, "*");
            }
        }
    }
}