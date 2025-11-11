using Domain.Models;
using R3;
using UnityEngine;

namespace Application.Interfaces
{
    public interface IInputService
    {
        ReactiveProperty<Vector2> MousePosition { get; }
        ReactiveProperty<Vector2> CameraMovement { get; }
        ReactiveProperty<float> CameraZoom { get; }

        Observable<Unit> OnLeftClick { get; }
        Observable<Unit> OnRightClick { get; }
        Observable<Unit> OnCancelBuild { get; }
        Observable<BuildingType> OnBuildingHotkey { get; }

        Vector3 GetMouseWorldPosition();
    }
}
