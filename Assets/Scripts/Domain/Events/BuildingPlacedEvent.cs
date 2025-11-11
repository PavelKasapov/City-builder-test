using Domain.Models;

namespace Domain.Events
{
    public struct BuildingPlacedEvent
    {
        public GridPosition Position;
        public BuildingType BuildingType;
        public BuildingSize Size;
    }
}
