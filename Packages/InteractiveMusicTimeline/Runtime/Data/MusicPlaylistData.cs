/**
  * @file :MusicPlaylistData
  * @brief :BRIEF
  * @details : Detail of this file
  * @author :六队
  * @version :0.1
  * @date :2025年8月26日
  * @copyright :Foeye
 */

using System;
using System.Collections.Generic;

namespace IMT.Runtime.Data {
    [Serializable]
    public class MusicPlaylistData {
        public MusicPlaylistNode rootNode;

        public MusicPlaylistData() {
            // 创建默认的根节点结构，匹配图中的样式
            rootNode = new MusicPlaylistNode {
                nodeType = NodeType.SequenceStep,
                nodeName = "Sequence Step",
                loopCount = 1,
                children = new List<MusicPlaylistNode> {
                    new MusicPlaylistNode {
                        nodeType = NodeType.RandomContinuous,
                        nodeName = "Random Continuous",
                        randomType = RandomType.Standard,
                        avoidRepeat = true,
                        loopCount = 1,
                        children = new List<MusicPlaylistNode> {
                            new MusicPlaylistNode {
                                nodeType = NodeType.SequenceContinuous,
                                nodeName = "Sequence Continuous",
                                weight = 50,
                                loopCount = 1,
                                children = new List<MusicPlaylistNode>()
                            },
                            new MusicPlaylistNode {
                                nodeType = NodeType.SequenceContinuous,
                                nodeName = "Sequence Continuous",
                                weight = 50,
                                loopCount = 1,
                                children = new List<MusicPlaylistNode>()
                            }
                        }
                    },
                    new MusicPlaylistNode {
                        nodeType = NodeType.SequenceContinuous,
                        nodeName = "Sequence Continuous",
                        loopCount = 1,
                        children = new List<MusicPlaylistNode>()
                    }
                }
            };
        }
    }
}