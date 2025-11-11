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

        public PlaceBuildingUseCase(
            Grid grid,
            EconomyService economyService,
            GridDataService gridDataService,
            IPublisher<BuildingPlacedEvent> publisher,
            IPublisher<NotEnoughResourcesEvent> errorPublisher,
            ISubscriber<PlaceBuildingCommand> commandSubscriber)
        {
            this._grid = grid;
            this._economyService = economyService;
            this._gridDataService = gridDataService;
            this._publisher = publisher;
            this._errorPublisher = errorPublisher;
            this._commandSubscriber = commandSubscriber;
            this._commandSubscriber.Subscribe(this.Handle).AddTo(this._disposables);
        }

        public void Dispose()
        {
            this._disposables?.Dispose();
        }

        private void Handle(PlaceBuildingCommand command)
        {
            Debug.Log($"Processing command: {command.BuildingType} at {command.Position}");

            GridCell cell = this._grid.GetCell(command.Position);
            if (cell == null || cell.IsOccupied)
            {
                Debug.Log($"Cell invalid or occupied");
                return;
            }

            ResourceData cost = this.GetBuildingCost(command.BuildingType);
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

            cell.BuildingType = command.BuildingType;

            this._gridDataService.UpdateCellState(command.Position, true);

            this._publisher.Publish(new BuildingPlacedEvent
            {
                Position = command.Position,
                BuildingType = command.BuildingType,
                IsOccupied = true
            });
        }

        private ResourceData GetBuildingCost(BuildingType type)
        {
            return type switch
            {
                BuildingType.House => new ResourceData { Type = ResourceType.Gold, Amount = 100 },
                BuildingType.Farm => new ResourceData { Type = ResourceType.Gold, Amount = 150 },
                BuildingType.Mine => new ResourceData { Type = ResourceType.Gold, Amount = 200 },
                _ => new ResourceData { Type = ResourceType.Gold, Amount = 0 }
            };
        }
    }
}
