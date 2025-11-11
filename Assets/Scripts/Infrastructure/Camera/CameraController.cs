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
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private UnityEngine.Camera _mainCamera; // ← Явно указан UnityEngine.Camera
        private Transform _cameraTransform;

        [SerializeField]
        private float _moveSpeed = 10f;
        [SerializeField]
        private float _zoomSpeed = 5f;

        [Inject]
        public CameraController(IInputService inputService)
        {
            this._inputService = inputService;
        }

        public void Initialize()
        {
            this._mainCamera = UnityEngine.Camera.main; // ← Явное указание
            if (this._mainCamera != null)
            {
                this._cameraTransform = this._mainCamera.transform;
            }

            this.SetupCameraInput();
            Debug.Log("[CameraController] Initialized");
        }

        public void Dispose()
        {
            this._disposables?.Dispose();
        }

        private void SetupCameraInput()
        {
            Observable.EveryUpdate()
                .Subscribe(_ => this.UpdateCameraMovement())
                .AddTo(this._disposables);

            this._inputService.CameraZoom
                .Subscribe(this.UpdateCameraZoom)
                .AddTo(this._disposables);
        }

        private void UpdateCameraMovement()
        {
            if (this._cameraTransform == null) return;

            Vector2 movement = this._inputService.CameraMovement.Value;
            if (movement != Vector2.zero)
            {
                Vector3 move = new Vector3(movement.x, movement.y, 0) * this._moveSpeed * Time.deltaTime;
                this._cameraTransform.Translate(move, Space.World);
            }
        }

        private void UpdateCameraZoom(float zoom)
        {
            if (this._cameraTransform == null || Mathf.Approximately(zoom, 0f)) return;

            Vector3 zoomMove = Vector3.forward * zoom * this._zoomSpeed * Time.deltaTime;
            this._cameraTransform.Translate(zoomMove, Space.Self);
        }
    }
}
