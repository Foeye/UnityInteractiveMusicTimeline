/**
  * @file :MusicPlaylistTrack
  * @brief :BRIEF
  * @details : Detail of this file
  * @author :六队
  * @version :0.1
  * @date :2025年8月26日
  * @copyright :Foeye
 */

using IMT.Runtime.Behaviour;
using IMT.Runtime.Clip;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace IMT.Runtime.Track {
    [TrackClipType(typeof(MusicPlaylistClip))]
    [TrackBindingType(typeof(AudioSource))]
    [TrackColor(0.2f, 0.5f, 0.8f)]
    public class MusicPlaylistTrack : TrackAsset {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
            return ScriptPlayable<MusicPlaylistMixerBehaviour>.Create(graph, inputCount);
        }
    }
}