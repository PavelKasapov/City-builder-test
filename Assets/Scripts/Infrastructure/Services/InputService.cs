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

        // Оптимизированные стримы - активируются только при наличии подписчиков
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
            this._inputActions = new PlayerInputActions();
            this._mainCamera = UnityEngine.Camera.main;
        }

        public void Initialize()
        {
            this.EnableInput();
            this.SetupObservables();
        }

        public void Dispose()
        {
            this._inputActions?.Dispose();
            this._disposables?.Dispose();
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
        }

        private void SetupObservables()
        {
            // Мышь - активируется только когда кто-то подписан
            this.MousePosition = Observable.EveryUpdate()
                .Select(_ => Mouse.current.position.ReadValue())
                .DistinctUntilChanged()
                .Publish()
                .RefCount();

            // Камера - активируется только когда кто-то подписан
            this.CameraMovement = this._inputActions.Camera.Movement
                .PerformedAsObservable()
                .Merge(this._inputActions.Camera.Movement.CanceledAsObservable())
                .Select(ctx => ctx.ReadValue<Vector2>())
                .Publish()
                .RefCount();

            this.CameraZoom = this._inputActions.Camera.Zoom
                .PerformedAsObservable()
                .Merge(this._inputActions.Camera.Zoom.CanceledAsObservable())
                .Select(ctx => ctx.ReadValue<float>())
                .Publish()
                .RefCount();

            // Клики - не нуждаются в Publish().RefCount() т.к. это одиночные события
            this.OnLeftClick = this._inputActions.Gameplay.LeftClick.PerformedAsObservable().Select(_ => Unit.Default);
            this.OnRightClick = this._inputActions.Gameplay.RightClick.PerformedAsObservable().Select(_ => Unit.Default);
            this.OnCancelBuild = this._inputActions.Gameplay.Cancel.PerformedAsObservable().Select(_ => Unit.Default);

            // Горячие клавиши - не нуждаются в Publish().RefCount()
            this.OnBuildingHotkey = Observable.Merge(
                this._inputActions.BuildingSelection.House.PerformedAsObservable().Select(_ => BuildingType.House),
                this._inputActions.BuildingSelection.Farm.PerformedAsObservable().Select(_ => BuildingType.Farm),
                this._inputActions.BuildingSelection.Mine.PerformedAsObservable().Select(_ => BuildingType.Mine)
            );
        }

        public Vector2 GetMousePosition() => Mouse.current.position.ReadValue();

        public Vector3 GetMouseWorldPosition()
        {
            Vector2 mousePos = this.GetMousePosition();
            Vector3 worldPosition = this._mainCamera.ScreenToWorldPoint(
                new Vector3(mousePos.x, mousePos.y, this._mainCamera.nearClipPlane));
            worldPosition.z = 0;
            return worldPosition;
        }
    }
}
