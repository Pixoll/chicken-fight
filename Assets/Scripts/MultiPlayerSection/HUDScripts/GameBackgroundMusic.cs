using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Multiplayer.PlayMode;
#endif

namespace MultiPlayerSection.HUDScripts {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GameBackgroundMusic : MonoBehaviour {
        [SerializeField] private AudioSource trackFirst60s;
        [SerializeField] private AudioSource trackLast30s;
        [SerializeField] [Range(0f, 3f)] private float transitionTime;

        private float _trackFirst60sOriginalVolume;
        private float _trackLast30sOriginalVolume;
#if UNITY_EDITOR
        private bool _playMusic;
#endif

        private void Awake() {
            _trackFirst60sOriginalVolume = trackFirst60s.volume;
            _trackLast30sOriginalVolume = trackLast30s.volume;
            trackFirst60s.volume = 0f;
            trackLast30s.volume = 0f;

#if UNITY_EDITOR
            IReadOnlyList<string> tags = CurrentPlayer.Tags;
            _playMusic = tags.Count > 0 && tags[0] == "P1";
#endif
        }

        public void PlayTrackFirst60s() {
#if UNITY_EDITOR
            if (!_playMusic) return;
#endif

            if (trackFirst60s.isPlaying) return;

            StartCoroutine(PlayTrack(trackFirst60s, _trackFirst60sOriginalVolume));
        }

        public void PlayTrackLast30s() {
#if UNITY_EDITOR
            if (!_playMusic) return;
#endif

            if (trackLast30s.isPlaying) return;

            StartCoroutine(PlayTrack(trackLast30s, _trackLast30sOriginalVolume));
        }

        public void StopPlaying() {
            if (trackFirst60s.isPlaying) {
                StartCoroutine(StopTrack(trackFirst60s, _trackFirst60sOriginalVolume));
            }

            if (trackLast30s.isPlaying) {
                StartCoroutine(StopTrack(trackLast30s, _trackLast30sOriginalVolume));
            }
        }

        private IEnumerator PlayTrack(AudioSource track, float targetVolume) {
            float percentage = 0;

            track.Play();

            while (track.volume < targetVolume) {
                track.volume = Mathf.Lerp(0, targetVolume, percentage);
                percentage += Time.deltaTime / transitionTime;
                yield return null;
            }
        }

        private IEnumerator StopTrack(AudioSource track, float originalVolume) {
            float percentage = 0;

            while (track.volume > 0) {
                track.volume = Mathf.Lerp(originalVolume, 0, percentage);
                percentage += Time.deltaTime / transitionTime;
                yield return null;
            }

            track.Stop();
        }
    }
}
