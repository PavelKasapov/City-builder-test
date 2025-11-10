namespace Domain.Models
{
    public class GridCell
    {
        public GridPosition Position { get; }
        public bool IsOccupied { get; set; }

        public GridCell(GridPosition position)
        {
            this.Position = position;
            this.IsOccupied = false;
        }
    }
}
