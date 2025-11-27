using Domain.Models;

public class GridCell
{
    public GridPosition Position { get; }
    public bool IsOccupied => BuildingType != BuildingType.None;
    public BuildingType BuildingType { get; set; } // Добавляем

    public GridCell(GridPosition position)
    {
        Position = position;
        BuildingType = BuildingType.None;
    }
}
