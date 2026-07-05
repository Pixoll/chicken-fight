using UnityEngine;

namespace MultiPlayerSection.HUDScripts {
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraFitToBackground : MonoBehaviour {
        [SerializeField] private Renderer background;

        private Camera _camera;

        private void Awake() {
            _camera = GetComponent<Camera>();
            Fit();
        }

        private void Fit() {
            if (background == null || _camera == null) return;

            Bounds bounds = background.bounds;
            float bgWidth = bounds.size.x;
            float bgTop = bounds.max.y;
            float bgCenterX = bounds.center.x;

            float aspect = (float)Screen.width / Screen.height;

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
