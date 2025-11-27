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
            _grid = grid;
        }

        public void UpdateHoveredArea(GridPosition position, BuildingSize size, bool isValid)
        {
            HoveredPosition.Value = position;
            HoveredSize.Value = size;
            IsPositionValid.Value = isValid;
        }

        public void ClearHover()
        {
            HoveredPosition.Value = null;
            HoveredSize.Value = null;
            IsPositionValid.Value = false;
        }
    }
}
