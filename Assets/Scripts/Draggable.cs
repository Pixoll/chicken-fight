using UnityEngine;
using UnityEngine.InputSystem.Controls;
using InputSystem = UnityEngine.InputSystem;

public class Draggable : MonoBehaviour {
    private bool _dragging;
    private Vector3 _offset;
    private Camera _camera;
    private Collider2D _collider;

    private void Start() {
        _camera = Camera.main;
        _collider = GetComponent<Collider2D>();
    }

    private void Update() {
        // Exit early if there's no touchscreen or no touches currently active
        // (Note: in the new system, Ended touches briefly stay in the touches array so we can process the release)
        if (InputSystem.Touchscreen.current == null || InputSystem.Touchscreen.current.touches.Count <= 0) {
            _dragging = false;
            return;
        }

        // Get the first touch
        TouchControl touch = InputSystem.Touchscreen.current.touches[0];

        // Convert screen coordinates to world space
        Vector3 position = _camera.ScreenToWorldPoint(touch.position.value);
        position.z = 0;

        // Process the touch phase
        switch (touch.phase.value) {
            case InputSystem.TouchPhase.Began:
                OnDragStart(position);
                break;

            case InputSystem.TouchPhase.Moved:
                OnDrag(position);
                break;

            case InputSystem.TouchPhase.Ended:
            case InputSystem.TouchPhase.Canceled:
                OnDragStop();
                break;
        }
    }

    // private void Update() {
    //     if (Input.touchCount <= 0) {
    //         _dragging = false;
    //         return;
    //     }
    //
    //     Touch touch = Input.GetTouch(0);
    //     Vector3 position = _camera.ScreenToWorldPoint(touch.position);
    //     position.z = 0;
    //
    //     switch (touch.phase) {
    //         case TouchPhase.Began:
    //             OnDragStart(position);
    //             break;
    //
    //         case TouchPhase.Moved:
    //             OnDrag(position);
    //             break;
    //
    //         case TouchPhase.Ended:
    //         case TouchPhase.Canceled:
    //             OnDragStop();
    //             break;
    //     }
    // }

    private void OnDragStart(Vector3 position) {
        if (!_collider.OverlapPoint(position)) {
            return;
        }

        _dragging = true;
        _offset = transform.position - position;
    }

    private void OnDrag(Vector3 position) {
        if (_dragging) {
            transform.position = position + _offset;
        }
    }

    private void OnDragStop() {
        _dragging = false;
    }
}
