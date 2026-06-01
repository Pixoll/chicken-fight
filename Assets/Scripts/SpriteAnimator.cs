using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour {
    public enum AnimationState {
        Idle,
        Run
    }

    [Header("Animaciones (Sprites)")] [SerializeField]
    private Sprite[] idleFrames;

    [SerializeField] private Sprite[] runFrames;

    [Header("Configuración de Velocidad")] [SerializeField]
    private float animationFps = 30f;

    private SpriteRenderer _spriteRenderer;
    private AnimationState _currentState;
    private Sprite[] _currentAnimationFrames;

    private int _currentFrameIndex;
    private float _frameTimer;
    private float _timePerFrame;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        CalculateFrameRate();

        ChangeState(AnimationState.Idle);
    }

    private void Update() {
        if (_currentAnimationFrames == null || _currentAnimationFrames.Length == 0) return;

        if (_currentAnimationFrames.Length == 1) {
            _spriteRenderer.sprite = _currentAnimationFrames[0];
            return;
        }

        _frameTimer += Time.deltaTime;

        if (_frameTimer >= _timePerFrame) {
            _frameTimer -= _timePerFrame;
            _currentFrameIndex = (_currentFrameIndex + 1) % _currentAnimationFrames.Length;
            _spriteRenderer.sprite = _currentAnimationFrames[_currentFrameIndex];
        }
    }

    public void ChangeState(AnimationState newState) {
        if (_currentState == newState && _currentAnimationFrames != null) return;

        _currentState = newState;
        _currentFrameIndex = 0;
        _frameTimer = 0f;

        switch (_currentState) {
            case AnimationState.Idle:
                _currentAnimationFrames = idleFrames;
                break;

            case AnimationState.Run:
                _currentAnimationFrames = runFrames;
                break;
        }

        if (_currentAnimationFrames != null && _currentAnimationFrames.Length > 0) {
            _spriteRenderer.sprite = _currentAnimationFrames[0];
        }
    }

    public void CalculateFrameRate() {
        if (animationFps <= 0f) animationFps = 1f;
        _timePerFrame = 1f / animationFps;
    }
}
