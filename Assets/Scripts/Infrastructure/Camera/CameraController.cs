using UnityEngine;
using R3;
using VContainer;
using Application.Interfaces;
using VContainer.Unity;

namespace Infrastructure.Camera
{
    public class CameraController : IInitializable, System.IDisposable
    {
        private readonly IInputService _inputService;
        private readonly CompositeDisposable _disposables = new();
        private UnityEngine.Camera _mainCamera;
        private Transform _cameraTransform;

        private float _baseMoveSpeed = 20f;

        private float _zoomSpeed = 2f;
        private float _zoomSmoothness = 0.05f;
        private float _minOrthoSize = 10f;
        private float _maxOrthoSize = 50f;
        private float _defaultOrthoSize = 25f;

        private float _acceleration = 8f;
        private float _deceleration = 12f;
        private float _maxSpeedMultiplier = 2.5f;

        private Vector2 _targetMovement;
        private Vector2 _currentMovement;
        private float _targetZoom;
        private float _currentZoom;
        private float _targetOrthoSize;

        private float _currentMoveSpeed;

        [Inject]
        public CameraController(IInputService inputService)
        {
            _inputService = inputService;
        }

        public void Initialize()
        {
            _mainCamera = UnityEngine.Camera.main;
            if (_mainCamera != null)
            {
                _cameraTransform = _mainCamera.transform;
                _targetOrthoSize = _mainCamera.orthographicSize;
                _defaultOrthoSize = _mainCamera.orthographicSize;
            }

            SetupCameraInput();
            Debug.Log("[CameraController] Initialized");
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void SetupCameraInput()
        {
            _inputService.CameraMovement
                .Subscribe(movement => {
                    _targetMovement = movement;
                })
                .AddTo(_disposables);

            _inputService.CameraZoom
                .Subscribe(zoom => {
                    _targetZoom = zoom;
                })
                .AddTo(_disposables);

            Observable.EveryUpdate()
                .Subscribe(_ => ApplySmoothCameraMovement())
                .AddTo(_disposables);
        }

        private void ApplySmoothCameraMovement()
        {
            if (_cameraTransform == null) return;

            UpdateMovementWithInertia();
            ApplyOrthographicZoom();
        }

        private void UpdateMovementWithInertia()
        {
            _currentMoveSpeed = _targetMovement != Vector2.zero
                ? Mathf.Lerp(_currentMoveSpeed, _maxSpeedMultiplier, _acceleration * Time.deltaTime)
                : Mathf.Lerp(_currentMoveSpeed, 1f, _deceleration * Time.deltaTime);
            
            _currentMovement = Vector2.Lerp(_currentMovement, _targetMovement, 5f * Time.deltaTime);

            if (_currentMovement != Vector2.zero)
            {
                float zoomFactor = CalculateZoomSpeedFactor();

                Vector3 move = new Vector3(_currentMovement.x, _currentMovement.y, 0)
                    * _baseMoveSpeed * _currentMoveSpeed * zoomFactor * Time.deltaTime;
                _cameraTransform.Translate(move, Space.World);
            }
        }

        private float CalculateZoomSpeedFactor()
        {
            if (_mainCamera == null) return 1f;

            float baseFactor = _mainCamera.orthographicSize / _defaultOrthoSize;
            return Mathf.Clamp(baseFactor, 0.3f, 3f);
        }

        private void ApplyOrthographicZoom()
        {
            if (_mainCamera == null) return;

            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, _zoomSmoothness);

            if (!Mathf.Approximately(_currentZoom, 0f))
            {
                _targetOrthoSize -= _currentZoom * _zoomSpeed;
                _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, _minOrthoSize, _maxOrthoSize);

                _mainCamera.orthographicSize = Mathf.Lerp(
                    _mainCamera.orthographicSize,
                    _targetOrthoSize,
                    _zoomSmoothness
                );
            }
        }

        /*public void SetBaseMoveSpeed(float speed) => _baseMoveSpeed = speed;
        public void SetZoomSpeed(float speed) => _zoomSpeed = speed;
        public void SetZoomSmoothness(float smoothness) => _zoomSmoothness = smoothness;
        public void SetOrthoSizeLimits(float min, float max)
        {
            _minOrthoSize = min;
            _maxOrthoSize = max;
            _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, min, max);
        }
        public void SetAcceleration(float acceleration) => _acceleration = acceleration;
        public void SetDeceleration(float deceleration) => _deceleration = deceleration;
        public void SetMaxSpeedMultiplier(float multiplier) => _maxSpeedMultiplier = multiplier;

        public float GetCurrentSpeed() => _baseMoveSpeed * _currentMoveSpeed * CalculateZoomSpeedFactor();
        public Vector2 GetCurrentMovement() => _currentMovement;
        public float GetCurrentOrthoSize() => _mainCamera != null ? _mainCamera.orthographicSize : 0f;
        public float GetZoomSpeedFactor() => CalculateZoomSpeedFactor();*/
    }
}
