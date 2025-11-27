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
        private readonly CompositeDisposable _disposables = new();
        private readonly PlayerInputActions _inputActions;
        private UnityEngine.Camera _mainCamera;
        public Observable<Vector2> MousePosition { get; private set; }
        public Observable<Vector2> CameraMovement { get; private set; }
        public Observable<float> CameraZoom { get; private set; }
        public Observable<Unit> OnLeftClick { get; private set; }
        public Observable<Unit> OnRightClick { get; private set; }
        public Observable<Unit> OnCancelBuild { get; private set; }
        public Observable<BuildingType> OnBuildingHotkey { get; private set; }

        [Inject]
        public InputService()
        {
            _inputActions = new PlayerInputActions();
            _mainCamera = UnityEngine.Camera.main;
        }

        public void Initialize()
        {
            EnableInput();
            SetupObservables();
        }

        public void Dispose()
        {
            _inputActions?.Dispose();
            _disposables?.Dispose();
        }

        private void EnableInput()
        {
            _inputActions.Gameplay.Enable();
            _inputActions.BuildingSelection.Enable();
            _inputActions.Camera.Enable();
        }

        private void DisableInput()
        {
            _inputActions.Gameplay.Disable();
            _inputActions.BuildingSelection.Disable();
            _inputActions.Camera.Disable();
        }

        private void SetupObservables()
        {
            MousePosition = Observable.EveryUpdate()
                .Select(_ => Mouse.current.position.ReadValue())
                .DistinctUntilChanged()
                .Publish()
                .RefCount();

            CameraMovement = _inputActions.Camera.Movement
                .PerformedAsObservable()
                .Merge(_inputActions.Camera.Movement.CanceledAsObservable())
                .Select(ctx => ctx.ReadValue<Vector2>())
                .DistinctUntilChanged()
                .Publish()
                .RefCount();

            CameraZoom = _inputActions.Camera.Zoom
                .PerformedAsObservable()
                .Merge(_inputActions.Camera.Zoom.CanceledAsObservable())
                .Select(ctx => ctx.ReadValue<float>())
                .DistinctUntilChanged()
                .Publish()
                .RefCount();

            OnLeftClick = _inputActions.Gameplay.LeftClick.PerformedAsObservable().Select(_ => Unit.Default);
            OnRightClick = _inputActions.Gameplay.RightClick.PerformedAsObservable().Select(_ => Unit.Default);
            OnCancelBuild = _inputActions.Gameplay.Cancel.PerformedAsObservable().Select(_ => Unit.Default);

            OnBuildingHotkey = Observable.Merge(
                _inputActions.BuildingSelection.House.PerformedAsObservable().Select(_ => BuildingType.House),
                _inputActions.BuildingSelection.Farm.PerformedAsObservable().Select(_ => BuildingType.Farm),
                _inputActions.BuildingSelection.Mine.PerformedAsObservable().Select(_ => BuildingType.Mine)
            );
        }

        public Vector2 GetMousePosition() => Mouse.current.position.ReadValue();

        public Vector3 GetMouseWorldPosition()
        {
            Vector2 mousePos = GetMousePosition();
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(
                new Vector3(mousePos.x, mousePos.y, _mainCamera.nearClipPlane));
            worldPosition.z = 0;
            return worldPosition;
        }
    }
}
