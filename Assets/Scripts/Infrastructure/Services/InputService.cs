using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using VContainer;
using Application.Interfaces;
using VContainer.Unity;
using Domain.Models;
using ReactiveInputSystem;

namespace Infrastructure.Input
{
    public class InputService : IInputService, IInitializable, System.IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly PlayerInputActions _inputActions;
        private UnityEngine.Camera _mainCamera;

        public ReactiveProperty<Vector2> MousePosition { get; } = new ReactiveProperty<Vector2>();
        public ReactiveProperty<Vector2> CameraMovement { get; } = new ReactiveProperty<Vector2>();
        public ReactiveProperty<float> CameraZoom { get; } = new ReactiveProperty<float>();

        public Observable<Unit> OnLeftClick { get; }
        public Observable<Unit> OnRightClick { get; }
        public Observable<Unit> OnCancelBuild { get; }
        public Observable<BuildingType> OnBuildingHotkey { get; }

        private readonly Subject<Unit> _leftClickSubject = new Subject<Unit>();
        private readonly Subject<Unit> _rightClickSubject = new Subject<Unit>();
        private readonly Subject<Unit> _cancelBuildSubject = new Subject<Unit>();
        private readonly Subject<BuildingType> _buildingHotkeySubject = new Subject<BuildingType>();

        [Inject]
        public InputService()
        {
            this._inputActions = new PlayerInputActions();
            this._mainCamera = UnityEngine.Camera.main;

            this.OnLeftClick = this._leftClickSubject;
            this.OnRightClick = this._rightClickSubject;
            this.OnCancelBuild = this._cancelBuildSubject;
            this.OnBuildingHotkey = this._buildingHotkeySubject;
        }

        public void Initialize()
        {
            this.EnableInput();
            this.SetupObservables();
        }

        public void Dispose()
        {
            this.DisableInput();
            this._disposables?.Dispose();
            this._leftClickSubject?.Dispose();
            this._rightClickSubject?.Dispose();
            this._cancelBuildSubject?.Dispose();
            this._buildingHotkeySubject?.Dispose();
        }

        private void EnableInput()
        {
            this._inputActions.Gameplay.Enable();
            this._inputActions.BuildingSelection.Enable();
            this._inputActions.Camera.Enable();
        }

        private void DisableInput()
        {
            this._inputActions.Gameplay.Disable();
            this._inputActions.BuildingSelection.Disable();
            this._inputActions.Camera.Disable();
            this._inputActions?.Dispose();
        }

        private void SetupObservables()
        {
            // Mouse position updates
            Observable.EveryUpdate()
                .Subscribe(_ => this.UpdateMousePosition())
                .AddTo(this._disposables);

            // Left Click
            this._inputActions.Gameplay.LeftClick
                .PerformedAsObservable()
                .Subscribe(_ => this._leftClickSubject.OnNext(Unit.Default))
                .AddTo(this._disposables);

            // Right Click
            this._inputActions.Gameplay.RightClick
                .PerformedAsObservable()
                .Subscribe(_ => this._rightClickSubject.OnNext(Unit.Default))
                .AddTo(this._disposables);

            // Building Hotkeys
            this._inputActions.BuildingSelection.House
                .PerformedAsObservable()
                .Subscribe(_ => this._buildingHotkeySubject.OnNext(BuildingType.House))
                .AddTo(this._disposables);

            this._inputActions.BuildingSelection.Farm
                .PerformedAsObservable()
                .Subscribe(_ => this._buildingHotkeySubject.OnNext(BuildingType.Farm))
                .AddTo(this._disposables);

            this._inputActions.BuildingSelection.Mine
                .PerformedAsObservable()
                .Subscribe(_ => this._buildingHotkeySubject.OnNext(BuildingType.Mine))
                .AddTo(this._disposables);

            // Camera Movement - используем ReadValue в Update
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    this.CameraMovement.Value = this._inputActions.Camera.Movement.ReadValue<Vector2>();
                    this.CameraZoom.Value = this._inputActions.Camera.Zoom.ReadValue<float>();
                })
                .AddTo(this._disposables);

            // Cancel
            this._inputActions.Gameplay.Cancel
                .PerformedAsObservable()
                .Subscribe(_ => this._cancelBuildSubject.OnNext(Unit.Default))
                .AddTo(this._disposables);
        }

        private void UpdateMousePosition()
        {
            this.MousePosition.Value = Mouse.current.position.ReadValue();
        }

        public Vector3 GetMouseWorldPosition()
        {
            Vector2 mousePos = this.MousePosition.Value;

            // Для 2D камеры используем ScreenToWorldPoint
            Vector3 worldPosition = this._mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, this._mainCamera.nearClipPlane));

            // В 2D нам нужны только X и Y координаты
            worldPosition.z = 0; // Или другое фиксированное значение, если нужно

            Debug.Log($"[InputService] 2D Mouse: {mousePos} -> World: {worldPosition}");

            return worldPosition;
        }
    }
}
