/**
  * @file :MusicPlaylistClip
  * @brief :BRIEF
  * @details : Detail of this file
  * @author :六队
  * @version :0.1
  * @date :2025年8月26日
  * @copyright :Foeye
 */

using System;
using IMT.Runtime.Behaviour;
using IMT.Runtime.Data;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace IMT.Runtime.Clip {
    [Serializable]
    public class MusicPlaylistClip : PlayableAsset, ITimelineClipAsset {
        [HideInInspector] public MusicPlaylistData playlistData = new MusicPlaylistData();
        public ClipCaps clipCaps => ClipCaps.Looping | ClipCaps.Extrapolation;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            var playable = ScriptPlayable<MusicPlaylistBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.playlistData = playlistData;

            return playable;
        }
    }
}