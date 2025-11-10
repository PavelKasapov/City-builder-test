using Domain;
using MessagePipe;

namespace Application
{
    public class PlaceBuildingUseCase
    {
        private readonly Grid _grid;
        private readonly IPublisher<BuildingPlacedEvent> _publisher;

        public PlaceBuildingUseCase(Grid grid, IPublisher<BuildingPlacedEvent> publisher)
        {
            this._grid = grid;
            this._publisher = publisher;
        }

        public bool Execute(GridPosition position, BuildingType buildingType)
        {
            GridCell cell = this._grid.GetCell(position);
            if (cell == null || cell.IsOccupied)
            {
                return false;
            }

            cell.IsOccupied = true;

            this._publisher.Publish(new BuildingPlacedEvent
            {
                Position = position,
                BuildingType = buildingType,
                IsOccupied = true
            });

            return true;
        }
    }
}
