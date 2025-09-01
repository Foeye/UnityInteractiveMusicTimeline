/**
  * @file :MusicPlaylistBehaviour
  * @brief :BRIEF
  * @details : Detail of this file
  * @author :六队
  * @version :0.1
  * @date :2025年8月26日
  * @copyright :Foeye
 */

using System.Collections.Generic;
using System.Linq;
using IMT.Runtime.Data;
using UnityEngine;
using UnityEngine.Playables;

namespace IMT.Runtime.Behaviour {
    public class MusicPlaylistBehaviour : PlayableBehaviour {
        public MusicPlaylistData playlistData;
        private AudioSource audioSource;
        private Queue<AudioClip> playQueue;
        private float nextPlayTime;

        public override void OnPlayableCreate(Playable playable) {
            if (playlistData != null && playlistData.rootNode != null) {
                BuildPlayQueue();
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            audioSource = playerData as AudioSource;

            if (audioSource == null || playQueue == null || playQueue.Count == 0)
                return;

            if (Time.time >= nextPlayTime && !audioSource.isPlaying) {
                PlayNext();
            }
        }

        private void BuildPlayQueue() {
            playQueue = new Queue<AudioClip>();
            var clips = new List<AudioClip>();

            if (playlistData.rootNode != null) {
                CollectClips(playlistData.rootNode, clips);
            }

            foreach (var clip in clips) {
                if (clip != null)
                    playQueue.Enqueue(clip);
            }
        }

        private void CollectClips(MusicPlaylistNode node, List<AudioClip> clips) {
            if (node == null) return;

            for (int loop = 0; loop < node.loopCount; loop++) {
                switch (node.nodeType) {
                    case NodeType.SequenceStep:
                        foreach (var child in node.children) {
                            CollectClips(child, clips);
                        }

                        break;

                    case NodeType.RandomContinuous:
                        if (node.children.Count > 0) {
                            var selected = SelectRandomNode(node);
                            if (selected != null)
                                CollectClips(selected, clips);
                        }

                        break;

                    case NodeType.SequenceContinuous:
                        foreach (var child in node.children) {
                            CollectClips(child, clips);
                        }

                        break;

                    case NodeType.MusicSegment:
                        if (node.audioClip != null)
                            clips.Add(node.audioClip);
                        break;
                }
            }
        }

        private MusicPlaylistNode SelectRandomNode(MusicPlaylistNode randomNode) {
            if (randomNode.children.Count == 0) return null;

            float totalWeight = randomNode.children.Sum(c => c.weight);
            float random = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0;

            foreach (var child in randomNode.children) {
                currentWeight += child.weight;
                if (random <= currentWeight) {
                    if (randomNode.avoidRepeat && child == randomNode.lastSelectedChild &&
                        randomNode.children.Count > 1) {
                        continue;
                    }

                    randomNode.lastSelectedChild = child;
                    return child;
                }
            }

            return randomNode.children[randomNode.children.Count - 1];
        }

        private void PlayNext() {
            if (playQueue.Count > 0) {
                var clip = playQueue.Dequeue();
                audioSource.clip = clip;
                audioSource.Play();
                nextPlayTime = Time.time + clip.length;
            }
        }
    }
}