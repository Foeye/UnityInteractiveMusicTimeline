using UnityEditor;
using UnityEngine;
using IMT.Runtime.Track;
using UnityEditor.Timeline.Actions;

namespace IMT.Editor.Track {
    [MenuEntry("InteractiveMusic/New Child/MusicSegmentTrack")]
    class MusicPlaylistTrackMenu : TimelineAction {
        public override bool Execute(ActionContext context) {
            return false;
        }

        public override ActionValidity Validate(ActionContext context) {
            return ActionValidity.Valid;
        }
    }
}

