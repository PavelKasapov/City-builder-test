using Presentation.Interfaces;
using Domain.Events;
using Domain.Models;
using Application.Interfaces;
using Application.Services;
using MessagePipe;
using R3;
using VContainer;
using UnityEngine;
using VContainer.Unity;
using Grid = Domain.Models.Grid;

namespace Presentation.Gameplay.Presenters
{
    public class BuildingInputPresenter : IInitializable, System.IDisposable
    {
        private readonly IHudView _hudView;
        private readonly IInputService _inputService;
        private readonly Grid _grid;
        private readonly GridHighlightService _highlightService;
        private readonly IPublisher<PlaceBuildingCommand> _commandPublisher;
        private readonly IGridCoordinateConverter _gridConverter;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private BuildingType? _selectedBuildingType;

        [Inject]
        public BuildingInputPresenter(
            IHudView hudView,
            IInputService inputService,
            Grid grid,
            GridHighlightService highlightService,
            IPublisher<PlaceBuildingCommand> commandPublisher,
            IGridCoordinateConverter gridConverter)
        {
            this._hudView = hudView;
            this._inputService = inputService;
            this._grid = grid;
            this._highlightService = highlightService;
            this._commandPublisher = commandPublisher;
            this._gridConverter = gridConverter;
        }

        public void Initialize()
        {
            this._hudView.OnBuildingSelected
                .Subscribe(this.OnBuildingSelected)
                .AddTo(this._disposables);

            this._inputService.OnBuildingHotkey
                .Subscribe(this.OnBuildingSelected)
                .AddTo(this._disposables);

            this._inputService.MousePosition
                .Subscribe(_ => this.OnMouseMoved())
                .AddTo(this._disposables);

            this._inputService.OnLeftClick
                .Subscribe(_ => this.OnLeftClick())
                .AddTo(this._disposables);

            this._inputService.OnRightClick
                .Subscribe(_ => this.OnRightClick())
                .AddTo(this._disposables);

            this._inputService.OnCancelBuild
                .Subscribe(_ => this.OnCancelBuild())
                .AddTo(this._disposables);
        }

        public void Dispose()
        {
            this._disposables?.Dispose();
            this._highlightService.ClearHover();
        }

        private void OnBuildingSelected(BuildingType buildingType)
        {
            Debug.Log($"Building selected: {buildingType}");
            this._selectedBuildingType = buildingType;
        }

        private void OnMouseMoved()
        {
            if (!this._selectedBuildingType.HasValue)
            {
                this._highlightService.ClearHover();
                return;
            }

            Vector3 worldPosition = this._inputService.GetMouseWorldPosition();
            GridPosition gridPosition = this.WorldToGridPosition(worldPosition);


            this._highlightService.UpdateHoveredPosition(gridPosition);
        }

        private void OnLeftClick()
        {
            if (!this._selectedBuildingType.HasValue ||
                !this._highlightService.HoveredPosition.Value.HasValue ||
                !this._highlightService.IsPositionValid.Value)
                return;

            GridPosition position = this._highlightService.HoveredPosition.Value.Value;
            BuildingType buildingType = this._selectedBuildingType.Value;

            Debug.Log($"Placing {buildingType} at {position}");

            this._commandPublisher.Publish(new PlaceBuildingCommand
            {
                Position = position,
                BuildingType = buildingType
            });
        }

        private void OnRightClick() => this.ClearSelection();
        private void OnCancelBuild() => this.ClearSelection();

        private void ClearSelection()
        {
            if (this._selectedBuildingType.HasValue)
            {
                Debug.Log("Build cancelled");
                this._selectedBuildingType = null;
                this._highlightService.ClearHover();
            }
        }

        private GridPosition WorldToGridPosition(Vector3 worldPosition)
        {
            GridPosition gridPosition = this._gridConverter.WorldToGridPosition(worldPosition);

            int x = Mathf.Clamp(gridPosition.X, 0, this._grid.Width - 1);
            int y = Mathf.Clamp(gridPosition.Y, 0, this._grid.Height - 1);

            return new GridPosition(x, y);
        }
    }
}
