using MessagePipe;
using Application.Services;
using Domain.Events;
using Domain.Models;
using R3;
using UnityEngine;
using Grid = Domain.Models.Grid;

namespace Application
{
    public class PlaceBuildingUseCase : System.IDisposable
    {
        private readonly Grid _grid;
        private readonly EconomyService _economyService;
        private readonly IPublisher<BuildingPlacedEvent> _publisher;
        private readonly IPublisher<NotEnoughResourcesEvent> _errorPublisher;
        private readonly ISubscriber<PlaceBuildingCommand> _commandSubscriber;
        private readonly CompositeDisposable _disposables = new();
        private readonly GridDataService _gridDataService;
        private readonly BuildingDataService _buildingDataService;

        public PlaceBuildingUseCase(
            Grid grid,
            EconomyService economyService,
            GridDataService gridDataService,
            IPublisher<BuildingPlacedEvent> publisher,
            IPublisher<NotEnoughResourcesEvent> errorPublisher,
            ISubscriber<PlaceBuildingCommand> commandSubscriber,
            BuildingDataService buildingDataService)
        {
            _grid = grid;
            _economyService = economyService;
            _gridDataService = gridDataService;
            _publisher = publisher;
            _errorPublisher = errorPublisher;
            _commandSubscriber = commandSubscriber;
            _commandSubscriber.Subscribe(Handle).AddTo(_disposables);
            _buildingDataService = buildingDataService;
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void Handle(PlaceBuildingCommand command)
        {
            Debug.Log($"Processing command: {command.BuildingType} at {command.Position}");

            BuildingSize size = _buildingDataService.GetBuildingSize(command.BuildingType);

            if (!_grid.CanPlaceBuilding(command.Position, size))
            {
                Debug.Log($"Cannot place building at {command.Position}");
                return;
            }

            ResourceData cost = _buildingDataService.GetBuildingCost(command.BuildingType);
            if (!_economyService.TrySpend(cost))
            {
                Debug.Log($"Not enough gold! Current: {_economyService.Gold.CurrentValue}, Required: {cost.Amount}");
                _errorPublisher.Publish(new NotEnoughResourcesEvent
                {
                    ResourceType = ResourceType.Gold
                });
                return;
            }

            Debug.Log($"Building placed! Gold spent: {cost.Amount}, Remaining: {_economyService.Gold.CurrentValue}");

            _grid.PlaceBuilding(command.Position, command.BuildingType, size);

            _gridDataService.UpdateCellArea(command.Position, size, true);

            _publisher.Publish(new BuildingPlacedEvent
            {
                Position = command.Position,
                BuildingType = command.BuildingType,
                Size = size
            });
        }
    }
}
