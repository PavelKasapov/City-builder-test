namespace Domain.Models
{
    public class Grid
    {
        private readonly GridCell[,] _cells;
        public int Width { get; }
        public int Height { get; }

        public Grid(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this._cells = new GridCell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    this._cells[x, y] = new GridCell(new GridPosition(x, y));
                }
            }
        }

        public GridCell GetCell(GridPosition position)
        {
            if (position.X >= 0 && position.X < this.Width &&
                position.Y >= 0 && position.Y < this.Height)
            {
                return this._cells[position.X, position.Y];
            }

            return null;
        }


        public bool CanPlaceBuilding(GridPosition position, BuildingSize size)
        {
            for (int x = position.X; x < position.X + size.Width; x++)
            {
                for (int y = position.Y; y < position.Y + size.Height; y++)
                {
                    GridPosition currentPos = new GridPosition(x, y);
                    GridCell cell = this.GetCell(currentPos);

                    if (cell == null || cell.IsOccupied)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public void PlaceBuilding(GridPosition position, BuildingType buildingType, BuildingSize size)
        {
            for (int x = position.X; x < position.X + size.Width; x++)
            {
                for (int y = position.Y; y < position.Y + size.Height; y++)
                {
                    GridPosition currentPos = new GridPosition(x, y);
                    GridCell cell = this.GetCell(currentPos);

                    if (cell != null)
                    {
                        cell.BuildingType = buildingType;
                    }
                }
            }
        }
    }
}
