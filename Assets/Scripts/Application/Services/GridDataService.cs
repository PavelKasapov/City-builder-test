using Domain.Models;
using R3;
using System.Collections.Generic;

namespace Application.Services
{
    public class GridDataService
    {
        private readonly Grid _grid;
        private readonly Dictionary<GridPosition, bool> _cellStates = new();

        public ReadOnlyReactiveProperty<GridPosition?> HoveredPosition { get; }
        public ReadOnlyReactiveProperty<bool> IsHoverValid { get; }

        public GridDataService(Grid grid, GridHighlightService highlightService)
        {
            this._grid = grid;

            this.HoveredPosition = highlightService.HoveredPosition.ToReadOnlyReactiveProperty();
            this.IsHoverValid = highlightService.IsPositionValid.ToReadOnlyReactiveProperty();

            this.InitializeCellStates();
        }

        private void InitializeCellStates()
        {
            for (int x = 0; x < this._grid.Width; x++)
            {
                for (int y = 0; y < this._grid.Height; y++)
                {
                    GridPosition position = new GridPosition(x, y);
                    GridCell cell = this._grid.GetCell(position);
                    this._cellStates[position] = cell?.IsOccupied ?? false;
                }
            }
        }

        public bool IsCellOccupied(GridPosition position)
        {
            return this._cellStates.TryGetValue(position, out bool occupied) && occupied;
        }

        public void UpdateCellState(GridPosition position, bool isOccupied)
        {
            this._cellStates[position] = isOccupied;
        }

        public IEnumerable<GridPosition> GetAllPositions()
        {
            for (int x = 0; x < this._grid.Width; x++)
            {
                for (int y = 0; y < this._grid.Height; y++)
                {
                    yield return new GridPosition(x, y);
                }
            }
        }

        public void UpdateCellArea(GridPosition position, BuildingSize size, bool isOccupied)
        {
            for (int x = position.X; x < position.X + size.Width; x++)
            {
                for (int y = position.Y; y < position.Y + size.Height; y++)
                {
                    this.UpdateCellState(new GridPosition(x, y), isOccupied);
                }
            }
        }
    }
}
