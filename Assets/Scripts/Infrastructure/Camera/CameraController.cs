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

        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _zoomSpeed = 5f;

        private Vector2 _currentMovement;
        private float _currentZoom;

        [Inject]
        public CameraController(IInputService inputService)
        {
            this._inputService = inputService;
        }

        public void Initialize()
        {
            this._mainCamera = UnityEngine.Camera.main;
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
            this._inputService.CameraMovement
                .Subscribe(movement => {
                    this._currentMovement = movement;
                })
                .AddTo(this._disposables);

            this._inputService.CameraZoom
                .Subscribe(zoom => {
                    this._currentZoom = zoom;
                })
                .AddTo(this._disposables);

            Observable.EveryUpdate()
                .Subscribe(_ => this.ApplyCameraMovement())
                .AddTo(this._disposables);
        }

        private void ApplyCameraMovement()
        {
            if (this._cameraTransform == null) return;

            if (this._currentMovement != Vector2.zero)
            {
                Vector3 move = new Vector3(this._currentMovement.x, this._currentMovement.y, 0)
                    * this._moveSpeed * Time.deltaTime;
                this._cameraTransform.Translate(move, Space.World);
            }

            if (!Mathf.Approximately(this._currentZoom, 0f))
            {
                Vector3 zoomMove = Vector3.forward * this._currentZoom * this._zoomSpeed * Time.deltaTime;
                this._cameraTransform.Translate(zoomMove, Space.Self);
            }
        }

        /*public void SetMoveSpeed(float speed) => this._moveSpeed = speed;
        public void SetZoomSpeed(float speed) => this._zoomSpeed = speed;*/
    }
}
