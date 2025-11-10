using Domain.Models;

namespace Domain.Events
{
    public struct PlaceBuildingCommand
    {
        public GridPosition Position;
        public BuildingType BuildingType;
    }
}
