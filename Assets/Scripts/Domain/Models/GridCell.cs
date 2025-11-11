using Domain.Models;

public class GridCell
{
    public GridPosition Position { get; }
    public bool IsOccupied => this.BuildingType != BuildingType.None;
    public BuildingType BuildingType { get; set; } // Добавляем

    public GridCell(GridPosition position)
    {
        this.Position = position;
        this.BuildingType = BuildingType.None;
    }
}
