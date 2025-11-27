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
        private readonly BuildingDataService _buildingDataService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly CompositeDisposable _mouseSubscription = new CompositeDisposable();

        private BuildingType? _selectedBuildingType;

        [Inject]
        public BuildingInputPresenter(
            IHudView hudView,
            IInputService inputService,
            Grid grid,
            GridHighlightService highlightService,
            IPublisher<PlaceBuildingCommand> commandPublisher,
            IGridCoordinateConverter gridConverter,
            BuildingDataService buildingDataService)
        {
            _hudView = hudView;
            _inputService = inputService;
            _grid = grid;
            _highlightService = highlightService;
            _commandPublisher = commandPublisher;
            _gridConverter = gridConverter;
            _buildingDataService = buildingDataService;
        }

        public void Initialize()
        {
            _hudView.OnBuildingSelected
                .Subscribe(OnBuildingSelected)
                .AddTo(_disposables);

            _inputService.OnBuildingHotkey
                .Subscribe(OnBuildingSelected)
                .AddTo(_disposables);

            _inputService.OnLeftClick
                .Subscribe(_ => OnLeftClick())
                .AddTo(_disposables);

            _inputService.OnRightClick
                .Subscribe(_ => OnRightClick())
                .AddTo(_disposables);

            _inputService.OnCancelBuild
                .Subscribe(_ => OnCancelBuild())
                .AddTo(_disposables);

            Debug.Log("✅ BuildingInputPresenter инициализирован");
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            _mouseSubscription?.Dispose();
            _highlightService.ClearHover();
        }

        private void OnBuildingSelected(BuildingType buildingType)
        {
            Debug.Log($"🏗️ Выбрано здание: {buildingType}");
            _selectedBuildingType = buildingType;

            EnableMouseTracking();
        }

        private void EnableMouseTracking()
        {
            _mouseSubscription.Clear();

            _inputService.MousePosition
                .Subscribe(_ => OnMouseMoved())
                .AddTo(_mouseSubscription);

            Debug.Log("🖱️ Включено отслеживание мыши для строительства");
        }

        private void DisableMouseTracking()
        {
            _mouseSubscription.Clear();
            _highlightService.ClearHover();
            Debug.Log("🖱️ Отключено отслеживание мыши");
        }

        private void OnMouseMoved()
        {
            Vector3 worldPosition = _inputService.GetMouseWorldPosition();
            GridPosition gridPosition = WorldToGridPosition(worldPosition);

            BuildingSize size = _buildingDataService.GetBuildingSize(_selectedBuildingType.Value);
            bool isValid = _buildingDataService.IsPositionValidForBuilding(_grid, gridPosition, _selectedBuildingType.Value);

            _highlightService.UpdateHoveredArea(gridPosition, size, isValid);
        }

        private void OnLeftClick()
        {
            if (!_selectedBuildingType.HasValue ||
                !_highlightService.HoveredPosition.Value.HasValue ||
                !_highlightService.IsPositionValid.Value)
                return;

            GridPosition position = _highlightService.HoveredPosition.Value.Value;
            BuildingType buildingType = _selectedBuildingType.Value;

            Debug.Log($"[BuildingInputPresenter] Размещение {buildingType} в {position}");

            _commandPublisher.Publish(new PlaceBuildingCommand
            {
                Position = position,
                BuildingType = buildingType
            });
        }

        private void OnRightClick() => ClearSelection();
        private void OnCancelBuild() => ClearSelection();

        private void ClearSelection()
        {
            if (_selectedBuildingType.HasValue)
            {
                Debug.Log("🗑️ Отмена выбора здания");
                _selectedBuildingType = null;
                DisableMouseTracking();
            }
        }

        private GridPosition WorldToGridPosition(Vector3 worldPosition)
        {
            GridPosition gridPosition = _gridConverter.WorldToGridPosition(worldPosition);
            int x = Mathf.Clamp(gridPosition.X, 0, _grid.Width - 1);
            int y = Mathf.Clamp(gridPosition.Y, 0, _grid.Height - 1);
            return new GridPosition(x, y);
        }
    }
}
