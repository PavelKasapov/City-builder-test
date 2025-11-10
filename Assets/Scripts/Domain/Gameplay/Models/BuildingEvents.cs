namespace Domain
{
    public struct BuildingPlacedEvent
    {
        public GridPosition Position;
        public BuildingType BuildingType;
        public bool IsOccupied;
    }

    public enum BuildingType
    {
        House,
        Farm,
        Mine
    }
}
