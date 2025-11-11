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
        public ReactiveProperty<BuildingSize?> HoveredSize { get; } = new();
        public ReactiveProperty<bool> IsPositionValid { get; } = new();

        public GridHighlightService(Grid grid)
        {
            this._grid = grid;
        }

        public void UpdateHoveredArea(GridPosition position, BuildingSize size, bool isValid)
        {
            this.HoveredPosition.Value = position;
            this.HoveredSize.Value = size;
            this.IsPositionValid.Value = isValid;
        }

        public void ClearHover()
        {
            this.HoveredPosition.Value = null;
            this.HoveredSize.Value = null;
            this.IsPositionValid.Value = false;
        }
    }
}
