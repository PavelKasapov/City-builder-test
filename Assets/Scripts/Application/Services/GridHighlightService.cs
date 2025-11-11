using Domain.Models;
using R3;
using UnityEngine;
using Grid = Domain.Models.Grid;

namespace Application.Services
{
    public class GridHighlightService
    {
        private readonly Grid _grid;

        public ReactiveProperty<GridPosition?> HoveredPosition { get; } = new();
        public ReactiveProperty<bool> IsPositionValid { get; } = new();

        public GridHighlightService(Grid grid)
        {
            _grid = grid;
        }

        public void UpdateHoveredPosition(GridPosition gridPosition)
        {
            HoveredPosition.Value = gridPosition;
            IsPositionValid.Value = IsPositionBuildable(gridPosition);
        }

        public void ClearHover()
        {
            HoveredPosition.Value = null;
            IsPositionValid.Value = false;
        }

        private bool IsPositionBuildable(GridPosition position)
        {
            if (position.X < 0 || position.X >= _grid.Width ||
                position.Y < 0 || position.Y >= _grid.Height)
                return false;

            GridCell cell = _grid.GetCell(position);
            return cell != null && !cell.IsOccupied;
        }
    }
}
