/**
  * @file :MusicPlaylistTrackEditor
  * @brief :BRIEF
  * @details : Detail of this file
  * @author :六队
  * @version :0.1
  * @date :2025年9月1日
  * @copyright :Foeye
 */


using IMT.Runtime.Clip;
using UnityEngine;
using UnityEngine.Timeline;

namespace IMT.Runtime.Track {
    [TrackClipType(typeof(MusicPlaylistClip))]
    [TrackBindingType(typeof(AudioSource))]
    [TrackColor(0.2f, 0.5f, 0.8f)]
    public partial class MusicPlaylistTrack {
        
    }
}