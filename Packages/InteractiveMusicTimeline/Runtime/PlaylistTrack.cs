
// ================== MusicPlaylistSystem.cs ==================
// 主系统文件 - Timeline Track, Clip, Behaviour 和数据模型

using System;
using System.Collections.Generic;
using System.Linq;
using IMT.Runtime.Clip;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

    
    public enum RandomType
    {
        Standard,
        Shuffle,
        ContinuousShuffle
    }
    
    public enum NodeType
    {
        SequenceStep,
        RandomContinuous,
        SequenceContinuous,
        MusicSegment
    }
    
