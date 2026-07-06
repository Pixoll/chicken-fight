using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Multiplayer.PlayMode;
#endif

namespace MainMenuSection.MenuScripts {
    public class MainMenuBackgroundMusic : MonoBehaviour {
        [SerializeField] private AudioSource track1;
        [SerializeField] private AudioSource track2;
        [SerializeField] [Range(0f, 3f)] private float transitionTime;

        private float _track1OriginalVolume;
        private float _track2OriginalVolume;
#if UNITY_EDITOR
        private bool _playMusic;
#endif

        private void Awake() {
            _track1OriginalVolume = track1.volume;
            _track2OriginalVolume = track2.volume;
            track2.volume = 0f;

#if UNITY_EDITOR
            IReadOnlyList<string> tags = CurrentPlayer.Tags;
            _playMusic = tags.Count > 0 && tags[0] == "P1";

            if (_playMusic) {
                track1.Play();
            }
#else
            track1.Play();
#endif

            Debug.Log($"<color=teal>[MenuBackgroundMusic] Track 1 = {track1.resource.name}</color>");
            Debug.Log($"<color=teal>[MenuBackgroundMusic] Track 2 = {track2.resource.name}</color>");
        }

        public void PlayTrack1() {
#if UNITY_EDITOR
            if (!_playMusic) return;
#endif

            if (track1.isPlaying) {
                Debug.LogWarning("[MenuBackgroundMusic] Track 1 already playing");
                return;
            }

            Debug.Log("<color=teal>[MenuBackgroundMusic] Mixing Track 1 -> Track 2</color>");
            StartCoroutine(MixTracks(track2, track1, _track2OriginalVolume, _track1OriginalVolume));
        }

        public void PlayTrack2() {
#if UNITY_EDITOR
            if (!_playMusic) return;
#endif

            if (track2.isPlaying) {
                Debug.LogWarning("[MenuBackgroundMusic] Track 2 already playing");
                return;
            }

            Debug.Log("<color=teal>[MenuBackgroundMusic] Mixing Track 2 -> Track 1</color>");
            StartCoroutine(MixTracks(track1, track2, _track1OriginalVolume, _track2OriginalVolume));
        }

        private IEnumerator MixTracks(
            AudioSource current,
            AudioSource target,
            float currentVolume,
            float targetVolume
        ) {
            float percentage = 0;

            Debug.Log($"<color=teal>[MenuBackgroundMusic] Play track {target.resource.name}</color>");
            target.Play();

            while (current.volume > 0 && target.volume < targetVolume) {
                current.volume = Mathf.Lerp(currentVolume, 0, percentage);
                target.volume = Mathf.Lerp(0, targetVolume, percentage);
                percentage += Time.deltaTime / transitionTime;
                yield return null;
            }

            Debug.Log($"<color=teal>[MenuBackgroundMusic] Stop track {current.resource.name}</color>");
            current.Stop();
        }
    }
}
