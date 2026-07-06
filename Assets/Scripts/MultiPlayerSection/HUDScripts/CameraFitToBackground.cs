using UnityEngine;

namespace MultiPlayerSection.HUDScripts {
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraFitToBackground : MonoBehaviour {
        [SerializeField] private Renderer background;

        private Camera _camera;
        private bool _shouldRun;
        private float _previousAspectRatio = -1;

        private void Awake() {
            _camera = GetComponent<Camera>();
            _shouldRun = _camera != null && background != null;
            Fit();
        }

        private void Update() {
            Fit();
        }

        private void Fit() {
            if (!_shouldRun) return;

            float aspect = (float)Screen.width / Screen.height;

            if (Mathf.Approximately(aspect, _previousAspectRatio)) return;

            _previousAspectRatio = aspect;

            Bounds bounds = background.bounds;
            float bgWidth = bounds.size.x;
            float bgTop = bounds.max.y;
            float bgCenterX = bounds.center.x;

            // Camera width must equal background width -> orthoSize = width / (2 * aspect)
            float orthoSize = bgWidth / (2f * aspect);
            _camera.orthographicSize = orthoSize;

            float camHeight = orthoSize * 2f;

            // Anchor: camera's horizontal center = bg center, camera's top edge = bg top
            Vector3 pos = _camera.transform.position;
            pos.x = bgCenterX;
            pos.y = bgTop - camHeight / 2f;
            _camera.transform.position = pos;
        }
    }
}
