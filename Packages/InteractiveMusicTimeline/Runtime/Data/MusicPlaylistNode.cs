/**
  * @file :MusicPlaylistNode
  * @brief :BRIEF
  * @details : Detail of this file
  * @author :六队
  * @version :0.1
  * @date :2025年8月26日
  * @copyright :Foeye
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IMT.Runtime.Data {
    [Serializable]
    public class MusicPlaylistNode
    {
        public NodeType nodeType = NodeType.SequenceStep;
        public string nodeName = "New Node";
        
        [Header("Common Parameters")]
        public int loopCount = 1;
        
        [Header("Random Parameters")]
        public RandomType randomType = RandomType.Standard;
        public bool avoidRepeat = false;
        public float weight = 50f;
        
        [Header("Segment Parameters")]
        public AudioClip audioClip;
        
        [Header("Hierarchy")]
        public List<MusicPlaylistNode> children = new List<MusicPlaylistNode>();
        
        [HideInInspector]
        public MusicPlaylistNode parent;
        
        [HideInInspector]
        public bool isExpanded = true;
        
        [HideInInspector]
        public MusicPlaylistNode lastSelectedChild;
        
        // Editor only
        [HideInInspector]
        public Vector2 nodePosition;
        
        [HideInInspector]
        public bool isSelected;
        
        [HideInInspector]
        public Rect lastRect;
    }

}