using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
#endif

namespace MultiPlayerSection.HUDScripts {
    public static class AudioPlayer {
#if UNITY_EDITOR
        private static bool PlayMusic => CurrentPlayer.Tags.Count > 0 && CurrentPlayer.Tags[0] == "P1";
#endif

        public static IEnumerator Play(AudioSource source, float targetVolume, float transitionTime) {
#if UNITY_EDITOR
            if (!PlayMusic) yield break;
#endif

            float percentage = 0;

            source.volume = 0;
            source.Play();

            while (source.volume < targetVolume) {
                source.volume = Mathf.Lerp(0, targetVolume, percentage);
                percentage += Time.deltaTime / transitionTime;
                yield return null;
            }
        }

        public static IEnumerator PlayClip(
            AudioSource source,
            AudioClip clip,
            float targetVolume,
            float transitionTime
        ) {
#if UNITY_EDITOR
            if (!PlayMusic) yield break;
#endif

            float percentage = 0;

            source.volume = 0;
            source.PlayOneShot(clip, 1);

            while (source.volume < targetVolume) {
                source.volume = Mathf.Lerp(0, targetVolume, percentage);
                percentage += Time.deltaTime / transitionTime;
                yield return null;
            }
        }

        public static IEnumerator Stop(AudioSource source, float transitionTime) {
#if UNITY_EDITOR
            if (!PlayMusic) yield break;
#endif

            float originalVolume = source.volume;
            float percentage = 0;

            while (source.volume > 0) {
                source.volume = Mathf.Lerp(originalVolume, 0, percentage);
                percentage += Time.deltaTime / transitionTime;
                yield return null;
            }

            source.Stop();
        }
    }
}
