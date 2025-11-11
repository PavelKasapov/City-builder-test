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
            this._grid = grid;
            this._economyService = economyService;
            this._gridDataService = gridDataService;
            this._publisher = publisher;
            this._errorPublisher = errorPublisher;
            this._commandSubscriber = commandSubscriber;
            this._commandSubscriber.Subscribe(this.Handle).AddTo(this._disposables);
            this._buildingDataService = buildingDataService;
        }

        public void Dispose()
        {
            this._disposables?.Dispose();
        }

        private void Handle(PlaceBuildingCommand command)
        {
            Debug.Log($"Processing command: {command.BuildingType} at {command.Position}");

            BuildingSize size = this._buildingDataService.GetBuildingSize(command.BuildingType);

            if (!this._grid.CanPlaceBuilding(command.Position, size))
            {
                Debug.Log($"Cannot place building at {command.Position}");
                return;
            }

            ResourceData cost = this._buildingDataService.GetBuildingCost(command.BuildingType);
            if (!this._economyService.TrySpend(cost))
            {
                Debug.Log($"Not enough gold! Current: {this._economyService.Gold.CurrentValue}, Required: {cost.Amount}");
                this._errorPublisher.Publish(new NotEnoughResourcesEvent
                {
                    ResourceType = ResourceType.Gold
                });
                return;
            }

            Debug.Log($"Building placed! Gold spent: {cost.Amount}, Remaining: {this._economyService.Gold.CurrentValue}");

            this._grid.PlaceBuilding(command.Position, command.BuildingType, size);

            this._gridDataService.UpdateCellArea(command.Position, size, true);

            this._publisher.Publish(new BuildingPlacedEvent
            {
                Position = command.Position,
                BuildingType = command.BuildingType,
                Size = size
            });
        }

    }
}
